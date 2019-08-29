using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace WhoAmIBotReloaded.Redis
{
    public class RedisGame
    {
        public long GroupId { get; set; }
        public string GroupTitle { get; set; }
        public List<RedisPlayer> Players { get; set; }
        public int PlayerTurn { get; set; } = 0;
        [IgnoreDataMember]
        public RedisPlayer TurnPlayer { get => Players[PlayerTurn]; }
        public GameState State { get; set; }
        public string GameId { get; set; }
        public List<string> CurrentTimerIds { get; set; }

        public RedisGame(Chat group, string gameId)
        {
            GroupId = group.Id;
            GroupTitle = group.Title;
            State = GameState.Starting;
            GameId = gameId;
            CurrentTimerIds = new List<string>();
            Players = new List<RedisPlayer>();
        }
    }

    public enum GameState
    {
        Starting,
        Running
    }
}
