using Microsoft.Win32;
using Newtonsoft.Json;
using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using WhoAmIBotReloaded.Handlers;
using WhoAmIBotReloaded.Helpers;
using WhoAmIBotReloaded.Redis;
using File = System.IO.File;

namespace WhoAmIBotReloaded
{
    public class Program
    {
        public static Bot Bot { get; private set; }
        public static WhoAmIDBContainer DB { get; private set; }
        public static RedisClient Redis { get; private set; }
        public static readonly ManualResetEvent ShutdownHandle = new ManualResetEvent(false);
        public static readonly Dictionary<string, Timer> TimerDict = new Dictionary<string, Timer>();
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var waitHandle = args[0];
                var eventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, waitHandle);
                eventWaitHandle.Set();
                // give old program time to shut down
                // Thread.Sleep(1000);
            }

            DB = new WhoAmIDBContainer(Settings.DbConnectionString);

            Redis = new RedisClient(Settings.RedisHost, Settings.RedisPort, db: Settings.RedisDb);

            Bot = new Bot(Settings.BotToken, args.Length < 1);
            Bot.Api.OnUpdate += UpdateHandler.OnUpdate;
            Bot.Start();
            Console.Title = $"WhoAmIBotReloaded - {Bot.Username} ({Bot.Id}) - Version {Assembly.GetExecutingAssembly().GetName().Version}";

            using (Redis.AcquireLock(RedisLocks.Timers))
            {
                if (!Redis.ContainsKey(RedisKeys.Timers)) Redis.Set(RedisKeys.Timers, new List<RedisTimer>());
                var timers = Redis.Get<List<RedisTimer>>(RedisKeys.Timers);
                foreach (var t in timers)
                {
                    int dueTime = (int)Math.Round((t.TimerEnd - DateTimeOffset.Now).TotalMilliseconds);
                    if (dueTime > 0) new Timer(Timers.TimerElapsed, t, dueTime, Timeout.Infinite);
                }
            }

            //UpdateListenerThread = new Thread(ListenForUpdates);
            //UpdateListenerThread.Start();
            ListenForUpdates();

            ShutdownHandle.WaitOne();

            // send the cleaner to clean up the execution directory
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = Settings.CleanerPath,
                WorkingDirectory = Path.Combine(Environment.CurrentDirectory, ".."),
                Arguments = Environment.CurrentDirectory
            };
            Process cleaner = new Process { StartInfo = psi };
            cleaner.Start();
            Environment.Exit(0);
        }

        /// <summary>
        /// Listens for updates from a git webhook
        /// </summary>
        private static void ListenForUpdates()
        {
            if (Settings.ListenForGitPrefix != null)
            {
                // give old program time to shut down
                Thread.Sleep(5000);
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add(Settings.ListenForGitPrefix);
                Console.WriteLine($"Listening on prefix {Settings.ListenForGitPrefix}");
                listener.Start();
                listener.BeginGetContext(RequestReceived, listener);
            }
            else Console.WriteLine("No prefix found, not listening for updates.");
        }

        private static void RequestReceived(IAsyncResult ar)
        {
            HttpListener listener = (HttpListener)ar.AsyncState;
            var context = listener.EndGetContext(ar);
            using (var sr = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                var req = sr.ReadToEnd();
                dynamic payload = JsonConvert.DeserializeObject(req);
                if (payload.@ref == $"refs/heads/{Settings.GitBranch}")
                {
                    Bot.Api.SendTextMessageAsync(Settings.DevChat,
                        $"{payload.commits.Count} new commits to <a href=\"{payload.compare}\">{payload.@ref}</a>.\n" +
                        $"Head commit: <a href=\"{payload.head_commit.url}\">{payload.head_commit.message}</a>\n" +
                        $"Update?",
                        replyMarkup: ReplyMarkups.GetUpdateMarkup(), parseMode: ParseMode.Html).Wait();
                }
            }
            using (var sw = new StreamWriter(context.Response.OutputStream))
            {
                sw.WriteLine("Response");
                sw.Flush();
            }
            listener.BeginGetContext(RequestReceived, listener);
        }

        public static async Task<bool> UpdateAsync(Message msg)
        {
            return await Task.Run(() => Update(msg));
        }

        /// <summary>
        /// Updates the bot
        /// </summary>
        /// <param name="msg">The message to update with the progress, if any</param>
        /// <returns></returns>
        public static bool Update(Message msg)
        {
            try
            {
                if (Settings.GitDirectory == null) return false;

                if (msg != null) Bot.Append(ref msg, "\nPulling git...");
                // Assumes that git is installed and on the PATH.
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    WorkingDirectory = Settings.GitDirectory,
                    FileName = "git",
                    Arguments = $"checkout {Settings.GitBranch}",
                    CreateNoWindow = true
                };
                var p = new Process() { StartInfo = psi };
                p.Start();
                p.WaitForExit();

                psi = new ProcessStartInfo
                {
                    WorkingDirectory = Settings.GitDirectory,
                    FileName = "git",
                    Arguments = $"pull {Settings.GitRepository ?? ""} {(Settings.GitRepository == null ? "" : Settings.GitBranch)}",
                    CreateNoWindow = true
                };
                p = new Process { StartInfo = psi };
                p.Start();
                p.WaitForExit();

                if (msg != null) Bot.Append(ref msg, "\nRestoring nuget packages...");
                // Assumes that nuget is installed and on the PATH
                psi = new ProcessStartInfo
                {
                    WorkingDirectory = Settings.GitDirectory,
                    FileName = "nuget",
                    Arguments = "restore",
                    CreateNoWindow = true
                };
                p = new Process { StartInfo = psi };
                p.Start();
                p.WaitForExit();

                if (msg != null) Bot.Append(ref msg, "\nBuilding Solution...");
                // Assumes that devenv is installed and on the PATH
                string logTxtPath = Path.Combine(Environment.CurrentDirectory, "buildlog.txt");
                psi = new ProcessStartInfo
                {
                    WorkingDirectory = Path.GetDirectoryName(Settings.SolutionPath),
                    FileName = "devenv",
#if DEBUG
                    Arguments = $"\"{Settings.SolutionPath}\" /Rebuild Debug /Out {logTxtPath}",
#else
                    Arguments = $"\"{Settings.SolutionPath}\" /Rebuild Release /Out {logTxtPath}",
#endif
                    CreateNoWindow = true
                };
                p = new Process { StartInfo = psi };
                p.Start();
                p.WaitForExit();
                if (p.ExitCode != 0)
                {
                    using (var stream = File.OpenRead(logTxtPath)) Bot.Api.SendDocumentAsync(Settings.DevChat, new InputOnlineFile(stream), 
                        caption: "Something went wrong while building the solution. Here is the build log.");
                    return false;
                }

                string fromDir = Path.GetDirectoryName(Settings.ExecutablePath);
                string newVersion = AssemblyName.GetAssemblyName(Settings.ExecutablePath).Version.ToString();
                string toDir = Path.Combine(Settings.ExecutionDirectory, newVersion);
                if (msg != null) Bot.Append(ref msg, $"\nCopying files to {toDir}...");
                CopyRecursively(Directory.CreateDirectory(fromDir), Directory.CreateDirectory(toDir));
                string copiedExe = Path.Combine(toDir, Path.GetFileName(Settings.ExecutablePath));

                if (msg != null) Bot.Append(ref msg, $"\nStarting new Executable with version {newVersion}!");
                string waitHandle = Guid.NewGuid().ToString();
                psi = new ProcessStartInfo
                {
                    FileName = copiedExe,
                    Arguments = waitHandle,
                    WorkingDirectory = toDir
                };
                p = new Process { StartInfo = psi };
                EventWaitHandle handle = new EventWaitHandle(false, EventResetMode.ManualReset, waitHandle);
                p.Start();
                handle.WaitOne();
                return true;
            }
            catch (Exception ex)
            {
                Bot.Send(Settings.DevChat, ex.ToString());
                return false;
            }
        }

        private static void CopyRecursively(DirectoryInfo fromDir, DirectoryInfo toDir)
        {
            foreach (var subdir in fromDir.EnumerateDirectories())
                CopyRecursively(subdir, toDir.CreateSubdirectory(subdir.Name));
            foreach (var file in fromDir.EnumerateFiles())
                file.CopyTo(Path.Combine(toDir.FullName, file.Name));
        }
    }
}
