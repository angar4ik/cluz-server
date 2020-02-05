using CLUZServer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CLUZServer
{
    public class Scavenger : BackgroundService
    {
        private readonly IHubContext<PlayersHub> _hubContext;
        private GamePool _gamePool;

        public Scavenger(IHubContext<PlayersHub> hubContext, GamePool gamePool)
        {
            _gamePool = gamePool;
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Guid gameToRemove = Guid.Empty;

                foreach (Game g in _gamePool.Games.Values.ToList())
                {
                    if ((DateTime.UtcNow - g.ChangeTimeSpamp).TotalHours > 1)
                    {
                        gameToRemove = g.Guid;
                        Log.Information("Game {0} had no changes more than hour, removing from pool", g.Name);
                    }
                }

                if (gameToRemove != Guid.Empty)
                {
                    _gamePool.Games.Remove(gameToRemove);
                    await _hubContext.Clients.All.SendAsync("RefreshGameList");
                }

                await Task.Delay(60000);
            }
        }
    }
}
