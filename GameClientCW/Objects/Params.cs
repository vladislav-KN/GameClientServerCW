using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GameClass.Objects
{
    [Serializable]
    public class Params
    {
        protected int ID;
        protected ItemParamsName param;
        protected string value;
        public string ValueID
        {
            get
            {
                return value;
            }
        }
    }
    public enum ItemParamsName
    {
        PhisicalDamage = 1,
        Stan,
        CriticalDMG,
        ChanceOfCriticalDamage,
        Hill,
        PeriodicDamage,
        Period
    }

}
