using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MatchingLib
{
    public class SimpleTcpClient : IDisposable
    {
        internal const int BufferSize = 1024;
        byte[] buffer { get; } = new byte[BufferSize];
        public Socket client { get; set; }
        public delegate void LogInfoEvent(string info);
        public LogInfoEvent LogInfo { get; set; }
        public delegate void LogErrorEvent(string error);
        public LogErrorEvent LogError { get; set; }
        public Exception exception { get; private set; } = null;
        string Dest { get; set; }
        int Port { get; set; }
        public SimpleTcpClient(string dest, int port)
        {
            // Establish the remote endpoint for the socket.  
            Dest = dest;
            Port = port;

            // Create a TCP/IP socket.  
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        public void Connect()
        {
            client.Connect(Dest, Port);
        }
        public void Send(byte[] bytes, int size)
        {
            // Connect to the remote endpoint.  
            client.Send(bytes, size, SocketFlags.None);
        }
        public int Receive(out byte[] bytes)
        {
            bytes = buffer;
            return client.Receive(buffer);
        }
        public void Shutdown()
        {
            client.Shutdown(SocketShutdown.Both);
            client.Disconnect(true);
            //client.Close();
        }

        public void Dispose()
        {
            client.Disconnect(false);
        }
    }
}
