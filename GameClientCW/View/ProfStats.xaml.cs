using GameClass.Users;
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
    /// Interaction logic for ProfStats.xaml
    /// </summary>
    public partial class ProfStats : UserControl
    {
        public ProfStats()
        {
            InitializeComponent();
        }
        private Player player;
        public Player Player
        {
            get
            {
                return player;
            }
            set
            {
                player = value;
                ManeyValue.Text = player.Coins.ToString();
                RateValue.Text = player.Rate.ToString();
                TimeValue.Text = player.InGameTime.ToString();
                WinRateValue.Text = player.WinRate.ToString();
                NumOfMatValue.Text = player.NumberOfMatch.ToString();
                UserName.Text = player.NickName.ToString();
               
            }
        }
    }
}
