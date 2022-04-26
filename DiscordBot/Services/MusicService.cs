using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Common;
using DiscordBot.Common.Entities;
using DiscordBot.Configs;
using DiscordBot.Services.Artwork;
using Lavalink4NET;
using Lavalink4NET.Events;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Serilog;

namespace DiscordBot.Services;

public class MusicService
{
    private readonly IAudioService _audioService;
    private readonly ArtworkService _artworkService;
    private readonly YoutubeSearchService _youtubeService;
    private readonly HttpClient _http;
    private readonly string _logPrefix = "[Music Player]";
    private static MusicConfig _config { get; set; }

    public static MusicConfig Config
    {
        get => _config ??= BuildConfig();
    }

    public QueuedLavalinkPlayer AudioPlayer { get; set; }

    private SocketCommandContext Context { get; set; }

    private IUserMessage InfoMessage { get; set; }
    
    private IUserMessage SelectionMessage { get; set; }
    
    private DateTimeOffset SelectionMessageDete { get; set; }
    
    private List<YouTubeSearchResult> SearchResults { get; set; }
    
    private bool ShuffleMode { get; set; }

    private bool InstSearch { get; set; } = Config.InstSearchByDefault;

    private CancellationTokenSource UpdateCancellationToken { get; set; }

    public MusicService(IServiceProvider services)
    {
        _audioService = services.GetRequiredService<IAudioService>();
        _artworkService = services.GetRequiredService<ArtworkService>();
        _youtubeService = services.GetRequiredService<YoutubeSearchService>();
        _http = new HttpClient();
        _http.DefaultRequestHeaders.Add($"Authorization", $"Bot {Program.Configuration.BotToken}");
    }

    public async Task HandleUserMessage(SocketCommandContext context)
    {
        Context = context;

        bool result = Uri.TryCreate(Context.Message.Content, UriKind.Absolute, out var uriResult) &&
                      (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        if (result || InstSearch)
        {
            if (Context.Message.Content.Contains("&list="))
            {
                var tracks = await _audioService.GetTracksAsync(Context.Message.Content, SearchMode.YouTube);
                await PlayAsync(tracks.ToList());
            }
            else
            {
                var tracks = await _audioService.GetTrackAsync(Context.Message.Content, SearchMode.YouTube);
                await PlayAsync(tracks);
            }
        }
        else
            await SendSelectionMessage();

        await EndProcessingMessage();
    }

    public async Task<(int, double)> UpdateInfoMessage()
    {
        InfoMessage ??= await Context.Channel.GetMessageAsync(Config.MusicInfoMessageId ?? 0) as IUserMessage;

        var embed = new EmbedBuilder()
            .WithTitle(AudioPlayer.CurrentTrack is null
                ? "Сейчас ничего не играет"
                : $"Сейчас играет: {AudioPlayer.CurrentTrack?.Title}")
            .AddField("Поддерживаемые сервисы:", "Youtube, Soundcloud, Bandcamp, Twitch, Vimeo, Audio files links")
            .WithColor(new Color(47, 49, 54));

        if (AudioPlayer.CurrentTrack is not null)
        {
            var curPos = AudioPlayer.Position.Position;
            var duration = AudioPlayer.CurrentTrack.Duration;
            double ticks1 = curPos.TotalSeconds;
            double ticks2 = duration.TotalSeconds;
            double max = 80;
            var progress = Remap(ticks1, 0, ticks2, 1, max);
            var rest = (int)Math.Ceiling(max - progress);
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < progress-1; i++)
            {
                if (i % 2 == 0)
                    stringBuilder.Append(@"\|");
                else
                    stringBuilder.Append("|");
            }
            stringBuilder.Append('.', rest);
            embed.AddField("Состояние:", $"{curPos:hh\\:mm\\:ss} {stringBuilder} {duration}");
        }

        if (!AudioPlayer.Queue.IsEmpty)
        {
            var queueList = AudioPlayer.Queue.Take(8)
                .Select((track, ind) => $"{ind + 1}. {track.Title} - {track.Duration}").ToList();
            var queueStr = string.Join("\n", queueList);
            if (!string.IsNullOrEmpty(queueStr))
                embed.AddField($"Очередь {queueList.Count} из {AudioPlayer.Queue.Count}:", queueStr);
        }

        if (AudioPlayer.CurrentTrack is not null)
        {
            var artworkUri = await _artworkService.ResolveAsync(AudioPlayer.CurrentTrack);
            if (artworkUri is not null)
                embed.ImageUrl = artworkUri.ToString();
        }

        var buttons = await GenerateButtons();

        var remaining = 3;
        var resetAfter = 5.0;

        if (InfoMessage is null)
        {
            var msChannel = Context.Channel as IMessageChannel;
            InfoMessage =
                await msChannel.SendMessageAsync(embed: embed.Build(), components: buttons);
            Utils.AddOrUpdateAppSetting("MusicConfig", "MusicInfoMessageId", InfoMessage?.Id);
            Config.MusicInfoMessageId = InfoMessage?.Id;
        }
        else
        {
            (remaining, resetAfter) = await UpdateMessage(embed.Build(), buttons);
        }
        
        return (remaining, resetAfter);
    }

