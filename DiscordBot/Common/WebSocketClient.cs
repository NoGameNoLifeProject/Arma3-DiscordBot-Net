using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DiscordBot.Common.Entities;
using DiscordBot.Configs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using WebSocketSharp;

namespace DiscordBot.Common
{
    public class WebSocketClient
    {
        private static WebSocketConfig _config { get; set; }
        public static WebSocketConfig Config { get => _config ??= BuildConfig(); }

        private static WebSocket _socket { get; set; }
        public static WebSocket Socket { get => SocketConnect(); }

        public static void Initialize()
        {
            var socketTimer = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(120)).Timestamp();
            socketTimer.Subscribe(x => SocketConnect());
        }

        public static void UpdateZeus(string steamID, string state)
        {
            var dict = new Dictionary<string, string>() { {"Command", "UpdateZeus" }, { "SteamID", steamID }, { "State", state } };
            Task.Run(() => Socket.Send(JsonConvert.SerializeObject(dict)));
        }

        public static void UpdateInfiSTAR(string steamID, string state)
        {
            var dict = new Dictionary<string, string>() { { "Command", "UpdateInfiSTAR" }, { "SteamID", steamID }, { "State", state } };
            Task.Run(() => Socket.Send(JsonConvert.SerializeObject(dict)));
        }

        public static void BanPlayer(string steamID, DateTime endTime, string reason)
        {
            var timestamp = endTime - DateTime.Now;
            var dict = new Dictionary<string, string>() { { "Command", "BanPlayer" }, { "SteamID", steamID }, { "BanTime", timestamp.ToString(@"dd\:hh\:mm\:ss") }, { "Reason", reason } };
            Task.Run(() => Socket.Send(JsonConvert.SerializeObject(dict)));
        }

        public static void KickPlayer(string steamID, string reason)
        {
            var dict = new Dictionary<string, string>() { { "Command", "KickPlayer" }, { "SteamID", steamID }, { "BanTime", "" }, { "Reason", reason } };
            Task.Run(() => Socket.Send(JsonConvert.SerializeObject(dict)));
        }

        private static async Task IncomeMessage(object? sender, MessageEventArgs e)
        {
            Log.Information("WebSocket message {Message}", e.Data);
            try
            {
                var data = JsonConvert.DeserializeObject<WebSocketIncomeMessage>(e.Data);
                if (!string.IsNullOrWhiteSpace(data?.Content)) await WebhooksNotifier.Send(data.Enum, data.Content);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при обработке полученного запроса");
            }
        }

        public static void SocketClose()
        {
            if (_socket != null)
            {
                _socket.Close();
            }
        }

        private static WebSocket SocketConnect()
        {
            if ((_socket == null || !_socket.IsAlive) && Arma3Server.IsServerRunning())
            {
                var ws = new WebSocket($"ws://{Config.Server}:{Config.Port}/BotCommands");
                ws.Log.Output = (data, s) => { }; 

                ws.OnMessage += async (sender, e) => await IncomeMessage(sender, e);

                ws.OnError += (sender, e) => Log.Error("WebSocket error {Error}", e.Message);

                ws.OnClose += (sender, e) => Log.Information("WebSocket closed {Reason} {Code}", e.Reason, e.Code);

                ws.OnOpen += (sender, e) => Log.Information("WebSocket connected");

                ws.SetCredentials(Config.User, Config.Password, true);
                
                ws.Connect();
                _socket = ws;
            }
            return _socket;
        }

        private static WebSocketConfig BuildConfig()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            return builder.GetSection("WebSocket").Get<WebSocketConfig>();
        }
    }
}
