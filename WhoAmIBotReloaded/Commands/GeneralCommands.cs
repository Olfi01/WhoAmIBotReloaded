using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WhoAmIBotReloaded.Helpers;
using TgUser = Telegram.Bot.Types.User;

namespace WhoAmIBotReloaded.Commands
{
    public class GeneralCommands : Commands
    {
        internal static new PermissionLevel DefaultPermissionLevel = PermissionLevel.All;
        internal static new CommandTypes DefaultCommandTypes = CommandTypes.Message;

        [Command("ping")]
        public static void Ping(Update u, string[] args)
        {
            Bot.SendLocale(u.Message.Chat.Id, "Ping");
        }

        [Command("start")]
        public static void Start(Update u, string[] args)
        {
            if (u.Message.Chat.Type != ChatType.Private || args.Length > 0) return;
            TgUser user = u.Message.From;
            var dbUser = DB.Users.Find(user.Id);
            if (dbUser == null) dbUser = DB.Users.Add(user.CreateDbUser(Bot.GetLanguage(user.LanguageCode)));
            dbUser.FirstName = user.FirstName;
            dbUser.LastName = user.LastName;
            dbUser.LanguageCode = user.LanguageCode;
            dbUser.Username = user.Username;
            DB.SaveChanges();
            Bot.SendLocale(user.Id, "Start");
        }

        [Command("version")]
        public static void Version(Update u, string[] args)
        {
            Bot.SendLocale(u.Message.Chat, "Version", values: Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }
    }
}