    private async Task<MessageComponent> GenerateButtons()
    {
        if (!_audioService.HasPlayer(Context.Guild.Id)) return null;
        if (AudioPlayer is null) return null;
        var buttons = new ComponentBuilder();

        //var emote = await Context.Guild.GetEmoteAsync(613656564888240129);
        buttons.WithButton(AudioPlayer.State is PlayerState.Playing ? "⏸" : "▶", "button-play", ButtonStyle.Secondary);
        buttons.WithButton("⏹", "button-stop", ButtonStyle.Secondary);
        buttons.WithButton("⏩", "button-skip", ButtonStyle.Secondary, disabled: AudioPlayer.Queue.IsEmpty);
        buttons.WithButton("🔀", "button-random-mode", ShuffleMode ? ButtonStyle.Success : ButtonStyle.Secondary);
        buttons.WithButton("🔂", "button-loop-mode", AudioPlayer.IsLooping ? ButtonStyle.Success : ButtonStyle.Secondary);
        buttons.WithButton("Мгновенный поиск", "button-inst-search", InstSearch ? ButtonStyle.Success : ButtonStyle.Secondary, row: 2);

        return buttons.Build();
    }

    private async Task SendSelectionMessage()
    {
        if (SelectionMessage is not null) return;
        SearchResults= await _youtubeService.SearchAsync(Context.Message.Content);
        var embed = new EmbedBuilder()
            .WithTitle("Нашлись следующие композиции:")
            .WithDescription(string.Join("\n", SearchResults.Select((x, i) => $"{i+1}. {x.Title} {x.Duration}")))
            .WithColor(new Color(47, 49, 54));
        
        var buttons = new ComponentBuilder();
        for (int i = 0; i < SearchResults.Count; i++)
        {
            buttons.WithButton($"{i+1}", $"button-selection-message-{i+1}", ButtonStyle.Secondary);
        }
        buttons.WithButton($"Отмена", $"button-selection-message-cancel", ButtonStyle.Danger);
        
        var msChannel = Context.Channel as IMessageChannel;
        SelectionMessage = await msChannel.SendMessageAsync(embed: embed.Build(), components: buttons.Build());
        SelectionMessageDete = DateTimeOffset.Now;
    }

    private async Task ProcessSelection(int selected)
    {
        var youtubeResult = SearchResults.ElementAtOrDefault(selected);
        if (youtubeResult is not null)
        {
            var url = $"https://youtu.be/{youtubeResult.Id}";
            var track = await _audioService.GetTrackAsync(url, SearchMode.YouTube);
            await PlayAsync(track);
        }

        if (SelectionMessage is not null)
        {
            await SelectionMessage.DeleteAsync();
            SelectionMessage = null;
        }
    }

    private async Task OnTrackStartedAsync(object sender, TrackStartedEventArgs args)
    {
        //await UpdateInfoMessasge();
    }

    private async Task OnTrackEndAsync(object sender, TrackEndEventArgs args)
    {
        if (args?.Player is null || AudioPlayer.Queue.IsEmpty)
        {
            //await UpdateInfoMessage();
        }
    }
    
    private async Task OnTrackStuck(object sender, TrackStuckEventArgs args)
    {
        Log.Information("{Prefix} Композиция {Track} непредвиденно остановилась, запускаем следующую", _logPrefix, args.TrackIdentifier);
        await Skip();
    }

