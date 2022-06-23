using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Net;
using System.Net.Sockets;
using System.Threading;
//using System.Windows.Controls;
//using Server;
using Client.Classes;
using System.IO;
using System.Windows.Controls;
using CustomLibrary_v2;
using Microsoft.Win32;
using AES;

namespace Client
{
    class Chat
    {
       


        private string self;
       
        public string Receiver
        {
            get
            {
                return receiver;
            }
            set
            {
                receiver = value;
            }
        }
        private string receiver;
        public string fileNameForSend;
        public byte[] receivedFile;
        public byte[] fileToSend;
       

        private bool IsConnected
        {
            get
            {
                return grLogIn.Visibility != Visibility.Visible;
            }
            set
            {
                if (value)
                {
                    grLogIn.Dispatcher.Invoke(
                        new Action(() =>
                        {
                            grLogIn.Visibility = Visibility.Hidden;
                        })
                    );
                }
                else
                {
                    grLogIn.Dispatcher.Invoke(
                        new Action(() =>
                        {
                            grLogIn.Visibility = Visibility.Visible;
                        })
                    );
                }
            }
        }

        private Socket socket;
        private Thread receiveThread;
        private Thread connectionThread;

        private Grid grLogIn;
        private ListBox messageBox, userBox;
        private TextBox txtboxFileForSave;
        private List<User> userList = new List<User>();

