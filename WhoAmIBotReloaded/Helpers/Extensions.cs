using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhoAmIBotReloaded.Redis;
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

        public static string Name(this TgUser user, bool withLink = true) => withLink ? $"<a href=\"tg://user?id={user.Id}\">{Name(user, false)}</a>" : string.Join(" ", user.FirstName, user.LastName);

        public static string MakeString(this object obj)
        {
            if (obj == null) return "null";
            List<int> test = new List<int>();
            switch (obj)
            {
                case string s:
                    return s;
                case IEnumerable<object> enumerable:
                    return "{" + string.Join("\n", enumerable.Select(x => x.MakeString())) + "}";
                case KeyValuePair<object, object> kvp:
                    return $"{{ {kvp.Key.MakeString()}: {kvp.Value.MakeString()}}}";
                default:
                    return obj.ToString();
            }
        }
    }
}
