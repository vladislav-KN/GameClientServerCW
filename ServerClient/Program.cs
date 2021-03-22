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
using GameClass.Objects;
using GameClientCW;

namespace ServerClient
{



    class Program
    {
        static string connectionString = "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog = CWGameDB; Integrated Security = True";

        static List<Item> shop = new List<Item>();
        public static async System.Threading.Tasks.Task HandleByPortNumber(TcpClient client, int portNumber)
        {
            var stream = client.GetStream();
            var binf = new BinaryFormatter();
            string sendStr;
            switch (portNumber / 10)
            {
                case 300:
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
                                    Player newPlayer = new Player(RegisterData.Login, RegisterData.Password, RegisterData.Email, index, RegisterData.NickName, 0, new TimeSpan(0), new List<GameClass.Objects.Item>(), 0, 0, new List<Game>());
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
                    }
                    break;
                case 301:
                    switch (portNumber)
                    {
                        case (int)Ports.GetMap:
                            var Maps = binf.Deserialize(stream) as List<Map>;
                            using (var con = new SqlConnection())
                            {
                                con.ConnectionString = connectionString;
                                con.Open();
                                string strSQL = "select* from Map;";
                                var cmd = new SqlCommand(strSQL, con);

                                using (var dr = cmd.ExecuteReader())
                                {
                                    while (dr.Read())
                                    {
                                        Map map = new Map();
                                        map.ID = int.Parse(dr["MapID"].ToString());
                                        map.Discription = dr["Discription"].ToString();
                                        map.Name = dr["Name"].ToString();
                                        map.ImageSource = dr["image"].ToString();
                                        Maps.Add(map);
                                    }

                                }

                                foreach(Map map in Maps)
                                {
                                    map.Mods = new List<Mod>();
                                    strSQL = "select Mods.[Name] as ModName, Mods.[ModID] as ModID from map_mod as con join Mods on con.[ModID] = Mods.[ModID] where con.[MapID] = @mapID";
                                    cmd = new SqlCommand(strSQL, con);
                                    cmd.Parameters.AddWithValue("@mapID", map.ID);
                                    using (var dr = cmd.ExecuteReader())
                                    {
                                        while (dr.Read())
                                        {
                                            Mod mod = new Mod();
                                            mod.ID = int.Parse(dr["ModID"].ToString());
                                            mod.Name = dr["ModName"].ToString();
                                            map.Mods.Add(mod);
                                        }

                                    }
                                }
                                binf.Serialize(stream, Maps);

                                return;
                            }
                            break;
                        case (int)Ports.GetMod:
                            var Mods = binf.Deserialize(stream) as List<Mod>;
                            using (var con = new SqlConnection())
                            {
                                con.ConnectionString = connectionString;
                                con.Open();
                                string strSQL = "select * from Mods;";
                                var cmd = new SqlCommand(strSQL, con);

                                using (var dr = cmd.ExecuteReader())
                                {
                                    while (dr.Read())
                                    {
                                        Mod mod = new Mod();
                                        mod.ID = int.Parse(dr["ModID"].ToString());
                                        mod.Discription = dr["Discription"].ToString();
                                        mod.Name = dr["Name"].ToString();
                                        Mods.Add(mod);
                                    }

                                }
                                binf.Serialize(stream, Mods);

                                return;
                            }
                            break;
                        case (int)Ports.GetPlayerInfo:
                            var userPlayer = binf.Deserialize(stream) as Player;
                            try
                            {
                                using (var con = new SqlConnection())
                                {
                                    con.ConnectionString = connectionString;
                                    con.Open();
                                    string strSQL = "SELECT Player.UserID, Player.Email, Player.NickName, Player.[Money],Player.[WinRate], Player.[NumOfMatch], Player.[Rate], Player.[GameTime] FROM [Users] join login_user as con  on  con.[login] = Users.[Login] join Player on con.UserID = Player.[UserID] WHERE Users.[Login] = @log;";
                                    var cmd = new SqlCommand(strSQL, con);
                                    cmd.Parameters.AddWithValue("@log", userPlayer.Login);
                                    using (var dr = cmd.ExecuteReader())
                                    {
                                        while (dr.Read())
                                        {
                                            userPlayer.ID = int.Parse(dr["UserID"].ToString());
                                            userPlayer.Email = dr["Email"].ToString();
                                            userPlayer.NickName = dr["NickName"].ToString();
                                            userPlayer.Coins = int.Parse(dr["Money"].ToString());
                                            userPlayer.WinRate = double.Parse(dr["WinRate"].ToString());
                                            userPlayer.Rate = int.Parse(dr["Rate"].ToString());
                                            userPlayer.InGameTime = (TimeSpan)dr["GameTime"];
                                        }

                                    }
                                    strSQL = "SELECT ItemID,Number From user_item as ui join Player con on ui.UserID = con.UserID Where con.UserID = @id";
                                    cmd = new SqlCommand(strSQL, con);
                                    cmd.Parameters.AddWithValue("@id", userPlayer.ID);
                                   
                                    userPlayer.Items = new List<Item>();
                                    using (var dr = cmd.ExecuteReader())
                                    {
                                        while (dr.Read())
                                        {
                                            Item item = new Item();
                                            item.ID = int.Parse(dr["ItemID"].ToString());
                                            item.Number = int.Parse(dr["Number"].ToString());

                                            userPlayer.Items.Add(item);

                                        }

                                    }
                                    foreach (Item it in userPlayer.Items)
                                    {
                                        strSQL = "SELECT * From Items Where ItemID = @id";
                                        cmd = new SqlCommand(strSQL, con);
                                        cmd.Parameters.AddWithValue("@id", it.ID);
                                       
                                        userPlayer.Items = new List<Item>();
                                        using (var dr = cmd.ExecuteReader())
                                        {
                                            while (dr.Read())
                                            {
                                                it.Name = dr["Name"].ToString();
                                                it.Discription = dr["Description"].ToString();
                                                it.Cost = int.Parse(dr["Value"].ToString());
                                            }

                                        }

                                        strSQL = "SELECT ParamID, Params.[Value], Params From Items join item_param as con on con.ItemID = Items.[ItemID] join Params on Params.ParamsID = con.ParamID  Where Items.ItemID = @id";
                                        cmd = new SqlCommand(strSQL, con);
                                        cmd.Parameters.AddWithValue("@id", it.ID);
                                        
                                        it.Parametrs = new List<Params>();
                                        using (var dr = cmd.ExecuteReader())
                                        {
                                            while (dr.Read())
                                            {

                                                Params param = new Params();
                                                param.ID = int.Parse(dr["ParamID"].ToString());
                                                param.iParam = (ItemParamsName)int.Parse(dr["Value"].ToString());
                                                param.value = dr["Params"].ToString();
                                                it.Parametrs.Add(param);
                                            }

                                        }
                                    }
                                    strSQL = "SELECT MatchID From Player join user_match con on Player.UserID = con.UserID Where Player.UserID = @id";
                                    cmd = new SqlCommand(strSQL, con);
                                    cmd.Parameters.AddWithValue("@id", userPlayer.ID);
                           
                                    userPlayer.Games = new List<Game>();
                                    using (var dr = cmd.ExecuteReader())
                                    {
                                        while (dr.Read())
                                        {
                                            Game Game = new Game();
                                            Game.GameId = int.Parse(dr["MatchID"].ToString());
                                            userPlayer.Games.Add(Game);

                                        }

                                    }
                                    foreach (Game gm in userPlayer.Games)
                                    {
                                        strSQL = "SELECT GameData, GameTime, Map.[image] as img, Map.[Name] as MapName, Mods.[Name] as ModName From [Match] join [match_ModMap] as con on con.matchID = [Match].MatchID join Map on con.MapID = Map.MapID join  Mods on Mods.ModID = con.ModID Where [Match].MatchID = @id";
                                        cmd = new SqlCommand(strSQL, con);
                                        cmd.Parameters.AddWithValue("@id", gm.GameId);
                                  
                                        userPlayer.Items = new List<Item>();
                                        using (var dr = cmd.ExecuteReader())
                                        {
                                            while (dr.Read())
                                            {
                                                gm.Map = new Map();
                                                gm.Map.Name = dr["MapName"].ToString();
                                                gm.Map.ImageSource = dr["img"].ToString();
                                                gm.ModName = dr["ModName"].ToString();
                                                gm.GameDate = (DateTime)dr["GameData"];
                                                gm.GameTime = (TimeSpan)dr["GameTime"];
                                            }

                                        }

                                        strSQL = "SELECT Player.NickName, con.Place from Player join user_match as con on con.UserID = Player.UserID Where con.MatchID = @id Order by con.Place;";
                                        cmd = new SqlCommand(strSQL, con);
                                        cmd.Parameters.AddWithValue("@id", gm.GameId);
                                   
                                        gm.PlayerList = new List<string>();
                                        using (var dr = cmd.ExecuteReader())
                                        {
                                            while (dr.Read())
                                            {
                                                gm.PlayerList.Add(dr["NickName"].ToString() + $" - #{dr["Place"]}");
                                            }

                                        }
                                    }
                                    binf.Serialize(stream, userPlayer);

                                    return;
                                }
                            }
                            catch
                            {
                                userPlayer.ID = -1;
                                binf.Serialize(stream, userPlayer);
                            }
                            break;
                        case (int)Ports.GetShop:
                            var sendShop = binf.Deserialize(stream) as List<Item>;
                            
                            binf.Serialize(stream, shop);
                            break;

                    }
                    break;
                case 302:
                    switch (portNumber)
                    {
                        case (int)Ports.UpdateMap:
                            var map = binf.Deserialize(stream) as Map;
                            
                            try
                            {
                                using (var con = new SqlConnection())
                                {
                                    con.ConnectionString = connectionString;
                                    con.Open();
                                    string strSQL = @"UPDATE Map SET Discription = @disc, [image] = @img, [Name] = @name WHERE [MapID] = @id;";
                                    var cmd = new SqlCommand(strSQL, con);

                                    cmd.Parameters.AddWithValue("@name", map.Name);
                                    cmd.Parameters.AddWithValue("@disc", map.Discription);
                                    cmd.Parameters.AddWithValue("@img", map.ImageSource);
                                    cmd.Parameters.AddWithValue("@id", map.ID);
                                    cmd.ExecuteNonQuery();
                                    if (map.Mods.Count>0)
                                    {
                                        strSQL = @"DELETE FROM map_mod WHERE [MapID] = @id;";
                                        cmd = new SqlCommand(strSQL, con);
                                        cmd.Parameters.AddWithValue("@id", map.ID);
                                        cmd.ExecuteNonQuery();
                                        foreach(Mod md in map.Mods)
                                        {
                                            strSQL = @"INSERT INTO  map_mod([MapID], [ModID]) VALUES (@idMap, @idMod);";
                                            cmd = new SqlCommand(strSQL, con);
                                            cmd.Parameters.AddWithValue("@idMap", map.ID);
                                            cmd.Parameters.AddWithValue("@idMod", md.ID);
                                            cmd.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                map.ID = -1;
                            }
                            binf.Serialize(stream, map);
                            break;

                    }
                    break;
                case 303:
                    switch (portNumber)
                    {
                        case (int)Ports.AddMap:
                            var Map = binf.Deserialize(stream) as Map; 
                            try
                            {
                                using (var con = new SqlConnection())
                                {
                                    con.ConnectionString = connectionString;
                                    con.Open();
                                    string strSQL = @" INSERT INTO Map([Name], [Discription], [image]) values(@name, @disc,@img) SELECT @@IDENTITY AS[@@IDENTITY];";
                                    var cmd = new SqlCommand(strSQL, con);

                                    cmd.Parameters.AddWithValue("@name", Map.Name);
                                    cmd.Parameters.AddWithValue("@disc", Map.Discription);
                                    cmd.Parameters.AddWithValue("@img", Map.ImageSource);
                                    int index = 0;
                                    using (var dr = cmd.ExecuteReader())
                                    {
                                        while (dr.Read())
                                        {
                                            index = int.Parse(dr[0].ToString());
                                        }
                                    }
                                    Map.ID = index;
                                }
                            } 
                            catch
                            {
                                Map.ID = -1;
                                
                            }
                            binf.Serialize(stream, Map);
                            break;
                    }
                    break;
                case 304:
                    switch (portNumber)
                    {
                        case (int)Ports.DeleteMap:
                            var Map = binf.Deserialize(stream) as Map;
                            try
                            {
                                using (var con = new SqlConnection())
                                {
                                    con.ConnectionString = connectionString;
                                    con.Open();
                                    string strSQL = @" DELETE FROM Map WHERE MapID = @ID";
                                    var cmd = new SqlCommand(strSQL, con);
                                    cmd.Parameters.AddWithValue("@ID", Map.ID);
                                    cmd.ExecuteNonQuery();
                                    Map.ID = 0;
                                }
                            }
                            catch
                            {
                                Map.ID = -1;

                            }
                            binf.Serialize(stream, Map);
                            break;
                    }
                    break;
                 case 305:
                    switch (portNumber)
                    {
                        case (int)Ports.DeleteMapMod:
                            
                            var MapMod = binf.Deserialize(stream) as List<int>;
                            try
                            {
                                using (var con = new SqlConnection())
                                {
                                    con.ConnectionString = connectionString;
                                    con.Open();
                                    string strSQL = @"DELETE FROM map_mod WHERE MapID = @maID AND ModID = @moID";
                                    var cmd = new SqlCommand(strSQL, con);
                                    cmd.Parameters.AddWithValue("@moID", MapMod[0]);
                                    cmd.Parameters.AddWithValue("@maID", MapMod[1]);
                                    cmd.ExecuteNonQuery();
                                    MapMod[0] = 0;
                                    MapMod[1] = 0;
                                }
                            }
                            catch
                            {
                                MapMod[0] = -1;
                                MapMod[1] = -2;

                            }
                            binf.Serialize(stream, MapMod);
                            break;
                    }
                    break;
                case 306:
                    switch (portNumber)
                    {
                        case (int)Ports.AddPlayerItem:

                            var addToInvent = binf.Deserialize(stream) as List<int>;
                            try
                            {
                                using (var con = new SqlConnection())
                                {
                                    con.ConnectionString = connectionString;
                                    con.Open();
                                    string strSQL = "SELECT [Money] From Player Where [UserID] = @uid ";
                                    var  cmd = new SqlCommand(strSQL, con);
                                    cmd.Parameters.AddWithValue("@uid", addToInvent[0]);
                                    int money = 0;
                                    using (var dr = cmd.ExecuteReader())
                                    {
                                        while (dr.Read())
                                        {
                                            money = int.Parse(dr[0].ToString());
                                        }
                                    }
                                    strSQL = "SELECT [Value] From Items Where [ItemID] = @iid ";
                                    cmd = new SqlCommand(strSQL, con);
                                    
                                    cmd.Parameters.AddWithValue("@iid", addToInvent[1]);
                                    int cost = 0;
                                    using (var dr = cmd.ExecuteReader())
                                    {
                                        while (dr.Read())
                                        {
                                            money = int.Parse(dr[0].ToString());
                                        }
                                    }
                                    if ((money - cost)<0)
                                    {
                                        addToInvent[0] = 0;
                                        addToInvent[1] = 0;
                                        binf.Serialize(stream, addToInvent);
                                        return;
                                    }
                                    strSQL = "UPDATE Player SET [Money] -=@cost";
                                    cmd.Parameters.AddWithValue("@cost", cost);
                                    cmd.ExecuteNonQuery();
                                    strSQL = @"MERGE user_item AS uiT 
USING ( SELECT CAST( GETDATE() AS DATE ) AS dt ) AS source
ON
  uiT.[ItemID] = @itmID and
  uiT.[UserID] = @usrID 
WHEN MATCHED THEN
   UPDATE SET uiT.Number +=1
WHEN NOT MATCHED BY TARGET THEN
   INSERT ([ItemID], [Number], [UserID])
   VALUES (1, 1, 2008); ";
                                     cmd = new SqlCommand(strSQL, con);
                                    cmd.Parameters.AddWithValue("@itmID", addToInvent[1]);
                                    cmd.Parameters.AddWithValue("@usrID ", addToInvent[0]);
                                    cmd.ExecuteNonQuery();
                                    addToInvent[0] = 0;
                                    addToInvent[1] = 1;
                                }
                            }
                            catch
                            {
                                addToInvent[0] = -1;
                                addToInvent[1] = -2;

                            }
                            binf.Serialize(stream, addToInvent);
                            break;
                    }
                    break;
            }
        }


        static void createShop()
        {
            using (var con = new SqlConnection())
            {
                con.ConnectionString = connectionString;
                con.Open();
                 
                string strSQL = @"SELECT * From Items;";
                var cmd = new SqlCommand(strSQL, con);
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        Item itm = new Item();
                        itm.Cost = int.Parse(dr["Value"].ToString());
                        itm.Discription = dr["Description"].ToString();
                        itm.Name = dr["Name"].ToString();
                        itm.ID = int.Parse(dr["ItemID"].ToString());
                        shop.Add(itm);
                    }

                }
                
            } 



            while (shop.Count > 5)
            {
                Random rnd = new Random();
                shop.RemoveAt(rnd.Next(0,shop.Count));
            }
            
            using (var con = new SqlConnection())
            {
                con.ConnectionString = connectionString;
                con.Open();
                foreach (Item it in shop)
                {
                    

                    string strSQL = "SELECT ParamID, Params.[Value], Params From Items join item_param as con on con.ItemID = Items.[ItemID] join Params on Params.ParamsID = con.ParamID  Where Items.ItemID = @id";
                    var cmd = new SqlCommand(strSQL, con);
                    cmd.Parameters.AddWithValue("@id", it.ID);

                    it.Parametrs = new List<Params>();
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {

                            Params param = new Params();
                            param.ID = int.Parse(dr["ParamID"].ToString());
                            param.iParam = (ItemParamsName)int.Parse(dr["Value"].ToString());
                            param.value = dr["Params"].ToString();
                            it.Parametrs.Add(param);
                        }

                    }
                }
            }
        }


        static async System.Threading.Tasks.Task Main(string[] args)
        {
            createShop();
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                while (true) 
                {
                    try
                    {
                        string str = Console.ReadLine();
                        if(str == "addadmin")
                        {
                            using (var con = new SqlConnection())
                            {
                                con.ConnectionString = connectionString;
                                con.Open();
                                Console.WriteLine("Введите имя пользователя");
                                string log = Console.ReadLine();
                                string strSQL = @"INSERT INTO Users ([Login], [pasword], [Admin]) values(@log, @pas, 1)";
                                var cmd = new SqlCommand(strSQL, con);
                                cmd.Parameters.AddWithValue("@log", log);
                                cmd.Parameters.Add("@pas", SqlDbType.Int);
                                Console.WriteLine("Введите пароль");
                               
                                string pass = Console.ReadLine();
                                 
                                cmd.Parameters["@pas"].Value = User.StringHashCode20(pass);
                               
                                cmd.ExecuteNonQuery();
                                Console.WriteLine("Успешно добавлено");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            });
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

                while ((tcpClientTask = await System.Threading.Tasks.Task.WhenAny(tcpListeners.Keys)) != null)
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
