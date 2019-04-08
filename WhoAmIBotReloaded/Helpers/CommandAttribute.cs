using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhoAmIBotReloaded.Helpers
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal class CommandAttribute : Attribute
    {
        /// <summary>
        /// The prefeixes to accept for commands in messages
        /// </summary>
        public static readonly char[] CommandPrefixes = { '!', '/' };
        /// <summary>
        /// The trigger for the command (without the prefix)
        /// </summary>
        public string Trigger { get; }
        /// <summary>
        /// From what kind of update to accept this command
        /// </summary>
        public CommandTypes Types { get; set; } = CommandTypes.Message;
        /// <summary>
        /// Required permission level to use this command
        /// </summary>
        public PermissionLevel PermissionLevel { get; set; } = PermissionLevel.All;

        public CommandAttribute(string trigger)
        {
            Trigger = trigger;
        }
    }

    [Flags]
    internal enum CommandTypes
    {
        Message = 1,
        CallbackQuery = 2,
        MessageAndCallbackQuery = Message | CallbackQuery,
    }

    internal enum PermissionLevel
    {
        All,
        AdminOnly,
        GlobalAdminOnly,
        DevOnly
    }
}
