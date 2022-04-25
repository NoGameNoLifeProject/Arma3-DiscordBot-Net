using Dawn;
using Discord.WebSocket;
using System;

namespace DiscordBot.Modules.Commands
{
    public static class CommonCommands
    {
        public static string Roll(SocketGuildUser user, string smin = "0", string smax = "100", bool custom = false)
        {
            if (!string.IsNullOrEmpty(smin) || !string.IsNullOrEmpty(smax))
                custom = true;

            int min = 0;
            int max = 100;
            Random rand = new Random();
            int result;
            string text = "";

            if (custom)
            {
                min = Utils.ConvertInt(smin);
                max = Utils.ConvertInt(smax);
                Guard.Argument(min, nameof(min)).Min(0);
                Guard.Argument(max, nameof(max)).Min(min + 1);
                max = max < 10 ? 10 : max;
                result = rand.Next(min, max);
                text = $"кинул кубики и получил {result} из {max}";
            }
            else
            {
                result = rand.Next(min, max);
                if (result < max * 0.1)
                    text = $"решил обмануть систему и посчитать на калькуляторе, но калькулятор оказался умнее {result} из {max}";
                else if (result < max * 0.2)
                    if (rand.Next(0, 2) % 2 == 1)
                        text = $"решил посчитать на пальцах и получил {result} из {max}";
                    else
                        text = $"кинул два кубика и получил {result} из {max}";
                else if (result < max * 0.4)
                    text = $"кинул два кубика и получил {result} из {max}, какие-то странные кубики...";
                else if (result < max * 0.65)
                    text = $"достаточно удачлив сегодня, он получил {result} из {max}";
                else if (result < max * 0.9)
                    text = $"поймал удачу, ему досталось {result} из {max}. Что бы это могло значить?";
                else if (result < max)
                    text = $"забрал почти все, что здесь было {result} из {max}";
                else if (result == max)
                    text = $"Забрал все, что здесь было {result} из {max}, можнно расходиться";
            }

            return $"{ user.Username} { text }";
        }

        public static string Try(SocketGuildUser user, string text)
        {
            string rtext = $"{ user.Username } {(new Random().Next(0, 2) % 2 == 1 ? "[Успешно]" : "[Неудачно]")} {text}";
            return rtext;
        }
    }
}
