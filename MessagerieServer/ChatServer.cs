using System.Net;
using System.Net.Sockets;

namespace MessagerieServer;

internal class ChatServer
{
    private const int Port = 5000;
    private static TcpListener _listener;
    private static readonly List<Client> Clients = [];

    private static void Main()
    {
        RunServer();
    }

    private static void RunServer()
    {
        try
        {
            _listener = new TcpListener(IPAddress.Any, Port);
            _listener.Start();
            
            Console.WriteLine($"Serveur démarré sur le port {Port}.");

            while (true)
            {
                var tcpClient = _listener.AcceptTcpClient();
                var client = new Client(Clients);
                Clients.Add(client);
                Thread clientThread = new(client.HandleClient) { IsBackground = true };
                clientThread.Start(tcpClient);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Erreur du serveur : {e.Message}");
        }
        finally
        {
            StopServer();
        }
    }

    private static void StopServer()
    {
        try
        {
            _listener.Stop();
            foreach (var client in Clients) client.GetTcpClient().Close();
        }
        catch (Exception e)
        {
            // ignored
        }

        Clients.Clear();
        Console.WriteLine("Serveur arrêté");
    }
    
}