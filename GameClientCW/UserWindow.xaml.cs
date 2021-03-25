

using GameClientCW.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GameClass.Users;
using GameClass.Objects;
using OfficeOpenXml;
using System.IO;
 

namespace GameClientCW
{
    public interface IMainWindowsCodeBehind
    {
    /// <summary>
        /// Показ сообщения для пользователя
        /// </summary>
        /// <param name="message">текст сообщения</param>
        void ShowMessage(string message);

        /// <summary>
        /// Загрузка нужной View
        /// </summary>
        /// <param name="view">экземпляр UserControl</param>
        void LoadView(ViewType typeView);
    }
    public enum ViewType
    {
        MapList,
        Profile,
        Second
    }
    /// <summary>
    /// Interaction logic for UserWindow.xaml
    /// </summary>
    public partial class UserWindow : Window
    {
       
        private MapInfo selectedChlMap;
        private ShopItem selectedChlItm;
        private ItemInfo selectedChlItem;
        private ItemView selectedChlitemView;
        private ModInfo selectedChlMod;
        private TaskInfo selectedTask;

        private List<Map> maps = new List<Map>(); 
        private List<Mod> mods = new List<Mod>();
        private List<Item> shopItems = new List<Item>();
        private List<Item> Items = new List<Item>();
        private List<UserTask> Task = new List<UserTask>();
        public User user
        {
            get; set;
        }
        public Player player
    {
            get;set;
        }
        public UserWindow()
        {
            MainWindow login = new MainWindow();
            bool? isClosed = login.ShowDialog();
            if (isClosed == true)
            {
                Close();
                
                return;
            }
            if(!(login.usr is null))
            {
                user = new User(login.usr);
            }else if(!(login.pl is null))
            {
                user = (User)login.pl;
                player = login.pl;
            } 
            InitializeComponent();
           
            this.Loaded += MainWindow_Load;
            
        }

        private void MainWindow_Load(object sender, RoutedEventArgs e)
        {
            if (user.IsAdmin)
            {
               
                MapLoad();

            }
            else
            {
                btnReps.Visibility = Visibility.Hidden;
                ProfileLoad();
            }
        }

       

