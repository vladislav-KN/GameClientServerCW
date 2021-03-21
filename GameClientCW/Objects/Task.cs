using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GameClass.Objects
{
    [Serializable]
    public class Task : GameObj
    {
        protected string name, discription;
        protected List<Params> parametrs;
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
        public List<Params> Parametrs
        {
            get
            {
                return parametrs;
            }
        }

        public void Update()
        {
            throw new System.NotImplementedException();
        }
    }
}
