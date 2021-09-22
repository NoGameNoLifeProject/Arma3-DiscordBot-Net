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

        public string A3ModsPath {  get; set; }

        public string A3KeysPath {  get; set; }

        public string SteamCmdPath { get; set; }

        public string SteamContentPath  {  get; set; }

        public string A3ServerConfigName { get; set; }

        public string A3NetworkConfigName { get; set; }

        public string A3serverBranch { get; set; }

        public Dictionary<string, long> Mods { get; set; }

        public Dictionary<string, long> ServerMods { get; set; }

        public string SteamUserLogin { get; set; }

        public string SteamUserPass {  get; set; }
    }
}
