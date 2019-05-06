﻿using ServiceStack.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
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
            Program.TimerDict.Add(timer.TimerId, new Timer(TimerElapsed, timer, (int)Math.Round((timer.TimerEnd - DateTimeOffset.Now).TotalMilliseconds), Timeout.Infinite));
        }

        public static void RemoveTimer(Guid timerId)
        {
            using (Redis.AcquireLock(RedisLocks.Timers))
            {
                var timers = Redis.Get<List<RedisTimer>>(RedisKeys.Timers);
                timers.RemoveAll(x => x.TimerId == timerId);
                Redis.Set(RedisKeys.Timers, timers);
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
            if (!(state is RedisTimer timer)) return;   // this method wasn't correctly invoked
            if (!Redis.Get<List<RedisTimer>>(RedisKeys.Timers).Any(x => x.TimerId == timer.TimerId)) return;    // the timer was cancelled
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
                Program.Bot.LogException(timer.ChatId, ex);
                Program.Bot.SendLocale(timer.ChatId, "GameError", values: ex.Message);
                GameCommands.KillGame(timer.GameId);
            }
            finally
            {
                RemoveTimer(timer.TimerId);
            }
        }

        [Elapsed(TimerType.GameStart)]
        public static void GameStartTimerElapsed(RedisTimer timer)
        {
            throw new NotImplementedException();
        }

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