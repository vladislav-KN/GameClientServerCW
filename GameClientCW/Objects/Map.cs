using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GameClass.Objects
{
    [Serializable]
    public class Map:GameObj
    {
        protected string name, discription;
        protected List<Mod> mods;
        protected int id;

        public string Name
        {
            get
            {
                return name;
            }
        }
        public string Discription
        {
            get
            {
                return discription;
            }
        }
        public int ID
        {
            get
            {
                return id;
            }
        }
        public List<Mod> Mods
        {
            get
            {
                return mods;
            }
        }

        public void Update()
        {
            throw new System.NotImplementedException();
        }
    }
}
