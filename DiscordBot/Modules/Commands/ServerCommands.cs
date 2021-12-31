using Dawn;
using Discord;
using Discord.WebSocket;
using DiscordBot.Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules.Commands
{
    public static class ServerCommands
    {
        public static string StartServer(SocketGuildUser user)
        {
            Utils.CheckPermissions(user, PermissionsEnumCommands.Manage);
            try
            {
                Arma3Server.StartServer();
            } catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при запуске сервера");
                return "Ошибка при запуске сервера";
            }
            return "Сервер успешно запущен";
        }

        public static string StopServer(SocketGuildUser user)
        {
            Utils.CheckPermissions(user, PermissionsEnumCommands.Manage);
            try
            {
                Arma3Server.StopServer();
            } catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при остановке сервера");
                return "Ошибка при остановке сервера";
            }
            return "Сервер успешно остановлен";
        }

        public static string RestartServer(SocketGuildUser user)
        {
            Utils.CheckPermissions(user, PermissionsEnumCommands.Manage);
            try
            {
                Arma3Server.RestartServer();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при перезагрузке сервера");
                return "Ошибка при перезагрузке сервера";
            }
            return "Сервер успешно перезагружен";
        }

        public static string SetMS(SocketGuildUser user, string mission)
        {
            Guard.Argument(mission, nameof(mission)).NotNull().NotEmpty();
            Utils.CheckPermissions(user, PermissionsEnumCommands.Manage);
            try {
                Arma3Server.SetMS(mission);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при установке миссии");
                return "Ошибка при установке миссии";
            }
            return "Миссия успешно изменена";
        }

        public static string MsList(SocketGuildUser user)
        {
            Utils.CheckPermissions(user, PermissionsEnumCommands.Manage);
            try
            {
                var list = Arma3Server.GetAvailableMissions();
                List<string> newlist = new List<string>();
                list.ForEach(m => newlist.Add(Path.GetFileNameWithoutExtension(m)));

                return "Доступные миссии:\n" + String.Join("\n", newlist);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при получении списка доступных миссий");
                return "Ошибка при получении списка доступных миссий";
            }
        }

        public static async Task<string> MsUpload(SocketGuildUser user, IReadOnlyCollection<Attachment> attachments, bool restart = false)
        {
            Utils.CheckPermissions(user, PermissionsEnumCommands.Manage);
            try
            {
                using (var client = new HttpClient())
                {
                    Directory.CreateDirectory("Downloads");
                    Arma3Server.ClearDownloadFolder();
                    var filename = attachments.First().Filename;
                    var response = await client.GetAsync(attachments.First().Url);
                    using (var stream = new FileStream($"Downloads\\{filename}", FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(stream);
                    }
                }
                if (restart)
                {
                    var res = RestartServer(user);
                    return $"Миссия успешно установлена. {res}";
                }
                return "Миссия успешно загружена и будет установлена при следующем запуске";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при загрузке миссии");
                return "Ошибка при загрузке миссии";
            }
        }
    }
}
