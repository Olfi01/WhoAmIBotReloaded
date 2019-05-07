using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace WhoAmIBotReloaded.Redis
{
    public class RedisGame
    {
        public long GroupId { get; }
        public string GroupTitle { get; set; }
        public List<RedisPlayer> Players { get; } = new List<RedisPlayer>();
        public int PlayerTurn { get; set; } = 0;
        public GameState State { get; set; }
        public string GameId { get; }
        public List<Guid> CurrentTimerIds { get; } = new List<Guid>();

        public RedisGame(Chat group, string gameId)
        {
            GroupId = group.Id;
            GroupTitle = group.Title;
            State = GameState.Starting;
            GameId = gameId;
        }
    }

    public enum GameState
    {
        Starting,
        Running
    }
}
