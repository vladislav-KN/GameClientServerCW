using GameClass.Objects;
using System;
using System.Collections.Generic;

namespace GameClass.Users
{
    public class Game
    {
        private List<Player> playerList;
        private Player winer;
        private DateTime gameTime;
        private DateTime gameDate;
        private Map map;
        private Mod mod;

        List<Player> RemovePersonalData()
        {
            List<Player> matchData = new List<Player>();
            foreach (Player pl in playerList)
            {
                matchData.Add(new Player(pl.ID, "", "", pl.NickName, "", pl.Rate, pl.InGameTime, pl.Items, winer.Rate, pl.WinRate, pl.Games));
            }return matchData;
        }

        public List<Player> PlayerList 
        { 
            get 
            {
                return  RemovePersonalData();
            }
            set
            {
                playerList = value;
            }
        }
        public Player Winer 
        {
            get
            {
                return new Player(winer.ID, "", "", winer.NickName, "", winer.Rate, winer.InGameTime, winer.Items, winer.Rate, winer.WinRate, winer.Games);
            }
            set
            {
                winer = value;
            }
        }
        public DateTime GameTime
        {
            get
            {
                return gameTime;
            }
            set
            {
                gameTime = value;
            }
        }
        public DateTime GameDate
        {
            get
            {
                return gameDate;
            }
            set
            {
                gameDate = value;
            }
        }
        public Map Map
        {
            get
            {
                return map;
            }
            set
            {
                map = value;
            }
        }
        public Mod Mod
        {
            get
            {
                return mod;
            }
            set
            {
                mod = value;
            }
        }

        public Game(List<Player> PL, Player W, DateTime GT, DateTime GD, Map Mp, Mod Md)
        {
            playerList = PL;
            winer = W;
            gameTime = GT;
            gameDate = GD;
            map = Mp;
            mod = Md;
        }
    }
}