        #region MapQuery
        private void MapLoad()
        {
            TCPQuery<List<Map>> query = new TCPQuery<List<Map>>("127.0.0.1", new List<Map>());
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                if (maps.Count == 0 || user.IsAdmin)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        if (TheGrid.Children.Count > 0)
                        {
                            TheGrid.Children.Clear();
                            TheGrid.RowDefinitions.Clear();
                            RowDefinition gridRow1 = new RowDefinition();
                            gridRow1.Height = GridLength.Auto;
                            TheGrid.RowDefinitions.Add(gridRow1);
                        }
                    });
                    bool normalExt = query.send((int)Ports.GetMap);
                    maps = query.objectTGS;
                }
                if (user.IsAdmin)
                {

                    this.Dispatcher.Invoke(() =>
                    {
                        var Add = new Add();
                        TheGrid.Children.Add(Add);
                        Add.ButtonFechar.Click += AddNewMap;
                       
                        Grid.SetRow(Add, TheGrid.RowDefinitions.Count - 1);
                        int every3 = 1;
                        foreach (Map map in maps)
                        {
                            if (every3 == 3)
                            {
                                RowDefinition gridRow1 = new RowDefinition();
                                gridRow1.Height = GridLength.Auto;
                                TheGrid.RowDefinitions.Add(gridRow1);
                                every3 = 0;
                            }

                            var uc = new MapInfo();
                            uc.RemoveMap.Click += RemoveMap;
                            uc.RemoveMod.Click += RemoveModFromMap;
                            uc.UpdateMap.Click += UpdateMap;
                            uc.AddMod.Click += AddModeToMap;
                            uc.cumap = map;
                            TheGrid.Children.Add(uc);
                            Grid.SetColumn(uc, every3);
                            Grid.SetRow(uc, TheGrid.RowDefinitions.Count - 1);
                            every3++;
                        }
                    });
                }
                else
                {
                    this.Dispatcher.Invoke(() =>
                    {

                        int every3 = 0;
                        foreach (Map map in maps)
                        {
                            if (every3 == 3)
                            {
                                RowDefinition gridRow1 = new RowDefinition();
                                gridRow1.Height = GridLength.Auto;
                                TheGrid.RowDefinitions.Add(gridRow1);
                                every3 = 0;
                            }

                            var uc = new MapViewer();
                            uc.cumap = map;
                            TheGrid.Children.Add(uc);
                            Grid.SetColumn(uc, every3);
                            Grid.SetRow(uc, TheGrid.RowDefinitions.Count - 1);
                            every3++;
                        }
                    });
                }
            });
        }
     
        void AddNewMap(object sender, EventArgs e)
        {
            AddMapWindow add = new AddMapWindow();

            Hide();
            bool? res = add.ShowDialog();
            if (res == true)
            {
                MapLoad();
                Show();
            }
        }
        void AddModeToMap(object sender, EventArgs e)
        {
            if (selectedChlMap is null)
                return;
            AddModToMapWindow addModToMapWindow = new AddModToMapWindow(selectedChlMap.cumap);
            Hide();
            bool? res = addModToMapWindow.ShowDialog();
            if (res == true)
            {
                MapLoad();
                Show();
            }
        }
        void RemoveModFromMap(object sender, EventArgs e)
        {
            if (selectedChlMap is null)
                return;
            var obj = selectedChlMap.combo.SelectedItem as Mod;
            if(obj is null)
                return;
            TCPQuery<List<int>> tCPQuery = new TCPQuery<List<int>>("127.0.0.1", new List<int> { obj.ID, selectedChlMap.cumap.ID});
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                bool compleat = tCPQuery.send((int)Ports.DeleteMapMod);
                if (compleat)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        MapLoad();
                    });
                }
            });
        }
        void UpdateMap(object sender, EventArgs e)
        {
            if (selectedChlMap is null)
                return;
            Map uMap = selectedChlMap.cumap;

            uMap.Discription = selectedChlMap.MapDiscriptution.Text;
            uMap.Name = selectedChlMap.MapName.Text;
            TCPQuery<Map> tCPQuery = new TCPQuery<Map>("127.0.0.1", uMap);
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                bool compleat = tCPQuery.send((int)Ports.UpdateMap);
                if (compleat)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        MapLoad();
                    });
                }
            });
        }
        void RemoveMap(object sender, EventArgs e)
        {
            if (selectedChlMap is null)
                return;
            TCPQuery<int> tCPQuery = new TCPQuery<int>("127.0.0.1", selectedChlMap.cumap.ID);
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                bool compleat = tCPQuery.send((int)Ports.DeleteMap);
                if (compleat)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        MapLoad();
                    });
                }
            });
        }
        #endregion
        #region Profile
        private void ProfileLoad()
        {
            TCPQuery<Player> query = new TCPQuery<Player>("127.0.0.1",  new Player(user.Login,user.Password,""));
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                if (!user.IsAdmin)
                {

                    this.Dispatcher.Invoke(() =>
                    {
                        if (TheGrid.Children.Count > 0)
                        {
                            TheGrid.Children.Clear();
                            TheGrid.RowDefinitions.Clear();
                            RowDefinition gridRow1 = new RowDefinition();
                            gridRow1.Height = GridLength.Auto;
                            TheGrid.RowDefinitions.Add(gridRow1);
                        }
                    });
                    if (player is null) 
                    { 
                        bool normalExt = query.send((int)Ports.GetPlayerInfo);
                        player = query.objectTGS;
                    }
                    this.Dispatcher.Invoke(() =>
                    {
                        var Add = new ProfStats();
                        Add.Player = player;
                        TheGrid.Children.Add(Add);
                        
                        Grid.SetRow(Add, TheGrid.RowDefinitions.Count - 1);
                        int every3 = 1;
                        foreach (Game gm in player.Games)
                        {
                            if (every3 == 3)
                            {
                                RowDefinition gridRow1 = new RowDefinition();
                                gridRow1.Height = GridLength.Auto;
                                TheGrid.RowDefinitions.Add(gridRow1);
                                every3 = 0;
                            }

                            var uc = new Match();
                            gm.Player = player.NickName;
                            uc.Game = gm;
                            TheGrid.Children.Add(uc);
                            Grid.SetColumn(uc, every3);
                            Grid.SetRow(uc, TheGrid.RowDefinitions.Count - 1);
                            every3++;
                        }
                    });
                }
                else
                {
                    Player player = new Player("","","",0, user.Login);
                    var Add = new ProfStats();
                    Add.Player = player;

                    Grid.SetRow(Add, TheGrid.RowDefinitions.Count - 1);

                }
            });
        }

        #endregion
        #region Shop
        private void ShopLoad()
        {
            TCPQuery<List<Item>> query = new TCPQuery<List<Item>>("127.0.0.1", shopItems);
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
 
                    this.Dispatcher.Invoke(() =>
                    {
                        if (TheGrid.Children.Count > 0)
                        {
                            TheGrid.Children.Clear();
                            TheGrid.RowDefinitions.Clear();
                            RowDefinition gridRow1 = new RowDefinition();
                            gridRow1.Height = GridLength.Auto;
                            TheGrid.RowDefinitions.Add(gridRow1);
                        }
                    });
                    if (shopItems.Count==0 )
            {
                        bool normalExt = query.send((int)Ports.GetShop);
                        shopItems = query.objectTGS;
            }
                    this.Dispatcher.Invoke(() =>
                    {
                        
                        int every3 = 0;
                        foreach (Item itm in shopItems)
                        {
                            if (every3 == 3)
            {
                                RowDefinition gridRow1 = new RowDefinition();
                                gridRow1.Height = GridLength.Auto;
                                TheGrid.RowDefinitions.Add(gridRow1);
                                every3 = 0;
                            }

                            var uc = new ShopItem();
                            uc.Item = itm;
                            uc.buy.Click += buyItem_Click;
                            TheGrid.Children.Add(uc);
                            Grid.SetColumn(uc, every3);
                            Grid.SetRow(uc, TheGrid.RowDefinitions.Count - 1);
                            every3++;
            }
                    });
                
            });
        }
        private void buyItem_Click(object sender, RoutedEventArgs e)
        {
            
            if (selectedChlItm is null)
                return;
            var obj = selectedChlItm.Item as Item;
            if (obj is null)
                return;
            TCPQuery<List<int>> tCPQuery = new TCPQuery<List<int>>("127.0.0.1", new List<int>() {player.ID, obj.ID});
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                bool compleat = tCPQuery.send((int)Ports.AddPlayerItem);

                this.Dispatcher.Invoke(() =>
                {
                    if (compleat)
                        switch (tCPQuery.objectTGS[1])
                        {
                            case 0:
                                MessageBox.Show("На счету недостаточно денег для покупки", "Невозможно купить предмет", MessageBoxButton.OK, MessageBoxImage.Information);
                                break;
                            case 1:
                                if (player.Items.Contains(obj))
                                {
                                    player.Items[player.Items.IndexOf(obj)].Number += 1;
                                }
                                else
                                {
                                    obj.Number = 1;
                                    player.Items.Add(obj);
                                }
                                break;
                            case -2:
                                MessageBox.Show("Ошибка запроса", "Невозможно купить предмет", MessageBoxButton.OK, MessageBoxImage.Information);
                                break;
                        }
                    else
                        MessageBox.Show("Сервер временно недоступен", "Невозможно купить предмет", MessageBoxButton.OK, MessageBoxImage.Information);
                });
            });
        }
        #endregion
        #region Invent 
        private void InventLoad()
        {
            TCPQuery<List<Item>> query = new TCPQuery<List<Item>>("127.0.0.1", new List<Item>());
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (TheGrid.Children.Count > 0)
                    {
                        TheGrid.Children.Clear();
                        TheGrid.RowDefinitions.Clear();
                        RowDefinition gridRow1 = new RowDefinition();
                        gridRow1.Height = GridLength.Auto;
                        TheGrid.RowDefinitions.Add(gridRow1);
                    }
                });
                if ( user.IsAdmin)
                {
                   
                    bool normalExt = query.send((int)Ports.GetItem);
                    Items = query.objectTGS;
                }
                if (user.IsAdmin)
                {

                    this.Dispatcher.Invoke(() =>
                    {
                        var Add = new Add();
                        TheGrid.Children.Add(Add);
                        Add.ButtonFechar.Click += AddNewItem;

                        Grid.SetRow(Add, TheGrid.RowDefinitions.Count - 1);
                        int every3 = 1;
                        foreach (Item it in Items)
                        {
                            if (every3 == 3)
                            {
                                RowDefinition gridRow1 = new RowDefinition();
                                gridRow1.Height = GridLength.Auto;
                                TheGrid.RowDefinitions.Add(gridRow1);
                                every3 = 0;
                            }

                            var uc = new ItemInfo();
                            uc.RemoveThisItem.Click += RemoveItem;
                            uc.RemoveParam.Click += RemoveParamFromItem;
                            uc.UpdateItem.Click += UpdateItem;
                            uc.AddParam.Click += AddParamToItem;
                            uc.Item = it;
                            TheGrid.Children.Add(uc);
                            Grid.SetColumn(uc, every3);
                            Grid.SetRow(uc, TheGrid.RowDefinitions.Count - 1);
                            every3++;
                        }
                    });
                }
                else
                {
                    this.Dispatcher.Invoke(() =>
                    {

                        int every3 = 0;
                        foreach (Item it in player.Items)
                        {
                            if (every3 == 3)
                            {
                                RowDefinition gridRow1 = new RowDefinition();
                                gridRow1.Height = GridLength.Auto;
                                TheGrid.RowDefinitions.Add(gridRow1);
                                every3 = 0;
                            }

                            var uc = new ItemView();
                            uc.btnUse.Click += useItem_Click;
                            uc.Item = it;
                            TheGrid.Children.Add(uc);
                            Grid.SetColumn(uc, every3);
                            Grid.SetRow(uc, TheGrid.RowDefinitions.Count - 1);
                            every3++;
                        }
                    });
                }
            });
        }
        private void useItem_Click(object sender, RoutedEventArgs e)
        {

            if (selectedChlitemView is null)
                return;
            ItemView itemInfo = selectedChlitemView;
            var obj = itemInfo.Item as Item;
            if (obj is null)
                return;
            TCPQuery<List<int>> tCPQuery = new TCPQuery<List<int>>("127.0.0.1", new List<int>() { player.ID, obj.ID });
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                bool compleat = tCPQuery.send((int)Ports.RemovePlayerItem);

                this.Dispatcher.Invoke(() =>
                {
                    if (compleat)
                        switch (tCPQuery.objectTGS[1])
                        {
                            case 2:
                                MessageBox.Show("В инвентаре недостаточно предметов для использования", "Невозможно использовать предмет", MessageBoxButton.OK, MessageBoxImage.Information);
                                break;
                            case 1:
                                if (player.Items.Contains(obj))
                                {
                                    player.Items[player.Items.IndexOf(obj)].Number -= 1;
                                    itemInfo.Item = player.Items[player.Items.IndexOf(obj)];
                                } 
                                break;
                            case 0:
                                if (player.Items.Contains(obj))
                                {
                                    player.Items.RemoveAt(player.Items.IndexOf(obj));
                                    TheGrid.Children.Remove(itemInfo);
                                }

                                break;
                            case -2:
                                MessageBox.Show("Ошибка запроса", "Невозможно купить предмет", MessageBoxButton.OK, MessageBoxImage.Information);
                                break;
                        }
                    else
                        MessageBox.Show("Сервер временно недоступен", "Невозможно купить предмет", MessageBoxButton.OK, MessageBoxImage.Information);
                });
            });
        }
        void AddNewItem(object sender, EventArgs e)
        {
            AddItemWindow add = new AddItemWindow();

            Hide();
            bool? res = add.ShowDialog();
            if (res == true)
            {
                InventLoad();
                Show();
            }
        }
        void AddParamToItem(object sender, EventArgs e)
        {
            if (selectedChlItem is null)
                return;
            ItemInfo it = selectedChlItem;
            AddParamWindow addModToMapWindow = new AddParamWindow(selectedChlItem.Item);
            Hide();
            bool? res = addModToMapWindow.ShowDialog();
            if (res == true)
            {
                InventLoad();
                Show();
            }
        }
        void RemoveParamFromItem(object sender, EventArgs e)
        {
            if (selectedChlItem is null)
                return;
            var obj = selectedChlItem.combo.SelectedItem as Params;
            if (obj is null)
                return;
            TCPQuery<List<int>> tCPQuery = new TCPQuery<List<int>>("127.0.0.1", new List<int> { obj.ID, selectedChlItem.Item.ID });
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                bool compleat = tCPQuery.send((int)Ports.DeleteItemParam);
                if (compleat)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        InventLoad();
                    });
                }
            });
        }
        void UpdateItem(object sender, EventArgs e)
        {
            if ((selectedChlItem is null))
                return;
            Item uitm = selectedChlItem.Item;

            uitm.Discription = selectedChlItem.itemDiscription;
            uitm.Name = selectedChlItem.ItemName.Text;
            int cost = 0;
            if (int.TryParse(selectedChlItem.ItemCost.Text, out cost))
            {
                uitm.Cost = cost;

                TCPQuery <Item > tCPQuery = new TCPQuery<Item>("127.0.0.1", uitm);
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    bool compleat = tCPQuery.send((int)Ports.UpdateItem);
                    if (compleat)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            InventLoad();
                        });
                    }
                });
            }
        }
        void RemoveItem(object sender, EventArgs e)
        {
            if (selectedChlItem is null)
                return;
            ItemInfo it = selectedChlItem;
            TCPQuery<int> tCPQuery = new TCPQuery<int>("127.0.0.1", selectedChlItem.Item.ID);
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                bool compleat = tCPQuery.send((int)Ports.DeleteItem);
                if (compleat)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        TheGrid.Children.Remove(it);
                    });
                }
            });
 
        }
        #endregion
        #region Mod
        private void ModLoad()
        {
            TCPQuery<List<Mod>> query = new TCPQuery<List<Mod>>("127.0.0.1", new List<Mod>());
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (TheGrid.Children.Count > 0)
                    {
                        TheGrid.Children.Clear();
                        TheGrid.RowDefinitions.Clear();
                        RowDefinition gridRow1 = new RowDefinition();
                        gridRow1.Height = GridLength.Auto;
                        TheGrid.RowDefinitions.Add(gridRow1);
                    }
                });
                if (user.IsAdmin || mods.Count<0)
                {

                    bool normalExt = query.send((int)Ports.GetMod);
                    mods = query.objectTGS;
                }
                if (user.IsAdmin)
                {

                    this.Dispatcher.Invoke(() =>
                    {
                        var Add = new Add();
                        TheGrid.Children.Add(Add);
                        Add.ButtonFechar.Click += AddNewMod;

                        Grid.SetRow(Add, TheGrid.RowDefinitions.Count - 1);
                        int every3 = 1;
                        foreach (Mod md in mods)
                        {
                            if (every3 == 3)
                            {
                                RowDefinition gridRow1 = new RowDefinition();
                                gridRow1.Height = GridLength.Auto;
                                TheGrid.RowDefinitions.Add(gridRow1);
                                every3 = 0;
                            }

                            var uc = new ModInfo();
                            uc.RemoveThisItem.Click += RemoveMod;
                            uc.RemoveParam.Click += RemoveParamFromMod;
                            uc.UpdateItem.Click += UpdateMod;
                            uc.AddParam.Click += AddParamToMod;
                            uc.Item = md;
                            TheGrid.Children.Add(uc);
                            Grid.SetColumn(uc, every3);
                            Grid.SetRow(uc, TheGrid.RowDefinitions.Count - 1);
                            every3++;
                        }
                    });
                }
                else
                {
                    this.Dispatcher.Invoke(() =>
                    {

                        int every3 = 0;
                        foreach (Mod md in mods)
                        {
                            if (every3 == 3)
                            {
                                RowDefinition gridRow1 = new RowDefinition();
                                gridRow1.Height = GridLength.Auto;
                                TheGrid.RowDefinitions.Add(gridRow1);
                                every3 = 0;
                            }

                            var uc = new ModView();
 
                            uc.Item = md;
                            TheGrid.Children.Add(uc);
                            Grid.SetColumn(uc, every3);
                            Grid.SetRow(uc, TheGrid.RowDefinitions.Count - 1);
                            every3++;
                        }
                    });
                }
            });
        }
      
        void AddNewMod(object sender, EventArgs e)
        {
            AddModWindow add = new AddModWindow();

            Hide();
            bool? res = add.ShowDialog();
            if (res == true)
            {
                ModLoad();
                Show();
            }
        }
        void AddParamToMod(object sender, EventArgs e)
        {
            if (selectedChlMod is null)
                return;
             
            AddParamWindow addModToMapWindow = new AddParamWindow(selectedChlMod.Item);
            Hide();
            bool? res = addModToMapWindow.ShowDialog();
            if (res == true)
            {
                ModLoad();
                this.Show();
            }
        }
        void RemoveParamFromMod(object sender, EventArgs e)
        {
            if (selectedChlMod is null)
                return;
            var obj = selectedChlMod.combo.SelectedItem as Params;
            if (obj is null)
                return;
            TCPQuery<List<int>> tCPQuery = new TCPQuery<List<int>>("127.0.0.1", new List<int> { obj.ID, selectedChlMod.Item.ID });
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                bool compleat = tCPQuery.send((int)Ports.DeleteModParam);
                if (compleat)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        ModLoad();
                    });
                }
            });
        }
        void UpdateMod(object sender, EventArgs e)
        {
            if ((selectedChlMod is null))
                return;
            Mod uitm = selectedChlMod.Item;

            uitm.Discription = selectedChlMod.itemDiscription;
            uitm.Name = selectedChlMod.ItemName.Text;
 
       

            TCPQuery<Mod> tCPQuery = new TCPQuery<Mod>("127.0.0.1", uitm);
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                bool compleat = tCPQuery.send((int)Ports.UpdateMod);
                if (compleat)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        ModLoad();
                    });
                }
            });
          
        }
        void RemoveMod(object sender, EventArgs e)
        {
            if (selectedChlMod is null)
                return;
            ModInfo it = selectedChlMod;
            TCPQuery<int> tCPQuery = new TCPQuery<int>("127.0.0.1", selectedChlMod.Item.ID);
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                bool compleat = tCPQuery.send((int)Ports.DeleteItem);
                if (compleat)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        TheGrid.Children.Remove(it);
                    });
                }
            });

        }
        #endregion
        #region Task
        private void TaskLoad()
        {
            TCPQuery<List<UserTask>> query = new TCPQuery<List<UserTask>>("127.0.0.1", new List<UserTask>());
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (TheGrid.Children.Count > 0)
                    {
                        TheGrid.Children.Clear();
                        TheGrid.RowDefinitions.Clear();
                        RowDefinition gridRow1 = new RowDefinition();
                        gridRow1.Height = GridLength.Auto;
                        TheGrid.RowDefinitions.Add(gridRow1);
                    }
                });
                if (user.IsAdmin || mods.Count < 0)
                {

                    bool normalExt = query.send((int)Ports.GetTasks);
                    Task = query.objectTGS;
                }
                if (user.IsAdmin)
                {

                    this.Dispatcher.Invoke(() =>
                    {
                        var Add = new Add();
                        TheGrid.Children.Add(Add);
                        Add.ButtonFechar.Click += AddNewTask;

                        Grid.SetRow(Add, TheGrid.RowDefinitions.Count - 1);
                        int every3 = 1;
                        foreach (UserTask md in Task)
                        {
                            if (every3 == 3)
                            {
                                RowDefinition gridRow1 = new RowDefinition();
                                gridRow1.Height = GridLength.Auto;
                                TheGrid.RowDefinitions.Add(gridRow1);
                                every3 = 0;
                            }

                            var uc = new TaskInfo();
                            uc.RemoveThisItem.Click += RemoveTask;
                            
                            uc.UpdateItem.Click += UpdateTask;
 
                            uc.Item = md;
                            TheGrid.Children.Add(uc);
                            Grid.SetColumn(uc, every3);
                            Grid.SetRow(uc, TheGrid.RowDefinitions.Count - 1);
                            every3++;
                        }
                    });
                }
                else
                {
                    this.Dispatcher.Invoke(() =>
                    {

                        int every3 = 0;
                        foreach (UserTask md in Task)
                        {
                            if (every3 == 3)
                            {
                                RowDefinition gridRow1 = new RowDefinition();
                                gridRow1.Height = GridLength.Auto;
                                TheGrid.RowDefinitions.Add(gridRow1);
                                every3 = 0;
                            }

                            var uc = new TaskView();
 
                            uc.Item = md;
                            TheGrid.Children.Add(uc);
                            Grid.SetColumn(uc, every3);
                            Grid.SetRow(uc, TheGrid.RowDefinitions.Count - 1);
                            every3++;
                        }
                    });
                }
            });
        }

        void AddNewTask(object sender, EventArgs e)
        {
            AddTask add = new AddTask ();

            Hide();
            bool? res = add.ShowDialog();
            if (res == true)
            {
                TaskLoad();
                Show();
            }
        }
         
         
        void UpdateTask(object sender, EventArgs e)
        {
            if ((selectedTask is null))
                return;
            UserTask uitm = selectedTask.Item;

            uitm.Discription = selectedTask.itemDiscription;
            uitm.Name = selectedTask.ItemName.Text;
            uitm.Prize = int.Parse(selectedTask.Prize.Text);


            TCPQuery<UserTask> tCPQuery = new TCPQuery<UserTask>("127.0.0.1", uitm);
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                bool compleat = tCPQuery.send((int)Ports.UpdateTasks);
                if (compleat)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        TaskLoad();
                    });
                }
            });

        }
        void RemoveTask(object sender, EventArgs e)
        {
            if (selectedTask is null)
                return;
            TaskInfo it = selectedTask;
            TCPQuery<int> tCPQuery = new TCPQuery<int>("127.0.0.1", selectedTask.Item.ID);
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                bool compleat = tCPQuery.send((int)Ports.DeleteTasks);
                if (compleat)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        TheGrid.Children.Remove(it);
                    });
                }
            });

        }
        #endregion
        private void TheGrid_MouseMove(object sender, MouseEventArgs e)
        {
            var selectedChild = e.Source;
            Type child = selectedChild.GetType();
            if (child == typeof(MapInfo))
            {
                selectedChlMap = (MapInfo)selectedChild;
            }
            else if(child == typeof(ShopItem))
            {
                selectedChlItm = (ShopItem)selectedChild;
            }
            else if (child == typeof(ItemInfo))
            {
                selectedChlItem = (ItemInfo)selectedChild;
            }else if (child == typeof(ItemView))
            {

                selectedChlitemView = (ItemView)selectedChild; 
            }
            else if (child == typeof(ModInfo))
            {

                selectedChlMod = (ModInfo)selectedChild;
            }else if (child == typeof(TaskInfo))
            {

                selectedTask = (TaskInfo)selectedChild;

            }
        }

        private void mapListBtn_Click(object sender, RoutedEventArgs e)
        {
            WinName.Text = "СПИСОК КАРТ";
            MapLoad();
        }
 
        private void btnLoadProf_Click(object sender, RoutedEventArgs e)
    {
            WinName.Text = "ПРОФИЛЬ";
            ProfileLoad();
        }

        private void btnShop_Click(object sender, RoutedEventArgs e)
        {
            WinName.Text = "МАГАЗИН";
            ShopLoad() ;
        }
        private void btnInvent_Click(object sender, RoutedEventArgs e)
        {
            WinName.Text = "ИНВЕНТАРЬ";
            InventLoad();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WinName.Text = "РЕЖИМЫ";
            ModLoad();
        }
 
        private void btnTask_Click(object sender, RoutedEventArgs e)
        {
            WinName.Text = "ЗАДАНИЯ";
            TaskLoad();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
             
            
            TCPQuery<int> tCPQuery = new TCPQuery<int>("127.0.0.1", 0);
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                tCPQuery.send((int)Ports.ReportsToFromUsers);
                 
            });
                
            
        }
    }
}
