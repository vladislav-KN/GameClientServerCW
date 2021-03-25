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
    /// Interaction logic for TaskInfo.xaml
    /// </summary>
    public partial class TaskInfo : UserControl
    {
        public TaskInfo()
        {
            InitializeComponent();
        }
        UserTask itm;
        public UserTask Item
        {
            get
            {
                return itm;
            }
            set
            {
                itm = value;
                Prize.Text = itm.Prize.ToString();
                ItemDiscriptution.Text = itm.Discription;
                ItemName.Text = itm.Name;

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
         public string val
        {
            get 
            {
                return Prize.Text; 
            }
            set
            {
                Prize.Text = value;
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
         

        private void ItemCost_TextChanged(object sender, TextChangedEventArgs e)
        {
            int val;
            if (!int.TryParse(Prize.Text, out val))
            {
                Prize.ToolTip = "Можно вводить только цифры";
                Prize.Foreground = Brushes.DarkRed;
            }
            else
            {
                Prize.ToolTip = "Можно вводить только цифры";
                Prize.Foreground = Brushes.Black;

            }
        }
    }
}
