using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhoAmIBotReloaded.Redis
{
    public static class RedisKeys
    {
        public const string GroupGameIdDict = "groupGameIdDict";
        public const string Timers = "timers";
    }

    public static class RedisLocks
    {
        public const string Timers = "timers";
        public const string Games = "games";
    }
}
