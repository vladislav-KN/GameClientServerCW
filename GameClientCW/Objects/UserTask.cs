using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GameClass.Objects
{
    [Serializable]
    public class UserTask  
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
        public int Prize
        {
            get; set;
        }
        public override string ToString()
        {
            return Name;
        }

    }
}
