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
using System.Windows.Shapes;

namespace Client.Classes
{
    /// <summary>
    /// Логика взаимодействия для Message.xaml
    /// </summary>
    public partial class Message : UserControl
    {
       // public string nickname;
        public string msg;
        public string time;

        public Message(string msg)
        {
            InitializeComponent();
            DateTime date1 = new DateTime();
            date1 = DateTime.Now;
            this.msg = msg;
            this.time = date1.ToShortTimeString();

            txtblockTime.Text = this.time;
            txtblockMessage.Text = msg;
        }
    }
}
