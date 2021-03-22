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
    /// Interaction logic for Match.xaml
    /// </summary>
    public partial class Match : UserControl
    {
        public Match()
        {
            InitializeComponent();
        }
        private Game game;
        public Game Game
        {
            get
            {
                return game;
            }
            set
            {
                game = value;
                MapName.Text = game.Map.Name + " : " + game.ModName;
                Time.Text =  game.GameDate.ToString() + " - " + game.GameTime.ToString();
                Map.Source = new BitmapImage(new Uri(game.Map.ImageSource, UriKind.Relative));
                if (game.PlayerList != null)
                    foreach (string pl in game.PlayerList)
                    {
                        
                        Paticipants.Items.Add(pl);
                        if (pl.Contains(game.Player))
                        {
                           Paticipants.SelectedIndex = Paticipants.Items.IndexOf(pl);
                        }
                    }

            }
        }
        }
    }

