using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameClass.Users
{
    [Serializable]
    public class User
    {
        string login;
        int password;
        bool isAdmin;
        public User(string log, string pass)
        {
            login = log;
            Password = pass;
        }
        public string Login
        {
            get { return login; }
        }
        public string Password
        {
            get 
            { 
                return password.ToString(); 
            }
            set
            {
                password = value.GetHashCode();                
            }
        }
        public bool IsAdmin
        {
            get
            {
                return isAdmin;
            }
            set
            {
                isAdmin = value;
            }
        }
    }
}
