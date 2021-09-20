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
    }
}