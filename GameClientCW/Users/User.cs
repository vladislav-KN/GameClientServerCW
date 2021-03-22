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
        public static int StringHashCode20(string value)
        {
            int num = 352654597;
            int num2 = num;

            for (int i = 0; i < value.Length; i += 4)
            {
                int ptr0 = value[i] << 16;
                if (i + 1 < value.Length)
                    ptr0 |= value[i + 1];

                num = (num << 5) + num + (num >> 27) ^ ptr0;

                if (i + 2 < value.Length)
                {
                    int ptr1 = value[i + 2] << 16;
                    if (i + 3 < value.Length)
                        ptr1 |= value[i + 3];
                    num2 = (num2 << 5) + num2 + (num2 >> 27) ^ ptr1;
                }
            }

            return num + num2 * 1566083941;
        }
        public User(User user)
        {
            login = user.Login;
            Password = user.Password;
            isAdmin = user.IsAdmin;
        }

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
                password = StringHashCode20(value);                
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
