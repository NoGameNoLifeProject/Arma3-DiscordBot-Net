using Dawn;
using Discord;
using Discord.WebSocket;
using DiscordBot.Common;
using DiscordBot.Common.SteamBridge;
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
        public static async Task<string> StartServer(SocketGuildUser user)
        {
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Manage);
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

        public static async Task<string> RestartServer(SocketGuildUser user)
        {
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Restart);
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

        public static async Task<string> MsUpload(SocketGuildUser user, IReadOnlyCollection<Attachment> attachments, bool restart = false)
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

        public static async Task InstallSteamCMD(SocketGuildUser user, IMessageChannel channel)
        {
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Manage);
            SteamInstaller installer = new SteamInstaller(Arma3Server.Config.SteamCmdPath);
            if (!installer.Installed)
            {
                var message = await channel.SendMessageAsync("Начинаем загрузку SteamCMD");
                try
                {
                    await Arma3Server.InstallSteamCMD(message);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Ошибка при установке SteamCMD");
                    await message.ModifyAsync(m => m.Content = "Ошибка при установке SteamCMD");
                }
                await message.ModifyAsync(m => m.Content = "SteamCMD успешно загржуен, начинаем процесс установки...");
            } else
            {
                await channel.SendMessageAsync("SteamCMD уже установлен");
            }
        }

        public static async Task UpdateServer(SocketGuildUser user, IMessageChannel channel)
        {
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Manage);
            SteamInstaller installer = new SteamInstaller(Arma3Server.Config.SteamCmdPath);
            if (installer.Installed)
            {
                var message = await channel.SendMessageAsync("Начинаем обновление сервера");
                try
                {
                    await Arma3Server.UpdateServer(message);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Ошибка при обновлении сервера");
                    await message.ModifyAsync(m => m.Content = "Ошибка при обновлении сервера");
                }
            }
            else
            {
                await channel.SendMessageAsync("Ошибка: SteamCMD не установлен");
            }
        }

        public static async Task PresetUpdate(SocketGuildUser user, IReadOnlyCollection<Attachment> attachments, IMessageChannel channel)
        {
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Manage);
            var message = await channel.SendMessageAsync("Начинаем обработку нового пресета модов");
            try
            {
                using var client = new HttpClient();
                Directory.CreateDirectory("Downloads");
                Arma3Server.ClearDownloadFolder();
                var filename = attachments.First().Filename;
                var response = await client.GetAsync(attachments.First().Url);
                var content = await response.Content.ReadAsStringAsync();
                Arma3Server.UpdatePreset(message, content);

                Log.Information("Пресет модов успешно обновлен");
                await message.ModifyAsync(m => m.Content = "Пресет модов успешно обновлен, необходимо запустить обновление модов");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при обработке пресета модов");
                await message.ModifyAsync(m => m.Content = "Ошибка при обработке пресета модов");
            }
        }

        public static async Task UpdateServerMods(SocketGuildUser user, IMessageChannel channel)
        {
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Manage);
            SteamInstaller installer = new SteamInstaller(Arma3Server.Config.SteamCmdPath);
            if (installer.Installed)
            {
                var message = await channel.SendMessageAsync("Начинаем обновление модов");
                try
                {
                    await Arma3Server.UpdateServerMods(message);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Ошибка при обновлении модов");
                    await message.ModifyAsync(m => m.Content = "Ошибка при обновлении модов");
                }
            }
            else
            {
                await channel.SendMessageAsync("Ошибка: SteamCMD не установлен");
            }
        }

        public static async Task SteamLogin(SocketGuildUser user, IMessageChannel channel, string login, string password, string steamGuard)
        {
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Manage);
            Guard.Argument(login, nameof(login)).NotNull().NotEmpty();
            Guard.Argument(password, nameof(password)).NotNull().NotEmpty();
            SteamInstaller installer = new SteamInstaller(Arma3Server.Config.SteamCmdPath);
            if (installer.Installed)
            {
                var message = await channel.SendMessageAsync("Начинаем попытку авторизации");
                try
                {
                    await Arma3Server.SteamLogin(message, login, password, steamGuard);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Ошибка при попытке авторизации");
                    await message.ModifyAsync(m => m.Content = "Ошибка при попытке авторизации");
                }
            }
            else
            {
                await channel.SendMessageAsync("Ошибка: SteamCMD не установлен");
            }
        }

        public static async Task DeleteUnusedMods(SocketGuildUser user, IMessageChannel channel)
        {
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Manage);
            var message = await channel.SendMessageAsync("Начинаем удаление модов");
            try
            {
                await Arma3Server.DeleteUnusedMods(message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при удалении модов");
                await message.ModifyAsync(m => m.Content = "Ошибка при удалении модов");
            }
        }

        public static async Task<string> GetModsList(SocketGuildUser user)
        {
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Manage);
            try
            {
                var res = Arma3Server.GetModsListWithNames();
                var stringBuilder = new StringBuilder();
                stringBuilder.Append("Список модов:");
                foreach (var item in res)
                {
                    stringBuilder.Append($"{item.Key} | {item.Value}");
                }
                return stringBuilder.ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при получении списка модов");
                return "Ошибка при получении списка модов";
            }
        }

        public static async Task<string> AddMod(SocketGuildUser user, string modId)
        {
            var modIdLong = Utils.ConvertLong(modId);
            Guard.Argument(modIdLong, nameof(modIdLong)).NotZero().NotNegative();
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Manage);
            try
            {
                Arma3Server.AddMods(modIdLong);
                return "Мод успешно добавлен, необходимо запустить обновление модов";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при добавлении мода");
                return "Ошибка при добавлении мода";
            }
        }

        public static async Task<string> DeleteMod(SocketGuildUser user, string modId)
        {
            var modIdLong = Utils.ConvertLong(modId);
            Guard.Argument(modIdLong, nameof(modIdLong)).NotZero().NotNegative();
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Manage);
            try
            {
                Arma3Server.DeleteMods(modIdLong);
                return "Мод успешно удален";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при удалении мода");
                return "Ошибка при удалении мода";
            }
        }
    }
}
