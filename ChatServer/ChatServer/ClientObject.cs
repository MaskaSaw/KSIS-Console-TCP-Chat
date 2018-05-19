using System;
using System.Net.Sockets;
using System.Text;

namespace ChatServer
{
    public class ClientObject
    {
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        public string userName;
        TcpClient client;
        ServerObject server; // объект сервера

        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }

        public void Process()
        {
            try
            {
                Stream = client.GetStream();
                // получаем имя пользователя
                string message = GetMessage();
                userName = message;

                message = userName + " вошел в чат";
                // посылаем сообщение о входе в чат всем подключенным пользователям
                server.BroadcastMessage(message, this.Id, null);
                Console.WriteLine(message);
                // в бесконечном цикле получаем сообщения от клиента
                while (true)
                {
                    try
                    {
                        if (message == "")
                            break;
                        message = GetMessage();
                         if (message == "%exit%")
                        {
                            message = String.Format("{0}: покинул чат", userName);
                            Console.WriteLine(message);
                            break;
                        }
                        else if (message.Contains("%p%"))
                        {
                            string add = message.Substring(3, message.IndexOf("^")-3);
                            message = message.Substring(message.IndexOf("^") + 1, message.Length - message.IndexOf("^") - 1);
                            Console.WriteLine("(Личное от {0} к {1}): {2}", userName, add, message);
                            message = String.Format("(личное от {0}): {1}", userName, message); 
                            server.BroadcastMessage(message, this.Id, add);
                        }
                        else if (message == "%online%")
                        {
                            string add = "online";
                            server.BroadcastMessage(message, this.Id, add);
                            Console.WriteLine("{0} запросил список пользователей онлайн", userName);

                        }
                        else
                        {
                            message = String.Format("{0}: {1}", userName, message);
                            Console.WriteLine(message);
                            server.BroadcastMessage(message, this.Id, null);
                        }
                        message = null;
                    }
                    catch 
                    {
                        message = String.Format("{0}: покинул чат", userName);
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this.Id, null);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                // в случае выхода из цикла закрываем ресурсы
                server.RemoveConnection(this.Id);
                Close();
            }
        }

        // чтение входящего сообщения и преобразование в строку
        private string GetMessage()
        {
            byte[] data = new byte[64]; // буфер для получаемых данных
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }

        // закрытие подключения
        protected internal int Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
            return 0;
        }
    }
}
