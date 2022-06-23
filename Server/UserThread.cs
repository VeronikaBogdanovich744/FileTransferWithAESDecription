using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using CustomLibrary_v2;
//using SystemCommands;

namespace Server
{
    class UserThread
    {
        private bool isConnect = false;

        private Thread thread;
      //  private Thread checkConnection;
        private Socket socket;

        private List<User> users;
        private User self;

        public UserThread(Socket socket, List<User> users)
        {
            this.socket = socket;
            this.socket.ReceiveBufferSize = 50000;
            this.users = users;

            thread = new Thread(Service);
            thread.Start();
        }
        /*
        private void IsConnected()
        {
            try
            {
                byte[] bytes = new byte[0];
                while (true)
                {
                    socket.Send(bytes);
                    Thread.Sleep(5000);
                }
            }
            catch(SocketException ex)
            {
                Console.WriteLine(ex.Message);
                Exit();
                thread.Abort();
                return;
            }
        }*/

        private void Service()
        {
            LogIn();

            string msg = string.Empty;
            try
            {
                MessageClass messg = new MessageClass();
                while (isConnect)
                {
                    messg = getMessg();
                    if (messg != null)
                    {
                        if (!IsSystemMsg(messg.type))
                        {
                            SendToReciever(messg);
                        }
                    }
                }
            }
            catch(SocketException ex)
            { 
                Console.WriteLine(ex.Message);
                Exit();
            }

        }
        private void SendToReciever(MessageClass msg)
        {
            try
            {
                Socket receiverSocket = users.Find(x => x.nickname == msg.receiver).socket;
                byte[] bytes = msg.getBytes();
                Console.WriteLine($"Size of message before sending: {bytes.Length}");
                receiverSocket.Send(bytes);
            }
            catch
            {
                msg.receiver = msg.sender;
                msg.sender = ""; 
                msg.type = Symbols.MSG_IS_NOT_RECEIVED;
                msg.container = new byte[1];
                msg.container[0] = 0;
                Socket receiverSocket = users.Find(x => x.nickname == msg.receiver).socket;
                byte[] bytes = msg.getBytes();
                receiverSocket.Send(bytes);
            }
        }

        private bool IsSystemMsg(Symbols type) //проверка на выход 
        {
            switch(type)
            {
                case Symbols.EXIT:
                    Exit();
                    return true;

                default:
                    return false;
            }
        }

        private MessageClass getMessg()
        {
            var messg = new MessageClass();
            byte[] bytes = new byte[16000];
            int receivedBytes = 0;
            byte[] buffer;
            using (var stream = new MemoryStream())
            {
                do
                {
                    receivedBytes = socket.Receive(bytes);
                    stream.Write(bytes, 0, receivedBytes);
                } while (socket.Available > 0);
                buffer = stream.ToArray();
            }
            Console.WriteLine($"Size of message: {buffer.Length}");
            if (buffer.Length != 0)
            {
                messg = MessageClass.getMessageFromBytes(buffer);
                return messg;
            }
            return null;
        }

        private string ReceiveUserName()
        {
            byte[] bytes = new byte[1024];
            int count = socket.Receive(bytes);
            MessageClass msg = new MessageClass();
            msg = MessageClass.getMessageFromBytes(bytes);
            return Encoding.UTF8.GetString(msg.container);
        }

        private void LogIn()
        {
            string nickname = ReceiveUserName();
            if (users.Find(x => x.nickname == nickname) == null)
            {
                self = new User(nickname, socket);
                users.Add(self);
                Console.WriteLine("Пользователь {0} присоединился.", self.nickname);
                SendUsersList();
                isConnect = true;
            }
            else
            {
                MessageClass msg = new MessageClass();
                msg.type = Symbols.NICKNAME_TAKEN;
                msg.formMessage(Symbols.NICKNAME_TAKEN,"","","","","");
                byte[] bytes = msg.getBytes();
                socket.Send(bytes); 
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
               // checkConnection.Abort();
                thread.Abort();

            }
        }

        private void SendUsersList()
        {
            MessageClass msg = new MessageClass();
            msg.type = Symbols.USER_LIST;
            string list = String.Empty;
            foreach (User user in users)
            {
                list += user.nickname + '\0';
            }

            msg.container = Encoding.UTF8.GetBytes(list);
            byte[] bytes = msg.getBytes();
            foreach (User user in users)
            {
                user.socket.Send(bytes);
            }
        }

        private void Exit()
        {
            Console.WriteLine("Пользователь {0} вышел.", self.nickname);

            users.Remove(self);
            SendUsersList();

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            isConnect = false;

          //  checkConnection.Abort();
            thread.Abort();
           
        }

    }
}
