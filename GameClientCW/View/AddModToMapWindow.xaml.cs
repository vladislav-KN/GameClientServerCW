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
using System.Windows.Shapes;

namespace GameClientCW.View
{
    /// <summary>
    /// Interaction logic for AddModToMapWindow.xaml
    /// </summary>
    public partial class AddModToMapWindow : Window
    {
        public AddModToMapWindow(Map map)
        {
            InitializeComponent();
            TCPQuery<List<Mod>> modlist = new TCPQuery<List<Mod>>("127.0.0.1", new List<Mod>());
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                bool normalExt = modlist.send((int)Ports.GetMod);
                if(normalExt)
                    this.Dispatcher.Invoke(() =>
                    {
                        map.Mods = new List<Mod>();   
                       foreach(Mod mod in modlist.objectTGS)
                        {
                            if (!map.Mods.Contains(mod))
                            {
                                cmb.Items.Add(mod);
                            }
                        }
                    });
                
            });
                tCPQuery = new TCPQuery<Map>("127.0.0.1", map);
        }
        public TCPQuery<Map> tCPQuery
        {
            get; set;
        }
 

        private void Regiser_Click(object sender, RoutedEventArgs e)
        {

            string map = cmb.Text.Trim();


            if (tCPQuery.objectTGS.Mods.Count > 0)
            {
                Mod addm = new Mod();
                addm = cmb.Items[cmb.SelectedIndex] as Mod;
                tCPQuery.objectTGS.Mods.Add(addm);
            }
            else
            {
                Mod addm = new Mod();
                addm = cmb.Items[cmb.SelectedIndex] as Mod;
                tCPQuery.objectTGS.Mods = new List<Mod>();
                tCPQuery.objectTGS.Mods.Add(addm);
            }
          

            formDE(false);
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                bool normalExt = tCPQuery.send((int)Ports.UpdateMap);
                this.Dispatcher.Invoke(() => {
                    if (normalExt)
                    {
                        RegFin(tCPQuery.objectTGS);
                    }
                    else
                    {
                        Error.Text = "Не удалось подключится к серверу, повторите попытку позднее";
                        formDE(true);
                    }
                });

            });
 
        }
        private void RegFin(Map map)
        {
            switch (map.ID)
            {
                case -1:
                    Error.Text = "Не удалось обновить карту, повторите попытку позднее";
                    formDE(true);
                    break;
                default:
                    Close();
                    break;

            }
        }
        private void formDE(bool isTF)
        {
            if (isTF)
            {
                load.Visibility = Visibility.Hidden;
            }
            else
            {
                load.Visibility = Visibility.Visible;
            }
            register.IsEnabled = isTF;
            cmb.IsEnabled = isTF;
            Auf.IsEnabled = isTF;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Form_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}

