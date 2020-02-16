using CLUZServer.Hubs;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CLUZServer.Models;

namespace CLUZServer.Helpers
{
    public class Voting
    {
        public static async void AllowRandomPlayerToVote(Game g, IHubContext<PlayersHub> hubContext)
        {
            if (Time.IsDay(g))
            {
                Random rand = new Random();

                //this shit should return true if any of active player still have AllowedToVote = false
                while (g.Players.Values.ToList()
                    .FindAll(p => p.Role != PlayerRole.Ghost && p.Role != PlayerRole.Kicked).ToList()
                    .Exists(p => p.AllowedToVote == false))
                {
                    Player p = g.Players.ElementAt(rand.Next(0, g.Players.Count)).Value;

                    if (p.AllowedToVote == false
                        && IsPlayerActive(p) == true)
                    {
                        p.AllowedToVote = true;

                        await hubContext.Clients.All.SendAsync("SnackbarMessage", $"'{p.Name}' is voting", 3, g.Guid);

                        break;
                    }
                }
            }
            else
            {
                g.Players.ToList().ForEach(p => p.Value.AllowedToVote = false);
            }
        }

        private static bool IsPlayerActive(Player p)
        {
            if(p.Role != PlayerRole.Ghost && p.Role != PlayerRole.Kicked)
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
