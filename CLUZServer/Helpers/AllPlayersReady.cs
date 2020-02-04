using CLUZServer.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CLUZServer.Helpers
{
    public static class AllPlayersReady
    {
        #region AllPlayersReady
        /// <summary>
        /// Algorithm function. Fired up when all players ready for next timeframe
        /// </summary>
        /// <param name="sender">Game generated event</param>
        /// <param name="e"></param>
        public static void Handler(object sender, EventArgs e)
        {
            Game g = sender as Game;
            Log.Information("GamePool: All players {0} in game '{1}'", "Ready", g.Name);

            #region Kill Results
            foreach (Player p in g.Players.Values.ToList())
            {
                if (p.KillRequest == true)
                    p.Role = PlayerRole.Ghost;
            }
            #endregion

            //should only by days
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
                g.TimeFrame += 1;
                g.Status = GameState.Locked;
                g.ResetPlayersReadyState();
                g.Raffle();
                Log.Information("GamePool: Iterating Timeframe with Raffle in game '{0}' now is '{1}'", g.Name, g.TimeFrame);
                #endregion
            }
            else if (g.TimeFrame == 1 && g.Status == GameState.Locked)
            {
                #region First Night Iteration
                g.TimeFrame += 1;
                g.ResetPlayersReadyState();
                Log.Information("GamePool: Iterating Timeframe in game '{0}' now is '{1}'", g.Name, g.TimeFrame);
                #endregion
            }
            else if (g.TimeFrame >= 2 && g.Status == GameState.Locked)
            {
                #region Regular Iteration
                g.TimeFrame += 1;
                g.ResetPlayersReadyState();
                Log.Information("GamePool: Iterating Timeframe in game '{0}' now is '{1}'", g.Name, g.TimeFrame);
                #endregion
            }

        }
        #endregion

        private static bool IsDay(Game g)
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
