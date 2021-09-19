using DiscordBot.Common;
using DiscordBot.Configs;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using SteamQueryNet.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Threading;

namespace DiscordBot.GoogleSheets
{
    public class GoogleSheetsClass
    {
        static string ApplicationName = "Star Line Bot";

        public static SheetsService SheetsService { get; set; }

        private static Dictionary<string, int> SavedPlayersScore { get; set; } = new Dictionary<string, int>();
        private static Dictionary<string, string> SavedPlayersTime { get; set; } = new Dictionary<string, string>();

        public static void Init()
        {

            GoogleCredential Credential;

            using (var stream = new MemoryStream(Resource.credentials))
            {
                Credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(SheetsService.Scope.Spreadsheets)
                    .CreateWithUser(Program.Configuration.GoogleServiceMail);
            }


            SheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = Credential,
                ApplicationName = ApplicationName,
            });
        }

        public static IList<IList<Object>> GetAllNames()
        {
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    SheetsService.Spreadsheets.Values.Get(Program.Configuration.GoogleSheetId, "A2:A");
            ValueRange response = request.Execute();
            IList<IList<Object>> values = response.Values;
            if (values == null)
            {
                values = new List<IList<Object>>();
            }
            return values;
        }

        public static IList<ValueRange> GetRowsByNames(Dictionary<string, int> dict)
        {
            List<string> ranges = new List<string>();
            foreach (KeyValuePair<string, int> pair in dict)
            {
                ranges.Add("A" + (pair.Value + 2).ToString() + ":D" + (pair.Value + 2).ToString());
            }
            var request = SheetsService.Spreadsheets.Values.BatchGet(Program.Configuration.GoogleSheetId);
            request.MajorDimension = SpreadsheetsResource.ValuesResource.BatchGetRequest.MajorDimensionEnum.ROWS;
            request.Ranges = ranges;

            var response = request.Execute();
            return response.ValueRanges;
        }

        public static Dictionary<string, int> GetNamesRows(List<Player> players)
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            var names = GetAllNames();

            foreach(Player player in players)
            {
                for (int i = 0; i <= names.Count - 1; i++)
                {
                    if (player.Name == (string)names[i][0])
{
                        dict[player.Name] = i;
                        break;
                    }
                }
            }
            return dict;
        }

        public static void AppendPlayers(List<Player> players, Dictionary<string, int> dict, string range)
        {
            List<IList<object>> vals = new List<IList<object>>();
            foreach (Player player in players)
            {
                if (!dict.ContainsKey(player.Name))
                {
                    try
                    {
                        if (player.TotalDurationAsString == "00:00:00") continue;
                        IList<object> val = new List<object>();
                        val.Add(player.Name);
                        val.Add(player.Score);
                        val.Add(TimeSpan.Parse(player.TotalDurationAsString).ToString(@"h\:mm\:ss"));
                        val.Add(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Russian Standard Time").ToString("dd.MM.yyyy HH:mm:ss"));
                        vals.Add(val);
                    } catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
            ValueRange requestBody = new ValueRange();
            requestBody.MajorDimension = "ROWS";
            requestBody.Range = range;
            requestBody.Values = vals;

            var request = SheetsService.Spreadsheets.Values.Append(requestBody, Program.Configuration.GoogleSheetId, range);
            request.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
            request.Execute();
        }

        public static Dictionary<string, List<object>> GetExistedDataAsDict(Dictionary<string, int> dict)
        {
            var existedPlayersData = GoogleSheetsClass.GetRowsByNames(dict);
            var existedPlayersDataDict = new Dictionary<string, List<object>>();
            foreach (var item in existedPlayersData)
            {
                foreach (var item2 in item.Values)
                {
                    existedPlayersDataDict[item2[0].ToString()] = new List<object>() { item2[1].ToString(), item2[2].ToString(), item2[3].ToString() };
                }
            }
            return existedPlayersDataDict;
        }

        public static void UpdatePlayers(List<Player> players, Dictionary<string, int> dict)
        {
            var existedPlayersData = GetExistedDataAsDict(dict);
            List<ValueRange> data = new List<ValueRange>();
            foreach (Player player in players)
            {
                if (dict.ContainsKey(player.Name))
                {
                    try
                    {
                        string range = "A" + (dict[player.Name] + 2).ToString();
                        List<object> val = new List<object>();
                        if (existedPlayersData.ContainsKey(player.Name))
                        {
                            int NewScore = player.Score;
                            if (SavedPlayersScore.ContainsKey(player.Name))
                            {
                                NewScore = player.Score - SavedPlayersScore[player.Name];
                            }
                            var oldTime = TimeSpan.Parse(existedPlayersData[player.Name][1].ToString());
                            var curTime = TimeSpan.FromSeconds(Program.Configuration.GoogleSheetsUpdateInterval);
                            if (SavedPlayersTime.ContainsKey(player.Name))
                            {
                                curTime = TimeSpan.Parse(player.TotalDurationAsString) - TimeSpan.Parse(SavedPlayersTime[player.Name]);
                            }
                            curTime = curTime + oldTime;
                            val.Add(player.Name);
                            val.Add(NewScore + Int32.Parse((string)existedPlayersData[player.Name][0]));
                            val.Add(curTime);
                            val.Add(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Russian Standard Time").ToString("dd.MM.yyyy HH:mm:ss"));
                            SavedPlayersScore[player.Name] = player.Score;
                            SavedPlayersTime[player.Name] = player.TotalDurationAsString;
                        }
                        else
                        {
                            val.Add(player.Name);
                            val.Add(player.Score);
                            val.Add(TimeSpan.Parse(player.TotalDurationAsString));
                            val.Add(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.Now, TimeZoneInfo.Local.Id, "Russian Standard Time").ToString("dd.MM.yyyy HH:mm:ss"));
                        }

                        ValueRange valueRange = new ValueRange();
                        valueRange.MajorDimension = "ROWS";
                        valueRange.Range = range;
                        valueRange.Values = new List<IList<object>> { val };
                        data.Add(valueRange);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }

            BatchUpdateValuesRequest requestBody = new BatchUpdateValuesRequest();
            requestBody.ValueInputOption = "USER_ENTERED";
            requestBody.Data = data;

            SheetsService.Spreadsheets.Values.BatchUpdate(requestBody, Program.Configuration.GoogleSheetId).Execute();
        }

        public static void UpdatePlayersTable()
        {
            try
            {
                Init();
                var players = SteamQueryServer.GetServerPlayers();
                var dict = GetNamesRows(players);
                UpdatePlayers(players, dict);
                AppendPlayers(players, dict, "A2:A");
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
