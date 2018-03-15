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
        /// <summary>
        /// Size of receive buffer
        /// </summary>
        internal const int BufferSize = 1400;
        /// <summary>
        /// Receive buffer
        /// </summary>
        public byte[] bytesStore { get; } = new byte[BufferSize];
        MemoryStream msStore { get; set; }
        MemoryStream msRead { get; set; }
        BinaryReader br { get; set; }
        public short MsgLen { get; private set; } = 0;
        public short ReceivedLen { get; private set; } = 0;
        public StateObject()
        {
            msStore = new MemoryStream(bytesStore);
            msRead = new MemoryStream(bytesStore);
            br = new BinaryReader(msRead);
        }
        public void ResetObj()
        {
            Array.Clear(bytesStore, 0, bytesStore.Length);
            this.msStore.Seek(0, SeekOrigin.Begin);
            this.msRead.Seek(0, SeekOrigin.Begin);
            this.MsgLen = 0;
            this.ReceivedLen = 0;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
