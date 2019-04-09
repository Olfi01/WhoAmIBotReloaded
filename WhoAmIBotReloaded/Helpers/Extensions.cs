using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TgUser = Telegram.Bot.Types.User;

namespace WhoAmIBotReloaded.Helpers
{
    public static class Extensions
    {
        public static User CreateDbUser(this TgUser user, string language)
        {
            return new User
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                LanguageCode = user.LanguageCode,
                Username = user.LanguageCode,
                Language = language
            };
        }

        public static string FileNameNoExt(this FileInfo file)
        {
            return Path.GetFileNameWithoutExtension(file.Name);
        }
    }
}
