using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Common
{
    public class TryAndRoll
    {
        public static string Roll(int min = 0, int max = 100, bool custom = false)
        {
            max = max < 10 ? 10 : max;
            var rand = new Random();
            var result = rand.Next(min, max);
            string text = "";

            if (custom)
            {
                text = $"кинул кубики и получил {result} из {max}";
            }
            else
            {
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

            return text;
        }

        public static string Try(string text)
        {
            var rand = new Random();
            string rtext = $"{(rand.Next(0, 2) % 2 == 1 ? "Успешно" : "Неудачно")} {text}";

            return rtext;
        }
    }
}
