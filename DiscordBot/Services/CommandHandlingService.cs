using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Configs;
using Serilog;

namespace DiscordBot.Services
{
    public class CommandHandlingService
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;
        private readonly MusicService _music;
        private Config _config;
        private Config Config => _config ??= Program.Configuration;

        public CommandHandlingService(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _music = services.GetRequiredService<MusicService>();
            _services = services;

            _commands.CommandExecuted += CommandExecutedAsync;
            _discord.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            // Ignore system messages, or messages from other bots
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            if (rawMessage.Channel.Id == MusicService.Config.MusicChannelId)
            {
                var msContext = new SocketCommandContext(_discord, message);
                await _music.HandleUserMessage(msContext);
                return;
            }

            var argPos = 0;
            if (!message.HasStringPrefix(Config.CommandsPrefix, ref argPos)) return;

            var context = new SocketCommandContext(_discord, message);

            await _commands.ExecuteAsync(context, argPos, _services);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified)
                return;

            if (result.IsSuccess)
                return;

            Log.Error("Ошибка: {Error}", result.ErrorReason);
            await context.Channel.SendMessageAsync($"Ошибка: {result.ErrorReason}");
        }
    }
}