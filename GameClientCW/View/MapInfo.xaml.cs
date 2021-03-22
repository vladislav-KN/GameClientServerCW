using GameClass.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace GameClientCW.View
{
    /// <summary>
    /// Interaction logic for MapInfo.xaml
    /// </summary>
    public partial class MapInfo : UserControl
    {
        public MapInfo()
        {
            InitializeComponent();
        }
        private Map map;
        public Map cumap {
            get { return map; }
            set
            {
                map = value;
                MapName.Text = map.Name;
                MapDiscriptution.Text = map.Discription;
                Map.Source = new BitmapImage(new Uri(map.ImageSource, UriKind.Relative)); ;
                if (map.Mods != null)
                    foreach (Mod mod in map.Mods)
                    {
                        combo.Items.Add(mod);
                    }
                
            }
        }
        public string mapName
        {
            get
            {
                return MapName.Text;
            }
            set
            {
                MapName.Text = value;
            }
        }
        public string mapDiscription
        {
            get
            {
                return MapDiscriptution.Text;
            }
            set
            {
                MapDiscriptution.Text = value;
            }
        }
        public ComboBox combo
        {
            get
            {
                return MapMods;
            }
            set
            {
                MapMods = value;
            }
        }
        public Image image
        {
            get
            {
                return Map;
            }
            set
            {
                Map = value;
            }
        }
        public MenuItem modAdd
        {
            get
            {
                return AddMod;
            }
            set
            {
                AddMod = value;
            }
        }
        public MenuItem modRemove
        {
            get
            {
                return RemoveMod;
            }
            set
            {
                RemoveMod = value;
            }
        }
        public MenuItem Update
        {
            get
            {
                return UpdateMap;
            }
            set
            {
                UpdateMap = value;
            }
        }
        public MenuItem Delete
        {
            get
            {
                return RemoveMap;
            }
            set
            {
                RemoveMap = value;
            }
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
