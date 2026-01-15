using System.Net.Sockets;
using System.Text;


namespace RPGFramework
{
    internal class PlayerNetwork
    {
        public TcpClient Client { get; }
        public TelnetConnection? TelnetConnection { get; set; }
        public StreamWriter Writer { get; }
        public StreamReader Reader { get; }
        public bool NeedsOutputNewline { get { return _needsOutputNewLine; } }

        private bool _needsOutputNewLine;
        public PlayerNetwork(TcpClient client)
        {
            Client = client;

            NetworkStream stream = client.GetStream();

            Writer = new StreamWriter(stream, new UTF8Encoding(false))
            {
                AutoFlush = true,
                NewLine = "\r\n"
            };

            //Writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };
            //Reader = new StreamReader(stream, Encoding.UTF8);
            TelnetConnection = new TelnetConnection(stream, Encoding.UTF8);
            TelnetConnection.OnlineCommitted = () => _needsOutputNewLine = true;
        }
    }
}
