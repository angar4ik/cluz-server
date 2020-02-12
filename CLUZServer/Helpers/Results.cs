using CLUZServer.Hubs;
using CLUZServer.Models;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CLUZServer.Helpers
{
    public class Results
    {
        private IHubContext<PlayersHub> _hubContext;
        private GamePool _gamePool;

        public Results(IHubContext<PlayersHub> hubContext, GamePool gamePool)
        {
            _hubContext = hubContext;
            _gamePool = gamePool;
        }

        public async void CheckIfGameEnded(Game g)
        {
            if (IsAnyMafiaLeftInGame(g) != true && g.Status == GameState.Locked)
            {
                Log.Information("No Mafia left in game {0}. Requesting modal", g.Name);
                await _hubContext.Clients.All.SendAsync("ShowModal", 8, "Citizens win!", true, g.Guid);
                g.ResetPlayers();
                g.GameHasEnded = true;
            }

            if (IsAnyMafiaLeftInGame(g) == true
                && HowManyActiveInGame(g) < 3
                && g.Status == GameState.Locked)
            {
                Log.Information("No Police left in game {0}. Requesting modal", g.Name);
                await _hubContext.Clients.All.SendAsync("ShowModal", 8, "Mafia wins!", true, g.Guid);
                g.ResetPlayers();
                g.GameHasEnded = true;
            }

            //Log.Information("Game {0} has {1} active players", g.Name, Helpers.Results.HowManyActiveInGame(g));
        }

        public int HowManyActiveInGame(Game g)
        {
            return g.Players.Values.ToList().Count(p => p.Role != PlayerRole.Ghost && p.Role != PlayerRole.Kicked);
        }
        public bool IsAnyMafiaLeftInGame(Game g)
        {
            int result = g.Players.Values.ToList().Count(p => p.Role == PlayerRole.Mafia);  //TrueForAll(p => p.State == PlayerState.Ready)

            //Log.Information("{count} mafia(s) in game", result);

            if (result > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
