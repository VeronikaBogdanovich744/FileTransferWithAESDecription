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
using System.Windows.Navigation;
using System.Threading;
//using System.Windows.Shapes;
using Microsoft.Win32;
using Client.Classes;
using System.IO;
using AES;
using System.Net;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private Chat chat;

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
                    grLogIn.Visibility = Visibility.Hidden;
                }
                else
                {
                    grLogIn.Visibility = Visibility.Visible;
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IsConnected = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsConnected)
            {
                chat?.Exit();
            }
        }

        private void btnLogIn_Click(object sender, RoutedEventArgs e)
        {
                if (txtboxNickname.Text != String.Empty && txtboxIP.Text!=String.Empty && txtboxPort.Text!=String.Empty)
            {
                     try
                     {
                    IPAddress ipAddr = IPAddress.Parse(txtboxIP.Text);
                    int port = Int32.Parse(txtboxPort.Text);
                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);
                    chat = new Chat(txtboxNickname.Text, ipEndPoint, grLogIn, listboxMessages, listboxUsers, txtboxFileForSave);
                    txtblockNickname.Text = txtboxNickname.Text;
                    }
                    catch
                    {
                    MessageBox.Show("Ошибка подключения!");
                    }
                }
        }
        private AES128 chifer;
        private void btnChooseFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                var filelength = new FileInfo(openFileDialog.FileName).Length;
                if (filelength > 50000000)
                { //50mb
                    chat.ShowMessage("Ошибка. Размер файла превышает 50 Mb. ");
                    return;
                }
                chat.fileNameForSend = openFileDialog.FileName;
                txtboxMessage.Text = Path.GetFileName(openFileDialog.FileName);
                chat.fileToSend = File.ReadAllBytes(chat.fileNameForSend);

                chifer = new AES128();
                txtboxSecretKey2.Text = Convert.ToBase64String(chifer.Key);
                txtboxIV2.Text = Convert.ToBase64String(chifer.IV);
            }
        }
        private void btnSendFile_Click(object sender, RoutedEventArgs e)
        {
            if (chat.Receiver == "")
            {
                chat.ShowMessage("Выберите пользователя!");
                return;
            }
            if (chat.fileToSend==null)
            {
                chat.ShowMessage("Выберите файл!");
                return;
            }
            chat.SendRequest();
        }

        private void listboxUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (chat == null || e.AddedItems.Count == 0)
                return;

            txtblockReceiver.Text = ((User)e.AddedItems[0]).nickname;
            chat.Receiver = ((User)e.AddedItems[0]).nickname;
          
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = txtboxFileForSave.Text;
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllBytes(saveFileDialog.FileName, chat.receivedFile);
                chat.ShowMessage("Файл сохранён");
            }
        }

        private void btnChiferFile_Click(object sender, RoutedEventArgs e)
        {
            if(chat.fileToSend == null)
            {
                chat.ShowMessage("Выберите файл!");
                return;
            }
            chat.fileToSend = chifer.ToAes128(chat.fileToSend);
            chat.ShowMessage("Файл зашифрован");
        }

        private void DechiferFile_Click(object sender, RoutedEventArgs e)
        {
            
            if (txtboxSecretKey.Text=="" || txtboxIV.Text == "")
            {
                chat.ShowMessage("Введите значения ключей");
                return;
            }
            if (chifer == null)
            {
                chifer = new AES128();
            }
            try
            {
                chifer.Key = Convert.FromBase64String(txtboxSecretKey.Text);
                chifer.IV = Convert.FromBase64String(txtboxIV.Text);
                chat.receivedFile = chifer.FromAes128(chat.receivedFile);
            }
            catch
            {
                chat.ShowMessage("Ошибка расшифровки!");
                return;
            }
            chat.ShowMessage("Файл расшифрован");
        }
    }
}
