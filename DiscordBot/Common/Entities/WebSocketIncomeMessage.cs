using DiscordBot.Common.Enums;

namespace DiscordBot.Common.Entities;

public class WebSocketIncomeMessage
{
    public WebhooksEnum Enum { get; set; } = 0;
    public string Content { get; set; } = "";
}