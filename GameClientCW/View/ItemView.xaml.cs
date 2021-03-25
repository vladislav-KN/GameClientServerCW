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
    /// Interaction logic for ItemView.xaml
    /// </summary>
    public partial class ItemView : UserControl
    {
        public ItemView()
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
                ItemName.Text = itm.Name + " x" + itm.Number;
                foreach (Params pr in itm.Parametrs)
                {
                    ItemParams.Text += pr.iParam.ToString() + "\t" + pr.value + "\n";
                }
            }
        }
        public Button use
        {
            get
            {

                return btnUse;
            }
            set
            {
                btnUse = value;
            }
        }
    }
}
