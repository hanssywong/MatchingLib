﻿using BaseHelper;
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
        public volatile int rps = 0;

        TcpClient client { get; set; } = new TcpClient();
        Task SenderTask { get; set; }
        Task ReceiverTask { get; set; }
        SpinQueue<BinaryObj> sendQueue { get; } = new SpinQueue<BinaryObj>();
        SpinQueue<byte[]> recvQueue { get; } = new SpinQueue<byte[]>();
        NetworkStream stream { get; set; }
        objPool<byte[]> bufferPool { get; set; }

        public bool IsReceiveQueueEmpty { get { return recvQueue.IsQueueEmpty; } }
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
            byte[] buffer = null;
            byte[] errorbuffer = new byte[200 * 1024];
            byte[] prevbuff = new byte[1024];
            int len = 0;
            int prevLen = 0;
            while (client != null && client.Connected)
            {
                try
                {
                    buffer = bufferPool.Checkout();
                    // Read data from the client socket.   
                    int bytesRead = stream.Read(buffer, 0, 2);
                    if (bytesRead > 0)
                    {
                        bytesRead = 0;
                        len = BitConverter.ToInt16(buffer, 0);
                        if (len > ReceiveBufferSize)
                        {
                            Array.Copy(buffer, errorbuffer, 2);
                            bytesRead = stream.Read(errorbuffer, 2, len - 2);
                            if (prevLen <= prevbuff.Length)
                            {
                                LogError("prevLen:" + prevLen);
                                LogError("prevbuff:" + BitConverter.ToString(prevbuff, 0, prevLen));
                            }
                            LogError("len:" + len);
                            LogError("errorbuffer:" + BitConverter.ToString(errorbuffer, 0, len));
                            bufferPool.Checkin(buffer);
                            continue;
                        }
                        else
                        {
                            bytesRead = stream.Read(buffer, 2, len - 2);
                            recvQueue.Enqueue(buffer);
                        }
                    }
                    else
                    {
                        if (client.Connected)
                            client.Client.Shutdown(SocketShutdown.Send);
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
                    if (client != null && !client.Connected)
                    {
                        IPEndPoint remoteIpEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
                        LogInfo?.Invoke("SimpleTcpClient socket closed from IP:" + remoteIpEndPoint?.Address + ", port:" + remoteIpEndPoint?.Port);
                        client.Close();
                        GC.SuppressFinalize(client);
                        client = null;
                        stream.Close();
                        GC.SuppressFinalize(stream);
                        stream = null;
                    }
                    prevLen = len;
                    Array.Copy(buffer, prevbuff, buffer.Length);
                }
            }
            LogInfo?.Invoke("SimpleTcpClient ReceiveTask shutdown");
        }

        private void SendTask()
        {
            while (client != null && client.Connected)
            {
                BinaryObj binObj = null;
                try
                {
                    if (sendQueue.TryDequeue(out binObj))
                    {
                        stream.Write(binObj.bytes, 0, binObj.length);
                        rps++;
                        //stream.Flush();
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
            client.Client.Shutdown(SocketShutdown.Send);
            SenderTask.Wait();
            ReceiverTask.Wait();
        }
    }
}
