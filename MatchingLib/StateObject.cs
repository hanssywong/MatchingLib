using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MatchingLib
{
    /// <summary>
    /// State object for reading client data asynchronously
    /// </summary>
    public class StateObject : IDisposable
    {
        public static byte[] terminator { get; } = { 0, 0 };
        /// <summary>
        /// Client socket
        /// </summary>
        public Socket workSocket { get; set; } = null;
        /// <summary>
        /// Size of receive buffer
        /// </summary>
        internal const int BufferSize = 1024;
        /// <summary>
        /// Receive buffer
        /// </summary>
        public byte[] buffer { get; } = new byte[BufferSize];
        public byte[] bytesStore { get; } = new byte[BufferSize];
        public delegate void MessageArriveEvent(byte[] bytes, long length, StateObject obj);
        MessageArriveEvent ProcMessage { get; set; }
        MemoryStream msStore { get; set; }
        MemoryStream msRead { get; set; }
        BinaryReader br { get; set; }
        public short MsgLen { get; private set; } = 0;
        public short ReceivedLen { get; private set; } = 0;
        BinaryObj BinObj { get; set; }
        //public StateObject() { br = new BinaryReader(msStore); }
        public StateObject(MessageArriveEvent msgArriveHdr)
        {
            ProcMessage = msgArriveHdr;
            msStore = new MemoryStream(bytesStore);
            msRead = new MemoryStream(bytesStore);
            br = new BinaryReader(msRead);
        }
        public void SetMsgHdr(MessageArriveEvent msgArriveHdr)
        {
            ProcMessage = msgArriveHdr;
        }
        public void ReadCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the handler socket  
                // from the asynchronous state object.  
                //StateObject state = (StateObject)ar.AsyncState;
                //Socket handler = state.workSocket;

                // Read data from the client socket.   
                int bytesRead = workSocket.EndReceive(ar);

                if (bytesRead > 0)
                {
                    msStore.Write(buffer, 0, bytesRead);
                    if (msStore.Position > 4 && MsgLen == 0)
                    {
                        MsgLen = br.ReadInt16();
                    }
                    if (MsgLen > 0 && MsgLen == msStore.Position)
                    {
                        ProcMessage(bytesStore, MsgLen, this);
                    }
                    workSocket.BeginReceive(buffer, 0, StateObject.BufferSize, 0, ReadCallback, this);
                }
                else
                {
                    workSocket.Close();
                }
            }
            catch (Exception e)
            {
                TcpServer.Server.LogError?.Invoke(e.ToString());
            }
            finally
            {
                if (!workSocket.Connected)
                {
                    TcpServer.Server.LogInfo?.Invoke("Socket close");
                    ResetObj();
                    TcpServer.Server.RecycleSO(this);
                }
            }
        }
        public void ResetObj()
        {
            if (this.BinObj != null) BinaryObjPool.Checkin(BinObj);
            Array.Clear(buffer, 0, buffer.Length);
            this.msStore.Seek(0, SeekOrigin.Begin);
            this.msRead.Seek(0, SeekOrigin.Begin);
            this.MsgLen = 0;
            this.ReceivedLen = 0;
            workSocket.Close();
        }
        public void Send(BinaryObj binObj)
        {
            BinObj = binObj;
            workSocket.BeginSend(BinObj.bytes, 0, (int)BinObj.ms.Position, SocketFlags.None, SendCallback, this);
        }
        void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                //Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = workSocket.EndSend(ar);
            }
            catch (Exception e)
            {
                TcpServer.Server.LogError?.Invoke(e.ToString());
            }
            finally
            {
                BinaryObjPool.Checkin(BinObj);
                BinObj = null;
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
