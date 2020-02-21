﻿using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CLUZServer.Models;
using Serilog;
using CLUZServer.Helpers;

namespace CLUZServer.Hubs
{
    public class PlayersHub : Hub
    {
        #region fields
        GamePool _gamePool;
        PlayerPool _playerPool;
        IHubContext<PlayersHub> _hubContext;
        #endregion
        public PlayersHub(GamePool gamePool, PlayerPool playerPool, IHubContext<PlayersHub> hubContext)
        {
            _gamePool = gamePool;
            _playerPool = playerPool;
            _hubContext = hubContext;
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

            try
            {
                _playerPool.GetPlayerByConnectionId(Context.ConnectionId)?.ExcludeMySelfFromAnyGame(_gamePool);

                _playerPool.RemovePlayerFromPoolByConnID(Context.ConnectionId);

                Log.Information("Player '{connid}' disconnected event", Context.ConnectionId);
            }
            catch { }


            await base.OnDisconnectedAsync(exception);

            //if (exception == null)
            //{
            //    Guid pGuid = _playerPool.GetPlayerByConnectionId(Context.ConnectionId);

            //    if (pGuid != Guid.Empty)
            //    {
            //        Guid gGuid = _gamePool.FindTheGamePlayerBelongsTo(pGuid);

            //        if (gGuid != Guid.Empty)
            //        {
            //            Game g = _gamePool.Games[gGuid];
            //            Player p = _playerPool.Players[pGuid];

            //            if (!(p.Role == PlayerRole.Ghost || p.Role == PlayerRole.Kicked))
            //            {
            //                g.RemovePlayer(pGuid);
            //            }
            //            else
            //            {
            //                Log.Information("Player '{name}' disconnected, but he is '{role}' so leaving him in game and on server until game" +
            //                    "{name} ends", p.Name, p.Role.ToString(), g.Name);
            //            }

            //            //_gamePool.Games[gGuid].RemovePlayer(pGuid);

            //            //_playerPool.RemovePlayerFromPoolByConnID(Context.ConnectionId);

            //            //Log.Information("Player {p} successifuly removed from game {g} on disconnection event", pGuid, gGuid);
            //        }
            //    }
            //}
            //else
            //{
            //    Log.Error("Client '{0}' diconnected with exception '{1}'", Context.ConnectionId, exception.Message);
            //}
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

        #region RemovePlayerFromPool
        public void RemovePlayerFromPool()
        {
            _playerPool.RemovePlayerFromPoolByConnID(Context.ConnectionId);
        }
        #endregion

        #region CreateGame
        /// <summary>
        ///  Will fire up after client clicked Create Game button
        /// </summary>
        /// <param name="name">Game name from payload</param>
        /// <param name="gamePin">Game pin from payload</param>
        public async Task<Guid> CreateGame(string name, string gamePin, double minimum)
        {
            Guid newGameGuid = _gamePool.AddGame(name, gamePin, minimum);
            Log.Information("New game '{0}' guid '{1}' added to game pool", name, newGameGuid);

            //await Clients.Caller.SendAsync("GameGuid", newGameGuid);
            //Log.Information("Hub: Sent to caller his game giud '{newGameGuid}'");

            await Clients.All.SendAsync("RefreshGameList");
            Log.Information("Request to all clients to refresh game list sent");

            return newGameGuid;
        }
        #endregion

        #region AddPlayerToGame
        /// <summary>
        /// Will fire up after client clicked Join Game button
        /// </summary>
        /// <param name="playerGuid">Client Guid from payload</param>
        /// <param name="gameGuid">Guid of game chosen by client</param>
        /// <returns></returns>
        public void AddPlayerToGame(Guid playerGuid, Guid gameGuid)
        {
            Game game = _gamePool.Games[gameGuid];

            Player player = _playerPool.Players[playerGuid];

            game.AddPlayer(player);

            //adding player to Hub group named by game GUID
            await Groups.AddToGroupAsync(player.ConnId, game.Guid.ToString());
            Log.Information("Added player '{connid}'('{pname}') to group '{guid}' ('{gname}')", player.ConnId, player.Name,game.Guid, game.Name);

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
            try
            {
                Game g = _gamePool.Games[gameGuid];
                Player p = _playerPool.Players[playerGuid];

                g.RemovePlayer(playerGuid);

                //reset player states
                p.AllowedToVote = false;
                p.KillRequest = false;
                p.Role = PlayerRole.None;
                p.State = PlayerState.Idle;
                p.VoteCount = 0;

                Log.Information("Removed player '{name}' from game '{name}' by his will", p.Name, g.Name);

                await _hubContext.Clients.All.SendAsync("SnackbarMessage", $"'{p.Name}' left game by will", 5, g.Guid);
            }
            catch { }


            //try
            //{
            //    Game g = _gamePool.Games[gameGuid];
            //    Player p = _playerPool.Players[playerGuid];
            //    //if player was ghost or kicked, don't do anything
            //    if(p.Role == PlayerRole.Ghost || p.Role == PlayerRole.Kicked)
            //    {
            //        Log.Information("Player '{name}' is '{role}' so leaving him in game", p.Name, p.Role.ToString());
            //        p.State = PlayerState.Ready;
            //    }
            //    else
            //    {
            //        g.RemovePlayer(playerGuid);
            //        Log.Information("Removed player '{0}' from game '{1}'", playerGuid, g.Name);
            //    }

            //    //remove game from game pool (check for ghosts and kicked)
            //    //count alive players
            //    int alivePlayers = g.Players.Values.ToList().FindAll(p => p.Role != PlayerRole.Kicked && p.Role != PlayerRole.Ghost).Count;

            //    Log.Information("'{count}' alive players in '{game}'", alivePlayers, g.Name);

            //    if (g.GameHasEnded && alivePlayers == 0)
            //    {
            //        Log.Information("Game '{game}' has ended, removing from the pool", g.Name);
            //        g.ResetPlayers();
            //        _gamePool.Games.Remove(g.Guid);
            //        await Clients.All.SendAsync("RefreshGameList");
            //    }
            //}
            //catch (KeyNotFoundException)
            //{
            //    Log.Error("Player {name} wasn't found in dict of game {name}", playerGuid, gameGuid);
            //}

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
            catch (KeyNotFoundException e)
            {
                Log.Error(e.Message ,"Game '{guid}' not existing anymore", gameGuid);
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
        public async void VoteRequest(Guid fromGuid, Guid kickGuid, Guid gameGuid)
        {
            Game g = _gamePool.Games[gameGuid];

            Log.Information("Request from '{0}' to kick '{1}'", _playerPool.Players[fromGuid].Name, _playerPool.Players[kickGuid].Name);

            if (_playerPool.Players[kickGuid].Role != PlayerRole.Kicked || _playerPool.Players[kickGuid].Role != PlayerRole.Ghost)
            {
                _playerPool.Players[kickGuid].VoteCount += 1;
            }

            _playerPool.Players[fromGuid].HasVoted = true;

            Voting.AllowRandomPlayerToVote(g, _hubContext);

            //await _hubContext.Clients.All.SendAsync("SnackbarMessage", $"'{_playerPool.Players[fromGuid].Name}' voted to kick '{_playerPool.Players[kickGuid].Name}'", 5, g.Guid);
        }
        #endregion
    }
}
