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
    /// Interaction logic for AddItemWindow.xaml
    /// </summary>
    public partial class AddItemWindow : Window
    {
        public AddItemWindow()
        {
            InitializeComponent();
        }
        public TCPQuery<Item> tCPQuery
        {
            get; set;
        }
        

        private void Regiser_Click(object sender, RoutedEventArgs e)
        {
            string Discrip = Dicription.Text.Trim();
            string name = Name.Text.Trim();
            string value = Value.Text.Trim();
            int vl = 0;
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
            if (!int.TryParse(value, out vl))
            {
                Value.ToolTip = "Можно вводить только числа";
                Value.Foreground = Brushes.DarkRed;
                mistake = true;
            }
            else
            {
                Value.ToolTip = "";
                Value.Foreground = Brushes.Black;
            }

            if (!mistake)
            {
                Item add = new Item();
                add.Discription = Discrip;
                add.Name = name;
                add.Cost = vl;
                tCPQuery = new TCPQuery<Item>("127.0.0.1", add);

                formDE(false);
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    bool normalExt = tCPQuery.send((int)Ports.AddItem);
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
        private void RegFin(Item item)
        {
            switch (item.ID)
            {
                case -1:
                    Error.Text = "Не удалось добавить предмет, повторите попытку позднее";
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
            Value.IsEnabled = isTF;
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
