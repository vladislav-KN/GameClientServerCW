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
using OfficeOpenXml;
using System.Reflection;
 
 


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
                        case (int)Ports.GetTasks:
                            var tasks = binf.Deserialize(stream) as List<UserTask>;
                            using (var con = new SqlConnection())
                            {
                                con.ConnectionString = connectionString;
                                con.Open();
                                string strSQL = "select* from Tasks;";
                                var cmd = new SqlCommand(strSQL, con);

                                using (var dr = cmd.ExecuteReader())
                                {
                                    while (dr.Read())
                                    {
                                        UserTask task = new UserTask();
                                        task.ID = int.Parse(dr["TaskID"].ToString());
                                        task.Discription = dr["Discription"].ToString();
                                        task.Name = dr["Name"].ToString();
                                        task.Prize = int.Parse(dr["Prize"].ToString());
                                        tasks.Add(task);
                                    }

                                }

                                 
                                binf.Serialize(stream, tasks);

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
                                foreach (Mod mod in Mods)
                                {
                                    mod.Parametrs = new List<Params>();
                                    strSQL = "select * from Mod_param as con join Params on con.[ParamID] = Params.[ParamsID] where con.[ModID] = @mapID";
                                    cmd = new SqlCommand(strSQL, con);
                                    cmd.Parameters.AddWithValue("@mapID", mod.ID);
                                    using (var dr = cmd.ExecuteReader())
                                    {
                                        while (dr.Read())
                                        {
                                            Params param = new Params();
                                            param.ID = int.Parse(dr["ParamsID"].ToString());
                                            param.mParam = (ModParamsName)int.Parse(dr["Value"].ToString());
                                            param.value = dr["Params"].ToString();
                                            mod.Parametrs.Add(param);
                                        }

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
                        case (int)Ports.GetItem:
                            var items = binf.Deserialize(stream) as List<Item>;
                            using (var con = new SqlConnection())
                            {
                                con.ConnectionString = connectionString;
                                con.Open();
                                string strSQL = "select * from Items;";
                                var cmd = new SqlCommand(strSQL, con);

                                using (var dr = cmd.ExecuteReader())
                                {
                                    while (dr.Read())
                                    {
                                        Item item = new Item();
                                        item.ID = int.Parse(dr["ItemID"].ToString());
                                        item.Cost = int.Parse(dr["Value"].ToString());
                                        item.Discription = dr["Description"].ToString();
                                        item.Name = dr["Name"].ToString();
                                        items.Add(item);
                                    }

                                }
                                foreach (Item itm in items)
                                {
                                    itm.Parametrs = new List<Params>();
                                    strSQL = "select * from item_param as con join Params on con.[ParamID] = Params.[ParamsID] where con.[ItemID] = @mapID"; 
                                    cmd = new SqlCommand(strSQL, con);
                                    cmd.Parameters.AddWithValue("@mapID", itm.ID);
                                    using (var dr = cmd.ExecuteReader())
                                    {
                                        while (dr.Read())
                                        {
                                            Params param = new Params();
                                            param.ID = int.Parse(dr["ParamsID"].ToString());
                                            param.iParam = (ItemParamsName)int.Parse(dr["Value"].ToString());
                                            param.value = dr["Params"].ToString();
                                            itm.Parametrs.Add(param);
                                        }

                                    }
                                }
                                binf.Serialize(stream, items);

                                return;
                            }
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
                        case (int)Ports.UpdateMod:
                            var mod = binf.Deserialize(stream) as Mod;

                            try
                            {
                                using (var con = new SqlConnection())
                                {
                                    con.ConnectionString = connectionString;
                                    con.Open();
                                    string strSQL = @"UPDATE Mods SET Discription = @disc, [Name] = @name WHERE [ModID] = @id;";
                                    var cmd = new SqlCommand(strSQL, con);

                                    cmd.Parameters.AddWithValue("@name", mod.Name);
                                    cmd.Parameters.AddWithValue("@disc", mod.Discription);
                                    cmd.Parameters.AddWithValue("@id", mod.ID);
                                    cmd.ExecuteNonQuery();
                                    if (mod.Parametrs.Count > 0)
                                    {
                                        strSQL = @"DELETE FROM Mod_param WHERE [ModID] = @id;";
                                        cmd = new SqlCommand(strSQL, con);
                                        cmd.Parameters.AddWithValue("@id", mod.ID);
                                        cmd.ExecuteNonQuery();
                                        foreach (Params md in mod.Parametrs)
                                        {
                                            if (md.ID != -1) 
                                            {
                                                strSQL = @" UPDATE Params SET [Value] = @param, [Params] = @value MERGE WHERE [ParamsID] = @prid;";
                                                cmd = new SqlCommand(strSQL, con);
                                                cmd.Parameters.AddWithValue("@prid", md.ID);
                                            }
                                            else
                                            {
                                                strSQL = @" INSERT INTO  Params([Value], [Params]) VALUES (@param, @value) SELECT @@IDENTITY AS [@@IDENTITY];";
                                                cmd = new SqlCommand(strSQL, con);

                                            }
        
                                            cmd.Parameters.AddWithValue("@param", md.mParam);
                                            cmd.Parameters.AddWithValue("@value", md.value); 
                                            if (md.ID != -1)
                                                cmd.ExecuteNonQuery();
                                            else
                                            {
                                                using (var dr = cmd.ExecuteReader())
                                                {
                                                    while (dr.Read())
                                                    {
                                                        md.ID = int.Parse(dr[0].ToString());
                                                    }
                                                }
                                            }
                                            strSQL = @"INSERT INTO  Mod_param([ModID], [ParamID]) VALUES (@idMod, @idParam);";
                                            cmd = new SqlCommand(strSQL, con);
                                            cmd.Parameters.AddWithValue("@idMod", mod.ID);
                                            cmd.Parameters.AddWithValue("@idParam", md.ID);
                                            cmd.ExecuteNonQuery();

                                        }
                                    }
                                }
                            }
                            catch
                            {
                                mod.ID = -1;
                            }
                            binf.Serialize(stream, mod);
                            break;
                        case (int)Ports.UpdateItem:
                            var item = binf.Deserialize(stream) as Item;

                            try
                            {
                                using (var con = new SqlConnection())
                                {
                                    con.ConnectionString = connectionString;
                                    con.Open();
                                    string strSQL = @"UPDATE Items SET [Description] = @disc, [Name] = @name, [Value] = @val WHERE [ItemID] = @id;";
                                    var cmd = new SqlCommand(strSQL, con);

                                    cmd.Parameters.AddWithValue("@name", item.Name);
                                    cmd.Parameters.AddWithValue("@disc", item.Discription);
                                    cmd.Parameters.AddWithValue("@id", item.ID);
                                    cmd.Parameters.AddWithValue("@val", item.Cost);
                                    cmd.ExecuteNonQuery();
                                    if (item.Parametrs.Count > 0)
                                    {
                                        strSQL = @"DELETE FROM item_param WHERE [ItemID] = @id;";
                                        cmd = new SqlCommand(strSQL, con);
                                        cmd.Parameters.AddWithValue("@id", item.ID);
                                        cmd.ExecuteNonQuery();
                                        foreach (Params md in item.Parametrs)
                                        {
                                            if (md.ID != -1)
                                            {
                                                strSQL = @" UPDATE Params SET [Value] = @param, [Params] = @value WHERE [ParamsID] = @prid;";
                                                cmd = new SqlCommand(strSQL, con);
                                                cmd.Parameters.AddWithValue("@prid", md.ID);
                                            }
                                            else
                                            {
                                                strSQL = @" INSERT INTO  Params([Value], [Params]) VALUES (@param, @value) SELECT @@IDENTITY AS [@@IDENTITY];";
                                                cmd = new SqlCommand(strSQL, con);

                                            }

                                            cmd.Parameters.AddWithValue("@param", (int)md.iParam);
                                            cmd.Parameters.AddWithValue("@value", md.value);
                                            if (md.ID != -1)
                                                cmd.ExecuteNonQuery();
                                            else
                                            {
                                                using (var dr = cmd.ExecuteReader())
                                                {
                                                    while (dr.Read())
                                                    {
                                                        md.ID = int.Parse(dr[0].ToString());
                                                    }
                                                }
                                            }
                                            strSQL = @"INSERT INTO  item_param([ItemID], [ParamID]) VALUES (@idItem, @idParam);";
                                            cmd = new SqlCommand(strSQL, con);
                                            cmd.Parameters.AddWithValue("@idItem", item.ID);
                                            cmd.Parameters.AddWithValue("@idParam", md.ID);
                                            cmd.ExecuteNonQuery();

                                        }
                                    }
                                }
                            }
                            catch
                            {
                                item.ID = -1;
                            }
                            binf.Serialize(stream, item);
                            break;
                        case (int)Ports.UpdateTasks:
                            var task = binf.Deserialize(stream) as UserTask;

                            try
                            {
                                using (var con = new SqlConnection())
                                {
                                    con.ConnectionString = connectionString;
                                    con.Open();
                                    string strSQL = @"UPDATE Tasks SET Discription = @disc, [Prize] = @img, [Name] = @name WHERE [TaskID] = @id;";
                                    var cmd = new SqlCommand(strSQL, con);

                                    cmd.Parameters.AddWithValue("@name", task.Name);
                                    cmd.Parameters.AddWithValue("@disc", task.Discription);
                                    cmd.Parameters.AddWithValue("@img", task.Prize);
                                    cmd.Parameters.AddWithValue("@id", task.ID);
                                    cmd.ExecuteNonQuery();
                                     
                                }
                            }
                            catch
                            {
                                task.ID = -1;
                            }
                            binf.Serialize(stream, task);
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
                        case (int)Ports.AddItem:
                            var item = binf.Deserialize(stream) as Item;
                            try
                            {
                                using (var con = new SqlConnection())
                                {
                                    con.ConnectionString = connectionString;
                                    con.Open();
                                    string strSQL = @" INSERT INTO Items([Name], [Description], [Value]) values(@name, @disc,@val) SELECT @@IDENTITY AS[@@IDENTITY];";
                                    var cmd = new SqlCommand(strSQL, con);

                                    cmd.Parameters.AddWithValue("@name", item.Name);
                                    cmd.Parameters.AddWithValue("@disc", item.Discription);
                                    cmd.Parameters.AddWithValue("@val", item.Cost);
                                    int index = 0;
                                    using (var dr = cmd.ExecuteReader())
                                    {
                                        while (dr.Read())
                                        {
                                            index = int.Parse(dr[0].ToString());
                                        }
                                    }
                                    item.ID = index;
                                }
                            }
                            catch
                            {
                                item.ID = -1;

                            }
                            binf.Serialize(stream, item);
                            break;
                        case (int)Ports.AddMod:
                            var mod = binf.Deserialize(stream) as Mod;
                            try
                            {
                                using (var con = new SqlConnection())
                                {
                                    con.ConnectionString = connectionString;
                                    con.Open();
                                    string strSQL = @" INSERT INTO Mods([Name], [Discription]) values(@name, @disc) SELECT @@IDENTITY AS[@@IDENTITY];";
                                    var cmd = new SqlCommand(strSQL, con);

                                    cmd.Parameters.AddWithValue("@name", mod.Name);
                                    cmd.Parameters.AddWithValue("@disc", mod.Discription);
 
                                    int index = 0;
                                    using (var dr = cmd.ExecuteReader())
                                    {
                                        while (dr.Read())
                                        {
                                            index = int.Parse(dr[0].ToString());
                                        }
                                    }
                                    mod.ID = index;
                                }
                            }
                            catch
                            {
                                mod.ID = -1;

                            }
                            binf.Serialize(stream, mod);
                            break;
                        case (int)Ports.AddTasks:
                            var task = binf.Deserialize(stream) as UserTask;
                            try
                            {
                                using (var con = new SqlConnection())
                                {
                                    con.ConnectionString = connectionString;
                                    con.Open();
                                    string strSQL = @" INSERT INTO Tasks([Name], [Discription], [Prize]) values(@name, @disc,@piz) SELECT @@IDENTITY AS[@@IDENTITY];";
                                    var cmd = new SqlCommand(strSQL, con);

                                    cmd.Parameters.AddWithValue("@name", task.Name);
                                    cmd.Parameters.AddWithValue("@disc", task.Discription);
                                    cmd.Parameters.AddWithValue("@piz", task.Prize);
                                    int index = 0;
                                    using (var dr = cmd.ExecuteReader())
                                    {
                                        while (dr.Read())
                                        {
                                            index = int.Parse(dr[0].ToString());
                                        }
                                    }
                                    task.ID = index;
                                }
                            }
                            catch
                            {
                                task.ID = -1;

                            }
                            binf.Serialize(stream, task);
                            break;
                    }
                    break;
                case 304:
                    switch (portNumber)
                    {
                        case (int)Ports.DeleteMap:
                            var Map = (int)binf.Deserialize(stream);
                            try
                            {
                                using (var con = new SqlConnection())
                                {
                                    con.ConnectionString = connectionString;
                                    con.Open();
                                    string strSQL = @" DELETE FROM Map WHERE MapID = @ID";
                                    var cmd = new SqlCommand(strSQL, con);
                                    cmd.Parameters.AddWithValue("@ID", Map);
                                    cmd.ExecuteNonQuery();
                                    Map = 0;
                                }
                            }
                            catch
                            {
                                Map = -1;

                            }
                            binf.Serialize(stream, Map);
                            break;
                        case (int)Ports.DeleteItem:
                            var item = (int)binf.Deserialize(stream);
                            try
                            {
                                using (var con = new SqlConnection())
                                {
                                    con.ConnectionString = connectionString;
                                    con.Open();
                                    string strSQL = @" DELETE FROM Items WHERE ItemID = @ID";
                                    var cmd = new SqlCommand(strSQL, con);
                                    cmd.Parameters.AddWithValue("@ID", item);
                                    cmd.ExecuteNonQuery();
                                    item = 0;
                                }
                            }
                            catch
                            {
                                item = -1;

                            }
                            binf.Serialize(stream, item);
                            break;
                        case (int)Ports.DeleteMod:
                            var mod = (int)binf.Deserialize(stream);
                            try
                            {
                                using (var con = new SqlConnection())
                                {
                                    con.ConnectionString = connectionString;
                                    con.Open();
                                    string strSQL = @" DELETE FROM Mods WHERE ModID = @ID";
                                    var cmd = new SqlCommand(strSQL, con);
                                    cmd.Parameters.AddWithValue("@ID", mod);
                                    cmd.ExecuteNonQuery();
                                    mod = 0;
                                }
                            }
                            catch
                            {
                               mod = -1;

                            }
                            binf.Serialize(stream, mod);
                            break;
                        case (int)Ports.DeleteTasks:
                            var task = (int)binf.Deserialize(stream);
                            try
                            {
                                using (var con = new SqlConnection())
                                {
                                    con.ConnectionString = connectionString;
                                    con.Open();
                                    string strSQL = @"DELETE FROM Tasks WHERE TaskID = @ID";
                                    var cmd = new SqlCommand(strSQL, con);
                                    cmd.Parameters.AddWithValue("@ID", task);
                                    cmd.ExecuteNonQuery();
                                    task = 0;
                                }
                            }
                            catch
                            {
                                task = -1;

                            }
                            binf.Serialize(stream, task);
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
                        case (int)Ports.DeleteItemParam:

                            var itemParam = binf.Deserialize(stream) as List<int>;
                            try
                            {
                                using (var con = new SqlConnection())
                                {
                                    con.ConnectionString = connectionString;
                                    con.Open();
                                    string strSQL = @"DELETE FROM item_param  WHERE ParamID = @pid AND ItemID = @iid";
                                    var cmd = new SqlCommand(strSQL, con);
                                    cmd.Parameters.AddWithValue("@iid", itemParam[0]);
                                    cmd.Parameters.AddWithValue("@pid", itemParam[1]);
                                    cmd.ExecuteNonQuery();
                                    itemParam[0] = 0;
                                    itemParam[1] = 0;
                                }
                            }
                            catch
                            {
                                itemParam[0] = -1;
                                itemParam[1] = -2;

                            }
                            binf.Serialize(stream, itemParam);
                            break;
                        case (int)Ports.DeleteModParam:

                            var modParam = binf.Deserialize(stream) as List<int>;
                            try
                            {
                                using (var con = new SqlConnection())
                                {
                                    con.ConnectionString = connectionString;
                                    con.Open();
                                    string strSQL = @" DELETE FROM Mod_param  WHERE ParamID = @pid AND ModID = @iid";
                                    var cmd = new SqlCommand(strSQL, con);
                                    cmd.Parameters.AddWithValue("@iid", modParam[0]);
                                    cmd.Parameters.AddWithValue("@pid", modParam[1]);
                                    cmd.ExecuteNonQuery();
                                    modParam[0] = 0;
                                    modParam[1] = 0;
                                }
                            }
                            catch
                            {
                                modParam[0] = -1;
                                modParam[1] = -2;

                            }
                            binf.Serialize(stream, modParam);
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
                                            cost = int.Parse(dr[0].ToString());
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
                                    cmd = new SqlCommand(strSQL, con);
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
   VALUES (@itmID, 1, @usrID); ";
                                     cmd = new SqlCommand(strSQL, con);
                                    cmd.Parameters.AddWithValue("@itmID", addToInvent[1]);
                                    cmd.Parameters.AddWithValue("@usrID ", addToInvent[0]);
                                    cmd.ExecuteNonQuery();
                                    strSQL = "INSERT INTO purchase([Date],[Sum]) VALUES(@date, @sum) SELECT @@IDENTITY AS [@@IDENTITY];";
                                    cmd = new SqlCommand(strSQL, con);
                                    cmd.Parameters.AddWithValue("@date", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@sum", cost);
                                    int ind = 0;
                                    using (var dr = cmd.ExecuteReader())
                                    {
                                        while (dr.Read())
                                        {
                                            ind = int.Parse(dr[0].ToString());
                                        }
                                    }
                                    strSQL = "INSERT INTO user_purchase([PurchaseID],[userID]) VALUES(@pind, @uind);"; 
                                    cmd = new SqlCommand(strSQL, con);
                                    cmd.Parameters.AddWithValue("@pind", ind);
                                    cmd.Parameters.AddWithValue("@uind", addToInvent[0]);
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
                        case (int)Ports.RemovePlayerItem:

                            var removePlayerItem = binf.Deserialize(stream) as List<int>;
                            try
                            {
                                using (var con = new SqlConnection())
                                {
                                    con.ConnectionString = connectionString;
                                    con.Open();
                                    string strSQL = "SELECT [Number] From user_item Where [UserID] = @uid and [ItemID] = @iid";
                                    var cmd = new SqlCommand(strSQL, con);
                                    cmd.Parameters.AddWithValue("@uid", removePlayerItem[0]);
                                    cmd.Parameters.AddWithValue("@iid", removePlayerItem[1]);
                                    int num = 0;
                                    using (var dr = cmd.ExecuteReader())
                                    {
                                        while (dr.Read())
                                        {
                                            num = int.Parse(dr[0].ToString());
                                        }
                                    }

                                    if ((num - 1) > 0)
                                    {
                                        strSQL = "UPDATE user_item SET [Number] -=1 Where [UserID] = @uid and [ItemID] = @iid";
                                         
                                        removePlayerItem[1] = 1;
                                    }
                                    else if (num - 1 == 0)
                                    {
                                        strSQL = "DELETE FROM user_item Where [UserID] = @uid and [ItemID] = @iid";
                                       
                                        removePlayerItem[1] = 0;
                                    }
                                    else
                                    {
                                        removePlayerItem[0] = 0;
                                        removePlayerItem[1] = 2;
                                        binf.Serialize(stream, removePlayerItem);
                                        return;
                                    }
                                    cmd = new SqlCommand(strSQL, con);
                                    cmd.Parameters.AddWithValue("@uid", removePlayerItem[0]);
                                    cmd.Parameters.AddWithValue("@iid", removePlayerItem[1]);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            catch
                            {
                                removePlayerItem[0] = -1;
                                removePlayerItem[1] = -1;

                            }
                            binf.Serialize(stream, removePlayerItem);
                            break;
                        case (int)Ports.UserTaskCopleat:
                            var userTaskCompl = binf.Deserialize(stream) as List<int>;
                            try
                            {
                                using (var con = new SqlConnection())
                                {
                                    con.ConnectionString = connectionString;
                                    con.Open();
                                    string strSQL = "SELECT [Prize] From Tasks Where [TaskID] = @tid";
                                    var cmd = new SqlCommand(strSQL, con);
                                    cmd.Parameters.AddWithValue("@tid", userTaskCompl[0]);
                                    
                                    int num = 0;
                                    using (var dr = cmd.ExecuteReader())
                                    {
                                        while (dr.Read())
                                        {
                                            num = int.Parse(dr[0].ToString());
                                        }
                                    }
                                    strSQL = "UPDATE Player SET [Money] += @mon Where [UserID] = @uid";
                                    cmd = new SqlCommand(strSQL, con);
                                    cmd.Parameters.AddWithValue("@uid", userTaskCompl[1]);
                                    cmd.Parameters.AddWithValue("@iid", num);
                                    cmd.ExecuteNonQuery();
                                    userTaskCompl[0] = 0;
                                    userTaskCompl[1] = 0;
                                }
                            }
                            catch
                            {
                                userTaskCompl[0] = -1;
                                userTaskCompl[1] = -1;

                            }
                            binf.Serialize(stream, userTaskCompl);
                            break;
                    }
                    break;
                case 307:
                    switch (portNumber)
                    {
                        case  (int)Ports.ReportsToFromUsers:
                            try {
                                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                                
                                using (var con = new SqlConnection())
                                {
                                    
                                    con.ConnectionString = connectionString;
                                    con.Open();
                                    string strSQL = @"SELECT * FROM Player LEFT JOIN user_purchase as UP on UP.userID = Player.UserID JOIN purchase as pur on UP.PurchaseID = pur.PurchaseID
Right Join user_item as ui on Player.UserID = ui.UserID
Right Join Items as itm on itm.ItemID = ui.ItemID
WHERE YEAR(pur.[Date]) <= YEAR(SYSDATETIME()); ";
                                    var cmd = new SqlCommand(strSQL, con);
                                    using (var dr = cmd.ExecuteReader())
                                    {
                                         
                                        FileInfo path = new FileInfo("rep"+DateTime.Now.ToString().Replace(" ","_").Replace(":", "_").Replace(".", "_") + ".xlsx");
                                        if (path.Exists)
                                        {
                                            path.Delete();

                                        }
                                        ExcelPackage np = new ExcelPackage(path);
                                        np.Workbook.Properties.Author = "DataBase";
                                        np.Workbook.Properties.Title = "Отчет";
                                        np.Workbook.Properties.Subject = "Отчет";
                                        ExcelWorksheet worksheet = np.Workbook.Worksheets.Add("Отчет");
                                        int numField = dr.FieldCount;
                                        for (int k = 0; k < numField; k++)
                                        {
                                            worksheet.Cells[1, k + 1].Value = Convert.ToString(dr.GetName(k));
                                        }
                                        int integer = 0;
                                        double floatnum = 0.0;
                                        int i = 0;
                                        while (dr.Read())
                                        {
                                            for (int j = 0; j < numField; j++)
                                            {
                                                if (int.TryParse(Convert.ToString(dr[j]), out integer))
                                                {
                                                    worksheet.Cells[i + 2, j + 1].Value = integer;
                                                    worksheet.Cells[i + 2, j + 1].Style.Numberformat.Format = "0";
                                                }
                                                else if (double.TryParse(Convert.ToString(dr[j]), out floatnum))
                                                {
                                                    worksheet.Cells[i + 2, j + 1].Value = floatnum;
                                                    worksheet.Cells[i + 2, j + 1].Style.Numberformat.Format = "#,###0.00";
                                                }
                                                else
                                                {
                                                    worksheet.Cells[i + 2, j + 1].Value = Convert.ToString(dr[j]);
                                                }

                                            }
                                            i++;
                                        }
                                        np.Save();
                                        StreamWriter sWriter = new StreamWriter(stream);

                                        byte[] bytes = File.ReadAllBytes(path.Name);
                                            
                                        sWriter.WriteLine(bytes.Length.ToString());
                                        sWriter.Flush();

                                        sWriter.WriteLine(path.Name);
                                        sWriter.Flush();

                                        Console.WriteLine("Sending file");
                                        client.Client.SendFile(path.Name);
                                        path.Delete();


                                    } 
                                }
                            }
                            catch(Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
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
