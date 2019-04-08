using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WhoAmIBotReloaded.Helpers;

namespace WhoAmIBotReloaded.Commands
{
    public static class DevCommands
    {
#pragma warning disable CS0649  // value will always be null (it won't, because it'll be injected)
        // will be injected
        private static Bot Bot;
#pragma warning restore CS0649

        [Command("update", PermissionLevel = PermissionLevel.GlobalAdminOnly, Types = CommandTypes.MessageAndCallbackQuery)]
        public static void Update(Update u, string[] args)
        {
            var message = (u.Type == UpdateType.CallbackQuery) ? u.CallbackQuery.Message : Bot.Send(u.Message.Chat.Id, "Updating...");
            if (u.Type == UpdateType.CallbackQuery)
            {
                Bot.Api.AnswerCallbackQueryAsync(u.CallbackQuery.Id);
                Bot.Append(ref message, $"\n\n{u.CallbackQuery.From.FirstName} chose to update.");
            }
            Program.UpdateAsync(message).ContinueWith(x => Program.ShutdownHandle.Set());
            // TODO: Wait for all games to end and then set the shutdown handle
        }
    }
}
