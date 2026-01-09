using System.Net;
using System.Net.Sockets;
using Spectre.Console;

using RPGFramework;
using RPGFramework.Display;
using RPGFramework.Commands;
using RPGFramework.Enums;
using RPGFramework.Geography;
using RPGFramework.Workflows;
using System.Text;


internal class TelnetServer
{
    private TcpListener _listener;
    private bool _isRunning;
    

    public TelnetServer(int port)
    {
        _listener = new TcpListener(IPAddress.Any, port);
    }

    public async Task StartAsync()
    {
        _listener.Start();
        _isRunning = true;
        GameState.Log(DebugLevel.Alert, "Telnet Server is running...");

        while (_isRunning && GameState.Instance.IsRunning)
        {
            TcpClient client = await _listener.AcceptTcpClientAsync();
            _ = HandleClientAsync(client);
        }

        GameState.Log(DebugLevel.Alert, "Shutting down Telnet Server...");
        _listener.Stop();
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        using (client)
        {
            // Create PlayerNetwork object, once logged in we'll attach it to player
            PlayerNetwork pn = new PlayerNetwork(client);
            var telnetConnection = new TelnetConnection(client.GetStream(), Encoding.UTF8);

            pn.Writer.WriteLine("Username: ");
            //string? playerName = pn.Reader.ReadLine();
            string? playerName = telnetConnection.ReadLine();
            
            while (string.IsNullOrEmpty(playerName))
            {
                pn.Writer.WriteLine("Username: ");
                playerName = pn.Reader.ReadLine();
            }
            
            GameState.Log(DebugLevel.Debug, $"Player '{playerName}' is connecting...");
            Player player;

            // If existing player
            if (GameState.Instance.Players.ContainsKey(playerName))
            {
                GameState.Log(DebugLevel.Debug, $"Existing player '{playerName}' found, loading data...");
                player = GameState.Instance.Players[playerName];                
            }
            else
            {
                GameState.Log(DebugLevel.Debug, $"No existing player '{playerName}' found, creating new player...");
                // New player creation (class, etc)
                player = new Player(client, playerName);
                player.CurrentWorkflow = new WorkflowOnboarding();
                GameState.Instance.AddPlayer(player);
                player.WriteLine("Welcome, new adventurer! Type start and hit enter to get going");
            }

            player.Network = pn;
            player.Login();

            // MOTD Should Be Settable in Game Settings
            player.Write(RPGPanel.GetPanel("Welcome to the game!", "Welcome!"));
            MapRenderer.RenderLocalMap(player);

            GameState.Log(DebugLevel.Alert, $"Player '{playerName}' has connected successfully.");

            // Listen for and process commands
            // TODO We might want to use TelnetConnection instead if we want to fully support telnet protocol
            try
            {
                while (client.Connected)
                {
                    string command = await player.Network.Reader.ReadLineAsync();
                    if (command == null)
                        break;

                    GameState.Log(DebugLevel.Debug, $"Received command from '{player.Name}': {command}");
                    CommandManager.Process(player, command);
                }
            }
            catch (Exception ex)
            {
                // Should this use GameState.Log instead?
                AnsiConsole.WriteException(ex);
            }
            finally
            {
                GameState.Log(DebugLevel.Alert, $"Player '{playerName}' has disconnected.");
            }
        } // end using client
    }

    public void Stop()
    {
        _isRunning = false;
    }

}
