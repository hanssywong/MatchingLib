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
    public class SimpleTcpClient
    {
        public int ReceiveBufferSize { get; } = 512;
        public delegate void LogInfoEvent(string info);
        public LogInfoEvent LogInfo { get; set; }
        public delegate void LogErrorEvent(string error);
        public LogErrorEvent LogError { get; set; }

        TcpClient client { get; set; } = new TcpClient();
        Task SenderTask { get; set; }
        Task ReceiverTask { get; set; }
        SpinQueue<BinaryObj> sendQueue { get; } = new SpinQueue<BinaryObj>();
        SpinQueue<byte[]> recvQueue { get; } = new SpinQueue<byte[]>();
        NetworkStream stream { get; set; }
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
            client.Connect(ip, port);
            stream = client.GetStream();
            SenderTask = Task.Factory.StartNew(() => SendTask());
            ReceiverTask = Task.Factory.StartNew(() => ReceiveTask());
        }

        private void ReceiveTask()
        {
            while (client.Connected)
            {
                try
                {
                    var buffer = bufferPool.Checkout();
                    // Read data from the client socket.   
                    int bytesRead = stream.Read(buffer, 0, 2);
                    int len = BitConverter.ToInt16(buffer, 0);
                    bytesRead = stream.Read(buffer, 2, len);
                    recvQueue.Enqueue(buffer);
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
                    if (client != null && !client.Connected)
                    {
                        IPEndPoint remoteIpEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                        LogInfo?.Invoke("SimpleTcpClient socket closed from IP:" + remoteIpEndPoint?.Address + ", port:" + remoteIpEndPoint?.Port);
                        client.Close();
                        GC.SuppressFinalize(client);
                        client = null;
                        stream.Close();
                        stream = null;
                        GC.SuppressFinalize(stream);
                    }
                }
            }
            LogInfo?.Invoke("SimpleTcpClient ReceiveTask shutdown");
        }

        private void SendTask()
        {
            while (client.Connected)
            {
                BinaryObj binObj = null;
                try
                {
                    if (sendQueue.TryDequeue(out binObj))
                    {
                        stream.Write(binObj.bytes, 0, binObj.length);
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
            sendQueue.Enqueue(binObj.ToBytes());
        }

        public void Shutdown()
        {
            client.Client.Shutdown(SocketShutdown.Both);
            SenderTask.Wait();
            ReceiverTask.Wait();
        }
    }
}
