using DiscordBot.Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace DiscordBot.Services
{
    public class Arma3ServerRestartsService
    {
        private List<string> RestartTimes = new();
        public void Initialize()
        {
            RestartTimes = Arma3Server.Config.A3ServerRestarts.Split(';').ToList();
            foreach (var time in RestartTimes)
            {
                CreateScheduledRestart(DateTimeOffset.Parse(time));
            }
        }

        public void CreateScheduledRestart(DateTimeOffset dateTimeOffset)
        {
            Scheduler.Default.Schedule(dateTimeOffset,
                () => {
                    CreateScheduledRestart(dateTimeOffset.AddDays(1));
                    if (dateTimeOffset < DateTimeOffset.Now.AddMinutes(-1))
                    {
                        return;
                    }
                    Log.Information("Запланированные рестарты: {RestartTimes}", RestartTimes);
                    Log.Information("Запускаем запланированый рестарт");
                    Arma3Server.RestartServer();
                });
        }
    }
}
