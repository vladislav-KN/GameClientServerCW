using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClass.Objects
{
    
    public interface GameObj
    {
        public string Name { get; }
        public string Discription { get;  }
        public int ID { get;}
        public void Update();

    }
}
