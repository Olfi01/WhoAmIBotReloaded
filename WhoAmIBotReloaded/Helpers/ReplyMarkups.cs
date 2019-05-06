using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace WhoAmIBotReloaded.Helpers
{
    public static class ReplyMarkups
    {
        public static InlineKeyboardMarkup GetUpdateMarkup()
        {
            InlineKeyboardButton update = new InlineKeyboardButton { Text = "Update", CallbackData = "update" };
            InlineKeyboardButton dont = new InlineKeyboardButton { Text = "Don't Update", CallbackData = "dontupdate" };
            return new InlineKeyboardMarkup(new InlineKeyboardButton[] { update, dont });
        }

        public static InlineKeyboardMarkup GetJoinMarkup(string gameId, string botusername)
        {
            InlineKeyboardButton join = new InlineKeyboardButton { Text = "Join", Url = $"https://t.me/{botusername}?start={gameId}" };
            return new InlineKeyboardMarkup(join);
        }
    }
}
