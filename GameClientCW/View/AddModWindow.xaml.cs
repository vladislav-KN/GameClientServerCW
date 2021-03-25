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
    /// Interaction logic for AddModWindow.xaml
    /// </summary>
    public partial class AddModWindow : Window
    {
        public AddModWindow()
        {
            InitializeComponent();
        }
        public TCPQuery<Mod> tCPQuery
        {
            get; set;
        }


        private void Regiser_Click(object sender, RoutedEventArgs e)
        {
            string Discrip = Dicription.Text.Trim();
            string name = Name.Text.Trim();
 
            bool mistake = false;
            if (name.Length < 1)
            {
                Name.ToolTip = "Имя должно содержать символы";
                Name.Foreground = Brushes.DarkRed;
                mistake = true;
            }
            else
            {
                Name.ToolTip = "";
                Name.Foreground = Brushes.Black;
            }
            if (Discrip.Length < 1)
            {
                Dicription.ToolTip = "Описание должно содержать символы";
                Dicription.Foreground = Brushes.DarkRed;
                mistake = true;
            }
            else
            {
                Dicription.ToolTip = "";
                Dicription.Foreground = Brushes.Black;
            }
             

            if (!mistake)
            {
                Mod add = new Mod();
                add.Discription = Discrip;
                add.Name = name;
 
                tCPQuery = new TCPQuery<Mod>("127.0.0.1", add);

                formDE(false);
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    bool normalExt = tCPQuery.send((int)Ports.AddMod);
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
        }
        private void RegFin(Mod item)
        {
            switch (item.ID)
            {
                case -1:
                    Error.Text = "Не удалось добавить модификатор, повторите попытку позднее";
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
             
            Dicription.IsEnabled = isTF;
            Name.IsEnabled = isTF;
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
 
