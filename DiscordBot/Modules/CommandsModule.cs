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
                await ReplyAsync("Начинаем перезагрузку сервера");
                Task.Run(async () => await ServerCommands.RestartServer(Context.User as SocketGuildUser, Context.Channel));
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
                await ReplyAsync("Запускаем сервер");
                Task.Run(async () => await ServerCommands.StartServer(Context.User as SocketGuildUser, Context.Channel));
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
                string res = await ServerCommands.MsUpload(Context.User as SocketGuildUser, attachments);
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