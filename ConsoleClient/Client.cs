using System;
using System.Linq;

namespace ConsoleClient
{
    enum Action
    {
        LOG_IN = 1,
        REGISTER = 2
    }

    class Client
    {
        static void Main(string[] args)
        {
            Console.WriteLine("----------------------------------- FedMessenger ---------------------------------------");
            Console.WriteLine("[1] - Вход");
            Console.WriteLine("[2] - Регистрация");
            Console.Write("Введите номер действия: ");

            if (!int.TryParse(Console.ReadLine(), out int actionInput) || !Enum.IsDefined(typeof(Action), actionInput))
            {
                Console.WriteLine("Неверный выбор действия.");
                return;
            }

            Action action = (Action)actionInput;

            string userName = "", userEmail = "", userPassword = "";

            if (action == Action.REGISTER)
            {
                Console.Write("Введите username: ");
                userName = Console.ReadLine();
            }

            Console.Write("Введите email: ");
            userEmail = Console.ReadLine();

            Console.Write("Введите пароль: ");
            userPassword = Console.ReadLine();

            string message = action == Action.REGISTER
                ? $"register|user_name:{userName},user_email:{userEmail},user_password:{userPassword}|"
                : $"log_in|user_email:{userEmail},user_password:{userPassword}|";

            NetworkClient client = new NetworkClient("127.0.0.1", 8888);

            try
            {
                client.Connect();
                client.SendMessage(message);
                string response = client.ReceiveMessage();
                Console.WriteLine($"Ответ сервера: {response}");

                if (response.StartsWith("success"))
                {
                    MessagingLoop(client, userEmail);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                client.Disconnect();
                Console.WriteLine("Нажмите любую клавишу для выхода...");
                Console.ReadKey();
            }
        }

        static void MessagingLoop(NetworkClient client, string senderEmail)
        {
            while (true)
            {
                Console.WriteLine("[1] Отправить сообщение");
                Console.WriteLine("[2] Проверить входящие сообщения");
                Console.WriteLine("[0] Выйти");

                Console.Write("Ваш выбор: ");
                string choice = Console.ReadLine();

                if (choice == "1")
                {
                    Console.WriteLine("Пользователи онлайн:");
                    client.SendMessage("get_online_users|");
                    string response = client.ReceiveMessage();

                    if (!response.StartsWith("online_users|"))
                    {
                        Console.WriteLine($"Ошибка получения списка пользователей: {response}");
                        return;
                    }

                    string emailsRaw = response.Substring("online_users|".Length);
                    var emailList = emailsRaw
                        .Split(';', (char)StringSplitOptions.RemoveEmptyEntries)
                        .Select(e => e.Trim())
                        .ToList();

                    if (emailList.Count == 0)
                    {
                        Console.WriteLine("Сейчас никто не в сети.");
                        return;
                    }

                    foreach (var email in emailList)
                    {
                        Console.WriteLine($"- {email}");
                    }

                    while (true)
                    {
                        Console.Write("Кому (email) или '0' для отмены: ");
                        string to = Console.ReadLine().Trim();

                        if (to == "0")
                        {
                            Console.WriteLine("Отправка сообщения отменена.");
                            break;
                        }

                        if (!emailList.Contains(to))
                        {
                            Console.WriteLine("Ошибка: указанный email не в списке пользователей онлайн. Попробуйте снова.");
                            continue;
                        }

                        Console.Write("Текст: ");
                        string text = Console.ReadLine();
                        string msg = $"message|from:{senderEmail},to:{to},text:{text}|";
                        client.SendMessage(msg);
                        Console.WriteLine(client.ReceiveMessage());
                        break;
                    }
                }


                else if (choice == "2")
                {
                    string getMsg = $"get_messages|user_email:{senderEmail}|";
                    client.SendMessage(getMsg);
                    string response = client.ReceiveMessage();

                    if (response.StartsWith("messages|"))
                    {
                        var msgBody = response.Substring("messages|".Length);
                        var messages = msgBody.Split(';', (char)StringSplitOptions.RemoveEmptyEntries);
                        Console.WriteLine("Входящие сообщения:");
                        foreach (var msg in messages)
                        {
                            Console.WriteLine(msg.Replace(",", "\n") + "\n---");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Ответ сервера: {response}");
                    }
                }
                else if (choice == "0")
                {
                    break;
                }
            }
        }
    }
}