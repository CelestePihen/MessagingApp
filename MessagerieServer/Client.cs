using System.Net.Sockets;
using System.Text;

namespace MessagerieServer;

public class Client(List<Client> clients)
{
    private TcpClient _tcpClient;
    private string _clientName;
    
    public TcpClient GetTcpClient()
    {
        return _tcpClient;
    }

    public void HandleClient(object clientObj)
    {
        _tcpClient = (TcpClient) clientObj;
        
        try
        {
            var stream = _tcpClient.GetStream();
            var buffer = new byte[1024];

            var bytesRead = stream.Read(buffer, 0, buffer.Length);
            _clientName = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
            
            Console.WriteLine($"{_clientName} s'est connecté");

            while (true)
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    Console.WriteLine($"{_clientName} s'est déconnecté.");
                    break;
                }

                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                SendMessage($"{_clientName} : {message}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Erreur avec le client {_clientName} : {e.Message}");
        }
        finally
        {
            if (_tcpClient != null)
            {
                SendMessage($"{_clientName} a quitté le chat.");
                _tcpClient.Close();
                clients.Remove(this);
            }
        }
    }
    
    /// <summary>
    ///     Envoie un message à tous les autres clients du serveur
    /// </summary>
    /// <param name="message">Le message à envoyer</param>
    private void SendMessage(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        foreach (var client in clients) client.GetTcpClient().GetStream().Write(buffer, 0, buffer.Length);
    }
    
}