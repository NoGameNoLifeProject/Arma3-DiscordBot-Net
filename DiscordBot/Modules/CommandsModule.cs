using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Modules.Commands;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;
using DiscordBot.Common.Enums;
using DiscordBot.Services;
using Utils = DiscordBot.Common.Utils;

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
                string res = await ServerCommands.RestartServer(Context.User as SocketGuildUser);
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
                string res = await ServerCommands.StartServer(Context.User as SocketGuildUser);
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
                string res = await ServerCommands.StopServer(Context.User as SocketGuildUser);
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
                string res = await ServerCommands.SetMS(Context.User as SocketGuildUser, objects.ElementAtOrDefault(0));
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
                string res = await ServerCommands.MsList(Context.User as SocketGuildUser);
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

        [Command("installSteam")]
        [Summary("Установить SteamCMD")]
        [RequireContext(ContextType.Guild)]
        public async Task InstallSteam(params string[] objects)
        {
            try
            {
                Log.Information("{User} использовал комманду installSteam", Context.User);
                var channel = Context.Channel as IMessageChannel;
                await ServerCommands.InstallSteamCMD(Context.User as SocketGuildUser, channel);
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

        [Command("updateServer")]
        [Summary("Обновить сервер")]
        [RequireContext(ContextType.Guild)]
        public async Task UpdateServer(params string[] objects)
        {
            try
            {
                Log.Information("{User} использовал комманду updateServer", Context.User);
                var channel = Context.Channel as IMessageChannel;
                await ServerCommands.UpdateServer(Context.User as SocketGuildUser, channel);
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

        [Command("updateServerMods")]
        [Summary("Обновить моды сервера")]
        [RequireContext(ContextType.Guild)]
        public async Task UpdateServerMods(params string[] objects)
        {
            try
            {
                Log.Information("{User} использовал комманду updateServerMods", Context.User);
                var channel = Context.Channel as IMessageChannel;
                await ServerCommands.UpdateServerMods(Context.User as SocketGuildUser, channel);
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

        [Command("steamLogin")]
        [Summary("Авторизоваться в steamCMD")]
        [RequireContext(ContextType.Guild)]
        public async Task SteamLogin(params string[] objects)
        {
            try
            {
                Log.Information("{User} использовал комманду steamLogin", Context.User);
                var channel = Context.Channel as IMessageChannel;
                await ServerCommands.SteamLogin(Context.User as SocketGuildUser, channel, objects.ElementAtOrDefault(0), objects.ElementAtOrDefault(1), objects.ElementAtOrDefault(2));
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

        [Command("presetUpload")]
        [Summary("Обновить пресет модов сервера")]
        [RequireContext(ContextType.Guild)]
        public async Task PrestUpload(params string[] objects)
        {
            try
            {
                Log.Information("{User} использовал комманду presetUpload", Context.User);
                var attachments = Context.Message.Attachments;
                var channel = Context.Channel as IMessageChannel;
                await ServerCommands.PresetUpdate(Context.User as SocketGuildUser, attachments, channel);
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

        [Command("deleteUnusedMods")]
        [Summary("Удаление не используемых модов")]
        [RequireContext(ContextType.Guild)]
        public async Task DeleteUnusedMods(params string[] objects)
        {
            try
            {
                Log.Information("{User} использовал комманду deleteUnusedMods", Context.User);
                await ServerCommands.DeleteUnusedMods(Context.User as SocketGuildUser, Context.Channel);
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

        [Command("getModsList")]
        [Summary("Список модов сервера")]
        [RequireContext(ContextType.Guild)]
        public async Task GetModsList(params string[] objects)
        {
            try
            {
                Log.Information("{User} использовал комманду getModsList", Context.User);
                string res = await ServerCommands.GetModsList(Context.User as SocketGuildUser);
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

        [Command("addMod")]
        [Summary("Добавить мод в пресет")]
        [RequireContext(ContextType.Guild)]
        public async Task AddMod(params string[] objects)
        {
            try
            {
                Log.Information("{User} использовал комманду addMod", Context.User);
                string res = await ServerCommands.AddMod(Context.User as SocketGuildUser, objects.ElementAtOrDefault(0));
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

        [Command("deleteMod")]
        [Summary("Удалить мод из пресета")]
        [RequireContext(ContextType.Guild)]
        public async Task DeleteMod(params string[] objects)
        {
            try
            {
                Log.Information("{User} использовал комманду deleteMod", Context.User);
                string res = await ServerCommands.AddMod(Context.User as SocketGuildUser, objects.ElementAtOrDefault(0));
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
                    await ReplyAsync(await PlayerCommands.ZeusGive(Context.User as SocketGuildUser, steamID, temp: temp));
                else
                    await ReplyAsync(await PlayerCommands.ZeusRemove(Context.User as SocketGuildUser, steamID));
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
                    await ReplyAsync(await PlayerCommands.InfistarRemove(Context.User as SocketGuildUser, steamID));
                else
                    await ReplyAsync(await PlayerCommands.InfistarGive(Context.User as SocketGuildUser, steamID, rank: rank, temp: temp));
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
                var res = await PlayerCommands.InfistarList(Context.User as SocketGuildUser);
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
                var res = await PlayerCommands.ZeusList(Context.User as SocketGuildUser);
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
                var res = await PlayerCommands.Ban(Context.User as SocketGuildUser, reason, banTime, steamID: steamID);
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
                var res = await PlayerCommands.Kick(Context.User as SocketGuildUser, reason, steamID: steamID);
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
                var res = await PlayerCommands.UnBan(Context.User as SocketGuildUser, steamID: steamID);
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
        
        [Command("setupmusic")]
        [Summary("Создать/Назначить канал для обработки музыкальных команд")]
        [RequireContext(ContextType.Guild)]
        public async Task SetupMusic(IGuildChannel channel = null)
        {
            try
            {
                Log.Information("{User} использовал комманду SetupMusic", Context.User);
                await Modules.Commands.Utils.CheckPermissions(Context.User as SocketGuildUser, PermissionsEnumCommands.Zeus);
                if (channel is null)
                {
                    var channelId = MusicService.Config.MusicChannelId;
                    if (channelId is null || channelId != 0)
                    {
                        channel = Context.Guild.TextChannels.FirstOrDefault(ch => ch.Id == channelId);
                        if (channel is not null)
                        {
                            await ReplyAsync($"Канал уже настроен");
                            return;
                        }
                        
                        channel = await Context.Guild.CreateTextChannelAsync($"music {Context.Client.CurrentUser.Username}");
                        await channel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(sendMessages: PermValue.Deny));
                    }
                }
                Utils.AddOrUpdateAppSetting("MusicConfig", "MusicChannelId", channel?.Id);
                MusicService.Config.MusicChannelId = channel?.Id;
                
                var embed = new EmbedBuilder()
                    .WithTitle($"Сейчас музыка не играет")
                    .AddField("Поддерживаемые сервисы:", "Youtube, Soundcloud, Bandcamp, Twitch, Vimeo, Audio files links")
                    .WithColor(new Color(47, 49, 54));

                var mchannel = channel as IMessageChannel;
                var infoMessage = await mchannel.SendMessageAsync(embed: embed.Build());

                Utils.AddOrUpdateAppSetting("MusicConfig", "MusicInfoMessageId", infoMessage?.Id);
                MusicService.Config.MusicInfoMessageId = infoMessage?.Id;
                
                await ReplyAsync($"Канал настроен <#{channel?.Id}>");
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