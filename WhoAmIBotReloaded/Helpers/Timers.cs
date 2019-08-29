using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WhoAmIBotReloaded.Commands;
using WhoAmIBotReloaded.Redis;

namespace WhoAmIBotReloaded.Helpers
{
    public static class Timers
    {
        private static RedisClient Redis { get => Program.Redis; }
        private static Bot Bot { get => Program.Bot; }

        public static void AddTimer(RedisTimer timer)
        {
            using (Redis.AcquireLock(RedisLocks.Timers))
            {
                var timers = Redis.Get<List<RedisTimer>>(RedisKeys.Timers);
                timers.Add(timer);
                Redis.Set(RedisKeys.Timers, timers);
            }
            var game = Redis.Get<RedisGame>(timer.GameId);
            if (game == null) Console.WriteLine("game is null"); else if (game.CurrentTimerIds == null) Console.WriteLine("timerIds is null");
            game.CurrentTimerIds.Add(timer.TimerId);
            Redis.Set(timer.GameId, game);
            Program.TimerDict.Add(timer.TimerId, new Timer(TimerElapsed, timer, (int)Math.Round((timer.TimerEnd - DateTimeOffset.Now).TotalMilliseconds), Timeout.Infinite));
        }

        public static void RemoveTimer(string timerId) => RemoveTimer(Redis.Get<List<RedisTimer>>(RedisKeys.Timers).First(x => x.TimerId == timerId));

        public static void RemoveTimer(RedisTimer timer)
        {
            if (timer == null) return;
            using (Redis.AcquireLock(RedisLocks.Timers))
            {
                var timers = Redis.Get<List<RedisTimer>>(RedisKeys.Timers);
                timers.RemoveAll(x => x.TimerId == timer.TimerId);
                Redis.Set(RedisKeys.Timers, timers);
            }

            using (Redis.AcquireLock(RedisLocks.Games))
            {
                var game = Redis.Get<RedisGame>(timer.GameId);
                game.CurrentTimerIds.RemoveAll(x => x == timer.TimerId);
                Redis.Set(timer.GameId, game);
            }
        }

        public static void ExtendTimer(string gameId, TimeSpan extendSpan)
        {
            var game = Redis.Get<RedisGame>(gameId);
            using (Redis.AcquireLock(RedisLocks.Timers))
            {
                var timers = Redis.Get<List<RedisTimer>>(RedisKeys.Timers);
                foreach (var timerId in game.CurrentTimerIds)
                {
                    var end = (timers.First(x => x.TimerId == timerId).TimerEnd += extendSpan);
                    Program.TimerDict[timerId].Change((int)Math.Round((end - DateTimeOffset.Now).TotalMilliseconds), Timeout.Infinite);
                }
                Redis.Set(RedisKeys.Timers, timers);
            }
        }

        public static void TimerElapsed(object state)
        {
            if (!(state is RedisTimer timer))
            {
                // this method wasn't correctly invoked
                Console.WriteLine("Incorrectly invoked timer with state {0}", state);
                return;
            }
#if DEBUG
            Console.WriteLine("Timer elapsed with type {0} and id {1}", timer.Type, timer.TimerId);
#endif
            List<RedisTimer> timers = Redis.Get<List<RedisTimer>>(RedisKeys.Timers);
            if (!timers.Any(x => x.TimerId == timer.TimerId))
            {
                // the timer was cancelled
#if DEBUG
                Console.WriteLine("Timer not found in List, assuming it was cancelled.");
#endif
                return;
            }
            try
            {
                foreach (var method in typeof(Timers).GetMethods())
                {
                    var attributes = (ElapsedAttribute[])method.GetCustomAttributes(typeof(ElapsedAttribute), false);
                    if (attributes.Any(x => x.Type == timer.Type)) method.Invoke(null, new object[] { timer });
                }
            }
            catch (Exception ex)
            {
                if (ex is TargetInvocationException tie) ex = tie.InnerException;
                Program.Bot.LogException(timer.ChatId, ex);
                Program.Bot.SendLocale(timer.ChatId, "GameError", values: ex.Message);
                GameCommands.KillGame(timer.GameId);
            }
            finally
            {
                RemoveTimer(timer);
            }
        }

#pragma warning disable IDE0060
        [Elapsed(TimerType.GameStart)]
        public static void GameStartTimerElapsed(RedisTimer timer)
        {
            throw new NotImplementedException();
        }
#pragma warning restore IDE0060

        [Elapsed(TimerType.GameStartSoon)]
        public static void GameStartSoonTimerElapsed(RedisTimer timer)
        {
            Bot.SendLocale(timer.ChatId, "SecondsLeft", values: timer.ExtraValue.ToString());
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class ElapsedAttribute : Attribute
    {
        public TimerType Type { get; }
        public ElapsedAttribute(TimerType type)
        {
            Type = type;
        }
    }
}
