using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Attributes;
using DiscordBot.Common;
using DiscordBot.Configs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    public class CommandsModule : ModuleBase<SocketCommandContext>
    {
        [Command("restart")]
        [Summary("Перезапустить сервер")]
        [RequireContext(ContextType.Guild)]
        [RequireRestartAccess(ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task RestartServer(params string[] objects)
        {
            string res = Arma3Server.RestartServer();
            await ReplyAsync(res);
        }

        [Command("start")]
        [Summary("Запустить сервер")]
        [RequireContext(ContextType.Guild)]
        [RequireManageAccess(ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task StartServer(params string[] objects)
        {
            bool res = Arma3Server.StartServer();
            if (res)
            {
                await ReplyAsync("Сервер успешно запущен");
            }
            else
            {
                await ReplyAsync("Ошибка при попытке запуска сервера");
            }
        }

        [Command("stop")]
        [Summary("Остановить сервер")]
        [RequireContext(ContextType.Guild)]
        [RequireManageAccess(ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task StopServer(params string[] objects)
        {
            bool res = Arma3Server.StopServer();
            if (res)
            {
                await ReplyAsync("Сервер успешно остановлен");
            }
            else
            {
                await ReplyAsync("Ошибка при попытке остановки сервера");
            }
        }

        [Command("msupload")]
        [Summary("Загрузить новую миссию на сервер. Миссия будет установлена во время ближайшего рестарта")]
        [RequireContext(ContextType.Guild)]
        [RequireManageAccess(ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task MsUpload(params string[] objects)
        {
            var attachments = Context.Message.Attachments;
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
            if (objects.Length > 0 && objects[0] == "restart")
            {
                await ReplyAsync("Миссия успешно загружена, сервер будет перезапущен");
                await RestartServer();
            }
            await ReplyAsync("Миссия успешно загружена и будет установлена при следующем запуске");
        }

        [Command("setms")]
        [Summary("Изменить текущую миссию (Сервер должен быть остановлен)")]
        [RequireContext(ContextType.Guild)]
        [RequireManageAccess(ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task SetMS(params string[] objects)
        {
            if (String.IsNullOrEmpty(objects[0]))
                await ReplyAsync("Необходимо указать название миссии");

            var rec = Arma3Server.SetMS(objects[0]);
            await ReplyAsync("Миссия " + (rec ? "успешно" : "не") + " изменена");
        }

        [Command("mplist")]
        [Summary("Список всех установленных миссий")]
        [RequireContext(ContextType.Guild)]
        [RequireManageAccess(ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task MPList(params string[] objects)
        {
            var list = Arma3Server.GetAvailableMissions();
            List<string> newlist = new List<string>();
            list.ForEach(m => newlist.Add(Path.GetFileNameWithoutExtension(m)));

            await ReplyAsync("Доступные миссии:\n" + String.Join("\n", newlist));
        }

        [Command("try")]
        [RequireContext(ContextType.Guild)]
        public async Task Try(params string[] objects)
        {
            await ReplyAsync($"{Context.User.Username}: {TryAndRoll.Try(string.Join(' ', objects))}");
        }

        [Command("roll")]
        [RequireContext(ContextType.Guild)]
        public async Task Roll(params string[] objects)
        {
            try
            {
                int min = int.Parse(objects[0]);
                int max = int.Parse(objects[1]);
                await ReplyAsync($"{Context.User.Username} {TryAndRoll.Roll(min, max, true)}");
            } catch (Exception ex) {
                Console.WriteLine("Roll command error: " + ex.Message);
                await ReplyAsync($"{Context.User.Username} {TryAndRoll.Roll()}");
            }
        }

        [Command("updateZeus")]
        [Summary("Выдать или забрать зевс")]
        [RequireContext(ContextType.Guild)]
        [RequireManageAccess(ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task UpdateZeus(params string[] objects)
        {
            if (string.IsNullOrEmpty(objects[0]))
                await ReplyAsync("Необходимо указать SteamID");

            long steamID;
            int state = 1;

            var success = long.TryParse(objects.ElementAtOrDefault(0), out steamID);
            if (!success)
            {
                await ReplyAsync($"Ошибка при обработке преданного SteamID [{objects[0]}]");
            }

            if (!string.IsNullOrEmpty(objects.ElementAtOrDefault(1))) {
                success = int.TryParse(objects[1], out state);
                if (!success)
                {
                    await ReplyAsync($"Ошибка при обработке управляющего параметра [{objects[1]}]");
                }
            }

            MySQLClient.UpdateZeus(steamID, state);
            WebSocketClient.UpdateZeus(steamID.ToString(), state.ToString());

            await ReplyAsync($"Для игрока с SteamID [{steamID}] зевс успешно обновлен [{(state == 1 ? "true" : "false")}]");
        }

        [Command("updateInfi")]
        [Summary("Выдать или забрать infiSTAR")]
        [RequireContext(ContextType.Guild)]
        [RequireManageAccess(ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task UpdateInfiSTAR(params string[] objects)
        {
            if (string.IsNullOrEmpty(objects[0]))
                await ReplyAsync("Необходимо указать SteamID");

            long steamID;
            int state = 1;

            var success = long.TryParse(objects.ElementAtOrDefault(0), out steamID);
            if (!success)
            {
                await ReplyAsync($"Ошибка при обработке преданного SteamID [{objects[0]}]");
            }

            if (!string.IsNullOrEmpty(objects.ElementAtOrDefault(1)))
            {
                success = int.TryParse(objects[1], out state);
                if (!success)
                {
                    await ReplyAsync($"Ошибка при обработке управляющего параметра [{objects[1]}]");
                }
            }

            MySQLClient.UpdateInfiSTAR(steamID, state);
            WebSocketClient.UpdateInfiSTAR(steamID.ToString(), state.ToString());

            await ReplyAsync($"Для игрока с SteamID [{steamID}] infiSTAR успешно обновлен [{state}]");
        }

        [Command("updateZeusTemp")]
        [Summary("Временно выдать или забрать зевс")]
        [RequireContext(ContextType.Guild)]
        [RequireManageAccess(ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task UpdateZeusTemp(params string[] objects)
        {
            if (string.IsNullOrEmpty(objects[0]))
                await ReplyAsync("Необходимо указать SteamID");

            long steamID;
            int state = 1;

            var success = long.TryParse(objects.ElementAtOrDefault(0), out steamID);
            if (!success)
            {
                await ReplyAsync($"Ошибка при обработке преданного SteamID [{objects[0]}]");
            }

            if (!string.IsNullOrEmpty(objects.ElementAtOrDefault(1)))
            {
                success = int.TryParse(objects[1], out state);
                if (!success)
                {
                    await ReplyAsync($"Ошибка при обработке управляющего параметра [{objects[1]}]");
                }
            }

            WebSocketClient.UpdateZeus(steamID.ToString(), state.ToString());

            await ReplyAsync($"Для игрока с SteamID [{steamID}] зевс успешно временно обновлен [{(state == 1 ? "true" : "false")}]");
        }

        [Command("updateInfiTemp")]
        [Summary("Временно выдать или забрать infiSTAR")]
        [RequireContext(ContextType.Guild)]
        [RequireManageAccess(ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task UpdateInfiSTARTemp(params string[] objects)
        {
            if (string.IsNullOrEmpty(objects[0]))
                await ReplyAsync("Необходимо указать SteamID");

            long steamID;
            int state = 1;

            var success = long.TryParse(objects.ElementAtOrDefault(0), out steamID);
            if (!success)
            {
                await ReplyAsync($"Ошибка при обработке преданного SteamID [{objects[0]}]");
            }

            if (!string.IsNullOrEmpty(objects.ElementAtOrDefault(1)))
            {
                success = int.TryParse(objects[1], out state);
                if (!success)
                {
                    await ReplyAsync($"Ошибка при обработке управляющего параметра [{objects[1]}]");
                }
            }

            WebSocketClient.UpdateInfiSTAR(steamID.ToString(), state.ToString());

            await ReplyAsync($"Для игрока с SteamID [{steamID}] infiSTAR успешно временно обновлен [{state}]");
        }

        [Command("infiPlayersList")]
        [Summary("Список пользователей с правами infiSTAR")]
        [RequireContext(ContextType.Guild)]
        [RequireManageAccess(ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task InfiPlayersList(params string[] objects)
        {
            var players = MySQLClient.InfiPlayersList();
            var embed = new EmbedBuilder
            {
                Title = "Список пользователей с infiSTAR",
                Color = Color.Red
            };
            var groups = players.GroupBy(p => p.Infistar);
            foreach (var group in groups)
            {
                embed.AddField($"Rank {group.Key}", string.Join("\n", group.Select(p => { return p.SteamName + " - " + p.SteamID.ToString(); })));
            }

            await ReplyAsync(embed: embed.Build());
        }

        [Command("zeusPlayersList")]
        [Summary("Список пользователей с правами Zeus")]
        [RequireContext(ContextType.Guild)]
        [RequireManageAccess(ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task ZeusPlayersList(params string[] objects)
        {
            var players = MySQLClient.ZeusPlayersList();
            var embed = new EmbedBuilder
            {
                Title = "Список пользователей с Zeus",
                Description = string.Join("\n", players.Select(p => { return p.SteamName + " - " + p.SteamID.ToString(); })),
                Color = Color.Blue
            };

            await ReplyAsync(embed: embed.Build());
        }

        [Command("banPlayer")]
        [Summary("Забанить игрока (SteamID, Время или 0 или null, Причина или null)")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.BanMembers, ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task BanPlayer(params string[] objects)
        {
            if (string.IsNullOrEmpty(objects[0]))
                await ReplyAsync("Необходимо указать SteamID");

            long steamID;
            TimeSpan banTime = TimeSpan.Zero;
            DateTime endTime = DateTime.Now;
            string reason = "";
            int infinity = 0;

            var success = long.TryParse(objects.ElementAtOrDefault(0), out steamID);
            if (!success)
            {
                await ReplyAsync($"Ошибка при обработке преданного SteamID [{objects[0]}]");
            }

            if (!string.IsNullOrEmpty(objects.ElementAtOrDefault(1)))
            {
                int temp;
                success = int.TryParse(objects[1], out temp);
                if (success)
                {
                    if (temp == 0)
                    {
                        infinity = 1;
                    }
                }
                else
                {
                    try
                    {
                        banTime = objects[1].ParseTimeSpan();
                        endTime = endTime + banTime;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            } else
            {
                infinity = 1;
            }

            if (!string.IsNullOrEmpty(objects.ElementAtOrDefault(2)))
            {
                reason = objects[2];
            }

            MySQLClient.BanPlayer(steamID, endTime, reason, infinity);
            WebSocketClient.BanPlayer(steamID.ToString(), endTime, reason);

            var user = Context.User as SocketGuildUser;
            var embed = new EmbedBuilder
            {
                Title = $"Игрок {steamID} забанен",
                Color = Color.Red
            };
            embed.AddField($"Окончание блокировки", infinity == 1 ? "Никогда" : endTime.ToString());
            embed.AddField($"Причина", reason);
            embed.AddField($"Админ", user.Mention);
            embed.Timestamp = DateTime.Now;

            await ReplyAsync(embed: embed.Build());
        }

        [Command("kickPlayer")]
        [Summary("Кикнуть игрока (SteamID, Причина или null)")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.BanMembers, ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task KickPlayer(params string[] objects)
        {
            if (string.IsNullOrEmpty(objects[0]))
                await ReplyAsync("Необходимо указать SteamID");

            long steamID;
            string reason = "";

            var success = long.TryParse(objects.ElementAtOrDefault(0), out steamID);
            if (!success)
            {
                await ReplyAsync($"Ошибка при обработке преданного SteamID [{objects[0]}]");
            }

            if (!string.IsNullOrEmpty(objects.ElementAtOrDefault(1)))
            {
                reason = objects[1];
            }

            WebSocketClient.KickPlayer(steamID.ToString(), reason);

            var user = Context.User as SocketGuildUser;
            var embed = new EmbedBuilder
            {
                Title = $"Игрок {steamID} кикнут",
                Color = Color.DarkBlue
            };
            embed.AddField($"Причина", reason);
            embed.AddField($"Админ", user.Mention);
            embed.Timestamp = DateTime.Now;

            await ReplyAsync(embed: embed.Build());
        }

        [Command("unBanPlayer")]
        [Summary("Разбанить игрока (SteamID)")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.BanMembers, ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task UnBanPlayer(params string[] objects)
        {
            if (string.IsNullOrEmpty(objects[0]))
                await ReplyAsync("Необходимо указать SteamID");

            long steamID;

            var success = long.TryParse(objects.ElementAtOrDefault(0), out steamID);
            if (!success)
            {
                await ReplyAsync($"Ошибка при обработке преданного SteamID [{objects[0]}]");
            }

            MySQLClient.UnBanPlayer(steamID);

            var user = Context.User as SocketGuildUser;
            var embed = new EmbedBuilder
            {
                Title = $"Игрок {steamID} разбанен",
                Color = Color.Green
            };
            embed.AddField($"Админ", user.Mention);
            embed.Timestamp = DateTime.Now;

            await ReplyAsync(embed: embed.Build());
        }
    }
}