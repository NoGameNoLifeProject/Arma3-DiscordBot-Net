using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Common.Entities
{
    public class Players
    {
        public long SteamID { get; set; }

        public string SteamName { get; set; }

        public DateTime FirstJoin { get; set; }

        public DateTime LastJoin { get; set; }

        public int Zeus { get; set; }

        public int Infistar { get; set; }
    }
}
