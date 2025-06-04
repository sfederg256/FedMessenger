using System;
using System.Net.Sockets;
using System.Text;

namespace ConsoleClient
{
    public class NetworkClient
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private readonly string serverIpAddress;
        private readonly int serverPort;
        private const int bufferSize = 4096;

        public NetworkClient(string ipAddress, int port)
        {
            serverIpAddress = ipAddress;
            serverPort = port;
        }

        public void Connect()
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(serverIpAddress, serverPort);
            stream = tcpClient.GetStream();
        }

        public void SendMessage(string message)
        {
            if (stream == null)
                throw new InvalidOperationException("Нет соединения с сервером.");

            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        public string ReceiveMessage()
        {
            byte[] buffer = new byte[bufferSize];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer, 0, bytesRead);
        }

        public void Disconnect()
        {
            stream?.Close();
            tcpClient?.Close();
        }
    }
}