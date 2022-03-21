using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Configs
{
    public class Arma3Config
    {
        public string A3serverPath { get; set; }

        public string A3ProfilesPath { get; set; }

        public string A3ServerModsPath { get; set; }

        public string A3KeysPath {  get; set; }

        public string SteamCmdPath { get; set; }

        public string SteamContentPath  {  get; set; }

        public string A3ServerConfigName { get; set; }

        public string A3NetworkConfigName { get; set; }

        public string A3ServerId { get; set; }

        public string A3ClientId { get; set; }

        public string A3ServerLaunchParams { get; set; }

        public string A3HCLaunchParams { get; set; }
        
        public bool UseHClient { get; set; }

        public string A3ServerRestarts { get; set; }

        public List<long> Mods { get; set; }

        public string SteamUserLogin { get; set; }
    }
}