        public Chat(string nickname, IPEndPoint address,  Grid grLogIn, ListBox messagesBox, ListBox usersBox, TextBox txtReceivedFileName)
        {
            this.grLogIn = grLogIn;

            this.messageBox = messagesBox;
            messagesBox.Items.Clear();

            this.userBox = usersBox;
            usersBox.Items.Clear();
            this.txtboxFileForSave = txtReceivedFileName;

            Receiver = "";

            // Устанавливаем удаленную точку для сокета
            try
            {
                // Подключение
                socket = new Socket(address.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.ReceiveBufferSize = 50000;
                socket.Connect(address);
                SendUserName(nickname);
                self = nickname;

                // Начало приема сообщений
                receiveThread = new Thread(ReceiveMsg);
                receiveThread.Start();
            }
            catch (SocketException e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void SendUserName(string msg)
        {
            MessageClass messg = new MessageClass();
            messg.type = Symbols.USER_NAME;
            messg.container = Encoding.UTF8.GetBytes(msg);

            SendMsg(messg);
        }

        private void ReceiveMsg()
        {
            MessageClass messg;

            // Получаем ответ от сервера
            while (true)
            {
                
                try
                {
                     messg = new MessageClass();
                     messg = getMessg();

                    IsConnected = true; //?
                }
                catch (SocketException e)
                {
                    MessageBox.Show(e.Message);
                    IsConnected = false;
                    receiveThread.Abort();
                    break;
                }
                if(messg == null)
                {
                    break;
                }

                if (messg.type == Symbols.USER_LIST)
                {
                    string msg = Encoding.UTF8.GetString(messg.container);
                    ShowUsers(msg);
                }else
                if (messg.type == Symbols.REQUEST)
                {
                    getRequest(messg);
                   
                }else
                if (messg.type == Symbols.CONFIRMATION)
                {
                    if (getConfirmation(messg))
                    {
                        sendFile(messg);
                    }
                }
                else
                if (messg.type == Symbols.FILE)
                {
                    getFile(messg);

                }else
                if (messg.type == Symbols.FILE_RECIEVED)
                {
                    ShowMessage($"Файл доставлен {Receiver}");
                }else
                if (messg.type == Symbols.MSG_IS_NOT_RECEIVED)
                {
                    ShowMessage($"Сообщение не было доставлено до {Receiver}");
                }else
                if (messg.type == Symbols.NICKNAME_TAKEN)
                {
                    IsConnected = false;
                    MessageBox.Show("Имя уже занято!");
                    receiveThread.Abort();             
                }
              //  Thread.Sleep(0);
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
            if (buffer.Length != 0)
            {
                messg = MessageClass.getMessageFromBytes(buffer);
                return messg;
            }
            return null;
        }
        private void getFile(MessageClass messg)
        {
            ShowMessage("Файл "+ messg.fileName + " получен" + " от " + messg.sender);

            txtboxFileForSave.Dispatcher.Invoke(
                   new Action(() =>
                   {
                       txtboxFileForSave.Text = messg.fileName;
                       
                   })
               );

            receivedFile = messg.container;
            sendMessageThatFileIsReceived(messg);
            
        }
        private void SendMsg(MessageClass messg)
        {
            byte[] bytes = messg.getBytes();
            try
            {
                int bytesSent = socket.Send(bytes);
            }
            catch (SocketException e)
            {
                MessageBox.Show(e.Message);
            }
        }
        private void sendMessageThatFileIsReceived(MessageClass messg)
        {
            messg.formMessage(Symbols.FILE_RECIEVED, messg.sender, self, messg.fileName, messg.fullfileName,String.Empty);
            SendMsg(messg);
        }

        public void ShowMessage(string msg)
        {
            messageBox.Dispatcher.Invoke(
                new Action(() =>
                {
                    Message m = new Message(msg); ;
                    messageBox.Items.Add(m);
                    messageBox.ScrollIntoView(m);
                })
            );
        }

        private bool getConfirmation(MessageClass msg)
        {
            if (Encoding.UTF8.GetString(msg.container) == "no")
            {
                ShowMessage($"Файл был отклонён {msg.sender}");
                return false;
            }
            else
            {
                ShowMessage($"Отправка файла {msg.sender}...");
                return true;
        
            }
        }
        public void sendFile(MessageClass msg)
        {
            FileInfo file = new FileInfo(fileNameForSend);
            if (msg.fileName!= file.Name)
            {
                ShowMessage($"Проверьте отправляемый файл");
                return;
            }


            msg.container = fileToSend;
            msg.receiver = msg.sender;
            msg.sender = self;
            msg.type = Symbols.FILE;

            SendMsg(msg);

        }

        private void getRequest(MessageClass msg)
        {

            if (MessageBox.Show("Получить файл от " + msg.sender + "?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                msg.formMessage(Symbols.CONFIRMATION, msg.sender, self, msg.fileName, msg.fullfileName, "yes");
            }
            else
            {
                msg.formMessage(Symbols.CONFIRMATION, msg.sender, self, msg.fileName, msg.fullfileName, "no");
            }
            SendMsg(msg);
        }

        public void SendRequest()
        {
            FileInfo file = new FileInfo(fileNameForSend);
            MessageClass msg = new MessageClass();
            msg.formMessage(Symbols.REQUEST, Receiver ,self, file.Name, file.FullName, String.Empty);
            ShowMessage("Ожидание подтверждения oт " + Receiver + "...");

            SendMsg(msg);

        }
        private void ShowUsers(string msg)
        {
            string[] nicknames = msg.Split('\0');
            nicknames = nicknames.Take(nicknames.Count() - 1).ToArray();
            nicknames = nicknames.Where(s => s != self).ToArray();

            int oldUsersCount = userList.Count;
            int delta = nicknames.Length - oldUsersCount;

            if (delta > 0)
            {
                userBox.Dispatcher.Invoke(
                    new Action(() =>
                    {
                        for (int i = 0; i < delta; i++)
                        {
                            User user = new User(nicknames[oldUsersCount + i]);
                            userBox.Items.Add(user);
                            userList.Add(user);
                        }
                    })
                );
            }
            else if (delta<0)
            {
                userBox.Dispatcher.Invoke(
                    new Action(() =>
                    {
                        //находим элемент который нужнго удалить
                        User user = userList.Find(u => !Array.Exists(nicknames, a => a == u.nickname));
                        userBox.Items.Remove(user);
                        userList.Remove(user);

                        if (user.nickname == Receiver)
                        {
                            userBox.SelectedIndex = -1;
                            Receiver = "";
                        }
                    })
                );
            }

            if (!userBox.Items.Contains(Receiver))
            {
                Receiver = "";
            }

            if (!IsConnected)
            {
                IsConnected = true;
            }
        }

        public void Exit()
        {
            MessageClass messg = new MessageClass();
            messg.type = Symbols.EXIT;

            SendMsg(messg);

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            receiveThread.Abort();

        }
 
    }
}
