﻿using CLUZServer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CLUZServer.Helpers;

namespace CLUZServer.Services
{
    public class Broadcaster : BackgroundService
    {
        private IHubContext<PlayersHub> _hubContext;
        private GamePool _gamePool;
        private Results _results;

        public Broadcaster(GamePool gamePool, IHubContext<PlayersHub> hubContext, Results results)
        {
            _hubContext = hubContext;
            _gamePool = gamePool;
            _results = results;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //every 500ms scan games, players, list for change. if any, broadcast to clients
                //to appropriate game. Client should validate packets as it's broadcasting

                foreach (Game game in _gamePool.Games.Values)
                {
                    if (game.ListChanged && game.GameHasEnded != true)
                    {
                        //Log.Information("List change in '{game}'", game.Name);
                        //await _hubContext.Clients.All.SendAsync("PlayerListChanged", game.Players.Values.ToList(), game.Guid);
                        await _hubContext.Clients.Group(game.Guid.ToString()).SendAsync("PlayerListChanged", game.Players.Values.ToList());

                        game.ListChanged = false;
                    }

                    if (game.PropChanged && game.GameHasEnded != true)
                    {
                        //Log.Information("Prop change in game '{name}'", game.Name);
                        //await _hubContext.Clients.All.SendAsync("GameChanged", game, game.Guid);
                        await _hubContext.Clients.Group(game.Guid.ToString()).SendAsync("GameChanged", game);

                        _results.CheckIfGameEnded(game);

                        game.PropChanged = false;
                    }

                    foreach (Player p in game.Players.Values)
                    {
                        if (p.PropChanged && game.GameHasEnded != true)
                        {
                            //Log.Information("'{player}' prop change in game '{game}'", p.Name, game.Name);
                            //await _hubContext.Clients.All.SendAsync("PlayerChanged", p, game.Guid);
                            await _hubContext.Clients.Group(game.Guid.ToString()).SendAsync("PlayerChanged", p);

                            p.PropChanged = false;
                        }
                    }
                }

                await Task.Delay(750);
            }
        }
    }
}
