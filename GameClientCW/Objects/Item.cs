﻿ 
using System;
using System.Collections.Generic;

namespace GameClass.Objects
{
    [Serializable]
    public class Item  
    {
 
        public string Name 
        {
            get; set;
        }
        public string Discription
        {
            get; set;
        }
        public int ID {
            get; set;
        }
        public int Number
        {
            get; set;
        }
        public int Cost
        {
            get; set;
        }
        public List<Params> Parametrs
        {
            get; set;
        }
        public override string ToString()
        {
            return Name;
        }
    }
 
}