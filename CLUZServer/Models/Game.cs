using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using CLUZServer.Models;
using CLUZServer.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CLUZServer
{
    public class Game
    {
        //public event EventHandler OnAllReady;
        [JsonIgnore]
        public DateTime ChangeTimeSpamp { get; set; } = DateTime.UtcNow;
        [JsonIgnore]
        public bool PropChanged { get; set; } = false;
        [JsonIgnore]
        public bool ListChanged { get; set; } = false;
        [JsonIgnore]
        public bool AllPlayersReady { get; set; } = false;
        [JsonIgnore]
        public bool GameHasEnded { get; set; } = false;

        private GameState _status = GameState.Unfilled;
        public GameState Status
        {
            get
            {
                return _status;
            }

            set
            {
                if (value != _status)
                {
                    _status = value;
                    GamePropertyChanged("GameState");
                }
            }
        }

        private int _minimumPlayersCount = 4;

        [JsonIgnore]
        public IDictionary<Guid, Player> Players { get; set; } = new Dictionary<Guid, Player>();

        public string Name { get; }

        public string GamePin { get; set; }

        public Guid Guid { get; }

        private int _timeFrame = 0;
        public int TimeFrame
        {
            get
            {
                return _timeFrame;
            }

            set
            {
                if (value != _timeFrame)
                {
                    _timeFrame = value;
                    GamePropertyChanged("TimeFrame");
                }
            }
        }

        public Game(string name, string gamePin, double minimum)
        {
            Guid = Guid.NewGuid();
            Name = name;
            GamePin = ComputeSha256Hash(gamePin);
            _minimumPlayersCount = (int)minimum;
        }

        private void PlayerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ChangeTimeSpamp = DateTime.UtcNow;

            if (Players.Values.ToList().TrueForAll(p => p.State == PlayerState.Ready))
            {
                //OnAllReady?.Invoke(this, new EventArgs());
                //set a flag all players ready
                AllPlayersReady = true;
                //Log.Information("All players ready in '{game}'", Name);
            }
        }

        private void GamePropertyChanged(string propName)
        {
            ChangeTimeSpamp = DateTime.UtcNow;

            PropChanged = true;
        }

        /// <summary>
        /// Will add player object to game players dict and will check game fullfilment
        /// </summary>
        /// <param name="player">Player object</param>
        public void AddPlayer(Player player)
        {
            if(Status != GameState.Locked)
            {
                //GamePropertyChanged("Players");

                player.PropertyChanged += PlayerPropertyChanged;

                Players.Add(player.Guid, player);

                CheckGameFulfillment();

                ListChanged = true;
                

                Log.Information("Game: Player '{0}' added to the game '{1}'", player.Name, this.Name);
            }
            else
            {
                Log.Information("Can't allow add player {0} to game {1}. Game is locked", player.Name, this.Name);
            }
        }

        /// <summary>
        /// Will remove player from players dict by guid key and will check game fullfillment
        /// </summary>
        /// <param name="playerGuid"></param>
        public void RemovePlayerFromGame(Guid playerGuid)
        {
            if (Players.Remove(playerGuid))
            {
                //GamePropertyChanged("Players");

                CheckGameFulfillment();

                ListChanged = true;

                //Log.Information("Game: Player '{0}' left game '{1}'", _playerPool.Players[playerGuid].Name, this.Name);
            }
            else
            {
                //Log.Warning("Game: Attempt to remove player '{0}' from game '{1}' UNSUCCESSFUL", _playerPool.Players[playerGuid].Name, this.Name);
            }
        }

        private void CheckGameFulfillment()
        {
            if(Players.Count < _minimumPlayersCount)
            {
                Status = GameState.Unfilled;

                TimeFrame = 0;

                ResetPlayers();
            }
            else if (Players.Count >= _minimumPlayersCount)
            {
                Status = GameState.Filled;
            }
        }

        public void Raffle()
        {
            int count = Players.Count; //4

            Random random = new Random();

            int mafia = random.Next(0, count); //3

            HashSet<int> exclude = new HashSet<int>() { mafia };

            int police = GiveMeANumber(0, count, exclude);

            for (int i = 0; i < Players.Count; i++)
            {
                if(i == mafia)
                {
                    Players.ElementAt(i).Value.Role = PlayerRole.Mafia;
                }
                else if(i == police)
                {
                    Players.ElementAt(i).Value.Role = PlayerRole.Police;
                }
                else
                {
                    Players.ElementAt(i).Value.Role = PlayerRole.Citizen;
                }
            }
        }

        public void ResetPlayers()
        {
            foreach(Player p in this.Players.Values.ToList())
            {
                p.Role = PlayerRole.None;
                p.State = PlayerState.Idle;
                ResetVotes();
            }
        }

        public void ResetVotes()
        {
            foreach (Player p in this.Players.Values.ToList())
            {
                p.VoteCount = 0;
            }
        }

        public void ResetPlayersReadyState()
        {
            foreach (Player p in this.Players.Values.ToList())
            {
                if(!(p.Role == PlayerRole.Ghost || p.Role == PlayerRole.Kicked))
                {
                    p.State = PlayerState.Idle;
                }
                    
            }
        }

        private int GiveMeANumber(int min, int max, HashSet<int> exclude)
        {
            //var exclude = new HashSet<int>() { 5, 7, 17, 23 };

            var range = Enumerable.Range(min, max).Where(i => !exclude.Contains(i));

            var rand = new Random();
            int index = rand.Next(min, max - exclude.Count);
            return range.ElementAt(index);
        }

        static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
