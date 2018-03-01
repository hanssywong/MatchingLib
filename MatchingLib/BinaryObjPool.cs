using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchingLib
{
    public class BinaryObj
    {
        public byte[] bytes { get; }
        public MemoryStream ms { get; }
        public BinaryWriter bw { get; }
        public BinaryObj(int byteArrayLength)
        {
            bytes = new byte[byteArrayLength];
            ms = new MemoryStream(bytes);
            bw = new BinaryWriter(ms);
        }
    }
    public class BinaryObjPool
    {
        /// <summary>
        /// 1k bytes buffer for Request and Response
        /// </summary>
        public static BinaryObjPool PoolForReq { get; } = new BinaryObjPool(RequestToMatching.TotalLength);
        public static BinaryObjPool PoolForResp { get; } = new BinaryObjPool(ProcessOrderResult.TotalLength);
        public static BinaryObjPool PoolForTx { get; } = new BinaryObjPool(Transaction.TotalLength);
        public BinaryObjPool(int byteArrayLength)
        {
            Pool = new objPool<BinaryObj>(() => new BinaryObj(byteArrayLength));
        }
        public objPool<BinaryObj> Pool { get; }
    }
}
