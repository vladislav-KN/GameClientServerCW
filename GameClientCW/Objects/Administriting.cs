using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClass.Objects
{
    public interface Administriting
    {
        public void Change(GameObj gameObj);
        public void Add(GameObj gameObj);
        public void Delete(GameObj gameObj);
    }
}
