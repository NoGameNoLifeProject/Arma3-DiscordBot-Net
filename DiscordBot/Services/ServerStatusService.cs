using DiscordBot.Configs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class ServerStatusService
    {
        private readonly HttpClient _http;

        public ServerStatusService(HttpClient http)
            => _http = http;

        public async Task<Server> GetServerInfo()
        {
            try
            {
                var resp = await _http.GetAsync(@"https://api.steampowered.com/IGameServersService/GetServerList/v1/?filter=\gameaddr\" + Program.Configuration.ServerAdress + ":" + Program.Configuration.ServerGamePort + "&key=" + Program.Configuration.SteamAuthToken);
                var content = await resp.Content.ReadAsStringAsync();
                var json = JsonConvert.DeserializeObject<Dictionary<string, Response>>(content);
                return json["response"].servers[0];
            } catch (Exception e)
            {
                Console.WriteLine($"Ошибка при выполнении запроса к Steam API {e.Message}");
                return new Server();
            }
        }

        public class Server
        {
            public string addr = "0.0.0.0";
            public string name = "Unknown";
            public string version = "0.0.0";
            public string players = "0";
            public string max_players = "0";
            public string map = "Earth??";
        }
        
        public class Response
        {
            public List<Server> servers = new List<Server>();
        }
    }
}
