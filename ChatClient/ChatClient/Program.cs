using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.IO;

namespace ChatClient
{
    class Program
    {
        static string userName;
        static string host ;
        private const int port = 8888;
        static TcpClient client;
        static NetworkStream stream;

        static void Main(string[] args)
        {
            Console.WriteLine("Добро пожаловать в CCC. Для начала работы вам необходимо ввести IP хоста и имя пользователя.");
            Console.WriteLine("Имя пользователя может содержать буквы, цифры, любые знаки кроме %, завязку, развитие, кульминацию");
            Console.WriteLine("и неожиданный финал с двумя шутками про хохлов. Не допускается ввод пустой строки.");
            Console.WriteLine("Список основных команд чата: ");
            Console.WriteLine("%online% - Получение и вывод списка пользователей онлайн;");
            Console.WriteLine("%p% - Отправка личного сообщения определенному пользователю.");
            Console.WriteLine("После ввода команды требуется ввести имя целевого пользователя. Введите all, чтобы сделать сообщение общим;");
            Console.WriteLine("%history% - Вывод на экран истории сообщений;");
            Console.WriteLine("%exit% - Закрытие подключения и выход из чата.");
            Console.WriteLine();
            Console.WriteLine("-------------------------------------------------------------------------------------------------------");
            Console.WriteLine();       
            Console.WriteLine("Введите IP хоста: ");
            host = Console.ReadLine();
            Console.Write("Введите свое имя: ");
            userName = Console.ReadLine();
            if (userName != "")
            {
                client = new TcpClient();
                try
                {
                    client.Connect(host, port); //подключение клиента
                    stream = client.GetStream(); // получаем поток

                    string message = userName;
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    stream.Write(data, 0, data.Length);

                    // запускаем новый поток для получения данных
                    Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
                    receiveThread.Start(); //старт потока
                    Console.WriteLine("Добро пожаловать, {0}", userName);
                    StreamWriter sw = new StreamWriter("History.txt", true);
                    sw.WriteLine("Добро пожаловать, {0}", userName);
                    sw.Close();
                    SendMessage();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    Disconnect();
                }
            }
            else
            {
                Console.WriteLine("Неподходящее имя пользователя.");
                Console.ReadLine();
            }
        }

        // отправка сообщений
        static void SendMessage()
        {
            string target = "";
            Console.WriteLine("Введите сообщение {0}: ", target);
            while (true)
            {      
                string message = Console.ReadLine();
                if (message == "%online%")
                {
                    Console.WriteLine("Список пользователей онлайн: ");
                    byte[] data = Encoding.Unicode.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                }
                else if (message == "%exit%")
                {
                    Console.WriteLine("Выход из программы");
                    Disconnect();
                }
                else if (message == "%p%")
                {
                    Console.WriteLine("Введите имя целевого пользователя: ");
                    target = Console.ReadLine(); 
                    
                    if (target == "all")
                    {
                        target = "";
                        Console.WriteLine("Сообщения для всех пользователей");
                    }
                    Console.WriteLine("Введите сообщение {0}: ", target);
                    continue;
                } 
                else if (message == "%history%")
                {
                    Console.WriteLine();
                    Console.WriteLine("--------------------------------------История сообщений---------------------------------------------");
                    Console.WriteLine();
                    StreamReader sr = new StreamReader("History.txt", true);
                    string sline = "";
                    sline = sr.ReadLine();
                    while (sline != null && sline != "")
                    {
                        Console.WriteLine(sline);
                        sline = sr.ReadLine();
                    }
                    sr.Close();
                    Console.WriteLine("-----------------------------------------------------------------------------------------------------");
                    Console.WriteLine("Введите сообщение {0}: ", target);
                }
                else
                {
                    if (target != "")
                    {
                        StreamWriter sw = new StreamWriter("History.txt", true);
                        sw.WriteLine(message);
                        sw.Close();
                        message = "%p%" + target + "^" + message;
                        byte[] data = Encoding.Unicode.GetBytes(message);
                        stream.Write(data, 0, data.Length);
                        Console.WriteLine("Введите сообщение {0}: ", target);
                    }
                    else
                    {
                        StreamWriter sw = new StreamWriter("History.txt", true);
                        sw.WriteLine(message);
                        sw.Close();
                        byte[] data = Encoding.Unicode.GetBytes(message);
                        stream.Write(data, 0, data.Length);
                        Console.WriteLine("Введите сообщение {0}: ", target);
                    }
                }
            }
        }
        // получение сообщений
        static void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    byte[] data = new byte[64]; // буфер для получаемых данных
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (stream.DataAvailable);

                    string message = builder.ToString();
                    Console.WriteLine(message);//вывод сообщения
                    StreamWriter sw = new StreamWriter("History.txt", true);
                    sw.WriteLine(message);
                    sw.Close();
                }
                catch
                {              
                    Console.WriteLine("Подключение прервано!"); //соединение было прервано
                    Disconnect();
                }
            }
        }

        static void Disconnect()
        {
            Console.WriteLine("Выход из программы");
            byte[] data = Encoding.Unicode.GetBytes("%exit%");
            stream.Write(data, 0, data.Length);
            if (stream != null)
                stream.Close();//отключение потока
            if (client != null)
                client.Close();//отключение клиента
            Environment.Exit(0); //завершение процесса
        }
    }

}
