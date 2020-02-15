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

                while (g.Players.ToList().Exists(p => p.Value.AllowedToVote == false))
                {
                    Player p = g.Players.ElementAt(rand.Next(0, g.Players.Count)).Value;

                    if (p.AllowedToVote == false)
                    {
                        p.AllowedToVote = true;

                        await hubContext.Clients.All.SendAsync("SnackbarMessage", $"'{p.Name}' is voting", 5, g.Guid);

                        break;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                g.Players.ToList().ForEach(p => p.Value.AllowedToVote = false);
            }
        }
    }
}
