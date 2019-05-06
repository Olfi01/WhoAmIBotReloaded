using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WhoAmIBotReloaded.Helpers;

namespace WhoAmIBotReloaded.Commands
{
#pragma warning disable IDE0060
    public class DevCommands : Commands
    {
        internal static new PermissionLevel DefaultPermissionLevel = PermissionLevel.DevOnly;

        [Command("update", Types = CommandTypes.MessageAndCallbackQuery)]
        public static void Update(Update u, string[] args)
        {
            var message = (u.Type == UpdateType.CallbackQuery) ? u.CallbackQuery.Message : Bot.Send(u.Message.Chat.Id, "Updating...");
            if (u.Type == UpdateType.CallbackQuery)
            {
                Bot.Api.AnswerCallbackQueryAsync(u.CallbackQuery.Id);
                Bot.Append(ref message, $"\n\n{u.CallbackQuery.From.FirstName} chose to update.");
            }
            Program.UpdateAsync(message).ContinueWith(x => { if (x.Result) Program.ShutdownHandle.Set(); else Bot.Append(ref message, "\nUpdate failed!"); });
            // TODO: Wait for all games to end and then set the shutdown handle
        }

        [Command("dontupdate", Types = CommandTypes.CallbackQuery)]
        public static void DontUpdate(Update u, string[] args)
        {
            Bot.Append(u.CallbackQuery.Message, $"\nNo work for me, then! :)");
            Bot.Api.AnswerCallbackQueryAsync(u.CallbackQuery.Id);
        }

        [Command("sql", Types = CommandTypes.Message)]
        public static void SQL(Update u, string[] args)
        {
            if (args.Length < 1) Bot.Send(u.Message.Chat.Id, "No query found.");
            var fullCommand = string.Join(" ", args);
            if (args[0].ToLower() == "select")
            {
                try
                {
                    string response = "";
                    using (IDbCommand command = DB.Database.Connection.CreateCommand())
                    {
                        command.Connection.Open();
                        command.CommandText = fullCommand;
                        command.CommandTimeout = DB.Database.Connection.ConnectionTimeout;
                        using (IDataReader reader = command.ExecuteReader())
                        {
                            var columns = new List<string>();
                            foreach (DataRow schemaRow in reader.GetSchemaTable().Rows) columns.Add($"{schemaRow["ColumnName"]} ({((Type)schemaRow["DataType"]).Name})");
                            response += string.Join(" | ", columns);
                            while (reader.Read())
                            {
                                response += "\n";
                                for (int i = 0; i < reader.FieldCount - 1; i++)
                                {
                                    response += reader.GetValue(i) + " | ";
                                }
                                response += reader.GetValue(reader.FieldCount - 1);
                            }
                        }
                        command.Connection.Close();
                    }
                    Bot.Send(u.Message.Chat.Id, response, ParseMode.Default);
                }
                catch (Exception ex)
                {
                    Bot.Send(u.Message.Chat.Id, $"Failed to execute query: <b>{ex.Message}</b>");
                }
            }
            else
            {
                try
                {
                    int affected = DB.Database.ExecuteSqlCommand(fullCommand);
                    Bot.Send(u.Message.Chat.Id, $"{affected} rows affected.");
                }
                catch (Exception ex)
                {
                    Bot.Send(u.Message.Chat.Id, $"Failed to execute query: <b>{ex.Message}</b>");
                }
            }
        }

        [Command("redis", Types = CommandTypes.Message)]
        public static void RedisCommand(Update u, string[] args)
        {
            if (args.Length < 1) return;
            var key = string.Join(" ", args);
            if (!Redis.ContainsKey(key))
            {
                Bot.Send(u.Message.Chat.Id, "Not present!");
                return;
            }
            var obj = Redis.Get<object>(key);
            Bot.Send(u.Message.Chat.Id, obj.MakeString(), parseMode: ParseMode.Default);
        }
    }
#pragma warning restore IDE0060
}
