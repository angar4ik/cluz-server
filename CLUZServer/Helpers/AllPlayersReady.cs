using CLUZServer.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CLUZServer.Helpers
{
    public class AllPlayersReady
    {
        #region AllPlayersReady
        public void Act(Game g)
        {
            //Game g = sender as Game;
            Log.Information("GamePool: All players 'Ready' in game '{game}'", g.Name);

            #region Kill Results
            foreach (Player p in g.Players.Values.ToList())
            {
                if (p.KillRequest == true)
                    p.Role = PlayerRole.Ghost;
            }
            #endregion

            //Votes should only by days
            if (IsDay(g) && g.TimeFrame >= 2 && g.Status == GameState.Locked)
            {
                #region Votes Results
                List<Player> playersSortedList = new List<Player>();

                if (g.Players.Count >= 2)
                {
                    //sort Players by Vote var
                    playersSortedList = g.Players.Values.ToList().OrderByDescending(o => o.VoteCount).ToList();
                    //assign kicked to first guid in sorted list
                    g.Players[playersSortedList[0].Guid].Role = PlayerRole.Kicked;
                }

                g.ResetVotes();

                Log.Information("Votes over. Kicked name {0}", g.Players[playersSortedList[0].Guid].Name);
                #endregion
            }

            if (g.TimeFrame == 0 && g.Status == GameState.Filled)
            {
                #region First Day to Night (Raffle)
                g.Status = GameState.Locked;
                g.ResetPlayersReadyState();
                g.Raffle();
                g.TimeFrame += 1;
                Log.Information("GamePool: Iterating Timeframe with Raffle in game '{0}' now is '{1}'", g.Name, g.TimeFrame);
                #endregion
            }
            //else if (g.TimeFrame == 1 && g.Status == GameState.Locked)
            //{
            //    #region First Night Iteration
            //    g.TimeFrame += 1;
            //    g.ResetPlayersReadyState();
            //    Log.Information("GamePool: Iterating Timeframe in game '{0}' now is '{1}'", g.Name, g.TimeFrame);
            //    #endregion
            //}
            else if (g.TimeFrame >= 1 && g.Status == GameState.Locked)
            {
                #region Regular Iteration
                g.ResetPlayersReadyState();
                g.TimeFrame += 1;
                Log.Information("GamePool: Iterating timeframe for '{game}'. Now is '{time}'", g.Name, g.TimeFrame);
                #endregion
            }

        }
        #endregion

        private bool IsDay(Game g)
        {
            int number = g.TimeFrame;

            if (number % 2 == 0)
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
