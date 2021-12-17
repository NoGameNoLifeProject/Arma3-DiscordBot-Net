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

        private ulong? ApplicationOwnerID { get; set; }

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
            PlayerCommandGroup();
        }

        public async void RegisterGlobalCommand(SlashCommandBuilder commandBuilder)
        {
            try
            {
                await Client.CreateGlobalApplicationCommandAsync(commandBuilder.Build());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
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
                .AddOption("name", ApplicationCommandOptionType.String, "Название миссии", isRequired: true);

            RegisterGlobalCommand(globalCommand);
        }

        private void MPListCommand()
        {
            var globalCommand = new SlashCommandBuilder()
                .WithName("mplist")
                .WithDescription("Список всех установленных миссий");

            RegisterGlobalCommand(globalCommand);
        }

        private void PlayerCommandGroup()
        {
            var globalCommand = new SlashCommandBuilder()
                .WithName("player")
                .WithDescription("Управление игроками на сервере")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("zeus")
                    .WithDescription("Увправление Zeus")
                    .WithType(ApplicationCommandOptionType.SubCommandGroup)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("give")
                        .WithDescription("Выдать Zeus по SteamID")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("steamid", ApplicationCommandOptionType.String, "SteamID игрока", required: true)
                    ).AddOption(new SlashCommandOptionBuilder()
                        .WithName("remove")
                        .WithDescription("Забрать Zeus по SteamID")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("steamid", ApplicationCommandOptionType.String, "SteamID игрока", required: true)
                    ).AddOption(new SlashCommandOptionBuilder()
                        .WithName("list")
                        .WithDescription("Список владельцев Zeus")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                    )
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("infistar")
                    .WithDescription("Увправление intiSTAR")
                    .WithType(ApplicationCommandOptionType.SubCommandGroup)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("give")
                        .WithDescription("Выдать intiSTAR по SteamID")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("steamid", ApplicationCommandOptionType.String, "SteamID игрока", required: true)
                        .AddOption("rank", ApplicationCommandOptionType.String, "Уровень infiSTAR", required: false)
                    ).AddOption(new SlashCommandOptionBuilder()
                        .WithName("remove")
                        .WithDescription("Забрать intiSTAR по SteamID")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("steamid", ApplicationCommandOptionType.String, "SteamID игрока", required: true)
                    ).AddOption(new SlashCommandOptionBuilder()
                        .WithName("list")
                        .WithDescription("Список владельцев infiSTAR")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                    )
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("ban")
                    .WithDescription("Забанить игрока")
                    .WithType(ApplicationCommandOptionType.SubCommandGroup)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("by-steamid")
                        .WithDescription("Забанить игрока по SteamID")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("steamid", ApplicationCommandOptionType.String, "SteamID игрока", required: true)
                        .AddOption("bantime", ApplicationCommandOptionType.String, "Время бана (0 = перманентный)", required: false)
                        .AddOption("reason", ApplicationCommandOptionType.String, "Причина бана", required: false)
                    ).AddOption(new SlashCommandOptionBuilder()
                        .WithName("by-profilename")
                        .WithDescription("Забанить игрока по имени")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("name", ApplicationCommandOptionType.String, "Имя игрока", required: true)
                        .AddOption("bantime", ApplicationCommandOptionType.String, "Время бана (0 = перманентный)", required: false)
                        .AddOption("reason", ApplicationCommandOptionType.String, "Причина бана", required: false)
                    )
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("kick")
                    .WithDescription("Кикнуть игрока")
                    .WithType(ApplicationCommandOptionType.SubCommandGroup)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("by-steamid")
                        .WithDescription("Кикнуть игрока по SteamID")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("steamid", ApplicationCommandOptionType.String, "SteamID игрока", required: true)
                        .AddOption("reason", ApplicationCommandOptionType.String, "Причина кика", required: false)
                    ).AddOption(new SlashCommandOptionBuilder()
                        .WithName("by-profilename")
                        .WithDescription("Кикнуть игрока по имени")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("name", ApplicationCommandOptionType.String, "Имя игрока", required: true)
                        .AddOption("reason", ApplicationCommandOptionType.String, "Причина кика", required: false)
                    )
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("unban")
                    .WithDescription("Разбанить игрока")
                    .WithType(ApplicationCommandOptionType.SubCommandGroup)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("by-steamid")
                        .WithDescription("Разбанить игрока по SteamID")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("steamid", ApplicationCommandOptionType.String, "SteamID игрока", required: true)
                    ).AddOption(new SlashCommandOptionBuilder()
                        .WithName("by-profilename")
                        .WithDescription("Разбанить игрока по имени")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("name", ApplicationCommandOptionType.String, "Имя игрока", required: true)
                    )
                );

            RegisterGlobalCommand(globalCommand);
        }

        public async Task SlashCommandHandler(SocketSlashCommand command)
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
                case "player":
                    await HandlePlayerCommand(command);
                    break;
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

        private async Task HandlePlayerCommand(SocketSlashCommand command)
        {
            try
            {
                await CheckPermissions(command);
                var commandName = command.Data.Options.First().Name;
                var commandSubName = command.Data.Options.First().Options.First().Name;
                var commandSubValues = command.Data.Options.First().Options.First().Options?.ToDictionary(x => x.Name, x => x.Value.ToString());

                long steamID;
                (int, DateTime) bantime;
                string name;
                string reason;
                int rank;
                EmbedBuilder embed = new EmbedBuilder();
                embed.Timestamp = DateTime.Now;
                switch (commandName)
                {
                    case "zeus":
                        switch (commandSubName)
                        {
                            case "give":
                                steamID = ConvertSteamID(commandSubValues.GetValueOrDefault("steamid", ""));
                                MySQLClient.UpdateZeus(steamID, 1);
                                WebSocketClient.UpdateZeus(steamID.ToString(), "1");
                                await command.RespondAsync($"Игроку {steamID} успешно выдан zeus");
                                break;
                            case "remove":
                                steamID = ConvertSteamID(commandSubValues.GetValueOrDefault("steamid", ""));
                                MySQLClient.UpdateZeus(steamID, 0);
                                WebSocketClient.UpdateZeus(steamID.ToString(), "0");
                                await command.RespondAsync($"У игрока {steamID} успешно забран zeus");
                                break;
                            case "list":
                                var players = MySQLClient.ZeusPlayersList();
                                embed.WithTitle($"Список пользователей с Zeus");
                                embed.WithColor(Color.Blue);
                                embed.WithDescription(string.Join("\n", players.Select(p => { return p.SteamName + " - " + p.SteamID.ToString(); })));
                                await command.RespondAsync(embed: embed.Build());
                                break;
                        }
                        break;
                    case "infistar":
                        switch (commandSubName)
                        {
                            case "give":
                                steamID = ConvertSteamID(commandSubValues.GetValueOrDefault("steamid", ""));
                                rank = ConvertRank(commandSubValues.GetValueOrDefault("rank", ""));
                                MySQLClient.UpdateInfiSTAR(steamID, rank);
                                WebSocketClient.UpdateInfiSTAR(steamID.ToString(), rank.ToString());
                                await command.RespondAsync($"Игроку {steamID} успешно выдан infiSTAR, Уровень = {rank}");
                                break;
                            case "remove":
                                steamID = ConvertSteamID(commandSubValues.GetValueOrDefault("steamid", ""));
                                MySQLClient.UpdateInfiSTAR(steamID, 0);
                                WebSocketClient.UpdateInfiSTAR(steamID.ToString(), "0");
                                await command.RespondAsync($"У игрока {steamID} успешно забран infiSTAR");
                                break;
                            case "list":
                                var players = MySQLClient.InfiPlayersList();
                                var groups = players.GroupBy(p => p.Infistar);
                                embed.WithTitle($"Список пользователей с infiSTAR");
                                embed.WithColor(Color.Red);
                                foreach (var group in groups)
                                {
                                    embed.AddField($"Rank {group.Key}", string.Join("\n", group.Select(p => { return p.SteamName + " - " + p.SteamID.ToString(); })));
                                }
                                await command.RespondAsync(embed: embed.Build());
                                break;
                        }
                        break;
                    case "ban":
                        switch (commandSubName)
                        {
                            case "by-steamid":
                                steamID = ConvertSteamID(commandSubValues.GetValueOrDefault("steamid", ""));
                                bantime = ConvertBanTime(commandSubValues.GetValueOrDefault("bantime", ""));
                                reason = commandSubValues.GetValueOrDefault("reason", "Не указана");
                                MySQLClient.BanPlayer(steamID, bantime.Item2, reason, bantime.Item1);
                                WebSocketClient.BanPlayer(steamID.ToString(), bantime.Item2, reason);

                                embed.WithTitle($"Игрок {steamID} забанен");
                                embed.WithColor(Color.Red);
                                embed.AddField($"Окончание блокировки", bantime.Item1 == 1 ? "Никогда" : bantime.Item2.ToString());
                                embed.AddField($"Причина", reason);
                                embed.AddField($"Админ", command.User.Mention);
                                await command.RespondAsync(embed: embed.Build());
                                break;
                            case "by-profilename":
                                name = CheckName(commandSubValues.GetValueOrDefault("name", ""));
                                bantime = ConvertBanTime(commandSubValues.GetValueOrDefault("bantime", ""));
                                reason = commandSubValues.GetValueOrDefault("reason", "Не указана");
                                steamID = MySQLClient.GetSteamIDByProfileName(name);
                                MySQLClient.BanPlayer(steamID, bantime.Item2, reason, bantime.Item1);
                                WebSocketClient.BanPlayer(steamID.ToString(), bantime.Item2, reason);

                                embed.WithTitle($"Игрок {steamID} забанен");
                                embed.WithColor(Color.Red);
                                embed.AddField($"Окончание блокировки", bantime.Item1 == 1 ? "Никогда" : bantime.Item2.ToString());
                                embed.AddField($"Причина", reason);
                                embed.AddField($"Админ", command.User.Mention);
                                await command.RespondAsync(embed: embed.Build());
                                break;
                        }
                        break;
                    case "kick":
                        switch (commandSubName)
                        {
                            case "by-steamid":
                                steamID = ConvertSteamID(commandSubValues.GetValueOrDefault("steamid", ""));
                                reason = commandSubValues.GetValueOrDefault("reason", "Не указана");
                                WebSocketClient.KickPlayer(steamID.ToString(), reason);

                                embed.WithTitle($"Игрок {steamID} кикнут");
                                embed.WithColor(Color.DarkBlue);
                                embed.AddField($"Причина", reason);
                                embed.AddField($"Админ", command.User.Mention);
                                await command.RespondAsync(embed: embed.Build());
                                break;
                            case "by-profilename":
                                name = CheckName(commandSubValues.GetValueOrDefault("name", ""));
                                reason = commandSubValues.GetValueOrDefault("reason", "Не указана");
                                steamID = MySQLClient.GetSteamIDByProfileName(name);
                                WebSocketClient.KickPlayer(steamID.ToString(), reason);

                                embed.WithTitle($"Игрок {steamID} кикнут");
                                embed.WithColor(Color.DarkBlue);
                                embed.AddField($"Причина", reason);
                                embed.AddField($"Админ", command.User.Mention);
                                await command.RespondAsync(embed: embed.Build());
                                break;
                        }
                        break;
                    case "unban":
                        switch (commandSubName)
                        {
                            case "by-steamid":
                                steamID = ConvertSteamID(commandSubValues.GetValueOrDefault("steamid", ""));
                                MySQLClient.UnBanPlayer(steamID);

                                embed.WithTitle($"Игрок {steamID} разбанен");
                                embed.WithColor(Color.Green);
                                embed.AddField($"Админ", command.User.Mention);
                                await command.RespondAsync(embed: embed.Build());
                                break;
                            case "by-profilename":
                                name = CheckName(commandSubValues.GetValueOrDefault("name", ""));
                                steamID = MySQLClient.GetSteamIDByProfileName(name);
                                MySQLClient.UnBanPlayer(steamID);

                                embed.WithTitle($"Игрок {steamID} разбанен");
                                embed.WithColor(Color.Green);
                                embed.AddField($"Админ", command.User.Mention);
                                await command.RespondAsync(embed: embed.Build());
                                break;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await command.RespondAsync(ex.Message, ephemeral: true);
            }
        }

        private long ConvertSteamID(string SteamID)
        {
            if (!string.IsNullOrEmpty(SteamID))
            {
                long steamID;
                var success = long.TryParse(SteamID, out steamID);
                if (success)
                {
                    return steamID;
                }
            }
            throw new Exception("Указан некорректный SteamID");
        }

        private int ConvertRank(string rank)
        {
            if (!string.IsNullOrEmpty(rank))
            {
                int nrank;
                var success = int.TryParse(rank, out nrank);
                if (success)
                {
                    return nrank;
                }
            }
            return 1;
        }

        private string CheckName(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                return name;
            }
            throw new Exception("Указано некорректное имя игрока");
        }

        private (int, DateTime) ConvertBanTime(string bantime)
        {
            var endTime = DateTime.Now;
            int infinity = 0;
            if (!string.IsNullOrEmpty(bantime))
            {
                var banTime = TimeSpan.Zero;
                int temp;
                var success = int.TryParse(bantime, out temp);
                if (success)
                {
                    if (temp == 0)
                    {
                        infinity = 1;
                    }
                }
                else
                {
                    try
                    {
                        banTime = bantime.ParseTimeSpan();
                        endTime = endTime + banTime;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            } else
            {
                infinity = 1;
            }
            return (infinity, endTime);
        }
        
        private async Task<bool> CheckPermissions(SocketSlashCommand command)
        {
            var user = command.User as SocketGuildUser;
            if (Program.Configuration.DiscordAdminRoleAccess && user.GuildPermissions.Administrator)
                return true;

            var commandName = command.Data.Options.First().Name;

            bool manage = false;
            foreach (var role in user.Roles)
            {
                if (Program.Configuration.DiscordManageRoleId.Contains(role.Id))
                {
                    manage = true;
                }
            }

            if (ApplicationOwnerID == null)
            {
                switch (Client.TokenType)
                {
                    case TokenType.Bot:
                        var application = await Client.GetApplicationInfoAsync().ConfigureAwait(false);
                        ApplicationOwnerID = application.Owner.Id;
                        break;
                }
            }

            if (user.Id == ApplicationOwnerID)
                manage = true;

            switch (commandName)
            {
                case "zeus":
                    return manage;
                case "infistar":
                    return manage;
                case "ban":
                    return manage || user.GuildPermissions.BanMembers;
                case "kick":
                    return manage || user.GuildPermissions.KickMembers;
                case "unban":
                    return manage || user.GuildPermissions.BanMembers;
            }

            throw new Exception("Недостаточно прав для использования команды");
        }
    }
}
