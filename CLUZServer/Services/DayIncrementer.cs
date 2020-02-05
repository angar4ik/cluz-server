using CLUZServer.Helpers;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace CLUZServer.Services
{
    public class DayIncrementer : BackgroundService
    {
        private GamePool _gamePool;
        private AllPlayersReady _allPlayersReady;

        public DayIncrementer(GamePool gamePool, AllPlayersReady allPlayersReady)
        {
            _gamePool = gamePool;
            _allPlayersReady = allPlayersReady;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (Game game in _gamePool.Games.Values)
                {
                    if (game.AllPlayersReady)
                    {
                        _allPlayersReady.Act(game);

                        game.AllPlayersReady = false;
                    }
                }

                await Task.Delay(1250);
            }
        }
    }
}
