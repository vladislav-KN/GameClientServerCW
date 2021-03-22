using GameClass.Objects;
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

namespace GameClientCW.View
{
    /// <summary>
    /// Interaction logic for ItemInfo.xaml
    /// </summary>
    public partial class ItemInfo : UserControl
    {
        public ItemInfo()
        {
            InitializeComponent();
        }
        Item itm;
        public Item Item
        {
            get
            {
                return itm;
            }
            set
            {
                itm = value;
                ItemCost.Text = itm.Cost + " $";
                ItemDiscriptution.Text = itm.Discription;
                ItemName.Text = itm.Name;
                foreach (Params pr in itm.Parametrs)
                {
                    ItemParams.Text += pr.iParam.ToString() + "\t" + pr.value + "\n";
                }
            }
        }
        public string mapName
        {
            get
            {
                return ItemCost.Text;
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
                return ItemDiscriptution.Text;
            }
            set
            {
                ItemName.Text = value;
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
    }
}
