
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
        public bool LogIn()
        {
            int Port = 3040;
            tcpClient?.Close();
            tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect(IP, Port);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Ошибка подключения к серверу", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            bool compleat = false;
            networkStream = tcpClient.GetStream();

            System.Threading.Tasks.Task.Factory.StartNew(() => 
            {
                try
                {
                    var binF = new BinaryFormatter();
                    binF.Serialize(networkStream, objectTGS);
                    while (tcpClient.Connected)
                    {
                       var serverObj = binF.Deserialize(networkStream);
                       objectTGS = (T)serverObj;
                    }
                    
                }
                catch
                {
                    MessageBox.Show("");
                }
                Disconect();
            });
            return compleat;
        }
    }
}