    public async Task EndProcessingMessage()
    {
        await Context.Message.DeleteAsync();
    }

    public async Task PlayAsync(List<LavalinkTrack> tracks)
    {
        foreach (var track in tracks)
        {
            await PlayAsync(track);
        }
    }

    public async Task PlayAsync(LavalinkTrack track)
    {
        if (track is null) return;
        
        var startUpdating = false;
        if (!_audioService.HasPlayer(Context.Guild.Id))
        {
            var channel = (Context.User as IGuildUser)?.VoiceChannel;
            if (channel is null)
            {
                await EndProcessingMessage();
                return;
            }

            var messagesForDelete =
                (await Context.Channel.GetMessagesAsync(Context.Message, Direction.Before, 100).FlattenAsync())
                .ToList();
            messagesForDelete.RemoveAll(x => x.Id == Config.MusicInfoMessageId);
            if (messagesForDelete.Count > 0)
                await (Context.Channel as SocketTextChannel)!.DeleteMessagesAsync(messagesForDelete);

            AudioPlayer = await _audioService.JoinAsync<QueuedLavalinkPlayer>(Context.Guild.Id, channel.Id);
            _audioService.TrackStarted += OnTrackStartedAsync;
            _audioService.TrackEnd += OnTrackEndAsync;
            _audioService.TrackStuck += OnTrackStuck;

            startUpdating = true;
            Log.Information("{Prefix} Начинаем воспроизведение в канале {Channel}", _logPrefix, channel.Name);
        }

        var count = AudioPlayer.Queue.Count;
        if (ShuffleMode && count > 0)
        {
            var rand = new Random();
            var pos = rand.Next(1, count);
            AudioPlayer.Queue.Insert(pos, track);
        } else
            await AudioPlayer.PlayAsync(track, enqueue: true);
        
        if (startUpdating)
        {
            UpdateCancellationToken = new();
            StartUpdatingInfoMessage();
        }
        
        Log.Information("{Prefix} Композиция {Title} {Duration} добавлена в очередь, пользователь {User}",
            _logPrefix, track.Title, track.Duration, Context.User);
    }

    public async Task PlayOrPause()
    {
        if (AudioPlayer is null) return;
        switch (AudioPlayer.State)
        {
            case PlayerState.Playing:
                await AudioPlayer.PauseAsync();
                Log.Information("{Prefix} Воспроизведение приостановлено", _logPrefix);
                break;
            case PlayerState.Paused:
                await AudioPlayer.ResumeAsync();
                Log.Information("{Prefix} Воспроизведение возобновлено", _logPrefix);
                break;
        }
    }

    public async Task Stop()
    {
        if (AudioPlayer is null) return;
        await AudioPlayer.StopAsync(disconnect: true);
        Log.Information("{Prefix} Воспроизведение остановлено, очередь очищена", _logPrefix);
    }

    public async Task Skip()
    {
        if (AudioPlayer is null) return;
        Log.Information("{Prefix} Композиция {Track} пропущена", _logPrefix, AudioPlayer.CurrentTrack.Title);
        await AudioPlayer.SkipAsync();
    }

    public void LoopModeTogle()
    {
        if (AudioPlayer is null) return;
        AudioPlayer.IsLooping = !AudioPlayer.IsLooping;
        Log.Information("{Prefix} Изменен режим воспроизведения - повтор композиции {IsLooping}", _logPrefix, AudioPlayer.IsLooping);
    }

    public void ShuffleTogle()
    {
        if (AudioPlayer is null) return;
        ShuffleMode = !ShuffleMode;

        if (ShuffleMode)
        {
            AudioPlayer.Queue.Shuffle();
        }
        Log.Information("{Prefix} Изменен режим воспроизведения - случайное композиция {ShuffleMode}", _logPrefix, ShuffleMode);
    }

    public void InstSearchToggle()
    {
        if (AudioPlayer is null) return;
        InstSearch = !InstSearch;
        Log.Information("{Prefix} Изменен режим воспроизведения - мгновенный поиск {ShuffleMode}", _logPrefix, ShuffleMode);
    }
    
