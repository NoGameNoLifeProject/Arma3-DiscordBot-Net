using System.Collections.Generic;

namespace DiscordBot.Configs
{
    public class Arma3Config
    {
        public string A3ProfilesPath { get; set; }

        public string A3ServerModsPath { get; set; }
        
        public string A3ServerConfigName { get; set; }
        
        public string A3CustomModsPath { get; set; }

        public string A3NetworkConfigName { get; set; }

        public uint A3ServerId { get; set; }

        public uint A3ClientId { get; set; }

        public string A3ServerLaunchParams { get; set; }

        public string A3HCLaunchParams { get; set; }
        
        public bool UseHClient { get; set; }

        public string A3ServerRestarts { get; set; }
        
        public int A3PlayersOnlineUpdateInterval { get; set; }
    }
}
