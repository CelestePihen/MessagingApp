using System.Windows;

namespace MessagerieClient;

public partial class Login
{
    private MainWindow? _mainWindow;

    public Login()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Gère le bouton "Se connecter"
    /// </summary>
    private void LoginButton(object sender, RoutedEventArgs e)
    {
        var username = TxtMessage.Text.Trim();
        if (string.IsNullOrWhiteSpace(username)) return;

        _mainWindow = new MainWindow("127.0.0.1", 5000);

        if (!_mainWindow.ConnectToServer(username)) return;
        _mainWindow.Show();
        Close();
    }
}