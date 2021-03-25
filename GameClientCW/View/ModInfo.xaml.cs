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
    /// Interaction logic for ModInfo.xaml
    /// </summary>
    public partial class ModInfo : UserControl
    {
        public ModInfo()
        {
            InitializeComponent();
        }
        Mod itm;
        public Mod Item
        {
            get
            {
                return itm;
            }
            set
            {
                itm = value;
                ItemDiscriptution.Text = itm.Discription;
                ItemName.Text = itm.Name;
                foreach (Params pr in itm.Parametrs)
                {
                    combo.Items.Add(pr);
                }
            }
        }
        public string itemName
        {
            get
            {
                return ItemName.Text;
            }
            set
            {
                ItemName.Text = value;
            }
        }
        public string itemDiscription
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
        public string itemVal
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

        public MenuItem itemAdd
        {
            get
            {
                return AddParam;
            }
            set
            {
                AddParam = value;
            }
        }
        public MenuItem itemRemove
        {
            get
            {
                return RemoveThisItem;
            }
            set
            {
                RemoveThisItem = value;
            }
        }
        public MenuItem Update
        {
            get
            {
                return UpdateItem;
            }
            set
            {
                UpdateItem = value;
            }
        }
        public MenuItem Delete
        {
            get
            {
                return RemoveParam;
            }
            set
            {
                RemoveParam = value;
            }
        }

  
    }
}
