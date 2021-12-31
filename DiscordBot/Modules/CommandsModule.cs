using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Attributes;
using DiscordBot.Common;
using DiscordBot.Configs;
using DiscordBot.Modules.Commands;
using Serilog;
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
        public async Task RestartServer(params string[] objects)
        {
            try
            {
                Log.Information("{User} использовал комманду restart", Context.User);
                string res = ServerCommands.RestartServer(Context.User as SocketGuildUser);
                await ReplyAsync(res);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Information("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
        }

        [Command("start")]
        [Summary("Запустить сервер")]
        [RequireContext(ContextType.Guild)]
        public async Task StartServer(params string[] objects)
        {
            try
            {
                Log.Information("{User} использовал комманду start", Context.User);
                string res = ServerCommands.StartServer(Context.User as SocketGuildUser);
                await ReplyAsync(res);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Information("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
        }

        [Command("stop")]
        [Summary("Остановить сервер")]
        [RequireContext(ContextType.Guild)]
        public async Task StopServer(params string[] objects)
        {
            try
            {
                Log.Information("{User} использовал комманду stop", Context.User);
                string res = ServerCommands.StopServer(Context.User as SocketGuildUser);
                await ReplyAsync(res);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Information("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
        }

        [Command("msupload")]
        [Summary("Загрузить новую миссию на сервер. Миссия будет установлена во время ближайшего рестарта")]
        [RequireContext(ContextType.Guild)]
        public async Task MsUpload(params string[] objects)
        {
            try
            {
                Log.Information("{User} использовал комманду msupload", Context.User);
                var attachments = Context.Message.Attachments;
                string res = await ServerCommands.MsUpload(Context.User as SocketGuildUser, attachments, objects.ElementAtOrDefault(0) == "restart" ? true : false);
                await ReplyAsync(res);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Information("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
        }

        [Command("setms")]
        [Summary("Изменить текущую миссию (Сервер должен быть остановлен)")]
        [RequireContext(ContextType.Guild)]
        public async Task SetMS(params string[] objects)
        {
            try
            {
                Log.Information("{User} использовал комманду setms", Context.User);
                string res = ServerCommands.SetMS(Context.User as SocketGuildUser, objects.ElementAtOrDefault(0));
                await ReplyAsync(res);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Information("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
        }

        [Command("mslist")]
        [Summary("Список всех установленных миссий")]
        [RequireContext(ContextType.Guild)]
        public async Task MsList(params string[] objects)
        {
            try
            {
                Log.Information("{User} использовал комманду mslist", Context.User);
                string res = ServerCommands.MsList(Context.User as SocketGuildUser);
                await ReplyAsync(res);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Information("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
        }

        [Command("try")]
        [RequireContext(ContextType.Guild)]
        public async Task Try(params string[] objects)
        {
            try
            {
                Log.Information("{User} использовал комманду try", Context.User);
                string res = CommonCommands.Try(Context.User as SocketGuildUser, string.Join(' ', objects));
                await ReplyAsync(res);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Information("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
        }

        [Command("roll")]
        [RequireContext(ContextType.Guild)]
        public async Task Roll(params string[] objects)
        {
            try
            {
                Log.Information("{User} использовал комманду roll", Context.User);
                string res = CommonCommands.Roll(Context.User as SocketGuildUser, objects.ElementAtOrDefault(0), objects.ElementAtOrDefault(1));
                await ReplyAsync(res);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Information("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
        }

        [Command("updateZeus")]
        [Summary("Выдать или забрать зевс")]
        [RequireContext(ContextType.Guild)]
        public async Task UpdateZeus(params string[] objects)
        {
            try
            {
                Log.Information("{User} использовал комманду updateZeus", Context.User);
                var steamID = objects.ElementAtOrDefault(0);
                var state = objects.ElementAtOrDefault(1);
                var temp = objects.ElementAtOrDefault(2) == "true" ? true : false;
                if (string.IsNullOrEmpty(state) || state == "1")
                    await ReplyAsync(PlayerCommands.ZeusGive(Context.User as SocketGuildUser, steamID, temp));
                else
                    await ReplyAsync(PlayerCommands.ZeusRemove(Context.User as SocketGuildUser, steamID));
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Information("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
        }

        [Command("updateInfi")]
        [Summary("Выдать или забрать infiSTAR")]
        [RequireContext(ContextType.Guild)]
        public async Task UpdateInfiSTAR(params string[] objects)
        {
            try
            {
                Log.Information("{User} использовал комманду updateInfi", Context.User);
                var steamID = objects.ElementAtOrDefault(0);
                var rank = objects.ElementAtOrDefault(1);
                var temp = objects.ElementAtOrDefault(2) == "true" ? true : false;
                if (string.IsNullOrEmpty(rank))
                    await ReplyAsync(PlayerCommands.InfistarRemove(Context.User as SocketGuildUser, steamID));
                else
                    await ReplyAsync(PlayerCommands.InfistarGive(Context.User as SocketGuildUser, steamID, rank, temp));
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Information("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }

        }

        [Command("infiPlayersList")]
        [Summary("Список пользователей с правами infiSTAR")]
        [RequireContext(ContextType.Guild)]
        public async Task InfiPlayersList(params string[] objects)
        {
            try
            {
                Log.Information("{User} использовал комманду infiPlayersList", Context.User);
                var res = PlayerCommands.InfistarList(Context.User as SocketGuildUser);
                await ReplyAsync(embed: res);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Information("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
        }

        [Command("zeusPlayersList")]
        [Summary("Список пользователей с правами Zeus")]
        [RequireContext(ContextType.Guild)]
        public async Task ZeusPlayersList(params string[] objects)
        {
            try
            {
                Log.Information("{User} использовал комманду zeusPlayersList", Context.User);
                var res = PlayerCommands.ZeusList(Context.User as SocketGuildUser);
                await ReplyAsync(embed: res);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Information("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
        }

        [Command("banPlayer")]
        [Summary("Забанить игрока (SteamID, Время или 0 или null, Причина или null)")]
        [RequireContext(ContextType.Guild)]
        public async Task BanPlayer(params string[] objects)
        {
            try
            {
                Log.Information("{User} использовал комманду banPlayer", Context.User);
                var steamID = objects.ElementAtOrDefault(0);
                var banTime = objects.ElementAtOrDefault(1);
                var reason = objects.ElementAtOrDefault(2);
                var res = PlayerCommands.Ban(Context.User as SocketGuildUser, reason, banTime, steamID: steamID);
                await ReplyAsync(embed: res);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Information("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
        }

        [Command("kickPlayer")]
        [Summary("Кикнуть игрока (SteamID, Причина или null)")]
        [RequireContext(ContextType.Guild)]
        public async Task KickPlayer(params string[] objects)
        {
            try
            {
                Log.Information("{User} использовал комманду kickPlayer", Context.User);
                var steamID = objects.ElementAtOrDefault(0);
                var reason = objects.ElementAtOrDefault(1);
                var res = PlayerCommands.Kick(Context.User as SocketGuildUser, reason, steamID: steamID);
                await ReplyAsync(embed: res);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Information("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
        }

        [Command("unBanPlayer")]
        [Summary("Разбанить игрока (SteamID)")]
        [RequireContext(ContextType.Guild)]
        public async Task UnBanPlayer(params string[] objects)
        {
            try
            {
                Log.Information("{User} использовал комманду unBanPlayer", Context.User);
                var steamID = objects.ElementAtOrDefault(0);
                var res = PlayerCommands.UnBan(Context.User as SocketGuildUser, steamID: steamID);
                await ReplyAsync(embed: res);
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Information("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error("{User} {Error}", Context.User, ex.Message);
                await ReplyAsync(ex.Message);
            }
        }
    }
}