using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CLUZServer.Models;
using Serilog;

namespace CLUZServer.Hubs
{
    public class PlayersHub : Hub
    {
        GamePool _gamePool;
        PlayerPool _playerPool;
        public PlayersHub(GamePool gamePool, PlayerPool playerPool)
        {
            _gamePool = gamePool;
            _playerPool = playerPool;
        }

        #region OnConnectedAsync
        public override Task OnConnectedAsync()
        {

            Log.Information("Client '{0}' connected", Context.ConnectionId);

            return base.OnConnectedAsync();
        }
        #endregion

        #region OnDisconnectedAsync
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Log.Information("OnDisconnected: Removing player connid '{0}'", Context.ConnectionId);

            if (exception == null)
            {
                Guid pGuid = _playerPool.GetPlayerGuidByConnectionId(Context.ConnectionId);
                
                if (pGuid != Guid.Empty)
                {
                    Guid gGuid = Guid.Empty;

                    gGuid = _gamePool.FindTheGamePlayerBelongsTo(pGuid);

                    if (gGuid != Guid.Empty)
                    {
                        _gamePool.Games[gGuid].RemovePlayerFromGame(pGuid);

                        Log.Information("Player {p} successifuly removed from game {g}", pGuid, gGuid);
                    }
                }
            }
            else
            {
                Log.Error("Client '{0}' diconnected with exception '{1}'", Context.ConnectionId, exception.Message);
            }

            _playerPool.RemovePlayerFromPool(Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }
        #endregion

        #region InitPlayer
        /// <summary>
        /// Will fire up after client entered his name and pressed "Registration"
        /// </summary>
        /// <param name="name">Name as payload from client</param>
        public Guid InitPlayer(string name)
        {
            Guid addedPlayerGuid = _playerPool.AddPlayerToPool(Context.ConnectionId, name);
            Log.Information("New player '{0}' guid '{1}' added to player pool", name, addedPlayerGuid);

            return addedPlayerGuid;
        }
        #endregion

        #region CreateGame
        /// <summary>
        ///  Will fire up after client clicked Create Game button
        /// </summary>
        /// <param name="name">Game name from payload</param>
        /// <param name="gamePin">Game pin from payload</param>
        public async void CreateGame(string name, string gamePin)
        {
            Guid newGameGuid = _gamePool.AddGame(name, gamePin);
            Log.Information("New game '{0}' guid '{1}' added to game pool", name, newGameGuid);

            //await Clients.Caller.SendAsync("GameGuid", newGameGuid);
            //Log.Information("Hub: Sent to caller his game giud '{newGameGuid}'");

            await Clients.All.SendAsync("RefreshGameList");
            Log.Information("Request to all clients to refresh game list sent");
        }
        #endregion

        #region AddPlayerToGame
        /// <summary>
        /// Will fire up after client clicked Join Game button
        /// </summary>
        /// <param name="playerGuid">Client Guid from payload</param>
        /// <param name="gameGuid">Guid of game chosen by client</param>
        /// <returns></returns>
        public async Task AddPlayerToGame(Guid playerGuid, Guid gameGuid)
        {
            Game game = _gamePool.Games[gameGuid];

            Player player = _playerPool.Players[playerGuid];

            game.AddPlayer(player);

            ////adding player to Hub group named by game GUID
            //await Groups.AddToGroupAsync(player.ConnId, game.Guid.ToString());
            //Log.Information("Added player '{0}' to group '{1}'", player.ConnId, game.Guid.ToString());

            //List<Player> players = GamePool.Games[gameGuid].Players.Values.ToList();
            //await Clients.Group(game.Guid.ToString()).SendAsync("RefreshPlayerList", players);
            //Log.Information("Request to refresh players sent to game '{0}'", game.Name);
        }
        #endregion

