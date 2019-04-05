using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhoAmIBotReloaded.Helpers
{
    internal static class Settings
    {
        /// <summary>
        /// List of people allowed to run dev commands
        /// </summary>
        public static int[] Devs = { 267376056 };
        /// <summary>
        /// Base directory of the git repository. If this is empty, git will not be used to update. Assumes that git is installed and on the PATH.
        /// </summary>
        public static string GitDirectory = "C:\\Olfi01\\WhoAmIBotReloaded\\";
        /// <summary>
        /// The port to actively listen for pushes on. If this is 0, don't listen at all.
        /// </summary>
        public static int ListenForGitPort = 4242;
    }
}
