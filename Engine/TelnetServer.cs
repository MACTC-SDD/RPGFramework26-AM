using System.Net;
using System.Net.Sockets;
using Spectre.Console;

using RPGFramework;
using RPGFramework.Display;
using RPGFramework.Commands;
using RPGFramework.Geography;
using RPGFramework.Workflows;

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
        Console.WriteLine("Telnet Server is running...");

        while (_isRunning && GameState.Instance.IsRunning)
        {
            TcpClient client = await _listener.AcceptTcpClientAsync();
            _ = HandleClientAsync(client);
        }

        Console.WriteLine("Telnet Server is stopped...");
        _listener.Stop();
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        using (client)
        {
            // Create PlayerNetwork object, once logged in we'll attach it to player
            PlayerNetwork pn = new PlayerNetwork(client);
            pn.Writer.WriteLine("Username: ");
            string? playerName = pn.Reader.ReadLine();
            
            while (string.IsNullOrEmpty(playerName))
            {
                pn.Writer.WriteLine("Username: ");
                playerName = pn.Reader.ReadLine();
            }
            
            Player player;

            // If existing player
            if (GameState.Instance.Players.ContainsKey(playerName))
            {
                player = GameState.Instance.Players[playerName];                
            }
            else
            {
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

            Console.WriteLine("New client connected!");

            // Listen for and process commands
            try
            {
                while (client.Connected)
                {
                    string command = await player.Network.Reader.ReadLineAsync();
                    if (command == null)
                        break;

                    Console.WriteLine($"Received command: {command}");
                    CommandManager.Process(player, command);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Connection error: {ex.Message}");
                AnsiConsole.WriteException(ex);
            }
            finally
            {
                Console.WriteLine("Client disconnected");
            }
        } // end using client
    }

    public void Stop()
    {
        _isRunning = false;
    }

}
