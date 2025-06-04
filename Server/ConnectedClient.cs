using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ConnectedClient
    {
        public TcpClient TcpClient { get; }
        public NetworkStream Stream { get; }
        public string Email { get; set; } 

        public ConnectedClient(TcpClient client)
        {
            TcpClient = client;
            Stream = client.GetStream();
        }
    }
}
