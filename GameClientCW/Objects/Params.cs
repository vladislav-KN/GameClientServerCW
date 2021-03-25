 
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
 
        public int ID
        {
            get; set;
        }
        public ItemParamsName iParam
        {
            get; set;
        }
        public ModParamsName mParam
        {
            get; set;
        }
        public string value
        {
            get; set;
        }
        public override string ToString()
        {
            if (mParam != 0)
            {
                return mParam.ToString() + "\t" + value + "\n";
            }
            else
            {
                return iParam.ToString() + "\t" + value + "\n";
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
    public enum ModParamsName
    {
        Time = 1,
        NumberOfPaticipants,
        NumberOfMaps
         
    }
}
