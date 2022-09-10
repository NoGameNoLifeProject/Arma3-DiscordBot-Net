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
using DiscordBot.Common.Enums;
using Steamworks;

namespace DiscordBot.Modules.Commands
{
    public static class ServerCommands
    {
        public static async Task StartServer(SocketGuildUser user, IMessageChannel channel)
        {
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Manage);
            try
            {
                await Arma3Server.StartServer();
                await channel.SendMessageAsync("Сервер успешно запущен");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при запуске сервера");
                await channel.SendMessageAsync("Ошибка при запуске сервера");
            }
        }

        public static async Task<string> StopServer(SocketGuildUser user)
        {
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Manage);
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

        public static async Task RestartServer(SocketGuildUser user, IMessageChannel channel)
        {
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Restart);
            IUserMessage message = null;
            string messageContent = "";
            try
            {
                await Arma3Server.RestartServer(async status =>
                {
                    if (message is null || message.Content.Length >= 1800)
                    {
                        messageContent = status;
                        message = await channel.SendMessageAsync(messageContent);
                    }
                    else
                    {
                        messageContent = $"{messageContent}\n{status}";
                        await message.ModifyAsync(m => m.Content = messageContent);
                    }
                });
            }
            catch (Exception ex)
            {
                await channel.SendMessageAsync("Ошибка при перезагрузке сервера");
                Log.Error(ex, "Ошибка при перезагрузке сервера");
            }
        }

        public static async Task<string> SetMS(SocketGuildUser user, string mission)
        {
            Guard.Argument(mission, nameof(mission)).NotNull().NotEmpty();
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Manage);
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

        public static async Task<string> MsList(SocketGuildUser user)
        {
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Manage);
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

        public static async Task<string> MsUpload(SocketGuildUser user, IReadOnlyCollection<Attachment> attachments)
        {
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Manage);
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
                return "Миссия успешно загружена и будет установлена при следующем запуске";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при загрузке миссии");
                return "Ошибка при загрузке миссии";
            }
        }
        
        public static async Task UpdateServerMods(SocketGuildUser user, IMessageChannel channel)
        {
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Restart);
            IUserMessage message = null;
            string messageContent = "";
            try
            {
                await Arma3Server.CheckForUpdates(async status =>
                {
                    if (message is null || message.Content.Length >= 1800)
                    {
                        messageContent = status;
                        message = await channel.SendMessageAsync(messageContent);
                    }
                    else
                    {
                        messageContent = $"{messageContent}\n{status}";
                        await message.ModifyAsync(m => m.Content = messageContent);
                    }
                });
            }
            catch (Exception ex)
            {
                await channel.SendMessageAsync("Ошибка при обновлении модов сервера");
                Log.Error(ex, "Ошибка при обновлении модов сервера");
            }
        }
    }
}
