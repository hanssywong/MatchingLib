using BaseHelper;
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
    public class SimpleSocketClient
    {
        public int ReceiveBufferSize { get; } = 512;
        public delegate void LogInfoEvent(string info);
        public LogInfoEvent LogInfo { get; set; }
        public delegate void LogErrorEvent(string error);
        public LogErrorEvent LogError { get; set; }

        Socket socket { get; set; } = new Socket(SocketType.Stream, ProtocolType.Tcp);
        Task SenderTask { get; set; }
        Task ReceiverTask { get; set; }
        SpinQueue<BinaryObj> sendQueue { get; } = new SpinQueue<BinaryObj>();
        SpinQueue<byte[]> recvQueue { get; } = new SpinQueue<byte[]>();
        objPool<byte[]> bufferPool { get; set; }

        public bool ReceiveBuffer(out byte[] buffer)
        {
            return recvQueue.TryDequeue(out buffer);
        }

        public void CheckinBuffer(byte[] buffer)
        {
            bufferPool.Checkin(buffer);
        }

        public void Connect(string ip, int port)
        {
            bufferPool = new objPool<byte[]>(() => new byte[ReceiveBufferSize]);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
            socket.Connect(ip, port);
            SenderTask = Task.Factory.StartNew(() => SendTask());
            ReceiverTask = Task.Factory.StartNew(() => ReceiveTask());
        }

        private void ReceiveTask()
        {
            while (socket != null && socket.Connected)
            {
                try
                {
                    var buffer = bufferPool.Checkout();
                    // Read data from the client socket.   
                    int bytesRead = socket.Receive(buffer, 0, 2, SocketFlags.None);
                    int len = BitConverter.ToInt16(buffer, 0);
                    bytesRead = socket.Receive(buffer, 2, len, SocketFlags.None);
                    if (bytesRead > 0)
                    {
                        recvQueue.Enqueue(buffer);
                    }
                    else
                    {
                        if (socket.Connected)
                            socket.Shutdown(SocketShutdown.Send);
                    }
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception e)
                {
                    LogError?.Invoke(e.ToString());
                }
                finally
                {
                    if (socket != null && !socket.Connected)
                    {
                        IPEndPoint remoteIpEndPoint = socket.RemoteEndPoint as IPEndPoint;
                        LogInfo?.Invoke("SimpleTcpClient socket closed from IP:" + remoteIpEndPoint?.Address + ", port:" + remoteIpEndPoint?.Port);
                        socket.Close();
                        GC.SuppressFinalize(socket);
                        socket = null;
                    }
                }
            }
            LogInfo?.Invoke("SimpleTcpClient ReceiveTask shutdown");
        }

        private void SendTask()
        {
            while (socket != null && socket.Connected)
            {
                BinaryObj binObj = null;
                try
                {
                    if (sendQueue.TryDequeue(out binObj))
                    {
                        socket.Send(binObj.bytes, 0, binObj.length, SocketFlags.None);
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception e)
                {
                    LogError?.Invoke(e.ToString());
                }
                finally
                {
                    if (binObj != null)
                    {
                        binObj.ResetOjb();
                        BinaryObjPool.Checkin(binObj);
                    }
                }
            }
            LogInfo?.Invoke("SimpleTcpClient SendTask shutdown");
        }

        public void Send(IBinaryProcess binObj)
        {
            var bin = binObj.ToBytes();
            //LogInfo?.Invoke("len:" + bin.length);
            //LogInfo?.Invoke("bin:" + BitConverter.ToString(bin.bytes, 0, bin.length + 2));
            sendQueue.Enqueue(bin);
        }

        public void Shutdown()
        {
            sendQueue.ShutdownGracefully();
            recvQueue.ShutdownGracefully();
            socket.Shutdown(SocketShutdown.Send);
            SenderTask.Wait();
            ReceiverTask.Wait();
        }
    }
}
