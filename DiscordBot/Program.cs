using System;
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
using DiscordBot.GoogleSheets;
using Microsoft.Extensions.Configuration;
using DiscordBot.Modules;
using Serilog;

namespace DiscordBot
{
    class Program
    {
        public static Config Configuration {  get; set; }
        public static DiscordSocketClient Client { get; private set; }
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
                .AddJsonFile("appsettings.json", false, true)
                .Build();

            Configuration = builder.GetSection("Config").Get<Config>();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Async(a => a.File("logs/DiscordBot.log", rollingInterval: RollingInterval.Day))
                .WriteTo.Async(a => a.Console())
                .CreateLogger();

            var botConfig = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.GuildMembers | GatewayIntents.GuildMessages | GatewayIntents.GuildMessageTyping | GatewayIntents.Guilds
            };

            using (var services = ConfigureServices(botConfig))
            {
                Client = services.GetRequiredService<DiscordSocketClient>();

                Client.Log += LogAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;

                await Client.LoginAsync(TokenType.Bot, Configuration.BotToken);
                await Client.StartAsync();

                await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

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
                .BuildServiceProvider();
        }
    }
}
