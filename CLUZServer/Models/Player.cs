using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using CLUZServer.Hubs;
using CLUZServer.Models;
using Microsoft.AspNetCore.SignalR;

namespace CLUZServer
{
    public class Player : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region Fileds
        private PlayerState _state = PlayerState.Idle;
        private PlayerRole _role = PlayerRole.None;
        private int _voteCount = 0;
        private string _name = "";
        #endregion

        #region Properties
        public bool PropChanged { get; set; } = false;

        public string ConnId { get; }

        public Guid Guid { get; }

        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                if (value != _name)
                {
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        [JsonIgnore]
        public bool KillRequest { get; set; }

        public int VoteCount
        {
            get
            {
                return _voteCount;
            }

            set
            {
                if (value != _voteCount)
                {
                    _voteCount = value;
                    NotifyPropertyChanged("VoteCount");
                }
            }
        }

        public PlayerState State
        {
            get
            {
                return _state;
            }

            set
            {
                if (value != _state)
                {
                    _state = value;
                    NotifyPropertyChanged("State");
                }
            }
        }

        public PlayerRole Role
        {

            get
            {
                return _role;
            }

            set
            {
                if (value != _role)
                {
                    _role = value;
                    NotifyPropertyChanged("Role");
                }
            }
        }
        #endregion

        public Player(string connId, string name, Guid guid)
        {
            ConnId = connId;
            Name = name;
            Guid = guid;
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropChanged = true;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
