using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server.Networking
{
    public class TcpServer
    {
        private readonly MessageRepository messageRepository;
        private readonly Dictionary<string, ConnectedClient> connectedClients = new Dictionary<string, ConnectedClient>();
        private readonly object clientLock = new object();
        private readonly TcpListener listener;
        private readonly AuthService authService;

        public TcpServer(string dbConnectionString, int port = 8888)
        {
            var userRepository = new UserRepository(dbConnectionString);
            authService = new AuthService(userRepository);
            messageRepository = new MessageRepository(dbConnectionString);
            listener = new TcpListener(IPAddress.Any, port);
        }

        public void Start()
        {
            listener.Start();
            Console.WriteLine("Сервер запущен...");

            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                ThreadPool.QueueUserWorkItem(HandleClient, client);
            }
        }

        private void HandleClient(object obj)
        {
            var client = (TcpClient)obj;
            var stream = client.GetStream();
            var buffer = new byte[1024];

            try
            {
                while (true)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break; 

                    var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    var response = ProcessMessage(message, client);
                    var responseData = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseData, 0, responseData.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка клиента: {ex.Message}");
            }
            finally
            {
                client.Close();
            }
        }


        private string HandleMessageCommand(Dictionary<string, string> data)
        {
            if (!data.ContainsKey("to") || !data.ContainsKey("text") || !data.ContainsKey("from"))
                return "error|Некорректные параметры сообщения.|";

            string to = data["to"];
            string text = data["text"];
            string from = data["from"];

            var msg = new Message
            {
                From = from,
                To = to,
                Text = text,
                Timestamp = DateTime.Now
            };

            lock (clientLock)
            {
                messageRepository.SaveMessage(msg);

                if (connectedClients.TryGetValue(to, out ConnectedClient recipientClient))
                {
                    string formatted = $"message|from:{from},text:{text}|";
                    byte[] msgBytes = Encoding.UTF8.GetBytes(formatted);
                    recipientClient.Stream.Write(msgBytes, 0, msgBytes.Length);
                }
            }

            return "success|Сообщение сохранено (и отправлено если получатель в сети).|";
        }

        private string HandleGetMessagesCommand(Dictionary<string, string> data)
        {
            if (!data.ContainsKey("user_email"))
                return "error|Не указан email.|";

            var email = data["user_email"];
            var messages = messageRepository.GetMessagesForUser(email);
            if (messages.Count == 0)
                return "success|Нет новых сообщений.|";

            StringBuilder sb = new StringBuilder("messages|");
            foreach (var msg in messages)
            {
                sb.Append($"from:{msg.From},text:{msg.Text},time:{msg.Timestamp:yyyy-MM-dd HH:mm};");
            }

            return sb.ToString();
        }

        private string HandleGetOnlineUsers()
        {
            StringBuilder sb = new StringBuilder("online_users|");

            lock (clientLock)
            {
                foreach (var client in connectedClients.Values)
                {
                    sb.Append($"{client.Email};");
                }
            }

            return sb.ToString();
        }



        private string ProcessMessage(string message, TcpClient client)
        {
            var parts = message.Split('|');
            if (parts.Length < 2) return "error|Неверный формат команды.|";

            var command = parts[0].ToLower();
            var data = ParseParams(parts[1]);

            string result;

            switch (command)
            {
                case "register":
                    result = authService.Register(data["user_name"], data["user_email"], data["user_password"]);
                    break;

                case "log_in":
                    result = authService.Authenticate(data["user_email"], data["user_password"]);
                    if (result.StartsWith("success"))
                    {
                        var currentClient = new ConnectedClient(client)
                        {
                            Email = data["user_email"]
                        };

                        lock (clientLock)
                        {
                            connectedClients[data["user_email"]] = currentClient;
                        }
                    }
                    break;

                case "message":
                    result = HandleMessageCommand(data);
                    break;
                case "get_messages":
                    result = HandleGetMessagesCommand(data);
                    break;
                case "get_online_users":
                    result = HandleGetOnlineUsers();
                    break;


                default:
                    result = "error|Неизвестная команда.|";
                    break;
            }

            return result;
        }


        private Dictionary<string, string> ParseParams(string data)
        {
            return data.Split(',').Select(p => p.Split(':'))
                .Where(p => p.Length == 2)
                .ToDictionary(p => p[0].Trim(), p => p[1].Trim());
        }
    }
}
