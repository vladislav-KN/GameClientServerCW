
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
    public class TCPQuery<T>
    {
        private TcpClient tcpClient;
        private List<int> RECIV_DATA_PORT_LIST = new List<int>() { 3040, 3030 };
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
                if (RECIV_DATA_PORT_LIST.Contains(Port))
                {
                    var serverObj = binF.Deserialize(networkStream);
                    objectTGS = (T)serverObj; 
                }
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
