using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using DiscordBot.Services;
using DiscordBot.Configs;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using DiscordBot.Common;
using Microsoft.Extensions.Configuration;
using DiscordBot.Modules;
using DiscordBot.Services.Artwork;
using Lavalink4NET;
using Lavalink4NET.Cluster;
using Lavalink4NET.DiscordNet;
using Lavalink4NET.MemoryCache;
using Lavalink4NET.Tracking;
using Serilog;
using Steamworks;

namespace DiscordBot
{
    class Program
    {
        public static Config Configuration {  get; set; }
        public static DiscordSocketClient Client { get; private set; }
        public static IAudioService AudioService { get; private set; }

        private static MusicService _musicService;
        
        #region Trap application termination
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private static EventHandler _handler;
        
        private delegate bool EventHandler(CtrlType sig);
        
        private static bool Handler(CtrlType sig) {
            Log.Information("Exiting system due to external CTRL-C, or process kill, or shutdown: {CtrlType}", sig);

            SteamClient.Shutdown();

            Environment.Exit(-1);

            return true;
        }
        
        private enum CtrlType {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }
        #endregion
        
        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async void SetBotStatus(DiscordSocketClient client)
        {
            ServerStatusService serverStatusService = new ServerStatusService(new HttpClient());
            string players, maxplayers;
            var server = await serverStatusService.GetServerInfo();
            players = server.players;
            maxplayers = server.max_players;
            if (server.addr == "0.0.0.0")
            {
                await client.SetGameAsync(Configuration.BotStatusServerDisabled);
            }
            else
            {
                await client.SetGameAsync(Configuration.BotStatusGame + players + "/" + maxplayers);
            }
        }

        public async Task MainAsync()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            Configuration = builder.GetSection("Config").Get<Config>();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Async(a => a.File("logs/DiscordBot.log", rollingInterval: RollingInterval.Day))
                .WriteTo.Async(a => a.Console())
                .CreateLogger();

            var botConfig = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.GuildMembers | GatewayIntents.GuildMessages | GatewayIntents.GuildMessageTyping | GatewayIntents.Guilds | GatewayIntents.GuildVoiceStates
            };
            
            _handler += Handler;
            SetConsoleCtrlHandler(_handler, true);

            SteamClient.Init(Arma3Server.Config.A3ClientId);
            Arma3Server.Arma3Path = SteamApps.AppInstallDir(Arma3Server.Config.A3ServerId);

            using (var services = ConfigureServices(botConfig))
            {
                Client = services.GetRequiredService<DiscordSocketClient>();
                AudioService = services.GetRequiredService<IAudioService>();

                Client.Log += LogAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;

                await Client.LoginAsync(TokenType.Bot, Configuration.BotToken);
                await Client.StartAsync();

                await services.GetRequiredService<CommandHandlingService>().InitializeAsync();
                services.GetRequiredService<Arma3ServerRestartsService>().Initialize();
                services.GetRequiredService<Arma3PlayersOnlineService>().Initialize();
                services.GetRequiredService<InactivityTrackingService>();
                _musicService = services.GetRequiredService<MusicService>();
                WebSocketClient.Initialize();

                var botStatusTimer = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(Configuration.BotStatusUpdateInterval)).Timestamp();
                botStatusTimer.Subscribe(x => SetBotStatus(Client));

                //No longer used
                //var googleSheetsTimer = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(Configuration.GoogleSheetsUpdateInterval)).Timestamp();
                //googleSheetsTimer.Subscribe(x => GoogleSheetsClass.UpdatePlayersTable());

                Client.Ready += Client_Ready;

                await Task.Delay(Timeout.Infinite);
            }
        }

        public async Task Client_Ready()
        {
            var slashCommands = new SlashCommands(Client);
            await Task.Run(() => slashCommands.RegisterCommands() );
            Client.SlashCommandExecuted += slashCommands.SlashCommandHandler;
            Client.ButtonExecuted += _musicService.ButtonsHandler;
            await AudioService.InitializeAsync();
        }

        private Task LogAsync(LogMessage log)
        {
            Log.Information(log.ToString());

            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices(DiscordSocketConfig botConfig)
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(botConfig))
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<HttpClient>()
                .AddSingleton<ServerStatusService>()
                .AddSingleton<Arma3ServerRestartsService>()
                .AddSingleton<Arma3PlayersOnlineService>()
                .AddSingleton<YoutubeSearchService>()
                .AddSingleton<MusicService>()

                // Lavalink
                .AddSingleton<IAudioService, LavalinkCluster>()
                .AddSingleton<IDiscordClientWrapper, DiscordClientWrapper>()
                .AddSingleton(new LavalinkClusterOptions {
                    Nodes = MusicService.Config.LavalinkNodes.Select(node => new LavalinkNodeOptions()
                    {
                        RestUri = node.RestUri,
                        WebSocketUri = node.WebSocketUri,
                        Password = node.Password
                    }).ToArray(),
                    StayOnline = true
                })
                
                .AddSingleton(new InactivityTrackingOptions{
                    DisconnectDelay = TimeSpan.FromMinutes(MusicService.Config.DisconnectDelay),
                    PollInterval = TimeSpan.FromSeconds(10),
                    TrackInactivity = true
                })
                .AddSingleton<InactivityTrackingService>()
                .AddSingleton<ArtworkService>()
                
                // Request Caching for Lavalink
                .AddSingleton<ILavalinkCache, LavalinkCache>()
                .BuildServiceProvider();
        }
    }
}
