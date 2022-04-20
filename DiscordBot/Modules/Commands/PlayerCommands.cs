using Dawn;
using Discord;
using Discord.WebSocket;
using DiscordBot.Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordBot.Common.Enums;

namespace DiscordBot.Modules.Commands
{
    public static class PlayerCommands
    {
        public static async Task<string> ZeusGive(SocketGuildUser user, string steamID, IGuildUser player = null, bool temp = false)
        {
            Guard.Argument(steamID, nameof(steamID)).NotNull().Length(17);
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Zeus);
            var steamIDlong = Utils.ConvertLong(steamID);
            if (!temp)
                MySQLClient.UpdateZeus(steamIDlong, 1, player);
            WebSocketClient.UpdateZeus(steamID, "1");

            Log.Information("{User} выдал zeus игроку {steamID}", user, steamID);
            return $"Игроку {steamID} успешно выдан zeus" + (temp ? " (Временно)" : "");
        }

        public static async Task<string> ZeusRemove(SocketGuildUser user, string steamID)
        {
            Guard.Argument(steamID, nameof(steamID)).NotNull().Length(17);
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Zeus);
            var steamIDlong = Utils.ConvertLong(steamID);
            MySQLClient.UpdateZeus(steamIDlong, 0);
            WebSocketClient.UpdateZeus(steamID, "0");

