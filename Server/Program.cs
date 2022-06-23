using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Server
{
    class Program
    {
        static private List<User> users = new List<User>();
        static void Main(string[] args)
        {
           
            Console.Write("Ip: ");
            string str_ip = Console.ReadLine();
            Console.Write("Port: ");
            int port = Int32.Parse(Console.ReadLine());
           
            IPAddress ipAddr = IPAddress.Parse(str_ip);
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

            // Создаем сокет TCP/IP
            Socket listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Назначаем сокет локальной конечной точке и слушаем входящие сокеты
            try
            {
                listener.Bind(ipEndPoint);
                listener.Listen(10);
                Console.WriteLine("Ожидаем подключение клиента.");

                // Начинаем слушать соеденения
                while (true)
                {
                    // Программа приостанавливается, ожидая вхдящее соединение
                    UserThread userThread = new UserThread(listener.Accept(), users);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                listener.Close();
                Console.ReadLine();
            }
        }
    }
}
