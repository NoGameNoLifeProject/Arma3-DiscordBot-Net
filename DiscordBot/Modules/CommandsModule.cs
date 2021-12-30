using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Attributes;
using DiscordBot.Common;
using DiscordBot.Configs;
using DiscordBot.Modules.Commands;
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
            string res = ServerCommands.RestartServer(Context.User as SocketGuildUser);
            await ReplyAsync(res);
        }

        [Command("start")]
        [Summary("Запустить сервер")]
        [RequireContext(ContextType.Guild)]
        [RequireManageAccess(ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task StartServer(params string[] objects)
        {
            string res = ServerCommands.StartServer(Context.User as SocketGuildUser);
            await ReplyAsync(res);
        }

        [Command("stop")]
        [Summary("Остановить сервер")]
        [RequireContext(ContextType.Guild)]
        [RequireManageAccess(ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task StopServer(params string[] objects)
        {
            string res = ServerCommands.StopServer(Context.User as SocketGuildUser);
            await ReplyAsync(res);
        }

        [Command("msupload")]
        [Summary("Загрузить новую миссию на сервер. Миссия будет установлена во время ближайшего рестарта")]
        [RequireContext(ContextType.Guild)]
        [RequireManageAccess(ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task MsUpload(params string[] objects)
        {
            var attachments = Context.Message.Attachments;
            string res = await ServerCommands.MsUpload(Context.User as SocketGuildUser, attachments, objects.ElementAtOrDefault(0) == "restart" ? true : false);
            await ReplyAsync(res);
        }

        [Command("setms")]
        [Summary("Изменить текущую миссию (Сервер должен быть остановлен)")]
        [RequireContext(ContextType.Guild)]
        [RequireManageAccess(ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task SetMS(params string[] objects)
        {
            string res = ServerCommands.SetMS(Context.User as SocketGuildUser, objects.ElementAtOrDefault(0));
            await ReplyAsync(res);
        }

        [Command("mplist")]
        [Summary("Список всех установленных миссий")]
        [RequireContext(ContextType.Guild)]
        [RequireManageAccess(ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task MPList(params string[] objects)
        {
            string res = ServerCommands.MsList(Context.User as SocketGuildUser);
            await ReplyAsync(res);
        }

        [Command("try")]
        [RequireContext(ContextType.Guild)]
        public async Task Try(params string[] objects)
        {
            string res = CommonCommands.Try(Context.User as SocketGuildUser, string.Join(' ', objects));
            await ReplyAsync(res);
        }

        [Command("roll")]
        [RequireContext(ContextType.Guild)]
        public async Task Roll(params string[] objects)
        {
            string res = CommonCommands.Roll(Context.User as SocketGuildUser, objects.ElementAtOrDefault(0), objects.ElementAtOrDefault(1));
            await ReplyAsync(res);
        }

        [Command("updateZeus")]
        [Summary("Выдать или забрать зевс")]
        [RequireContext(ContextType.Guild)]
        [RequireManageAccess(ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task UpdateZeus(params string[] objects)
        {
            var steamID = objects.ElementAtOrDefault(0);
            var state = objects.ElementAtOrDefault(1);
            var temp = objects.ElementAtOrDefault(2) == "true" ? true : false;
            if (string.IsNullOrEmpty(state) || state == "1")
                await ReplyAsync(PlayerCommands.ZeusGive(Context.User as SocketGuildUser, steamID, temp));
            else
                await ReplyAsync(PlayerCommands.ZeusRemove(Context.User as SocketGuildUser, steamID));
        }

        [Command("updateInfi")]
        [Summary("Выдать или забрать infiSTAR")]
        [RequireContext(ContextType.Guild)]
        [RequireManageAccess(ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task UpdateInfiSTAR(params string[] objects)
        {
            var steamID = objects.ElementAtOrDefault(0);
            var rank = objects.ElementAtOrDefault(1);
            var temp = objects.ElementAtOrDefault(2) == "true" ? true : false;
            if (string.IsNullOrEmpty(rank))
                await ReplyAsync(PlayerCommands.InfistarRemove(Context.User as SocketGuildUser, steamID));
            else
                await ReplyAsync(PlayerCommands.InfistarGive(Context.User as SocketGuildUser, steamID, rank, temp));

        }

        [Command("infiPlayersList")]
        [Summary("Список пользователей с правами infiSTAR")]
        [RequireContext(ContextType.Guild)]
        [RequireManageAccess(ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task InfiPlayersList(params string[] objects)
        {
            var res = PlayerCommands.InfistarList(Context.User as SocketGuildUser);
            await ReplyAsync(embed: res);
        }

        [Command("zeusPlayersList")]
        [Summary("Список пользователей с правами Zeus")]
        [RequireContext(ContextType.Guild)]
        [RequireManageAccess(ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task ZeusPlayersList(params string[] objects)
        {
            var res = PlayerCommands.ZeusList(Context.User as SocketGuildUser);
            await ReplyAsync(embed: res);
        }

        [Command("banPlayer")]
        [Summary("Забанить игрока (SteamID, Время или 0 или null, Причина или null)")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.BanMembers, ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task BanPlayer(params string[] objects)
        {
            var steamID = objects.ElementAtOrDefault(0);
            var banTime = objects.ElementAtOrDefault(1);
            var reason = objects.ElementAtOrDefault(2);
            var res = PlayerCommands.Ban(Context.User as SocketGuildUser, reason, banTime, steamID: steamID);
            await ReplyAsync(embed: res);
        }

        [Command("kickPlayer")]
        [Summary("Кикнуть игрока (SteamID, Причина или null)")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.BanMembers, ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task KickPlayer(params string[] objects)
        {
            var steamID = objects.ElementAtOrDefault(0);
            var reason = objects.ElementAtOrDefault(1);
            var res = PlayerCommands.Kick(Context.User as SocketGuildUser, reason, steamID: steamID);
            await ReplyAsync(embed: res);
        }

        [Command("unBanPlayer")]
        [Summary("Разбанить игрока (SteamID)")]
        [RequireContext(ContextType.Guild)]
        [RequireUserPermission(GuildPermission.BanMembers, ErrorMessage = "Недостаточно прав для использования команды")]
        public async Task UnBanPlayer(params string[] objects)
        {
            var steamID = objects.ElementAtOrDefault(0);
            var res = PlayerCommands.UnBan(Context.User as SocketGuildUser, steamID: steamID);
            await ReplyAsync(embed: res);
        }
    }
}