 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GameClass.Objects
{
    [Serializable]
    public class Map  
    {
        public string Name
        {
            get; set;
        }
        public string Discription
        {
            get; set;
        }
        public int ID
        {
            get; set;
        }
        public List<Mod> Mods
        {
            get; set;
        }
        public string ImageSource
        {
            get;set;
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
