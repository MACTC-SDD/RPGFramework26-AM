using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RPGFramework
{
    public class PlayerNetwork
    {
        public TcpClient Client { get; }
        public StreamWriter Writer { get; }
        public StreamReader Reader { get; }

        public PlayerNetwork(TcpClient client)
        {
            Client = client;
           
            NetworkStream stream = client.GetStream();
            Writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };
            Reader = new StreamReader(stream, Encoding.ASCII);
        }
    }
}
