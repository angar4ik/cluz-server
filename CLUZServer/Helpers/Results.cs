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
    public static class Results
    {
        public static async void CheckIfGameEnded(IHubContext<PlayersHub> hubContext, Game g)
        {
            if (IsAnyMafiaLeftInGame(g) != true && g.Status == GameState.Locked)
            {
                Log.Information("No Mafia left in game {0}. Requesting modal", g.Name);
                await hubContext.Clients.All.SendAsync("ShowModal", 10, "Citizens won!", true, g.Guid);
            }

            if (IsAnyMafiaLeftInGame(g) == true
                && HowManyActiveInGame(g) < 3
                && g.Status == GameState.Locked)
            {
                Log.Information("No Police left in game {0}. Requesting modal", g.Name);
                await hubContext.Clients.All.SendAsync("ShowModal", 10, "Mafia won!", true, g.Guid);
            }

            Log.Information("Game {0} has {1} active players", g.Name, Helpers.Results.HowManyActiveInGame(g));
        }

        public static int HowManyActiveInGame(Game g)
        {
            return g.Players.Values.ToList().Count(p => p.Role != PlayerRole.Ghost && p.Role != PlayerRole.Kicked);
        }
        public static bool IsAnyMafiaLeftInGame(Game g)
        {
            int result = g.Players.Values.ToList().Count(p => p.Role == PlayerRole.Mafia);  //TrueForAll(p => p.State == PlayerState.Ready)

            //Log.Information("{0} Mafia in game", result);

            if(result > 0)
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
