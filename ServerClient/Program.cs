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
using System.Data;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;

namespace ServerClient
{



    class Program
    {
        static string connectionString = "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog = CWGameDB; Integrated Security = True";
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
                    try
                    {
                        using (var con = new SqlConnection())
                        {
                            con.ConnectionString = connectionString;
                            con.Open();
                            string strSQL = "select * from Users where [Login] = @log;";
                            var cmd = new SqlCommand(strSQL, con);
                            cmd.Parameters.AddWithValue("@log", LoginData.Login);

                            using (var dr = cmd.ExecuteReader())
                            {
                                if (dr.HasRows)
                                {

                                    while (dr.Read())
                                    {
                                        string pas = dr["pasword"].ToString();
                                        if (pas == LoginData.Password)
                                        {
                                            LoginData.IsAdmin = bool.Parse(dr["Admin"].ToString());

                                        }
                                        else
                                        {
                                            LoginData = new User("", "");
                                        }
                                    }

                                }
                                else
                                {
                                    LoginData = new User("1", "");
                                }
                            }
                             
                            binf.Serialize(stream, LoginData);


                            return;
                        }
                    }
                    catch
                    {
                        LoginData = new User("11", "");
                        binf.Serialize(stream, LoginData);
                    }
 
                    break;
                case (int)Ports.Register:
                    var RegisterData = binf.Deserialize(stream) as Player;
                    try
                    {
                        using (var con = new SqlConnection())
                        {
                            con.ConnectionString = connectionString;
                            con.Open();
                            string strSQL = @"select pl.Email, usr.[Login] from Player as pl join login_user as con on pl.UserID = con.UserID join Users as usr on con.[login] = usr.[Login] where pl.Email = @eml OR usr.[Login] = @log;";
                            var cmd = new SqlCommand(strSQL, con);
                            cmd.Parameters.AddWithValue("@eml", RegisterData.Email);
                            cmd.Parameters.AddWithValue("@log", RegisterData.Login);
                            using (var dr = cmd.ExecuteReader())
                            {
                                if (dr.HasRows)
                                {
                                    RegisterData.Coins = -2;
                                    binf.Serialize(stream, RegisterData);

                                    return;
                                }
                            }
                            strSQL = @"INSERT INTO Users ([Login], [pasword], [Admin]) values(@log, @pas, 0)";
                            cmd = new SqlCommand(strSQL, con);
                            cmd.Parameters.Add("@pas", SqlDbType.Int);
                            cmd.Parameters["@pas"].Value = RegisterData.Password;
                            cmd.Parameters.AddWithValue("@log", RegisterData.Login);
                            cmd.ExecuteNonQuery();
                            strSQL = @"INSERT INTO Player ([Email], [NickName]) values(@eml, @nic) SELECT @@IDENTITY AS [@@IDENTITY];";
                            cmd = new SqlCommand(strSQL, con);
                            string nick = RegisterData.Email.Remove(RegisterData.Email.IndexOf("@"));
                            cmd.Parameters.AddWithValue("@eml", RegisterData.Email);
                            cmd.Parameters.AddWithValue("@nic", RegisterData.NickName = nick.Length > 15 ? nick.Remove(14) : nick);

                            int index = 0;
                            using (var dr = cmd.ExecuteReader())
                            {
                                while (dr.Read())
                                {
                                    index = int.Parse(dr[0].ToString());
                                }
                            }


                            strSQL = @" INSERT INTO login_user (UserID, login) values(@uid, @log)";
                            cmd = new SqlCommand(strSQL, con);
                            cmd.Parameters.AddWithValue("@uid", index);
                            cmd.Parameters.AddWithValue("@log", RegisterData.Login);
                            cmd.ExecuteNonQuery();
                            Player newPlayer = new Player(RegisterData.Login, RegisterData.Password, RegisterData.Email, index, RegisterData.NickName, 0, new DateTime(0), new List<GameClass.Objects.Item>(), 0, 0, new List<Game>());
                            binf.Serialize(stream, newPlayer);

                            return;
                        }
                    }
                    catch
                    {
                        RegisterData.Coins = -1;
                        binf.Serialize(stream, RegisterData);
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
