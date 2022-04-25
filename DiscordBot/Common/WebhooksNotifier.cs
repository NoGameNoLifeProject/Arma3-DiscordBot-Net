using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using DiscordBot.Common.Enums;
using DiscordBot.Configs;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace DiscordBot.Common;

public static class WebhooksNotifier
{
    private static WebhooksConfig _config { get; set; }
    public static WebhooksConfig Config { get => _config ??= BuildConfig(); }

    private static readonly DiscordWebhookClient MainClient = new (Config.MainWebhook);
    private static readonly DiscordWebhookClient ChatClient = new (Config.ChatWebhook);
    private static readonly DiscordWebhookClient ModsUpdatesClient = new (Config.ModsUpdatesWebhook);
    private static readonly DiscordWebhookClient RestartsClient = new (Config.RestartsWebhook);
    private static readonly DiscordWebhookClient InfistarClient = new (Config.InfistarWebhook);
    private static readonly DiscordWebhookClient ZeusClient = new (Config.ZeusWebhook);

    public static async Task<ulong> Send(WebhooksEnum webhook, string message = "", List<Embed> embeds = null)
    {
        Log.Information("[Webhooks] Send {Message}, embed = {Embeds} to {Webhook}", message, embeds, webhook);
        switch (webhook)
        {
            case WebhooksEnum.Main:
                return await MainClient.SendMessageAsync(message, embeds: embeds);
            case WebhooksEnum.ModsUpdates:
                return await ModsUpdatesClient.SendMessageAsync(message, embeds: embeds);
            case WebhooksEnum.Restarts:
                return await RestartsClient.SendMessageAsync(message, embeds: embeds);
            case WebhooksEnum.Infistar:
                return await InfistarClient.SendMessageAsync(message, embeds: embeds);
            case WebhooksEnum.Zeus:
                return await ZeusClient.SendMessageAsync(message, embeds: embeds);
            case WebhooksEnum.Chat:
                return await ChatClient.SendMessageAsync(message, embeds: embeds);
            default:
                throw new ArgumentOutOfRangeException(nameof(webhook), webhook, null);
        }
    }

    public static async Task Delete(WebhooksEnum webhook, ulong messageId)
    {
        switch (webhook)
        {
            case WebhooksEnum.Main:
                await MainClient.DeleteMessageAsync(messageId);
                break;
            case WebhooksEnum.ModsUpdates:
                await ModsUpdatesClient.DeleteMessageAsync(messageId);
                break;
            case WebhooksEnum.Restarts:
                await RestartsClient.DeleteMessageAsync(messageId);
                break;
            case WebhooksEnum.Infistar:
                await InfistarClient.DeleteMessageAsync(messageId);
                break;
            case WebhooksEnum.Zeus:
                await ZeusClient.DeleteMessageAsync(messageId);
                break;
            case WebhooksEnum.Chat:
                await ChatClient.DeleteMessageAsync(messageId);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(webhook), webhook, null);
        }
    }
    
    public static async Task Modify(WebhooksEnum webhook, ulong messageId, Action<WebhookMessageProperties> properties)
    {
        switch (webhook)
        {
            case WebhooksEnum.Main:
                await MainClient.ModifyMessageAsync(messageId, properties);
                break;
            case WebhooksEnum.ModsUpdates:
                await ModsUpdatesClient.ModifyMessageAsync(messageId, properties);
                break;
            case WebhooksEnum.Restarts:
                await RestartsClient.ModifyMessageAsync(messageId, properties);
                break;
            case WebhooksEnum.Infistar:
                await InfistarClient.ModifyMessageAsync(messageId, properties);
                break;
            case WebhooksEnum.Zeus:
                await ZeusClient.ModifyMessageAsync(messageId, properties);
                break;
            case WebhooksEnum.Chat:
                await ChatClient.ModifyMessageAsync(messageId, properties);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(webhook), webhook, null);
        }
    }

    
    private static WebhooksConfig BuildConfig()
    {
        var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", true, true)
            .Build();

        return builder.GetSection("WebhooksConfig").Get<WebhooksConfig>();
    }
}