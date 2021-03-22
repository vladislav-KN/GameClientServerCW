using GameClass.Objects;
using System;
using System.Collections.Generic;

namespace GameClass.Users
{
    [Serializable]
    public class Game
    {
        
        public int GameId { get; set; }
        public List<string> PlayerList 
        {
            get;
            set;
        }
        public string Player 
        {
            get;
            set;
        }
        public TimeSpan GameTime
        {
            get;
            set;
        }
        public DateTime GameDate
        {
            get;
            set;
        }
        public Map Map
        {
            get;
            set;
        }
        public string ModName
        {
            get;
            set;
        }
         
    }
}