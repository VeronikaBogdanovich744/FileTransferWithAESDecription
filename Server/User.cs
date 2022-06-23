using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Server
{
    class User
    {
        public User(string nickname, Socket socket)
        {
            this.nickname = nickname;
            this.socket = socket;
        }

        public string nickname;
        public Socket socket;
    }
}
