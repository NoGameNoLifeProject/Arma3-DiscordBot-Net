using System;

namespace DiscordBot.Common.Entities;

public class Players_Bans
{
    public int ID { get; set; }

    public long SteamID { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int Infinity { get; set; }

    public string Reason { get; set; }
}