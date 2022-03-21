using DiscordBot.Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Services
{
    public class Arma3ServerRestartsService
    {
        private List<string> RestartTimes = new();
        public void Initialize()
        {
            RestartTimes = Arma3Server.Config.A3ServerRestarts.Split(';').ToList();
            var serverRestartTimer = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(10)).Timestamp();
            serverRestartTimer.Subscribe(x => CheckRestartTime());
        }

        public void CheckRestartTime()
        {
            var curTime = DateTime.Now.ToString("HH:mm");
            foreach (var time in RestartTimes)
            {
                if (time.CompareTo(curTime) == 0)
                {
                    Log.Information("Список времени рестартов: {RestartTimes}", RestartTimes);
                    Log.Information("Время текущего рестарта: {curTime}", curTime);
                    Arma3Server.RestartServer();
                }
            }
            
        }
    }
}
