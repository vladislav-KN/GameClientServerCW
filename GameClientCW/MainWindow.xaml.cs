using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Media;
using GameClass.Users;

namespace GameClientCW
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TCPQuery<User> Login;
        public MainWindow()
        {
            InitializeComponent();
        }
        List<string> hosts = new List<string>();
        private void Button_Click(object sender, RoutedEventArgs e)
        {

            

        }
        
        private void btnReg_Click(object sender, RoutedEventArgs e)
        {

           
            Registration registration = new Registration();
            Hide();
            bool? res = registration.ShowDialog();
            if(res == true)
            {
                if (string.IsNullOrEmpty(registration.tCPQuery.objectTGS.NickName))
                {
                    Show();
                }
                else
                {
                    UserWindow ur = new UserWindow(registration.tCPQuery.objectTGS);
                    ur.ShowDialog();
                    Show();
                }
            }
            
            

        }
 
        private void RegFin(User user)
        {
            switch (user.Login.Length)
            {
                case 0:
                    Error.Text = "Введён не правильный пароль";
                    formDE(true);
                    break;
                case 1:
                    Error.Text = "Аккаунта с таким логином не существует";
                    formDE(true);
                    break;
                case 2:
                    Error.Text = "Не удалось подключится к серверу, повторите попытку позднее";
                    formDE(true);
                    break;
                default:
                    UserWindow ur = new UserWindow(user);
                    Hide();
                    ur.ShowDialog();
                    Show();
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
            login.IsEnabled = isTF;
            password.IsEnabled = isTF;
        }
        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {

            string Lgin = login.Text.Trim();
            string pass1 = password.Password.Trim();
            Error.Text = string.Empty;
            bool mistake = false;
            if (Lgin.Length < 4)
            {
                login.ToolTip = "Логин может содержать как минимум 4 символа, логин может содержать только цифры и буквы\nПример: name123";
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
                password.ToolTip = "Пароль должен состоять как минимум из 4-х символов\n Пример: @1sfe3$;svfe";
                password.Foreground = Brushes.DarkRed;
                mistake = true;
            }
            else
            {
                password.ToolTip = "Пример: @1sfe3$;svfe";
                password.Foreground = Brushes.Black;
            }
            if (!mistake)
            {
                Login = new TCPQuery<User>("127.0.0.1", new User(login.Text, password.Password));
                formDE(false);
                System.Threading.Tasks.Task.Factory.StartNew(() =>
                {
                    bool normalExt = Login.send((int)Ports.Login);
                    this.Dispatcher.Invoke(() => {
                        if (normalExt)
                        {
                            RegFin(Login.objectTGS);
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
    }
}
