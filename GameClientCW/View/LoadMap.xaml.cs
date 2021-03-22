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
    /// Interaction logic for LoadMap.xaml
    /// </summary>
    public partial class LoadMap : UserControl
    {
        public LoadMap()
        {
            InitializeComponent();
        }
        public Button button
        {
            get
            {
                return ButtonFechar;
            }
            set
            {
                ButtonFechar = value;
            }
        }

        private void ButtonFechar_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
