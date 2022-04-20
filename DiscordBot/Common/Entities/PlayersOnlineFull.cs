using System;

namespace DiscordBot.Common.Entities;

public class PlayersOnlineFull
{
    public long SteamID { get; set; }

    public string SteamName { get; set; }

    public DateTime FirstJoin { get; set; }

    public DateTime LastJoin { get; set; }

    public int Zeus { get; set; }

    public int Infistar { get; set; }
        
    public long Discord { get; set; }
    
    public DateTime Date { get; set; }
    
    public int Time { get; set; }
}