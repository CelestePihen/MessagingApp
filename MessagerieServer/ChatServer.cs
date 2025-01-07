namespace MessagerieServer;

using System.Net;
using System.Net.Sockets;
using System.Text;

class ChatServer
{
    private const int Port = 5000;
    private static TcpListener _listener;
    private static readonly List<TcpClient> Clients = [];
    private static readonly List<string> ClientNames = [];
    
    static void Main()
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
                TcpClient client = _listener.AcceptTcpClient();
                Clients.Add(client);
                Thread clientThread = new(HandleClient) { IsBackground = true };
                clientThread.Start(client);
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
        _listener.Stop();
        foreach (var client in Clients)
        {
            client.Close();
        }
        Clients.Clear();
        ClientNames.Clear();
        Console.WriteLine("Serveur arrêté");
    }

    private static void HandleClient(object clientObj)
    {
        TcpClient client = (TcpClient) clientObj;
        string clientName = null;
        try
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            
            clientName = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
            ClientNames.Add(clientName);
            Console.WriteLine($"{clientName} s'est connecté");
            BroadcastClientList();
            
            while (true)
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    Console.WriteLine($"{clientName} s'est déconnecté.");
                    break;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                Broadcast($"{clientName} : {message}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Erreur avec le client {clientName} : {e.Message}");
        }
        finally
        {
            if (clientName != null)
            {
                ClientNames.Remove(clientName);
                Clients.Remove(client);   
            }
            client.Close();
            Broadcast($"{clientName} a quitté le chat.");
            BroadcastClientList();
        }
    }

    private static void BroadcastClientList()
    {
        var clientListMessage = "/clients" + string.Join(",", ClientNames);
        byte[] buffer = Encoding.UTF8.GetBytes(clientListMessage);

        foreach (var stream in Clients.Select(client => client.GetStream()))
        {
            stream.Write(buffer, 0, buffer.Length);
        }
    }

    /// <summary>
    /// Envoie un message à tous les clients
    /// </summary>
    /// <param name="message">Le message à envoyer</param>
    private static void Broadcast(string message)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(message);
        foreach (var client in Clients)
        {
            try
            {
                client.GetStream().Write(buffer, 0, buffer.Length);
            }
            catch
            {
                // ignored
            }
        }
    }
    
}