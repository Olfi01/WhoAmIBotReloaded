using Microsoft.Win32;
using System.IO;

namespace WhoAmIBotReloaded.Helpers
{
    internal static class Settings
    {
        /// <summary>
        /// List of people allowed to run dev commands
        /// </summary>
        public static readonly int[] Devs = { 267376056 };

        /// <summary>
        /// The id of the chat to send update requests to
        /// </summary>
        public static readonly long DevChat = -1001070844778;

        #region Updating
        /// <summary>
        /// Base directory of the git repository. If this is empty, git will not be used to update.
        /// </summary>
        public static readonly string GitDirectory = "C:\\Olfi01\\WhoAmIBotReloaded\\";

        /// <summary>
        /// Path of the solution to compile
        /// </summary>
        public static readonly string SolutionPath = Path.Combine(GitDirectory, "WhoAmIBotReloaded.sln");

        /// <summary>
        /// Path of the executable after it has been compiled
        /// </summary>
        public static readonly string ExecutablePath = Path.Combine(GitDirectory, "WhoAmIBotReloaded\\bin\\Release\\WhoAmIBotReloaded.exe");

        /// <summary>
        /// The path to copy to and execute the program in
        /// </summary>
        public static readonly string ExecutionDirectory = "C:\\Olfi01\\WhoAmIBotReloadedExec\\";

        /// <summary>
        /// Path of the cleaner executable
        /// </summary>
        public static readonly string CleanerPath = Path.Combine(GitDirectory, "Cleaner\\bin\\Release\\Cleaner.exe");

        /// <summary>
        /// Branch to use for updating.
        /// </summary>
#if DEBUG
        public static readonly string GitBranch = "dev";
#else
        public static readonly string GitBranch = "master";
#endif

        /// <summary>
        /// Repository to pull from. If this is null, the default upstream repo will be used.
        /// </summary>
        public static readonly string GitRepository = null;

        /// <summary>
        /// The port to actively listen for pushes on. If this is null, don't listen at all.
        /// </summary>
#if DEBUG
        public static readonly string ListenForGitPrefix = null;
#else
        public static readonly string ListenForGitPrefix = "http://185.249.197.95:4242/whoAmIGit/";
#endif
#endregion

        /// <summary>
        /// The token of the telegram bot
        /// </summary>
        public static string BotToken
        {
#if DEBUG
            get => (string)RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                .OpenSubKey("SOFTWARE").OpenSubKey("Crazypokemondev").OpenSubKey("WhoAmI").GetValue("DebugAPIToken");
#else
            get => (string)RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                .OpenSubKey("SOFTWARE").OpenSubKey("Crazypokemondev").OpenSubKey("WhoAmI").GetValue("APIToken");
#endif
        }

        /// <summary>
        /// The connection string for the sql database
        /// </summary>
        public static string DbConnectionString
        {
            get => (string)RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
                .OpenSubKey("SOFTWARE").OpenSubKey("Crazypokemondev").OpenSubKey("WhoAmI").GetValue("ConnectionString");
        }

        /// <summary>
        /// Directory to store the language files in
        /// </summary>
        public static readonly string LanguageDirectory = Path.Combine(ExecutionDirectory, "Language\\");

        /// <summary>
        /// Name of the file to use as a master file, without the file extension
        /// </summary>
        public static readonly string MasterLanguageFile = "English";

        /// <summary>
        /// Host of redis cache
        /// </summary>
        public static readonly string RedisHost = "localhost";
    }
}
