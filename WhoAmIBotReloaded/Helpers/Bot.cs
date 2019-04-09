using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace WhoAmIBotReloaded.Helpers
{
    public class Bot
    {
        public TelegramBotClient Api { get; }
        public string Username { get; }
        public int Id { get; }
        private const ParseMode defaultParseMode = ParseMode.Html;

        public Bot(string token, bool clearupdates)
        {
            Api = new TelegramBotClient(token);
            if (clearupdates) ClearUpdates();
            var me = Api.GetMeAsync().Result;
            Username = me.Username;
            Id = me.Id;
        }

        public void Append(Message message, string textToAppend) => Append(ref message, textToAppend);
        /// <summary>
        /// Appends the specified text to the end of the message
        /// </summary>
        /// <param name="message">The message to edit</param>
        /// <param name="textToAppend">The text to append</param>
        public void Append(ref Message message, string textToAppend)
        {
            Edit(ref message, message.Text + textToAppend);
        }

        /// <summary>
        /// Edits a message sent by the bot and updates the message object
        /// </summary>
        /// <param name="message">The message to edit</param>
        /// <param name="newText">The new text of the message</param>
        public void Edit(ref Message message, string newText)
        {
            if (message.Text == newText) return;
            if (message.From.Id != Id) return;
            message = Api.EditMessageTextAsync(message.Chat.Id, message.MessageId, newText).Result;
        }

        /// <summary>
        /// Replies to a callback query with a localized string
        /// </summary>
        /// <param name="query">The callback query to answer to</param>
        /// <param name="localeStringKey">The key used to identify the string in localization</param>
        /// <param name="values">The values to replace {#} with</param>
        public void ReplyLocaleToQuery(CallbackQuery query, string localeStringKey, bool showAlert = false, params string[] values)
        {
            var localizedString = GetUserLocaleString(query.From.Id, localeStringKey, values);
            Api.AnswerCallbackQueryAsync(query.Id, text: localizedString, showAlert: showAlert).Wait();
        }

        /// <summary>
        /// Sends a localized string to the chat with the specified id
        /// </summary>
        /// <param name="chat">The chat to send the message to</param>
        /// <param name="localeStringKey">The key used to identify the string in localization</param>
        /// <param name="values">The values to replace {#} with</param>
        /// <returns></returns>
        public Message SendLocale(Chat chat, string localeStringKey, params string[] values) => SendLocale(chat.Id, localeStringKey, values);
        /// <summary>
        /// Sends a localized string to the chat with the specified id
        /// </summary>
        /// <param name="chatId">The unique identifier of the chat to send the message to</param>
        /// <param name="localeStringKey">The key used to identify the string in localization</param>
        /// <param name="values">The values to replace {#} with</param>
        /// <returns></returns>
        public Message SendLocale(long chatId, string localeStringKey, params string[] values)
        {
            var localizedString = GetChatLocaleString(chatId, localeStringKey, values);
            return Send(chatId, localizedString);
        }

        /// <summary>
        /// Returns a localized string for the given chat (private or group)
        /// </summary>
        /// <param name="chatId">Id of the chat</param>
        /// <param name="localeStringKey">The key used to identify the string in localization</param>
        /// <param name="values">The values to replace {#} with</param>
        /// <returns>The localized string</returns>
        public string GetChatLocaleString(long chatId, string localeStringKey, params string[] values)
        {
            if (chatId > 0 && chatId <= int.MaxValue) return GetUserLocaleString((int)chatId, localeStringKey, values);
            throw new NotImplementedException();
        }

        public string GetUserLocaleString(int userId, string localeStringKey, params string[] values)
        {
            throw new NotImplementedException();
        }

        public Message Send(ChatId chatId, string messageText, ParseMode parseMode = defaultParseMode)
        {
            return Api.SendTextMessageAsync(chatId, messageText, parseMode: parseMode).Result;
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
