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
                    .AddOption("name", ApplicationCommandOptionType.String, "Название миссии", required: true)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("installsteam")
                    .WithDescription("Установить SteamCMD если не установлен")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("update")
                    .WithDescription("Проверить сервер на наличие обновлений и обновить если требуется")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("updatemods")
                    .WithDescription("Актуализация модов по пресету, проверка модов на наличие обновлений и обновлени если требуется")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("steamlogin")
                    .WithDescription("Авторизация в steam. После авторизации будет сохранен только логин для дальнешей авторизации из кэша")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption("login", ApplicationCommandOptionType.String, "Логин", required: true)
                    .AddOption("password", ApplicationCommandOptionType.String, "Пароль", required: true)
                    .AddOption("steamguard", ApplicationCommandOptionType.String, "Steam Guard", required: false)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("addmod")
                    .WithDescription("Добавить один мод в пресет")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption("modid", ApplicationCommandOptionType.String, "Id мода", required: true)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("deletemod")
                    .WithDescription("Удалить один мод из пресета")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption("modid", ApplicationCommandOptionType.String, "Id мода", required: true)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("deleteunusedmods")
                    .WithDescription("Удалить неиспользуемые моды")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                ).AddOption(new SlashCommandOptionBuilder()
                    .WithName("getmodslist")
                    .WithDescription("Список модов сервера")
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
                        .AddOption("steamid", ApplicationCommandOptionType.String, "SteamID игрока", required: true)
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
                        res = await ServerCommands.StartServer(command.User as SocketGuildUser);
                        await command.RespondAsync(res);
                        break;
                    case "stop":
                        res = await ServerCommands.StopServer(command.User as SocketGuildUser);
                        await command.RespondAsync(res);
                        break;
                    case "restart":
                        res = await ServerCommands.RestartServer(command.User as SocketGuildUser);
                        await command.RespondAsync(res);
                        break;
                    case "mslist":
                        res = await ServerCommands.MsList(command.User as SocketGuildUser);
                        await command.RespondAsync(res);
                        break;
                    case "setms":
                        res = await ServerCommands.SetMS(command.User as SocketGuildUser, commandSubValues.GetValueOrDefault("name", ""));
                        await command.RespondAsync(res);
                        break;
                    case "installsteam":
                        await command.RespondAsync("Запускаем команду");
                        await ServerCommands.InstallSteamCMD(command.User as SocketGuildUser, command.Channel);
                        break;
                    case "update":
                        await command.RespondAsync("Запускаем команду");
                        await ServerCommands.UpdateServer(command.User as SocketGuildUser, command.Channel);
                        break;
                    case "updatemods":
                        await command.RespondAsync("Запускаем команду");
                        await ServerCommands.UpdateServerMods(command.User as SocketGuildUser, command.Channel);
                        break;
                    case "steamlogin":
                        await command.RespondAsync("Запускаем команду");
                        await ServerCommands.SteamLogin(command.User as SocketGuildUser, command.Channel, commandSubValues.GetValueOrDefault("login", ""), commandSubValues.GetValueOrDefault("password", ""), commandSubValues.GetValueOrDefault("steamguard", ""));
                        break;
                    case "addmod":
                        res = await ServerCommands.AddMod(command.User as SocketGuildUser, commandSubValues.GetValueOrDefault("modid", ""));
                        await command.RespondAsync(res);
                        break;
                    case "deletemod":
                        res = await ServerCommands.DeleteMod(command.User as SocketGuildUser, commandSubValues.GetValueOrDefault("modid", ""));
                        await command.RespondAsync(res);
                        break;
                    case "deleteunusedmods":
                        await command.RespondAsync("Запускаем команду");
                        await ServerCommands.DeleteUnusedMods(command.User as SocketGuildUser, command.Channel);
                        break;
                    case "getmodslist":
                        res = await ServerCommands.GetModsList(command.User as SocketGuildUser);
                        await command.RespondAsync(res);
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

                Log.Information("{User} использовал комманду player {commandName} {commandSubName}", command.User, commandName, commandSubName);
                string res;
                Embed embed;
                switch (commandName)
                {
                    case "zeus":
                        switch (commandSubName)
                        {
                            case "give":
                                res = await PlayerCommands.ZeusGive(command.User as SocketGuildUser, commandSubValues.GetValueOrDefault("steamid"), bool.Parse(commandSubValues.GetValueOrDefault("temp")));
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
                                res = await PlayerCommands.InfistarGive(command.User as SocketGuildUser,
                                    commandSubValues.GetValueOrDefault("steamid"),
                                    commandSubValues.GetValueOrDefault("rank", "1"),
                                    bool.Parse(commandSubValues.GetValueOrDefault("temp"))
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
