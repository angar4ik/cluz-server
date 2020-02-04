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
        //private readonly IHubContext<GamesHub> _hubContext;
        //public Scavenger(IHubContext<GamesHub> hubContext)
        //{
        //    _hubContext = hubContext;
        //}

        GamePool _gamePool;

        public Scavenger(GamePool gamePool)
        {
            _gamePool = gamePool;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (Game g in _gamePool.Games.Values.ToList())
                {
                    if ((DateTime.UtcNow - g.ChangeTimeSpamp).TotalHours > 1)
                    {
                        _gamePool.Games.Remove(g.Guid);
                        Log.Information("Game {0} had no changes more than hour, removing from pool", g.Name);
                        //await _hubContext.Clients.All.SendAsync("RefreshGameList");
                    }
                }

                //await _hubContext.Clients.All.SendAsync("Ping");

                await Task.Delay(60000);
            }
        }
    }
}
