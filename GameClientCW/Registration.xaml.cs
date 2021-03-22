using GameClass.Users;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Interaction logic for Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        public TCPQuery<Player> tCPQuery { get; set; }
        public Registration()
        {
            InitializeComponent();
            load.Visibility = Visibility.Hidden;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
            
        }

        private void RegWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DialogResult = true;
        }

        private void Regiser_Click(object sender, RoutedEventArgs e)
        {
            string Lgin = login.Text.Trim();
            string pass1 = passwordF.Password.Trim();
            string pass2 = passwordL.Password.Trim(); 
            string email = Email.Text.Trim();
            Error.Text = string.Empty;
            bool mistake = false;
            if (Lgin.Length<4)
            {
                login.ToolTip = "Логин должен содержать больше 3-х символов, логин может содержать только цифры и буквы\nПример: name123";
                login.Foreground = Brushes.DarkRed;
                mistake = true;
            }
            else
            {
                login.ToolTip = "Пример: name123";
                login.Foreground = Brushes.Black;
            }
            if (pass1.Length < 4)
            {
                passwordF.ToolTip = "Пароль должен состоять как минимум из 3-х символов\n Пример: @1sfe3$;svfe";
                passwordF.Foreground = Brushes.DarkRed;
                mistake = true;
            }
            else
            {
                passwordF.ToolTip = "Пример: @1sfe3$;svfe";
                passwordF.Foreground = Brushes.Black;
            }
            if (pass1 != pass2)
            {
                passwordF.ToolTip += "Пароли не совпадают";
                passwordF.Foreground = Brushes.DarkRed; 
                passwordL.ToolTip = "Пароли не совпадают";
                passwordL.Foreground = Brushes.DarkRed;
                mistake = true;
            }
            else
            {
                if (passwordF.ToolTip.ToString().Contains("Пароли не совпадают"))
                {
                    passwordF.ToolTip = passwordF.ToolTip.ToString().Replace("Пароли не совпадают", "");
                }
                passwordF.Foreground = Brushes.Black;
                passwordL.ToolTip = null;
                passwordL.Foreground = Brushes.Black;
            }
            if(!IsValidEmail(email))
            {
                Email.ToolTip = "Не корректный ввод почты\n Пример: example@mail.com";
                Email.Foreground = Brushes.DarkRed;
                mistake = true;
            }
            else
            {
                Email.ToolTip = "Пример: example@mail.com";
                Email.Foreground = Brushes.Black;
            }
            if (!mistake)
            {
                tCPQuery = new TCPQuery<Player>("127.0.0.1", new Player(login.Text, "", Email.Text));
                tCPQuery.objectTGS.Password = passwordF.Password;
                formDE(false);
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                   bool normalExt = tCPQuery.send((int)Ports.Register);
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
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {

        }
        private void RegFin(Player player)
        {
            switch (player.Coins)
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
            login.IsEnabled = isTF;
            passwordF.IsEnabled = isTF;
            passwordL.IsEnabled = isTF;
            Email.IsEnabled = isTF;
            Auf.IsEnabled = isTF;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(!String.IsNullOrEmpty(login.Text))
                foreach(char c in login.Text)
                {
                    if (!char.IsLetterOrDigit(c))
                    {
                        login.ToolTip = "Логин должен содержать больше 3-х символов, логин может содержать только цифры и буквы\nПример: name123";
                        login.Foreground = Brushes.DarkRed;
                        return;
                    }
                
               }
            login.Foreground = Brushes.Black;
            login.ToolTip = "Пример: name123";
        }
        bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
