using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using BaseHelper;
using static MatchingLib.StateObject;

namespace MatchingLib
{
    /// <summary>
    /// TCP Server
    /// </summary>
    public class TcpServer : IDisposable
    {
        /// <summary>
        /// Singleton
        /// </summary>
        public static TcpServer Server { get; } = new TcpServer();
        objPool<StateObject> bufferPool { get; set; }
        public delegate void LogInfoEvent(string info);
        public LogInfoEvent LogInfo { get; set; }
        public delegate void LogErrorEvent(string error);
        public LogErrorEvent LogError { get; set; }
        Socket listener { get; set; }
        public MessageArriveEvent MsgArriveHdr { get; set; }
        public void StartListening(string ip, int port, int backlog = 1000)
        {
            bufferPool = new objPool<StateObject>(() => new StateObject(MsgArriveHdr));
            // Establish the local endpoint for the socket.  
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

            // Create a TCP/IP socket.  
            listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(backlog);

                listener.BeginAccept(
                    AcceptCallback,
                    //new AsyncCallback(AcceptCallback),
                    listener);
            }
            catch (Exception e)
            {
                LogError?.Invoke(e.ToString());
            }
            LogInfo?.Invoke("Tcp Server Listener started");
        }
        public void AcceptCallback(IAsyncResult ar)
        {
            // Get the socket that handles the client request.  
            //Socket listener = (Socket)ar.AsyncState;
            var so = bufferPool.Checkout();
            so.workSocket = listener.EndAccept(ar);
            so.workSocket.BeginReceive(so.buffer, 0, StateObject.BufferSize, 0, so.ReadCallback, so);
            listener.BeginAccept(AcceptCallback, listener);
        }
        internal void RecycleSO(StateObject stateObject)
        {
            bufferPool.Checkin(stateObject);
        }

        /// <summary>
        /// Shutdown the listener, please call dispose before calling next new StartListening again
        /// </summary>
        public void Shutdown()
        {
            listener.Disconnect(true);
        }

        public void Dispose()
        {
            listener.Disconnect(false);
            // bufferPool need to be disposed
            //while (bufferPool. > 0)
            //{
            //    var item = bufferPool.Checkout();
            //    item.Dispose();
            //}
            //listener.Dispose();
        }
    }
}
