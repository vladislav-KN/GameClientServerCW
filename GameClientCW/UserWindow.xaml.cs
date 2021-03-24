

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
        public UserWindow(User usr)
        {
            InitializeComponent();
            user = new User(usr);
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
                        var Add = new LoadMap();
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
                            case 1:
                                MessageBox.Show("На счету недостаточно денег для покупки", "Невозможно купить предмет", MessageBoxButton.OK, MessageBoxImage.Information);
                                break;
                            case 0:
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
>>>>>>> 98e14fdcfe07d388da674ca2abf481bef917fea8:GameClientCW/UserWindow.xaml.cs
    }
}
