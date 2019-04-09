using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using WhoAmIBotReloaded.Helpers;

namespace WhoAmIBotReloaded.Commands
{
    public class GeneralCommands : Commands
    {
        [Command("ping")]
        public static void Ping(Update u, string[] args)
        {
            Bot.SendLocale(u.Message.Chat.Id, "Ping");
        }
    }
}
