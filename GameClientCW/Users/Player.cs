﻿using GameClass.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GameClass.Users
{
    [Serializable]
    public class Player:User
    {
        private int id;
        private string nickName;
        private string email;
        private int rate;
        private TimeSpan inGameTime;
        private List<Item> items;
        private int coins;
        private double winRate;
        private List<Game> games;

        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }
        public string NickName
        {
            get
            {
                return nickName;
            }
            set
            {
                if (value.Length<16)
                {
                    nickName = value;
                }
            }
        }
        public string Email
        {
            get
            {
                return email;
            }
            set
            {
                if (IsValidEmail(value))
                {
                    email = value;
                }
            }
        }
        public int Rate
        {
            get
            {
                return rate;
            }
            set
            {
                rate = value;
            }
        }
        public TimeSpan InGameTime
        {
            get
            {
                return inGameTime;
            }
            set
            {
                inGameTime = value;
            }
        }
        public List<Item> Items
        {
            get
            {
                return items;
            }
            set
            {
                items = value;
            }
        }
        public double WinRate
        {
            get
            {
                return winRate;
            }
            set
            {
                winRate = value;
            }
        }
        public int Coins
        {
            get
            {
                return coins;
            }
            set
            {
                coins = value;
            }
        }
        public List<Game> Games
        {
            get
            {
                return games;
            }
            set
            {
                games = value;
            }
        }
        public int NumberOfMatch
        {
            get;set;
        }

        public Player(string log , string pass, string eml, int Id = default, string nick = default,  int rte = default, TimeSpan IGT = default, List<Item> itms = default, int cn = default, double wr = default, List<Game> gms = default, int numOfMatch = 0) : base(log,pass)
        {
            id = Id;
            email = eml;
            nickName = nick;
            rate = rte;
            inGameTime = IGT;
            items = itms;
            coins = cn;
            winRate = wr;
            games = gms;
            NumberOfMatch = numOfMatch;
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    string domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

    }
}
