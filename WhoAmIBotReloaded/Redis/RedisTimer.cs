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
        public Guid TimerId { get; }
        public int ExtraValue { get; set; } = 0;

        public RedisTimer(TimerType type, DateTimeOffset timerEnd, string gameId, long chatId)
        {
            Type = type;
            TimerEnd = timerEnd;
            GameId = gameId;
            ChatId = chatId;
            TimerId = Guid.NewGuid();
        }
    }

    public enum TimerType
    {
        GameStart,
        GameStartSoon
    }
}