        #region PlayerNameExistsInPool
        public async Task<bool> PlayerNameExistsInPool(string name)
        {

            Dictionary<Guid, Player> players = _playerPool.Players;

            if (players.Count > 0)
            {
                foreach (KeyValuePair<Guid, Player> entry in players)
                {
                    if (entry.Value.Name == name)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region GameNameExistsInPool
        public async Task<bool> GameNameExistsInPool(string name)
        {

            Dictionary<Guid, Game> games = _gamePool.Games;

            if (games.Count > 0)
            {
                foreach (KeyValuePair<Guid, Game> entry in games)
                {
                    if (entry.Value.Name == name)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region GetAllGames
        public async Task<List<Game>> GetAllGames()
        {
            try
            {
                List<Game> result = _gamePool.Games.Values.ToList();
                return result;
            }
            catch
            {
                Log.Information("No games found, returning empty list");
                return new List<Game>();
            }
        }
        #endregion

        // ******************** GAME PART ********************** //

        //#region RemovePlayerFromHubGroup
        //public async Task RemovePlayerFromHubGroup(Guid playerGuid, Guid groupGuid)
        //{
        //    string connId = _playerPool.Players[playerGuid].ConnId;
        //    await Groups.RemoveFromGroupAsync(connId, groupGuid.ToString());
        //    Log.Information("Removed '{0}' player from HubGroup '{1}' (with equal to game)", connId, groupGuid);
        //}
        //#endregion

        #region RemovePlayerFromGame
        /// <summary>
        /// Will fire up after client clicked Leave game or OnDisconnect event
        /// </summary>
        /// <param name="playerGuid">Player Guid from payload</param>
        /// <param name="gameGuid">Game Guid from payload</param>
        public async void RemovePlayerFromGame(Guid playerGuid, Guid gameGuid)
        {
            Game g = _gamePool.Games[gameGuid];
            g.RemovePlayerFromGame(playerGuid);
            Log.Information("Removed player '{0}' from game '{1}'", playerGuid, g.Name);

            if (g.GameHasEnded && g.Players.Count == 0)
            {
                Log.Information("Game '{game}' has ended, removing from the pool", g.Name);
                _gamePool.Games.Remove(g.Guid);
                await Clients.All.SendAsync("RefreshGameList");
            }

        }
        #endregion

        #region GetGameByGuid
        public async Task<Game> GetGameByGuid(Guid gameGuid)
        {
            Game result = _gamePool.Games[gameGuid];
            return result;
        }
        #endregion

        #region PlayerStatusUpdate
        /// <summary>
        /// User request to update his state. Will trigger group update for their players list 
        /// </summary>
        /// <param name="playerState">New state</param>
        /// <param name="PlayerGuid">Player Guid</param>
        /// <param name="gameGuid">Game Guid</param>
        public void PlayerStatusUpdate(PlayerState playerState, Guid PlayerGuid, Guid gameGuid)
        {
            try
            {
                Game g = _gamePool.Games[gameGuid];

                _playerPool.Players[PlayerGuid].State = playerState;
                Log.Information("Updated state for player '{0}' in game '{1}' to '{2}'", _playerPool.Players[PlayerGuid].Name, _gamePool.Games[gameGuid].Name, playerState);
            }
            catch (KeyNotFoundException)
            {
                Log.Error("Game '{guid}' not existing anymore", gameGuid);
            }
            
            //List<Player> players = GamePool.Games[gameGuid].Players.Values.ToList();
            //await Clients.Group(gameGuid.ToString()).SendAsync("RefreshPlayerList", players);
            //Log.Information("Request to refresh players list sent to group '{0}'", GamePool.Games[gameGuid].Name);
        }
        #endregion

        #region KillRequest
        public void KillRequest(Guid fromGuid, Guid killGuid, Guid gameGuid)
        {
            Game g = _gamePool.Games[gameGuid];

            if (g.Players[fromGuid].Role == PlayerRole.Mafia)
            {
                Log.Information("Request from '{0}' to kill '{1}'", _playerPool.Players[fromGuid].Name, _playerPool.Players[killGuid].Name);
                g.Players[killGuid].KillRequest = true;
            }
        }
        #endregion

        #region VoteRequest
        public void VoteRequest(Guid fromGuid, Guid kickGuid, Guid gameGuid)
        {
            Game g = _gamePool.Games[gameGuid];

            Log.Information("Request from '{0}' to kick '{1}'", _playerPool.Players[fromGuid].Name, _playerPool.Players[kickGuid].Name);

            if (_playerPool.Players[kickGuid].Role != PlayerRole.Kicked || _playerPool.Players[kickGuid].Role != PlayerRole.Ghost)
            {
                _playerPool.Players[kickGuid].VoteCount += 1;
            }
        }
        #endregion
    }
}
