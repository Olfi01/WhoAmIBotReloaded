using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WhoAmIBotReloaded.Helpers;
using WhoAmIBotReloaded.Redis;

namespace WhoAmIBotReloaded.Commands
{
#pragma warning disable IDE0060
    public class GameCommands : Commands
    {
        internal static new PermissionLevel DefaultPermissionLevel = PermissionLevel.All;
        internal static new CommandTypes DefaultCommandTypes = CommandTypes.Message;

        public static void KillGame(string gameId)
        {
            var game = Redis.Get<RedisGame>(gameId);
            if (game.CurrentTimerIds.Count > 0)
            {
                foreach (var id in game.CurrentTimerIds) Timers.RemoveTimer(id);
            }
            using (Redis.AcquireLock(RedisLocks.Games))
            {
                var groupGameIdDict = Redis.Get<Dictionary<long, string>>(RedisKeys.GroupGameIdDict);
                groupGameIdDict.Remove(game.GroupId);
                Redis.Set(RedisKeys.GroupGameIdDict, groupGameIdDict);
            }
            Redis.Remove(gameId);
        }

        [Command("startgame")]
        public static void StartGame(Update u, string[] args)
        {
            if (u.Message.Chat.Type != ChatType.Group && u.Message.Chat.Type != ChatType.Supergroup)
            {
                Bot.SendLocale(u.Message.Chat, "UseInGroup");
                return;
            }
            RedisGame game;
            using (Redis.AcquireLock(RedisLocks.Games))
            {
                var groupGameIdDict = Redis.Get<Dictionary<long, string>>(RedisKeys.GroupGameIdDict);
                if (groupGameIdDict == null) groupGameIdDict = new Dictionary<long, string>();
                if (groupGameIdDict.ContainsKey(u.Message.Chat.Id))
                {
                    // game already exists?
                    if (Redis.Get<RedisGame>(groupGameIdDict[u.Message.Chat.Id]) != null)
                    {
                        Bot.SendLocale(u.Message.Chat, "AlreadyRunning");
                        return;
                    }
                }
                var guid = Guid.NewGuid();
                game = new RedisGame(u.Message.Chat, guid.ToString());
                groupGameIdDict.Add(u.Message.Chat.Id, game.GameId);
                Redis.Set(game.GameId, game);
                Redis.Set(RedisKeys.GroupGameIdDict, groupGameIdDict);
            }

            Bot.SendLocale(u.Message.Chat, "GameStart", replyMarkup: ReplyMarkups.GetJoinMarkup(game.GameId, Bot.Username), values: u.Message.From.Name());

            Timers.AddTimer(new RedisTimer(TimerType.GameStart, DateTimeOffset.Now.AddSeconds(120), game.GameId, game.GroupId));
            Timers.AddTimer(new RedisTimer(TimerType.GameStartSoon, DateTimeOffset.Now.AddSeconds(60), game.GameId, game.GroupId) { ExtraValue = 60 });
            Timers.AddTimer(new RedisTimer(TimerType.GameStartSoon, DateTimeOffset.Now.AddSeconds(90), game.GameId, game.GroupId) { ExtraValue = 30 });
        }

        [Command("start")]
        public static void Join(Update u, string[] args)
        {
            if (args.Length < 1 || (u.Message.Chat.Type != ChatType.Group && u.Message.Chat.Type != ChatType.Supergroup)) return;
            RedisGame game;
            using (Redis.AcquireLock(RedisLocks.Games))
            {
                var groupGameIdDict = Redis.Get<Dictionary<long, string>>(RedisKeys.GroupGameIdDict);
                if (groupGameIdDict == null) return;
                if (!groupGameIdDict.ContainsKey(u.Message.Chat.Id) || (game = Redis.Get<RedisGame>(groupGameIdDict[u.Message.Chat.Id])) == null)
                {
                    // no game running in this chat
                    Bot.SendLocale(u.Message.Chat, "NoGameRunning");
                    return;
                }
                game.Players.Add(new RedisPlayer(u.Message.From));
                Redis.Set(groupGameIdDict[u.Message.Chat.Id], game);
            }

            Bot.SendLocale(u.Message.Chat, "JoinedGame", values: game.GroupTitle);
            Bot.SendLocale(game.GroupId, "PlayerJoined", values: u.Message.From.Name());

            Timers.ExtendTimer(game.GameId, TimeSpan.FromSeconds(30));
            var earliestTimer = Redis.Get<List<RedisTimer>>(RedisKeys.Timers).Where(x => x.GameId == game.GameId).OrderBy(x => x.TimerEnd - DateTimeOffset.Now).First();
            Timers.AddTimer(new RedisTimer(TimerType.GameStartSoon, earliestTimer.TimerEnd.AddSeconds(-30), game.GameId, game.GroupId) { ExtraValue = earliestTimer.ExtraValue + 30 });
        }

        [Command("killgame", PermissionLevel = PermissionLevel.AdminOnly)]
        public static void KillGame(Update u, string[] args)
        {
            if (u.Message.Chat.Type != ChatType.Group && u.Message.Chat.Type != ChatType.Supergroup)
            {
                Bot.SendLocale(u.Message.Chat, "UseInGroup");
                return;
            }
            using (Redis.AcquireLock(RedisLocks.Games))
            {
                var groupGameIdDict = Redis.Get<Dictionary<long, string>>(RedisKeys.GroupGameIdDict);
                if (groupGameIdDict == null) groupGameIdDict = new Dictionary<long, string>();
                if (!groupGameIdDict.ContainsKey(u.Message.Chat.Id))
                {
                    Bot.SendLocale(u.Message.Chat, "NoGameRunning");
                    return;
                }
                KillGame(groupGameIdDict[u.Message.Chat.Id]);
                Bot.SendLocale(u.Message.Chat, "GameKilled");
            }
        }
    }
#pragma warning restore IDE0060
}
