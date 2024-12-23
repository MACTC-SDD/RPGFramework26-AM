using RPGFramework;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

public class TelnetServer
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

        while (_isRunning)
        {
            TcpClient client = await _listener.AcceptTcpClientAsync();
            _ = HandleClientAsync(client);
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        using (client)
        {

            // TODO: Handle Login
            // Populate player object and attach to client

            // Loop here until player name is entered
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

            if (GameState.Instance.Players.ContainsKey(playerName))
            {
                player = GameState.Instance.Players[playerName];

            }
            else
            {
                player = new Player(client, playerName);
                GameState.Instance.AddPlayer(player);
            }

            player.Network = pn;
            player.IsOnline = true;
            player.LastLogin = DateTime.Now;
           

            await player.Network.Writer.WriteLineAsync("MOTD");
            Console.WriteLine("New client connected!");

            try
            {
                while (client.Connected)
                {
                    string command = await player.Network.Reader.ReadLineAsync();
                    if (command == null)
                        break;

                    Console.WriteLine($"Received command: {command}");
                    Command.Process(player, command);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
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
        _listener.Stop();
    }

}
