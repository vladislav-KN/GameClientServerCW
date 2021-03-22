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

namespace GameClientCW
{
    /// <summary>
    /// Interaction logic for AddMapWindow.xaml
    /// </summary>
    public partial class AddMapWindow : Window
    {
         
        public TCPQuery<Map> tCPQuery
        {
            get;set;
        }
        public AddMapWindow()
        {
            InitializeComponent();
        }

        private void Regiser_Click(object sender, RoutedEventArgs e)
        {
            string Discrip = Dicription.Text.Trim();
            string name = Name.Text.Trim();
            string map = cmb.Text.Trim();
              
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
                Map add = new Map();
                add.Discription = Discrip;
                add.Name = name;
                add.ImageSource = map;
                tCPQuery = new TCPQuery<Map>("127.0.0.1", add);

                formDE(false);
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    bool normalExt = tCPQuery.send((int)Ports.AddMap);
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
        private void RegFin(Map map)
        {
            switch (map.ID)
            {
                case -1:
                    Error.Text = "Не удалось зарегистрировать ваш аккаунт, повторите попытку позднее";
                    formDE(true);
                    break;
                case -2:
                    Error.Text = "Не удалось зарегистрировать ваш аккаунт, аккаунт с таким логином или почтой уже существует";
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
