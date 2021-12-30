using Dawn;
using Discord;
using Discord.WebSocket;
using DiscordBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules.Commands
{
    public static class PlayerCommands
    {
        public static string ZeusGive(SocketGuildUser user, string steamID, bool temp = false)
        {
            Guard.Argument(steamID, nameof(steamID)).NotNull().Length(17);
            Utils.CheckPermissions(user, PermissionsEnumCommands.Zeus);
            var steamIDlong = Utils.ConvertLong(steamID);
            if (!temp)
                MySQLClient.UpdateZeus(steamIDlong, 1);
            WebSocketClient.UpdateZeus(steamID, "1");
            return $"Игроку {steamID} успешно выдан zeus";
        }

        public static string ZeusRemove(SocketGuildUser user, string steamID)
        {
            Guard.Argument(steamID, nameof(steamID)).NotNull().Length(17);
            Utils.CheckPermissions(user, PermissionsEnumCommands.Zeus);
            var steamIDlong = Utils.ConvertLong(steamID);
            MySQLClient.UpdateZeus(steamIDlong, 0);
            WebSocketClient.UpdateZeus(steamID, "0");
            return $"У игрока {steamID} успешно забран zeus";
        }

        public static Embed ZeusList(SocketGuildUser user)
        {
            Utils.CheckPermissions(user, PermissionsEnumCommands.Zeus);
            var players = MySQLClient.ZeusPlayersList();
            EmbedBuilder embed = new EmbedBuilder();
            embed.Timestamp = DateTime.Now;
            embed.WithTitle($"Список пользователей с Zeus");
            embed.WithColor(Color.Blue);
            embed.WithDescription(string.Join("\n", players.Select(p => { return p.SteamName + " - " + p.SteamID.ToString(); })));
            return embed.Build();
        }

        public static string InfistarGive(SocketGuildUser user, string steamID, string rank = "1", bool temp = false)
        {
            Guard.Argument(steamID, nameof(steamID)).NotNull().Length(17);
            Guard.Argument(rank, nameof(rank)).NotEmpty();
            Utils.CheckPermissions(user, PermissionsEnumCommands.Infistar);
            var steamIDlong = Utils.ConvertLong(steamID);
            var ranklong = Utils.ConvertInt(rank);
            if (!temp)
                MySQLClient.UpdateInfiSTAR(steamIDlong, ranklong);
            WebSocketClient.UpdateInfiSTAR(steamID, rank);
            return $"Игроку {steamID} успешно выдан infiSTAR, Уровень = {rank}";
        }

        public static string InfistarRemove(SocketGuildUser user, string steamID)
        {
            Guard.Argument(steamID, nameof(steamID)).NotNull().Length(17);
            Utils.CheckPermissions(user, PermissionsEnumCommands.Infistar);
            var steamIDlong = Utils.ConvertLong(steamID);
            MySQLClient.UpdateInfiSTAR(steamIDlong, 0);
            WebSocketClient.UpdateInfiSTAR(steamID, "0");
            return $"У игрока {steamID} успешно забран infiSTAR";
        }

        public static Embed InfistarList(SocketGuildUser user)
        {
            Utils.CheckPermissions(user, PermissionsEnumCommands.Infistar);
            var players = MySQLClient.InfiPlayersList();
            var groups = players.GroupBy(p => p.Infistar).OrderBy(p => p.Key);
            EmbedBuilder embed = new EmbedBuilder();
            embed.Timestamp = DateTime.Now;
            embed.WithTitle($"Список пользователей с infiSTAR");
            embed.WithColor(Color.Red);
            foreach (var group in groups)
            {
                embed.AddField($"Rank {group.Key}", string.Join("\n", group.Select(p => { return p.SteamName + " - " + p.SteamID.ToString(); })));
            }
            return embed.Build();
        }

        public static Embed Ban(SocketGuildUser user, string reason, string banTime, int infinity = 0, string steamID = "", string name = "")
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
            Utils.CheckPermissions(user, PermissionsEnumCommands.Ban);
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
            return embed.Build();
        }

        public static Embed UnBan(SocketGuildUser user, string steamID = "", string name = "")
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
            Utils.CheckPermissions(user, PermissionsEnumCommands.UnBan);
            MySQLClient.UnBanPlayer(steamIDlong);

            EmbedBuilder embed = new EmbedBuilder();
            embed.Timestamp = DateTime.Now;
            embed.WithTitle($"Игрок {steamIDlong} разбанен");
            embed.WithColor(Color.Green);
            embed.AddField($"Админ", user.Mention);
            return embed.Build();
        }

        public static Embed Kick(SocketGuildUser user, string reason, string steamID = "", string name = "")
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
            Utils.CheckPermissions(user, PermissionsEnumCommands.Kick);
            WebSocketClient.KickPlayer(steamIDlong.ToString(), reason);

            EmbedBuilder embed = new EmbedBuilder();
            embed.Timestamp = DateTime.Now;
            embed.WithTitle($"Игрок {steamIDlong} кикнут");
            embed.WithColor(Color.DarkBlue);
            embed.AddField($"Причина", reason);
            embed.AddField($"Админ", user.Mention);
            return embed.Build();
        }
    }
}
