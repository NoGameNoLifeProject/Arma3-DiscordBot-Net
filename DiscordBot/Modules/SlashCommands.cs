using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using DiscordBot.Modules.Commands;
using Serilog;

namespace DiscordBot.Modules
{
    public class SlashCommands
    {
        public DiscordSocketClient Client { get; set; }

        private List<ApplicationCommandProperties> ApplicationCommandProperties = new();
        public SlashCommands(DiscordSocketClient client)
        {
            Client = client;
        }

        public void RegisterCommands()
        {
            ServerCommandGroup();
            PlayerCommandGroup();

            RegisterGlobalCommands();
        }

        public async void RegisterGlobalCommands()
        {
            try
            {
                await Client.BulkOverwriteGlobalApplicationCommandsAsync(ApplicationCommandProperties.ToArray());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при регистрации slash комманд");
            }
        }

        private void ServerCommandGroup()
        {
            var globalCommand = new SlashCommandBuilder()
                .WithName("server")
                .WithDescription("Управление сервером")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("start")
                    .WithDescription("Включить сервер")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("stop")
                    .WithDescription("Выключить сервер")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("restart")
                    .WithDescription("Перезагрузить сервер")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("mslist")
                    .WithDescription("Список доступных миссий")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("setms")
                    .WithDescription("Изменить миссию")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption("name", ApplicationCommandOptionType.String, "Название миссии", isRequired: true)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("updatemods")
                    .WithDescription("Проверка обновления модов")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                );

