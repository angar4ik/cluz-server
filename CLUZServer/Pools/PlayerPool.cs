using Serilog;
using System;
using System.Collections.Generic;
using CLUZServer.Models;

namespace CLUZServer
{
    public class PlayerPool
    {
        
        /// <summary>
        /// Dictionary which holds all players on server
        /// </summary>
        public Dictionary<Guid, Player> Players { get; set; } = new Dictionary<Guid, Player>();

        /// <summary>
        /// Initialize player and insert him to Players dict
        /// </summary>
        /// <param name="connId">Context Connection Id given by SignalR</param>
        /// <param name="name">Name of player chosen in time of registation</param>
        /// <returns>New generated Guid which will be key to dict</returns>
        public Guid AddPlayerToPool(string connId, string name)
        {
            Player newPlayer = new Player(connId, name, Guid.NewGuid());

            Players.Add(newPlayer.Guid, newPlayer);

            return newPlayer.Guid;
        }

        /// <summary>
        /// Will remove player from dict by connid
        /// </summary>
        /// <param name="connId"></param>
        public void RemovePlayerFromPoolByConnID(string connId)
        {
            Player p = GetPlayerByConnectionId(connId);
            if (Players.ContainsKey(p.Guid))
            {
                Players.Remove(p.Guid);
            }
            
            Log.Information("PlayerPool: Player '{name}' linked with '{id}' has been removed from players pool", p.Name, connId);
        }

        /// <summary>
        /// Get Player Guid by ConnectionID
        /// </summary>
        /// <param name="ConnId">Connection Id to seach for</param>
        /// <returns>Returns Guid.Empty if not player found</returns>
        public Player GetPlayerByConnectionId(string ConnId)
        {
            if(Players.Count > 0)
            {
                foreach(KeyValuePair<Guid, Player> entry in Players)
                {
                    if(entry.Value.ConnId == ConnId)
                    {
                        return entry.Value;
                    }
                }
            }

            return null;
        }
    }
}
