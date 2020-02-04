using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using CLUZServer.Models;
using Microsoft.AspNetCore.SignalR;
using CLUZServer.Hubs;
using CLUZServer.Helpers;

namespace CLUZServer
{
    public class GamePool
    {
        PlayerPool _playerPool;
        public GamePool(PlayerPool playerPool)
        {
            _playerPool = playerPool;
        }

        public Dictionary<Guid, Game> Games { get; set; } = new Dictionary<Guid, Game>();
        public Guid AddGame(string name, string gamePing)
        {
            Game newGame = new Game(name, gamePing);

            newGame.OnAllReady += new EventHandler(AllPlayersReady.Handler);

            Games.Add(newGame.Guid, newGame);

            Log.Information("GamePool: New game added to game pool '{0}'", newGame.Name);

            return newGame.Guid;
        }

        #region FindPlayerInGame
        /// <summary>
        /// Search for Game to which Player belongs to
        /// </summary>
        /// <param name="pGuid">Player Guid</param>
        /// <returns>Game Guid</returns>
        public Guid FindTheGamePlayerBelongsTo(Guid pGuid)
        {
            try
            {
                foreach (Game g in Games.Values.ToList())
                {
                    Player p = g.Players.Values.ToList().Find(p => p.Guid == pGuid);
                    return g.Guid;
                }
                Log.Warning("Player {0} wasn't belong to any game", _playerPool.Players[pGuid].Name);
                return Guid.Empty;
            }
            catch
            {
                Log.Warning("Player {0} wasn't belong to any game", _playerPool.Players[pGuid].Name);
                return Guid.Empty;
            }

        }
        #endregion
    }
}