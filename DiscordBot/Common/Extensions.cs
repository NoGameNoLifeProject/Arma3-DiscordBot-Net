using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DiscordBot.Common
{
    public static class Extensions
    {
        private static Dictionary<string, (string, int)> TimeDict = new Dictionary<string, (string, int)>(){
                {@"(\d+)(?:ms|mili(?:secon)?s?)", ("ms", 1) },
                {@"(\d+)(?:s(?:ec)?|seconds?)", ("s", 1) },
                {@"(\d+)(?:m|mins?)", ("m", 1) },
                {@"(\d+)(?:h|hours?)", ("h", 1) },
                {@"(\d+)(?:d|days?)", ("d", 1) },
                {@"(\d+)(?:w|weeks?)", ("d", 7) },
                {@"(\d+)(?:y|yars?)", ("d", 365) }
            };
        public static TimeSpan ParseTimeSpan(this string timeString)
        {
            var timespan = new TimeSpan();
            foreach (var key in TimeDict)
            {
                var matches = Regex.Matches(timeString, key.Key);
                foreach (Match match in matches)
                {
                    var inptime = Convert.ToInt32(match.Groups[1].Value);
                    switch (key.Value.Item1)
                    {
                        case "ms":
                            timespan += TimeSpan.FromMilliseconds(inptime * key.Value.Item2);
                            break;
                        case "s":
                            timespan += TimeSpan.FromSeconds(inptime * key.Value.Item2);
                            break;
                        case "m":
                            timespan += TimeSpan.FromMinutes(inptime * key.Value.Item2);
                            break;
                        case "h":
                            timespan += TimeSpan.FromHours(inptime * key.Value.Item2);
                            break;
                        case "d":
                            timespan += TimeSpan.FromDays(inptime * key.Value.Item2);
                            break;
                    }
                }
            }
            return timespan;
        }
    }
}
