
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        AddPlayerItem = 3062,
        RemovePlayerItem = 3063,
        UserTaskCopleat = 3064,

        ReportsToFromUsers = 3070,
        ReportsItems = 3071,
        ReportsCastom = 3072
    }
    public class TCPQuery<T>
    {
        private TcpClient tcpClient;
        private List<int> UN_SEND_DATA_PORT_LIST = new List<int>() { 3070, 3071,3072 };
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
                if (UN_SEND_DATA_PORT_LIST.Contains(Port))
                {

                    StreamReader reader = new StreamReader(networkStream);

                    // The first message from the client is the file size    
                    string cmdFileSize = reader.ReadLine();

                    // The first message from the client is the filename    
                    string cmdFileName = reader.ReadLine();

                    int length = Convert.ToInt32(cmdFileSize);
                    byte[] buffer = new byte[length];
                    int received = 0;
                    int read = 0;
                    int size = 1024;
                    int remaining = 0; while (received < length)
                    {
                        remaining = length - received;
                        if (remaining < size)
                        {
                            size = remaining;
                        }

                        read = tcpClient.GetStream().Read(buffer, received, size);
                        received += read;
                    }

                    // Save the file using the filename sent by the client    
                    using (FileStream fStream = new FileStream(Path.GetFileName(cmdFileName), FileMode.Create))
                    {
                        fStream.Write(buffer, 0, buffer.Length);
                        fStream.Flush();
                        fStream.Close();
                        

                    }
                    Process.Start(Path.GetFileName(cmdFileName));
                }
                else {
                    var binF = new BinaryFormatter();

                    binF.Serialize(networkStream, objectTGS);

                    var serverObj = binF.Deserialize(networkStream);
                    objectTGS = (T)serverObj;

                    tcpClient?.Close();
                    return true; 
                }
                
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
           
            return compleat;
        }
    }
}
