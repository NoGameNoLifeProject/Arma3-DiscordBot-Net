using System.Collections.Generic;

namespace DiscordBot.Configs;

public class MusicConfig
{
    public ulong? MusicChannelId { get; set; }
    
    public ulong? MusicInfoMessageId { get; set; }
    
    public bool TranslateMusicToArma3Players { get; set; }
    
    public string YoutubeAPIKey { get; set; }
    
    
    public bool InstSearchByDefault { get; set; }
    
    public int InfoMessageUpdateInterval { get; set; }
    
    public int DisconnectDelay { get; set; }
    
    public List<LavalinkNodeConfig> LavalinkNodes { get; set; }
}

public class LavalinkNodeConfig
{
    public string RestUri { get; set; }
    public string WebSocketUri { get; set; }
    public string Password { get; set; }
}