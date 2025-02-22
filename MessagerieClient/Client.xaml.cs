using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace MessagerieClient;

public partial class MainWindow : Window
{
    private readonly string _ip;
    private readonly int _port;
    private TcpClient? _client;
    private NetworkStream _stream;

    public MainWindow(string ip, int port)
    {
        _ip = ip;
        _port = port;

        InitializeComponent();
    }

    public bool ConnectToServer(string username)
    {
        try
        {
            _client = new TcpClient(_ip, _port);
            _stream = _client.GetStream();

            // Envoyer le pseudo
            Send(username);

            // Démarrer un thread pour écouter les messages
            Thread receiveThread = new(ReceiveMessages);
            receiveThread.Start();
            return true;
        }
        catch (SocketException e)
        {
            MessageBox.Show($"Impossible de se connecter au serveur : {e.Message}", "Erreur de connexion");
            return false;
        }
        catch (Exception e)
        {
            MessageBox.Show($"Erreur : {e.Message}", "Erreur innatendue");
            _client?.Close();
            return false;
        }
    }

    private void Send(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        _stream.Write(buffer, 0, buffer.Length);
    }

    private void SendMessageButton(object sender, RoutedEventArgs e)
    {
        var message = txtMessage.Text.Trim();
        if (string.IsNullOrWhiteSpace(message)) return;
        Send(message);
        txtMessage.Clear();
    }

    /// <summary>
    ///     Permet de recevoir les messages venant du serveur
    /// </summary>
    private void ReceiveMessages()
    {
        var buffer = new byte[1024];
        while (true)
            try
            {
                var bytesRead = _stream.Read(buffer, 0, buffer.Length);

                // Serveur fermé/crash
                if (bytesRead == 0) break;

                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // permet de mettre à jour la liste des clients connectés
                if (message.StartsWith("/clients"))
                {
                    var clients = message[8..].Split(',');
                    Dispatcher.Invoke(() =>
                    {
                        connectedClients.Items.Clear();
                        foreach (var clientName in clients) connectedClients.Items.Add(clientName);
                    });
                }
                else
                {
                    Dispatcher.Invoke(() => listMessages.Items.Add(message));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                break;
            }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        Disconnect();
        Application.Current.Shutdown();
    }

    private void Disconnect()
    {
        if (_client == null) return;
        _stream.Close();
        _client.Close();
    }
}