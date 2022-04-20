using System;

namespace DiscordBot.Common.Entities;

public class Players_Online
{
    public long SteamID { get; set; }
    
    public DateTime Date { get; set; }
    
    public int Time { get; set; }
}