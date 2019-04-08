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
    }
}
