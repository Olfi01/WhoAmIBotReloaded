using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WhoAmIBotReloaded.Commands;
using WhoAmIBotReloaded.Helpers;
using TgUser = Telegram.Bot.Types.User;
using System.Runtime.Caching;
using ServiceStack.Redis;

namespace WhoAmIBotReloaded.Handlers
{
    public static class UpdateHandler
    {
        private static readonly Dictionary<CommandAttribute, MethodInfo> commands = new Dictionary<CommandAttribute, MethodInfo>();
        private static bool initialized = false;
        private static Bot Bot { get => Program.Bot; }
        private static WhoAmIDBContainer DB { get => Program.DB; }
        private static RedisClient Redis { get => Program.Redis; }
        private static readonly MemoryCache groupAdminCache = new MemoryCache("WhoAmIAdminCache");
        /// <summary>
        /// Loads all the commands if it hasn't happened yet
        /// </summary>
        private static void Init()
        {
            if (initialized) return;
            Type[] commandClasses = { typeof(DevCommands), typeof(GeneralCommands), typeof(GameCommands) };
            foreach (var cc in commandClasses)
            {
                cc.GetField("Bot", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy).SetValue(null, Bot);
                cc.GetField("DB", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy).SetValue(null, DB);
                cc.GetField("Redis", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy).SetValue(null, Redis);
                var defaultPermissionLevel = (PermissionLevel)cc.GetField("DefaultPermissionLevel", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy).GetValue(null);
                var defaultCommandTypes = (CommandTypes)cc.GetField("DefaultCommandTypes", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy).GetValue(null);
                foreach (var method in cc.GetMethods())
                {
                    CommandAttribute commandAttribute = method.GetCustomAttribute<CommandAttribute>();
                    if (commandAttribute != null)
                    {
                        if (commandAttribute.PermissionLevel == PermissionLevel.Null) commandAttribute.PermissionLevel = defaultPermissionLevel;
                        if (commandAttribute.Types == CommandTypes.Null) commandAttribute.Types = defaultCommandTypes;
                        commands.Add(commandAttribute, method);
                    }
                }
            }
            initialized = true;
        }

        public static void OnUpdate(object sender, UpdateEventArgs e)
        {
            Init();

            try
            {
                if (e.Update.Type == UpdateType.CallbackQuery)
                {
                    var exec = commands.Where(
                        x => x.Key.Types.HasFlag(CommandTypes.CallbackQuery)
                        && x.Key.Trigger == e.Update.CallbackQuery.Data);
                    foreach (var command in exec)
                    {
                        if (CheckPermissions(e.Update.CallbackQuery.From, command.Key.PermissionLevel))
                        {
                            command.Value.Invoke(null, new object[] { e.Update, e.Update.CallbackQuery.Data.Split(' ') });
                        }
                        else
                        {
                            Bot.ReplyLocaleToQuery(e.Update.CallbackQuery, "NoPermission");
                        }
                        return;
                    }
                }
                else if (e.Update.Type == UpdateType.Message && e.Update.Message.Type == MessageType.Text)
                {
                    var maybeCommand = e.Update.Message.Text.Split(' ').First().ToLower();
                    if (maybeCommand.EndsWith($"@{Bot.Username.ToLower()}")) maybeCommand = maybeCommand.Substring(0, maybeCommand.Length - 1 - Bot.Username.Length);
                    var exec = commands.Where(
                        x => x.Key.Types.HasFlag(CommandTypes.Message)
                        && CommandAttribute.CommandPrefixes.Any(pref => pref + x.Key.Trigger.ToLower() == maybeCommand));
                    foreach (var command in exec)
                    {
                        if (CheckPermissions(e.Update.Message.From, command.Key.PermissionLevel, e.Update.Message.Chat))
                        {
                            command.Value.Invoke(null, new object[] { e.Update, e.Update.Message.Text.Split(' ').Skip(1).ToArray() });
                        }
                        else
                        {
                            Bot.SendLocale(e.Update.Message.Chat, "NoPermission");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Bot.LogException(e.Update.Message?.Chat?.Id ?? e.Update.CallbackQuery?.Message?.Chat?.Id ?? 0, ex);
            }
        }

        private static bool CheckPermissions(TgUser user, PermissionLevel permissionLevel, Chat chat = null)
        {
            // First of all, commands without restrictions can be executed by anyone.
            if (permissionLevel == PermissionLevel.All) return true;
            // Devs can do anything
            if (Settings.Devs.Contains(user.Id)) return true;
            // If you are not a dev, dev commands are nothing for you. Devs have already returned.
            if (permissionLevel == PermissionLevel.DevOnly) return false;
            // Global admins can do anything except dev only commands. Those have already been sorted out.
            if (DB.Users.Find(user.Id)?.IsGlobalAdmin ?? false) return true;
            // Again, if you're neither a dev nor a global admin, you can't do global admin commands. If you were, you'd already have returned.
            if (permissionLevel == PermissionLevel.GlobalAdminOnly) return false;
            // If this is not in a group chat, then you are an admin, because it's your private chat.
            if (chat == null) return true;
            // Okay then, let's check whether you are a group admin.
            bool? isAdmin = groupAdminCache[$"{chat.Id}:{user.Id}"] as bool?;
            if (!isAdmin.HasValue)
            {
                isAdmin = IsChatAdmin(user, chat);
                groupAdminCache.Set($"{chat.Id}:{user.Id}", isAdmin, DateTimeOffset.Now.AddMinutes(15));
            }
            return isAdmin.Value;
        }

        private static bool IsChatAdmin(TgUser user, Chat chat)
        {
            var chatmember = Bot.Api.GetChatMemberAsync(chat.Id, user.Id).Result;
            return chatmember.Status == ChatMemberStatus.Administrator || chatmember.Status == ChatMemberStatus.Creator;
        }
    }
}
