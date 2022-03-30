using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordBot.Common.Entities;

namespace DiscordBot.Configs
{
    class Config
    {
        /// <summary>
        ///     Discord api bot token
        /// </summary>
        public string BotToken { get; set; }
        public string CommandsPrefix { get; set; }
        public string BotStatusGame { get; set; }
        public string BotStatusServerDisabled { get; set; }
        public int BotStatusUpdateInterval { get; set; }
        public int GoogleSheetsUpdateInterval { get; set; }
        
        /// <summary>
        ///  Discord access by individual rights for each role
        /// </summary>
        public Dictionary<string, UserPermissions> RoleAccess { get; set; }

        /// <summary>
        ///  Do the admin rights give full access to the bot commands
        /// </summary>
        public bool DiscordAdminRoleAccess { get; set; }
        /// <summary>
        ///     Arma 3 server ip
        /// </summary>
        public string ServerAdress { get; set; }
        /// <summary>
        ///     Arma 3 server game port
        /// </summary>
        public long ServerGamePort { get; set; }
        /// <summary>
        ///     Arma 3 server query port
        /// </summary>
        public long ServerQueryPort { get; set; }

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
