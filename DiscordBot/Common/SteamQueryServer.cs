using System.Collections.Generic;
using SteamQueryNet.Interfaces;
using SteamQueryNet.Models;
using SteamQueryNet;

namespace DiscordBot.Common
{
    public static class SteamQueryServer
    {
        public static List<Player> GetServerPlayers()
        {
            IServerQuery serverQuery = new ServerQuery(Program.Configuration.ServerAdress + ":" + Program.Configuration.ServerQueryPort);
            List<Player> players = serverQuery.GetPlayers();
            return  players;
        }

        public static ServerInfo GetInfo()
        {
            IServerQuery serverQuery = new ServerQuery(Program.Configuration.ServerAdress + ":" + Program.Configuration.ServerQueryPort);
            ServerInfo server = serverQuery.GetServerInfo();
            return server;
        }
    }
}
