using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace MessagerieClient;

public partial class MainWindow
{
    private readonly string _ip;
    private readonly int _port;
    private TcpClient? _client;
    private NetworkStream? _stream;

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
            MessageBox.Show("Impossible de se connecter au serveur : " + e.Message, "Erreur de connexion");
            return false;
        }
        catch (Exception e)
        {
            MessageBox.Show("Erreur : " + e.Message, "Erreur innatendue");
            _client?.Close();
            return false;
        }
    }
    private void SendMessageButton(object sender, RoutedEventArgs e)
    {
        var message = TxtMessage.Text.Trim();
        if (string.IsNullOrWhiteSpace(message)) return;
        Send(message);
        TxtMessage.Clear();
    }

    // TODO transformer en envoi de packet avec ID
    private void Send(string message)
    {
        var buffer = Encoding.UTF8.GetBytes(message);
        Console.WriteLine(buffer);
        _stream?.Write(buffer, 0, buffer.Length);
    }
    
    // TODO transformer en reçu de packet avec ID
    /// <summary>
    ///     Reçoit les messages venant (des autres clients) du serveur
    /// </summary>
    private void ReceiveMessages()
    {
        var buffer = new byte[1024];
        while (true)
            try
            {
                if (_stream == null) continue;
                var bytesRead = _stream.Read(buffer, 0, buffer.Length);

                // Serveur fermé/crash
                if (bytesRead == 0) break;

                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Dispatcher.Invoke(() => ListMessages.Items.Add(message));
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
        _stream?.Close();
        _client.Close();
    }

    private enum PacketId
    {
        CONNECT_TO_SERVER,
        SEND_MESSAGE,
        RECEIVE_MESSAGE
    }

}