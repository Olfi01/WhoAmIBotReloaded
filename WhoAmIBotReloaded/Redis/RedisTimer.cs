using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhoAmIBotReloaded.Redis
{
    public class RedisTimer
    {
        public TimerType Type { get; }
        public DateTimeOffset TimerEnd { get; set; }
        public string GameId { get; }
        public long ChatId { get; }
        public string TimerId { get; }
        public int ExtraValue { get; set; }

        public RedisTimer(TimerType type, DateTimeOffset timerEnd, string gameId, long chatId, int extraValue = 0)
        {
            Type = type;
            TimerEnd = timerEnd;
            GameId = gameId;
            ChatId = chatId;
            TimerId = Guid.NewGuid().ToString();
            ExtraValue = extraValue;
        }
    }

    public enum TimerType
    {
        GameStart,
        GameStartSoon
    }
}
