using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WhoAmIBotReloaded.Handlers;
using WhoAmIBotReloaded.Helpers;

namespace WhoAmIBotReloaded
{
    public class Program
    {
        private static Bot Bot { get; set; }
        private static Thread KeepAliveThread;
        private static readonly ManualResetEvent ShutdownHandle = new ManualResetEvent(false);
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var waitHandle = args[0];
                var eventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset, waitHandle);
                eventWaitHandle.Set();
            }

            Bot = new Bot(Settings.BotToken, args.Length < 1);
            Bot.Api.OnMessage += MessageHandler.MessageReceived;
            Bot.Start();

            KeepAliveThread = new Thread(KeepAlive);
            KeepAliveThread.Start();
        }

        private static void KeepAlive()
        {
            if (Settings.ListenForGitPrefix != null)
            {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add(Settings.ListenForGitPrefix);
                listener.Start();
                var context = listener.GetContext();
                using (var sr = new StreamReader(context.Request.InputStream))
                {
                    var req = sr.ReadToEnd();
                    dynamic payload = JsonConvert.DeserializeObject<dynamic>(req);
                    // TODO: check for correct json and react
                }
                using (var sw = new StreamWriter(context.Response.OutputStream))
                {
                    sw.WriteLine("Response");
                    sw.Flush();
                }
            }
            else
            {
                ShutdownHandle.WaitOne();
            }
        }

        public static bool Update()
        {
            if (Settings.GitDirectory == null) return false;

            // Assumes that git is installed and on the PATH.
            ProcessStartInfo psi = new ProcessStartInfo
            {
                WorkingDirectory = Settings.GitDirectory,
                FileName = "git",
                Arguments = $"pull {Settings.GitRepository ?? ""} {(Settings.GitRepository == null ? "" : Settings.GitBranch ?? "")}",
                CreateNoWindow = true
            };
            var p = new Process { StartInfo = psi };
            p.Start();
            p.WaitForExit();

            // Assumes that nuget is installed and on the PATH
            psi = new ProcessStartInfo
            {
                WorkingDirectory = Settings.GitDirectory,
                FileName = "nuget",
                Arguments = "restore",
                CreateNoWindow = true
            };

            // Assumes that devenv is installed and on the PATH
            psi = new ProcessStartInfo
            {
                WorkingDirectory = Path.GetDirectoryName(Settings.SolutionPath),
                FileName = "devenv",
                Arguments = $"\"{Settings.SolutionPath}\" /Build Release",
                CreateNoWindow = true
            };
            p = new Process { StartInfo = psi };
            p.Start();
            p.WaitForExit();

            string waitHandle = Guid.NewGuid().ToString();
            psi = new ProcessStartInfo
            {
                FileName = Settings.ExecutablePath,
                Arguments = waitHandle
            };
            p = new Process { StartInfo = psi };
            EventWaitHandle handle = new EventWaitHandle(false, EventResetMode.ManualReset, waitHandle);
            p.Start();
            handle.WaitOne();
            return true;
        }
    }
}