            ApplicationCommandProperties.Add(globalCommand.Build());
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
                        .AddOption("steamid", ApplicationCommandOptionType.String, "SteamID игрока", isRequired: true)
                        .AddOption("user", ApplicationCommandOptionType.User, "Discord игрока", isRequired: true)
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("temp")
                            .WithDescription("Выдать временно")
                            .AddChoice("Да", bool.TrueString)
                            .AddChoice("Нет", bool.FalseString)
                            .WithType(ApplicationCommandOptionType.String)
                        )
                    ).AddOption(new SlashCommandOptionBuilder()
                        .WithName("remove")
                        .WithDescription("Забрать Zeus по SteamID")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("steamid", ApplicationCommandOptionType.String, "SteamID игрока", isRequired: true)
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
                        .AddOption("steamid", ApplicationCommandOptionType.String, "SteamID игрока", isRequired: true)
                        .AddOption("user", ApplicationCommandOptionType.User, "Discord игрока", isRequired: true)
                        .AddOption("rank", ApplicationCommandOptionType.String, "Уровень infiSTAR", isRequired: false)
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("temp")
                            .WithDescription("Выдать временно")
                            .AddChoice("Да", bool.TrueString)
                            .AddChoice("Нет", bool.FalseString)
                            .WithType(ApplicationCommandOptionType.String)
                        )
                    ).AddOption(new SlashCommandOptionBuilder()
                        .WithName("remove")
                        .WithDescription("Забрать intiSTAR по SteamID")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("steamid", ApplicationCommandOptionType.String, "SteamID игрока", isRequired: true)
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
                        .AddOption("steamid", ApplicationCommandOptionType.String, "SteamID игрока", isRequired: true)
                        .AddOption("bantime", ApplicationCommandOptionType.String, "Время бана (0 = перманентный)", isRequired: false)
                        .AddOption("reason", ApplicationCommandOptionType.String, "Причина бана", isRequired: false)
                    ).AddOption(new SlashCommandOptionBuilder()
                        .WithName("by-profilename")
                        .WithDescription("Забанить игрока по имени")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("name", ApplicationCommandOptionType.String, "Имя игрока", isRequired: true)
                        .AddOption("bantime", ApplicationCommandOptionType.String, "Время бана (0 = перманентный)", isRequired: false)
                        .AddOption("reason", ApplicationCommandOptionType.String, "Причина бана", isRequired: false)
                    )
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("kick")
                    .WithDescription("Кикнуть игрока")
                    .WithType(ApplicationCommandOptionType.SubCommandGroup)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("by-steamid")
                        .WithDescription("Кикнуть игрока по SteamID")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("steamid", ApplicationCommandOptionType.String, "SteamID игрока", isRequired: true)
                        .AddOption("reason", ApplicationCommandOptionType.String, "Причина кика", isRequired: false)
                    ).AddOption(new SlashCommandOptionBuilder()
                        .WithName("by-profilename")
                        .WithDescription("Кикнуть игрока по имени")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("name", ApplicationCommandOptionType.String, "Имя игрока", isRequired: true)
                        .AddOption("reason", ApplicationCommandOptionType.String, "Причина кика", isRequired: false)
                    )
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("unban")
                    .WithDescription("Разбанить игрока")
                    .WithType(ApplicationCommandOptionType.SubCommandGroup)
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("by-steamid")
                        .WithDescription("Разбанить игрока по SteamID")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("steamid", ApplicationCommandOptionType.String, "SteamID игрока", isRequired: true)
                    ).AddOption(new SlashCommandOptionBuilder()
                        .WithName("by-profilename")
                        .WithDescription("Разбанить игрока по имени")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("name", ApplicationCommandOptionType.String, "Имя игрока", isRequired: true)
                    )
                );

            ApplicationCommandProperties.Add(globalCommand.Build());
        }

        public async Task SlashCommandHandler(SocketSlashCommand command)
        {
            switch (command.Data.Name)
            {
                case "server":
                    await HandleServerCommand(command);
                    break;
                case "player":
                    await HandlePlayerCommand(command);
                    break;
            }
        }

        private async Task HandleServerCommand(SocketSlashCommand command)
        {
            try
            {
                var commandName = command.Data.Options.First().Name;
                var commandSubValues = command.Data.Options.First().Options?.ToDictionary(x => x.Name, x => x.Value.ToString());

                Log.Information("{User} использовал комманду server {commandName}", command.User, commandName);
                string res;
                switch (commandName)
                {
                    case "start":
                        await command.RespondAsync("Запускаем сервер");
                        Task.Run(async () =>  await ServerCommands.StartServer(command.User as SocketGuildUser, command.Channel));
                        break;
                    case "stop":
                        res = await ServerCommands.StopServer(command.User as SocketGuildUser);
                        await command.RespondAsync(res);
                        break;
                    case "restart":
                        await command.RespondAsync("Начинаем перезагрузку сервера");
                        Task.Run(async () => await ServerCommands.RestartServer(command.User as SocketGuildUser, command.Channel));
                        break;
                    case "mslist":
                        res = await ServerCommands.MsList(command.User as SocketGuildUser);
                        await command.RespondAsync(res);
                        break;
                    case "setms":
                        res = await ServerCommands.SetMS(command.User as SocketGuildUser, commandSubValues.GetValueOrDefault("name", ""));
                        await command.RespondAsync(res);
                        break;
                    case "updatemods":
                        await command.RespondAsync("Начинаем проверку обновлений модов");
                        Task.Run(async () => await ServerCommands.UpdateServerMods(command.User as SocketGuildUser, command.Channel));
                        break;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Information("{User} {Error}", command.User, ex.Message);
                await command.RespondAsync(ex.Message, ephemeral: true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при обработке комманды");
                await command.RespondAsync(ex.Message, ephemeral: true);
            }
        }

        private async Task HandlePlayerCommand(SocketSlashCommand command)
        {
            try
            {
                var commandName = command.Data.Options.First().Name;
                var commandSubName = command.Data.Options.First().Options.First().Name;
                var commandSubValues = command.Data.Options.First().Options.First().Options?.ToDictionary(x => x.Name, x => x.Value.ToString());
                var commandSubValuesTest = command.Data.Options.First().Options.First().Options;

                Log.Information("{User} использовал комманду player {commandName} {commandSubName}", command.User, commandName, commandSubName);
                string res;
                Embed embed;
                switch (commandName)
                {
                    case "zeus":
                        switch (commandSubName)
                        {
                            case "give":
                                var user = commandSubValuesTest.FirstOrDefault(x => x.Name == "user")?.Value as IGuildUser;
                                var tempValue = commandSubValuesTest.FirstOrDefault(x => x.Name == "temp")?.Value.ToString();
                                var temp = tempValue != null && bool.Parse(tempValue);
                                res = await PlayerCommands.ZeusGive(command.User as SocketGuildUser, commandSubValues.GetValueOrDefault("steamid"), user, temp);
                                await command.RespondAsync(res);
                                break;
                            case "remove":
                                res = await PlayerCommands.ZeusRemove(command.User as SocketGuildUser, commandSubValues.GetValueOrDefault("steamid"));
                                await command.RespondAsync(res);
                                break;
                            case "list":
                                embed = await PlayerCommands.ZeusList(command.User as SocketGuildUser);
                                await command.RespondAsync(embed: embed);
                                break;
                        }
                        break;
                    case "infistar":
                        switch (commandSubName)
                        {
                            case "give":
                                var user = commandSubValuesTest.FirstOrDefault(x => x.Name == "user")?.Value as IGuildUser;
                                var tempValue = commandSubValuesTest.FirstOrDefault(x => x.Name == "temp")?.Value.ToString();
                                var temp = tempValue != null && bool.Parse(tempValue);
                                res = await PlayerCommands.InfistarGive(command.User as SocketGuildUser,
                                    commandSubValues.GetValueOrDefault("steamid"),
                                    user,
                                    commandSubValues.GetValueOrDefault("rank", "1"),
                                    temp
                                    );
                                await command.RespondAsync(res);
                                break;
                            case "remove":
                                res = await PlayerCommands.InfistarRemove(command.User as SocketGuildUser, commandSubValues.GetValueOrDefault("steamid"));
                                await command.RespondAsync(res);
                                break;
                            case "list":
                                embed = await PlayerCommands.InfistarList(command.User as SocketGuildUser);
                                await command.RespondAsync(embed: embed);
                                break;
                        }
                        break;
                    case "ban":
                        switch (commandSubName)
                        {
                            case "by-steamid":
                                embed = await PlayerCommands.Ban(command.User as SocketGuildUser,
                                    commandSubValues.GetValueOrDefault("reason", "Не указана"),
                                    commandSubValues.GetValueOrDefault("bantime"),
                                    steamID: commandSubValues.GetValueOrDefault("steamid")
                                   );
                                await command.RespondAsync(embed: embed);
                                break;
                            case "by-profilename":
                                embed = await PlayerCommands.Ban(command.User as SocketGuildUser,
                                    commandSubValues.GetValueOrDefault("reason", "Не указана"),
                                    commandSubValues.GetValueOrDefault("bantime"),
                                    name: commandSubValues.GetValueOrDefault("name")
                                   );
                                await command.RespondAsync(embed: embed);
                                break;
                        }
                        break;
                    case "kick":
                        switch (commandSubName)
                        {
                            case "by-steamid":
                                embed = await PlayerCommands.Kick(command.User as SocketGuildUser,
                                    commandSubValues.GetValueOrDefault("reason", "Не указана"),
                                    steamID: commandSubValues.GetValueOrDefault("steamid")
                                   );
                                await command.RespondAsync(embed: embed);
                                break;
                            case "by-profilename":
                                embed = await PlayerCommands.Kick(command.User as SocketGuildUser,
                                    commandSubValues.GetValueOrDefault("reason", "Не указана"),
                                    name: commandSubValues.GetValueOrDefault("name")
                                   );
                                await command.RespondAsync(embed: embed);
                                break;
                        }
                        break;
                    case "unban":
                        switch (commandSubName)
                        {
                            case "by-steamid":
                                embed = await PlayerCommands.UnBan(command.User as SocketGuildUser, steamID: commandSubValues.GetValueOrDefault("steamid"));
                                await command.RespondAsync(embed: embed);
                                break;
                            case "by-profilename":
                                embed = await PlayerCommands.UnBan(command.User as SocketGuildUser, name: commandSubValues.GetValueOrDefault("name"));
                                await command.RespondAsync(embed: embed);
                                break;
                        }
                        break;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                Log.Information("{User} {Error}", command.User, ex.Message);
                await command.RespondAsync(ex.Message, ephemeral: true);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Ошибка при обработке комманды");
                await command.RespondAsync(ex.Message, ephemeral: true);
            }
        }
    }
}
