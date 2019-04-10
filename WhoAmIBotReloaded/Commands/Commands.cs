//using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhoAmIBotReloaded.Helpers;

namespace WhoAmIBotReloaded.Commands
{
    public abstract class Commands
    {
        // will be injected
        protected static Bot Bot;
        // will also be injected
        protected static WhoAmIDBContainer DB;
        // and this will also be injected
        //protected static RedisClient Redis;

        internal static PermissionLevel DefaultPermissionLevel = PermissionLevel.All;
        internal static CommandTypes DefaultCommandTypes = CommandTypes.Message;
    }
}
