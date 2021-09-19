using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Configs
{
    class Config
    {
        /// <summary>
        ///     Discord api bot token
        /// </summary>
        public string BotToken { get; set; }
        public string BotStatusGame { get; set; }
        public int BotStatusUpdateInterval { get; set; }
        public int GoogleSheetsUpdateInterval { get; set; }
        public string A3serverPath { get; set; }
        public string A3serverExecutable { get; set; }
        public string A3ServerConfigName { get; set; }

        /// <summary>
        ///  Discord role id with full access to the bot commands
        /// </summary>
        public ulong DiscordManageRoleId { get; set; }

        /// <summary>
        ///  Discord role id with access to restart command
        /// </summary>
        public ulong DiscordServerRestartRoleId { get; set; }

        /// <summary>
        ///  Do the admin rights give full access to the bot commands
        /// </summary>
        public bool DiscordAdminRoleAccess { get; set; }
        /// <summary>
        ///     Arma 3 server ip
        /// </summary>
        public string ServerAdress { get; set; }

        /// <summary>
        ///     Steam api key https://steamcommunity.com/dev/apikey
        /// </summary>
        public string SteamAuthToken { get; set; }

        // Google Sheets

        /// <summary>
        ///     Google service account mail
        /// </summary>
        public string GoogleServiceMail { get; set; }

        /// <summary>
        ///     Google talbe id
        /// </summary>
        public string GoogleSheetId { get; set; }
    }
}
