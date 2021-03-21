using System;
using GameClass.Users;
using NLog.Fluent;
using NLog.Internal;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace ServerClient
{



        class Program
        {
            static string connectionString = "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog = CWGameDB; Integrated Security = True";
            static List<string> hosts = new List<string>();
            public enum Ports
            {
                Update = 3000,
                Add = 3010,
                Delete = 3020,
                Register = 3030,
                Login = 3040
            }

            public static async Task HandleByPortNumber(TcpClient client, int portNumber)
            {
                var stream = client.GetStream();
                var binf = new BinaryFormatter();
                string sendStr;
                switch (portNumber)
                {
                    case (int)Ports.Login:
                        var LoginData = binf.Deserialize(stream) as User;
                         
                        break;
                    case (int)Ports.Register:
                        var RegisterData = binf.Deserialize(stream) as Player;
                        using (var con = new SqlConnection())
                        {
                            con.ConnectionString = connectionString;
                            con.Open();
                            string strSQL = @"select pl.Email, usr.[Login] from Player as pl join login_user as con 
    on pl.UserID = con.UserID join Users as usr 
    on con.[login] = usr.[Login] 
    where pl.Email = @eml OR usr.[Login] = @log;";
                            var cmd = new SqlCommand(strSQL, con);
                            cmd.Parameters.AddWithValue("@eml", RegisterData.Email);
                            cmd.Parameters.AddWithValue("@log", RegisterData.Login);
                            using (var dr = cmd.ExecuteReader())
                            {
                                if (dr.HasRows)
                                {
                                     binf.Serialize(stream, RegisterData);
                                     return;
                                }
                            }
                        strSQL = @"INSERT INTO Users (Login, pasword, Admin) values(@log, @pas, 0)";
                        cmd = new SqlCommand(strSQL, con);
                        cmd.Parameters.AddWithValue("@pas", RegisterData.Password);
                        cmd.Parameters.AddWithValue("@log", RegisterData.Login);
                        cmd.ExecuteNonQuery(); 
                        strSQL = @"INSERT INTO Users (Login, pasword, Admin) values(@log, @pas, 0)";
                        cmd = new SqlCommand(strSQL, con);
                        cmd.Parameters.AddWithValue("@pas", RegisterData.Password);
                        cmd.Parameters.AddWithValue("@log", RegisterData.Login);

                    }
                        
                    break;
                    case (int)Ports.Update:
                        break;
                    case (int)Ports.Add:
                        break;
                    case (int)Ports.Delete:
                        break;

                }



            }

            static async Task Main(string[] args)
            {
                
                try
                {
                    IPAddress ip;
                    if (!IPAddress.TryParse("127.0.0.1", out ip))
                    {
                        Console.WriteLine("Failed to get IP address, service will listen for client activity on all network interfaces.");
                        ip = IPAddress.Any;
                    }
                    IDictionary<Task<TcpClient>, Tuple<int, TcpListener>> tcpListeners = new Dictionary<Task<TcpClient>, Tuple<int, TcpListener>>();

                    foreach (int port in Enum.GetValues(typeof(Ports)))
                    {
                        var tcpListener = new TcpListener(ip, port);

                        tcpListener.Start();

                        var task = tcpListener.AcceptTcpClientAsync();
                        var tcpListenerPortPair = new Tuple<int, TcpListener>(port, tcpListener);

                        tcpListeners.Add(task, tcpListenerPortPair);
                    }

                    Task<TcpClient> tcpClientTask;

                    while ((tcpClientTask = await Task.WhenAny(tcpListeners.Keys)) != null)
                    {
                        var tcpListenerPortPair = tcpListeners[tcpClientTask];
                        var port = tcpListenerPortPair.Item1;
                        var tcpListener = tcpListenerPortPair.Item2;

                        tcpListeners.Remove(tcpClientTask);

                        // This needs to be async. What to do with its Task?
                        // It cannot be awaited here.
                        var handlerTask = HandleByPortNumber(tcpClientTask.Result, port);

                        var task = tcpListener.AcceptTcpClientAsync();

                        tcpListeners.Add(task, tcpListenerPortPair);
                    }
                }
                catch (Exception e)
                {
                    Log.Info(DateTime.Now.ToString("MM/dd/yyyy h:mm tt") + " Ошибка " + e.Message);
                }
            }
        }
    }
