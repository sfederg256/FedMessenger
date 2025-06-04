using Server.Networking;

class Program
{
    static void Main(string[] args)
    {
        string connectionString = "Server=localhost;Port=5432;Database=fedmessenger;User Id=postgres;Password=fed16shell;";
        var server = new TcpServer(connectionString);
        server.Start();
    }
}
