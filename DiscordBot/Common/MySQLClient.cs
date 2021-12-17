using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordBot.Configs;
using Microsoft.Extensions.Configuration;
using Dapper;
using DiscordBot.Common.Entities;
using MySqlConnector;

namespace DiscordBot.Common
{
    public class MySQLClient
    {
        private static MySQLConfig _config { get; set; }
        private static string _connectionString { get; set; }

        public static MySQLConfig Config { get => _config ??= BuildConfig(); }
        public static string ConnectionString { get { return _connectionString ?? GenConnectionString(); } }

        public static void UpdateZeus(long steamID, int state)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Query($"Insert into Players (SteamID, Zeus) Values({steamID}, {state}) on duplicate key update Zeus = {state}");
            }
        }

        public static void UpdateInfiSTAR(long steamID, int state)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Query($"Insert into Players (SteamID, Infistar) Values({steamID}, {state}) on duplicate key update Infistar = {state}");
            }
        }

        public static List<Players> InfiPlayersList()
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                var result = connection.Query<Players>($"select * from Players where Infistar > 0").ToList();
                return result;
            }
        }

        public static List<Players> ZeusPlayersList()
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                var result = connection.Query<Players>($"select * from Players where Zeus = 1").ToList();
                return result;
            }
        }

        public static void BanPlayer(long steamID, DateTime endDate = new DateTime(), string reason = "", int infinity = 0)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Query($"Insert into Players_Bans (SteamID, EndDate, Reason, Infinity) Values ({steamID}, ?EndDate?, '{reason}', {infinity})", new { EndDate = endDate });
            }
        }

        public static void UnBanPlayer(long steamID)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Query($"update Players_Bans set EndDate = now(), Infinity = 0 where SteamID = {steamID} and EndDate > now()");
            }
        }

        public static long GetSteamIDByProfileName(string name)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                List<Players_Profiles> result = connection.Query<Players_Profiles>($"select * from players_profiles where lower(ProfileName) like '%{name.ToLower()}%'").ToList();
                if (result.Count > 1)
                {
                    var groups = result.GroupBy(x => x.SteamID);
                    if (groups.Count() > 1)
                    {
                        throw new Exception($"Найдено больше одного игрока, уточните ник \n {string.Join("\n", groups.Select(p => { return p.Key + " - " + string.Join(" | ", p.Select(x => x.ProfileName)); }))}");
                    }
                    return result.First().SteamID;
                }
                else if (result.Count == 0)
                    throw new Exception("Не найдено ни одного игрока, уточните ник");

                return result.First().SteamID;
            }
        }

        private static MySQLConfig BuildConfig()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            return builder.GetSection("MySQL").Get<MySQLConfig>();
        }

        private static string GenConnectionString()
        {
            _connectionString = $"Server={Config.Server};Database={Config.Database};Uid={Config.User};Pwd={Config.Password};";
            return _connectionString;
        }
    }
}
