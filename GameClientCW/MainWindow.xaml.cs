using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using GameClass.Users;

namespace GameClientCW
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
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
                Show();
            }
            
            

        }

        private void forgotPass_Click(object sender, RoutedEventArgs e)
        {
            ForgetPass forgetPass = new ForgetPass();
            Hide();
            bool? res = forgetPass.ShowDialog();
            if (res == true)
            {
                Show();
            }
        }

        private void btnEnter_Click(object sender, RoutedEventArgs e)
        {
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\hosts";
            FileInfo fi1 = new FileInfo(path);

            if (!fi1.Exists)
            {
                //Create a file to write to.
                using (StreamWriter sw = fi1.CreateText())
                {
                    sw.WriteLine("127.0.0.1");
                }
            }
            else
            {
                using (StreamReader sr = fi1.OpenText())
                {
                    string s = "";
                    while ((s = sr.ReadLine()) != null)
                    {
                        hosts.Add(s);
                    }
                }
            }
            TCPQuery<User> tCPQuery = new TCPQuery<User>("127.0.0.1",new User("",""));
            tCPQuery.LogIn(); 
            
        }
    }
}
