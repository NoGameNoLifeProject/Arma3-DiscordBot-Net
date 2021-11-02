using Discord.Net;
using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.Modules;
using DiscordBot.Common;
using System.IO;
using Discord.Commands;
using DiscordBot.Configs;
using DiscordBot;

namespace DiscordBotTest.Common
{
    public class SlashCommands
    {
        public DiscordSocketClient Client { get; set; }

        public SlashCommands(DiscordSocketClient client)
        {
            Client = client;
        }

        private async Task<bool> CanUseCommand(SocketSlashCommand command, bool restart = false)
        {
            var user = command.User as SocketGuildUser;
            if (Program.Configuration.DiscordAdminRoleAccess && user.GuildPermissions.Administrator)
                return true;

            foreach (var role in user.Roles)
            {
                if (Program.Configuration.DiscordManageRoleId.Contains(role.Id) || (restart && Program.Configuration.DiscordServerRestartRoleId.Contains(role.Id)))
                {
                    return true;
                }
            }

            switch (Client.TokenType)
            {
                case TokenType.Bot:
                    var application = await Client.GetApplicationInfoAsync().ConfigureAwait(false);
                    if (command.User.Id == application.Owner.Id)
                        return true;
                    break;
            }
            return false;
        }

        public void RegisterCommands()
        {
            StartCommand();
            StopCommand();
            RestartCommand();
            SetMSCommand();
            MPListCommand();
        }

        public async void RegisterGlobalCommand(SlashCommandBuilder commandBuilder)
        {
            try
            {
                await Client.Rest.CreateGlobalCommand(commandBuilder.Build());
            }
            catch (ApplicationCommandException exception)
            {
                var json = JsonConvert.SerializeObject(exception.Error, Formatting.Indented);
                Console.WriteLine(json);
            }
        }

        private void StartCommand()
        {
            var globalCommand = new SlashCommandBuilder()
                .WithName("start")
                .WithDescription("Запуск сервера Arma 3");

            RegisterGlobalCommand(globalCommand);
        }

        private void StopCommand()
        {
            var globalCommand = new SlashCommandBuilder()
                .WithName("stop")
                .WithDescription("Остановка сервера Arma 3");

            RegisterGlobalCommand(globalCommand);
        }

        private void RestartCommand()
        {
            var globalCommand = new SlashCommandBuilder()
                .WithName("restart")
                .WithDescription("Перезапуск сервера Arma 3");

            RegisterGlobalCommand(globalCommand);
        }

        private void SetMSCommand()
        {
            var globalCommand = new SlashCommandBuilder()
                .WithName("setms")
                .WithDescription("Изменить текущую миссию (Сервер должен быть остановлен)")
                .AddOption("name", ApplicationCommandOptionType.String, "Название миссии", required: true);

            RegisterGlobalCommand(globalCommand);
        }

        private void MPListCommand()
        {
            var globalCommand = new SlashCommandBuilder()
                .WithName("mplist")
                .WithDescription("Список всех установленных миссий");

            RegisterGlobalCommand(globalCommand);
        }

        public async Task Client_InteractionCreated(SocketInteraction arg)
        {
            if (arg is SocketSlashCommand command)
            {
                switch (command.Data.Name)
                {
                    case "start":
                        await HandleStartCommand(command);
                        break;
                    case "stop":
                        await HandleStopCommand(command);
                        break;
                    case "restart":
                        await HandleRestartCommand(command);
                        break;
                    case "setms":
                        await HandleSetMSCommand(command);
                        break;
                    case "mplist":
                        await HandleMPListCommand(command);
                        break;
                }
            }
        }

        private async Task HandleStartCommand(SocketSlashCommand command)
        {
            bool canUse = await CanUseCommand(command);
            if (!canUse)
            {
                await command.RespondAsync("Недостаточно прав для использования команды");
                return;
            }

            bool res = Arma3Server.StartServer();
            if (res)
            {
                await command.RespondAsync("Сервер успешно запущен");
            }
            else
            {
                await command.RespondAsync("Ошибка при попытке запуска сервера");
            }
        }

        private async Task HandleStopCommand(SocketSlashCommand command)
        {
            bool canUse = await CanUseCommand(command);
            if (!canUse)
            {
                await command.RespondAsync("Недостаточно прав для использования команды");
                return;
            }

            bool res = Arma3Server.StopServer();
            if (res)
            {
                await command.RespondAsync("Сервер успешно остановлен");
            }
            else
            {
                await command.RespondAsync("Ошибка при попытке остановки сервера");
            }
        }

        private async Task HandleRestartCommand(SocketSlashCommand command)
        {
            bool canUse = await CanUseCommand(command, true);
            if (!canUse)
            {
                await command.RespondAsync("Недостаточно прав для использования команды");
                return;
            }

            string res = Arma3Server.RestartServer();
            await command.RespondAsync(res);
        }

        private async Task HandleSetMSCommand(SocketSlashCommand command)
        {
            bool canUse = await CanUseCommand(command);
            if (!canUse)
            {
                await command.RespondAsync("Недостаточно прав для использования команды");
                return;
            }

            var rec = Arma3Server.SetMS((string)command.Data.Options.First().Value);
            await command.RespondAsync("Миссия " + (rec ? "успешно" : "не") + " изменена");
        }

        private async Task HandleMPListCommand(SocketSlashCommand command)
        {
            bool canUse = await CanUseCommand(command);
            if (!canUse)
            {
                await command.RespondAsync("Недостаточно прав для использования команды");
                return;
            }

            var list = Arma3Server.GetAvailableMissions();
            List<string> newlist = new List<string>();
            list.ForEach(m => newlist.Add(Path.GetFileNameWithoutExtension(m)));

            await command.RespondAsync("Доступные миссии:\n" + String.Join("\n", newlist));
        }
    }
}