    public async Task ButtonsHandler(SocketMessageComponent component)
    {
        if (AudioPlayer is null)
        {
            await component.DeferAsync();
            return;
        }
        switch(component.Data.CustomId)
        {
            case "button-play":
                await PlayOrPause();
                await component.UpdateAsync(async x => x.Components = await GenerateButtons());
                break;
            case "button-stop":
                await Stop();
                //await UpdateInfoMessasge();
                await component.DeferAsync();
                break;
            case "button-skip":
                await Skip();
                await component.DeferAsync();
                break;
            case "button-random-mode":
                ShuffleTogle();
                await component.UpdateAsync(async x => x.Components = await GenerateButtons());
                break;
            case "button-loop-mode":
                LoopModeTogle();
                await component.UpdateAsync(async x => x.Components = await GenerateButtons());
                break;
            case "button-inst-search":
                InstSearchToggle();
                await component.UpdateAsync(async x => x.Components = await GenerateButtons());
                break;
            case "button-selection-message-1":
                await ProcessSelection(1);
                await component.DeferAsync();
                break;
            case "button-selection-message-2":
                await ProcessSelection(2);
                await component.DeferAsync();
                break;
            case "button-selection-message-3":
                await ProcessSelection(3);
                await component.DeferAsync();
                break;
            case "button-selection-message-4":
                await ProcessSelection(4);
                await component.DeferAsync();
                break;
            case "button-selection-message-5":
                await ProcessSelection(5);
                await component.DeferAsync();
                break;
            case "button-selection-message-cancel":
                await SelectionMessage.DeleteAsync();
                SelectionMessage = null;
                await component.DeferAsync();
                break;
        }
    }
    
    private void StartUpdatingInfoMessage()
    {
        Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    var (remaining, resetAfter) = await UpdateInfoMessage();
                    if (remaining <= 1)
                    {
                        if (UpdateCancellationToken.Token.IsCancellationRequested)
                            break;
                        Log.Information("Discord limit reached {Remaining} {ResetAfter}", remaining, resetAfter);
                        await Task.Delay((int)(resetAfter * 1000), UpdateCancellationToken.Token);
                    }

                    await Task.Delay(Config.InfoMessageUpdateInterval * 1000, UpdateCancellationToken.Token);
                    if (SelectionMessage is not null && SelectionMessageDete < DateTimeOffset.Now.AddMinutes(-1))
                    {
                        await SelectionMessage.DeleteAsync();
                        SelectionMessage = null;
                    }

                    if (UpdateCancellationToken.Token.IsCancellationRequested)
                        break;

                    if (_audioService.HasPlayer(Context.Guild.Id)) continue;
                    UpdateCancellationToken.Cancel();
                    Log.Information("{Prefix} Обновления информационного сообщения остановлены", _logPrefix);
                    await Task.Delay((int)(resetAfter * 1000));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "{Prefix}  Ошибка при обновлении информационного сообщения", _logPrefix);
                }
            }
        });

    }

    private async Task<(int, double)> UpdateMessage(Embed embed, MessageComponent component = null)
    {
        var discordAPI = $"https://discord.com/api/v9/channels/{Config.MusicChannelId}/messages/{Config.MusicInfoMessageId}";
        var request = new MessageUpdateRequest("", embed, component);
        var json = JsonConvert.SerializeObject(request);
        using var content = new StringContent(json, System.Text.Encoding.UTF8,
            "application/json");
        var result = await _http.PatchAsync(discordAPI, content);
        var remaining = 0;
        var resetAfter = 5.0;
        if (!int.TryParse(result.Headers.GetValues("X-RateLimit-Remaining").FirstOrDefault("0"), out remaining));
        if (!double.TryParse(result.Headers.GetValues("X-RateLimit-Reset-After").FirstOrDefault("5.0"), out resetAfter));
        return (remaining, resetAfter);
    }
    
    private int Remap(double value, double fromMin, double fromMax, double toMin, double toMax)
    {
        return (int)Math.Ceiling((value - fromMin) / (fromMax - fromMin) * (toMax - toMin + toMin));
    }

    private static MusicConfig BuildConfig()
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true, true)
            .Build();

        return builder.GetSection("MusicConfig").Get<MusicConfig>();
    }
}