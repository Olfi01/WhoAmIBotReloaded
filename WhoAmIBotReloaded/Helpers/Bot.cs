using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace WhoAmIBotReloaded.Helpers
{
    public class Bot
    {
        public TelegramBotClient Api { get; }

        public Bot(string token, bool clearupdates)
        {
            Api = new TelegramBotClient(token);
            if (clearupdates) ClearUpdates();
        }

        public void ClearUpdates()
        {
            Api.GetUpdatesAsync(offset: (Api.GetUpdatesAsync(offset: -1).Result.FirstOrDefault()?.Id ?? 0) + 1).Wait();
        }

        public void Start()
        {
            Api.StartReceiving();
        }

        public void Stop()
        {
            Api.StopReceiving();
        }
    }
}
