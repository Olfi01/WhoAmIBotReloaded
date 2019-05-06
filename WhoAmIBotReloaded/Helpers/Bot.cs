using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using File = System.IO.File;

namespace WhoAmIBotReloaded.Helpers
{
    public class Bot
    {
        public TelegramBotClient Api { get; }
        public string Username { get; }
        public int Id { get; }
        private const ParseMode defaultParseMode = ParseMode.Html;
        public List<Language> Languages = new List<Language>();
        public Language DefaultLanguage { get => Languages.First(x => x.Info.Name == Settings.MasterLanguageFile ); }
        private static WhoAmIDBContainer DB { get => Program.DB; }

        public Bot(string token, bool clearupdates)
        {
            Api = new TelegramBotClient(token);
            if (clearupdates) ClearUpdates();
            var me = Api.GetMeAsync().Result;
            Username = me.Username;
            Id = me.Id;
            LoadLanguages();
        }

        private void LoadLanguages()
        {
            var languageDirectory = Directory.CreateDirectory(Settings.LanguageDirectory);
            foreach (var file in languageDirectory.EnumerateFiles())
            {
                if (file.Extension != "json") continue;
                using (var sr = new StreamReader(file.OpenRead(), Encoding.UTF8))
                {
                    Languages.Add(JsonConvert.DeserializeObject<Language>(sr.ReadToEnd()));
                }
            }
            string masterFileName = $"{Settings.MasterLanguageFile}.json";
            string masterSourceFile = masterFileName;   // should be in working directory
            string masterDestinationFile = Path.Combine(Settings.LanguageDirectory, masterFileName);
            if (!Languages.Any(x => x.Info.Name == Settings.MasterLanguageFile) || File.GetLastWriteTimeUtc(masterSourceFile) > File.GetLastWriteTimeUtc(masterDestinationFile))
            {
                File.Copy(masterSourceFile, masterDestinationFile, true);
                ReloadLangFile(masterDestinationFile);
            }
        }

        public void ReloadLangFile(string filePath)
        {
            Language lang = JsonConvert.DeserializeObject<Language>(File.ReadAllText(filePath, Encoding.UTF8));
            Languages.RemoveAll(x => x.Info.Name == lang.Info.Name);
            Languages.Add(lang);
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
        public Message SendLocale(Chat chat, string localeStringKey, IReplyMarkup replyMarkup = null, params string[] values) => SendLocale(chat.Id, localeStringKey, replyMarkup: replyMarkup, values);
        /// <summary>
        /// Sends a localized string to the chat with the specified id
        /// </summary>
        /// <param name="chatId">The unique identifier of the chat to send the message to</param>
        /// <param name="localeStringKey">The key used to identify the string in localization</param>
        /// <param name="values">The values to replace {#} with</param>
        /// <returns></returns>
        public Message SendLocale(long chatId, string localeStringKey, IReplyMarkup replyMarkup = null, params string[] values)
        {
            var localizedString = GetChatLocaleString(chatId, localeStringKey, values);
            return Send(chatId, localizedString, replyMarkup: replyMarkup);
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
            string language = GetChatLanguage(chatId);
            return GetLocaleString(language, localeStringKey, values);
        }

        private string GetChatLanguage(long chatId)
        {
            return DB.Groups.Find(chatId)?.Language ?? DefaultLanguage.Info.Name;
        }

        public string GetUserLocaleString(int userId, string localeStringKey, params string[] values)
        {
            string language = GetUserLanguage(userId);
            return GetLocaleString(language, localeStringKey, values);
        }

        private string GetUserLanguage(int userId)
        {
            return DB.Users.Find(userId)?.Language ?? DefaultLanguage.Info.Name;
        }

        private string GetLocaleString(string language, string localeStringKey, params string[] values)
        {
            var lang = Languages.FirstOrDefault(x => x.Info.Name == language) ?? DefaultLanguage;
            if (lang.Strings.ContainsKey(localeStringKey)) return string.Format(lang.Strings[localeStringKey], values);
            if (DefaultLanguage.Strings.ContainsKey(localeStringKey)) return string.Format(DefaultLanguage.Strings[localeStringKey], values);
            return $"Missing string: {localeStringKey}";
        }

        internal string GetLanguage(string languageCode)
        {
            if (string.IsNullOrWhiteSpace(languageCode)) return DefaultLanguage.Info.Name;
            var lang = Languages.FirstOrDefault(x => x.Info.LanguageCode.StartsWith(languageCode));
            return (lang ?? DefaultLanguage).Info.Name;
        }

        public Message Send(ChatId chatId, string messageText, ParseMode parseMode = defaultParseMode, IReplyMarkup replyMarkup = null)
        {
            if (chatId.Identifier == 0) return null;
            return Api.SendTextMessageAsync(chatId, messageText, parseMode: parseMode, replyMarkup: replyMarkup).Result;
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

        public void LogException(long chatId, Exception ex)
        {
            if (ex is TargetInvocationException outerex) ex = outerex.InnerException;
            Api.SendTextMessageAsync(Settings.DevChat, ex.ToString(), disableNotification: true).Wait();
            SendLocale(chatId, "ErrorOccurred");
        }
    }
}
