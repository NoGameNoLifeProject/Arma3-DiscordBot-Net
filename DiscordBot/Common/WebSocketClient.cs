using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public static void SocketClose()
        {
            if (_socket != null)
            {
                _socket.Close();
            }
        }

        private static WebSocket SocketConnect()
        {
            if (_socket == null || !_socket.IsAlive)
            {
                var ws = new WebSocket($"ws://{Config.Server}:{Config.Port}/BotCommands");

                ws.OnMessage += (sender, e) => Log.Information("WebSocket message {Message}", e.Data);

                ws.OnError += (sender, e) => Log.Error("WebSocket error {Error}", e.Message);

                ws.OnClose += (sender, e) => Log.Information("WebSocket closed {Reason}", e.Reason);

                ws.OnOpen += (sender, e) => ws.Send("Discord Bot Connected");

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
