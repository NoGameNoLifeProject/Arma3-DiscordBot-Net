using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DiscordBot.Common;
using Serilog;

namespace DiscordBot.Services;

public class Arma3PlayersOnlineService
{
    public void Initialize()
    {
        var playersOnlineTimer = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(Arma3Server.Config.A3PlayersOnlineUpdateInterval)).Timestamp();
        playersOnlineTimer.Subscribe(async x => await UpdatePlayers());
    }

    public async Task UpdatePlayers()
    {
        try
        {
            var players = SteamQueryServer.GetServerPlayers();
            await MySQLClient.UpdatePlayersOnline(players);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Ошибка при обновлении онлайна игроков");
        }
    }
}