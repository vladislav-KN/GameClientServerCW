
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GameClientCW
{
    public enum Ports
    {

        Register = 3000,
        Login = 3001,

        GetMap = 3010,
        GetItem = 3011,
        GetMod = 3012,
        GetParams = 3013,
        GetTasks = 3014,
        GetPlayerInfo = 3015,
        GetShop = 3016,

        UpdateMap = 3020,
        UpdateItem = 3021,
        UpdateMod = 3022,
        UpdateParams = 3023,
        UpdateTasks = 3024,

        AddMap = 3030,
        AddItem = 3031,
        AddMod = 3032,
        AddParams = 3033,
        AddTasks = 3034,

        DeleteMap = 3040,
        DeleteItem = 3041,
        DeleteMod = 3042,
        DeleteParams = 3043,
        DeleteTasks = 3044,

        DeleteMapMod = 3050,
        DeleteModParam = 3051,
        DeleteItemParam = 3052,
        DeleteItemInventory = 3053,

        UpdatePlayer = 3060,
        UpdateMatch = 3061,
        AddPlayerItem = 3062
       
    }
    public class TCPQuery<T>
    {
        private TcpClient tcpClient;
        private List<int> UN_RECIV_DATA_PORT_LIST = new List<int>() { 3040, 3030 };
        private NetworkStream networkStream;
        public string IP { get; set; }
        public T objectTGS {get;set;}
        public TCPQuery(string ip, T OTSG)
        {
            IP = ip;
            objectTGS = OTSG;
        }
        public void Disconect()
        {
            tcpClient?.Close();
        }
        public bool send(int Port)
        {
 
            tcpClient?.Close();
            tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect(IP, Port);
            }
            catch 
            {
                
                return false;
            }
            bool compleat = false;
            networkStream = tcpClient.GetStream();
            try
            {
                var binF = new BinaryFormatter();
                binF.Serialize(networkStream, objectTGS);

                var serverObj = binF.Deserialize(networkStream);
                objectTGS = (T)serverObj; 

                tcpClient?.Close();
                return true;
                
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
           
            return compleat;
        }
    }
}