            Log.Information("{User} забрал zeus у игрока {steamID}", user, steamID);
            return $"У игрока {steamID} успешно забран zeus";
        }

        public static async Task<Embed> ZeusList(SocketGuildUser user)
        {
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Zeus);
            var players = MySQLClient.ZeusPlayersList();
            EmbedBuilder embed = new EmbedBuilder();
            embed.Timestamp = DateTime.Now;
            embed.WithTitle($"Список пользователей с Zeus");
            embed.WithColor(Color.Blue);
            embed.WithDescription(string.Join("\n", players.Select(p => { return (p.Discord != 0 ? $"<@{p.Discord}>" : "") + $" - {p.SteamName} - {p.SteamID.ToString()}"; })));
            return embed.Build();
        }

        public static async Task<string> InfistarGive(SocketGuildUser user, string steamID, IGuildUser player = null, string rank = "1", bool temp = false)
        {
            Guard.Argument(steamID, nameof(steamID)).NotNull().Length(17);
            Guard.Argument(rank, nameof(rank)).NotEmpty();
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Infistar);
            var steamIDlong = Utils.ConvertLong(steamID);
            var ranklong = Utils.ConvertInt(rank);
            if (!temp)
                MySQLClient.UpdateInfiSTAR(steamIDlong, ranklong, player);
            WebSocketClient.UpdateInfiSTAR(steamID, rank);

            Log.Information("{User} выдал infiSTAR игроку {steamID}, Уровень = {rank}", user, steamID, rank);
            return $"Игроку {steamID} успешно выдан infiSTAR, Уровень = {rank}" + (temp ? " (Временно)" : "");
        }

        public static async Task<string> InfistarRemove(SocketGuildUser user, string steamID)
        {
            Guard.Argument(steamID, nameof(steamID)).NotNull().Length(17);
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Infistar);
            var steamIDlong = Utils.ConvertLong(steamID);
            MySQLClient.UpdateInfiSTAR(steamIDlong, 0);
            WebSocketClient.UpdateInfiSTAR(steamID, "0");

            Log.Information("{User} забрал infiSTAR у игрока {steamID}", user, steamID);
            return $"У игрока {steamID} успешно забран infiSTAR";
        }

        public static async Task<Embed> InfistarList(SocketGuildUser user)
        {
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Infistar);
            var players = MySQLClient.InfiPlayersList();
            var groups = players.GroupBy(p => p.Infistar).OrderBy(p => p.Key);
            EmbedBuilder embed = new EmbedBuilder();
            embed.Timestamp = DateTime.Now;
            embed.WithTitle($"Список пользователей с infiSTAR");
            embed.WithColor(Color.Red);
            foreach (var group in groups)
            {
                embed.AddField($"Rank {group.Key}", string.Join("\n", group.Select(p => { return (p.Discord != 0 ? $"<@{p.Discord}>" : "") + $" - {p.SteamName} - {p.SteamID.ToString()}"; })));
            }
            return embed.Build();
        }

        public static async Task<Embed> Ban(SocketGuildUser user, string reason, string banTime, int infinity = 0, string steamID = "", string name = "")
        {
            long steamIDlong;
            if (string.IsNullOrEmpty(steamID))
            {
                Guard.Argument(name, nameof(name)).NotNull().NotEmpty().MinLength(4);
                steamIDlong = MySQLClient.GetSteamIDByProfileName(name);
            } else
            {
                steamIDlong = Utils.ConvertLong(steamID);
            }
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Ban);
            var cbanTime = Utils.ConvertBanTime(banTime);

            MySQLClient.BanPlayer(steamIDlong, cbanTime.Item2, reason, cbanTime.Item1);
            WebSocketClient.BanPlayer(steamID.ToString(), cbanTime.Item2, reason);

            EmbedBuilder embed = new EmbedBuilder();
            embed.Timestamp = DateTime.Now;
            embed.WithTitle($"Игрок {steamID} забанен");
            embed.WithColor(Color.Red);
            embed.AddField($"Окончание блокировки", cbanTime.Item1 == 1 ? "Никогда" : cbanTime.Item2.ToString());
            embed.AddField($"Причина", reason);
            embed.AddField($"Админ", user.Mention);

            Log.Information("{User} забанил игрока {steamID} \n Окончание блокировки: {BanTime} \n Причина: {reason} ", user, steamID, (cbanTime.Item1 == 1 ? "Никогда" : cbanTime.Item2.ToString()), reason);
            return embed.Build();
        }

        public static async Task<Embed> UnBan(SocketGuildUser user, string steamID = "", string name = "")
        {
            long steamIDlong;
            if (string.IsNullOrEmpty(steamID))
            {
                Guard.Argument(name, nameof(name)).NotNull().NotEmpty().MinLength(4);
                steamIDlong = MySQLClient.GetSteamIDByProfileName(name);
            }
            else
            {
                steamIDlong = Utils.ConvertLong(steamID);
            }
            await Utils.CheckPermissions(user, PermissionsEnumCommands.UnBan);
            MySQLClient.UnBanPlayer(steamIDlong);

            EmbedBuilder embed = new EmbedBuilder();
            embed.Timestamp = DateTime.Now;
            embed.WithTitle($"Игрок {steamIDlong} разбанен");
            embed.WithColor(Color.Green);
            embed.AddField($"Админ", user.Mention);

            Log.Information("{User} разбанил игрока {steamID}", user, steamID);
            return embed.Build();
        }

        public static async Task<Embed> Kick(SocketGuildUser user, string reason, string steamID = "", string name = "")
        {
            long steamIDlong;
            if (string.IsNullOrEmpty(steamID))
            {
                Guard.Argument(name, nameof(name)).NotNull().NotEmpty().MinLength(4);
                steamIDlong = MySQLClient.GetSteamIDByProfileName(name);
            }
            else
            {
                steamIDlong = Utils.ConvertLong(steamID);
            }
            await Utils.CheckPermissions(user, PermissionsEnumCommands.Kick);
            WebSocketClient.KickPlayer(steamIDlong.ToString(), reason);

            EmbedBuilder embed = new EmbedBuilder();
            embed.Timestamp = DateTime.Now;
            embed.WithTitle($"Игрок {steamIDlong} кикнут");
            embed.WithColor(Color.DarkBlue);
            embed.AddField($"Причина", reason);
            embed.AddField($"Админ", user.Mention);

            Log.Information("{User} кикнул игрока {steamID} \n Причина: {reason}", user, steamID, reason);
            return embed.Build();
        }
    }
}
