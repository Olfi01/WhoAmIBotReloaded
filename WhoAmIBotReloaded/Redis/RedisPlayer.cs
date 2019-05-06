using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TgUser = Telegram.Bot.Types.User;

namespace WhoAmIBotReloaded.Redis
{
    public class RedisPlayer
    {
        public TgUser User { get; }
        public string Role { get; set; }
        public int ChoseRoleForId { get; set; } = 0;
        public bool GuessedRoleCorrectly { get; set; } = false;
        public int Guesses { get; set; } = 0;
        public int QuestionsAsked { get; set; } = 0;
        public int QuestionsAnswered { get; set; } = 0;
        public int DidNotKnow { get; set; } = 0;

        public RedisPlayer(TgUser user)
        {
            User = user;
        }
    }
}
