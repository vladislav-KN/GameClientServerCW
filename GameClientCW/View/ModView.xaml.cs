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
    /// Interaction logic for ModView.xaml
    /// </summary>
    public partial class ModView : UserControl
    {
        public ModView()
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
                    ItemParams.Text += pr.mParam.ToString() + "\t" + pr.value + "\n";
                }
            }
        }
    }
}
