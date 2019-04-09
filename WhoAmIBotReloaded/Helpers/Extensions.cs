using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TgUser = Telegram.Bot.Types.User;

namespace WhoAmIBotReloaded.Helpers
{
    public static class Extensions
    {
        public static User CreateDbUser(this TgUser user)
        {
            return new User
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                LanguageCode = user.LanguageCode,
                Username = user.LanguageCode,
            };
        }
    }
}
