using BaseHelper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchingLib
{
    public interface IBinaryProcess
    {
        BinaryObj ToBytes();
    }
    public class BinaryObj
    {
        public enum PresetType
        {
            Unknown,
            RequestToMatching,
            ProcessOrderResult,
            Transaction,
        }
        public PresetType type { get; } = PresetType.Unknown;
        public byte[] bytes { get; }
        public MemoryStream ms { get; }
        public BinaryWriter bw { get; }
        public byte[] lenInBytes { get; } = { 0, 0 };
        public MemoryStream lenMs { get; }
        public BinaryWriter lenBw { get; }
        public int length { get; set; } = 0;

        public BinaryObj(int byteArrayLength, PresetType t = PresetType.Unknown)
        {
            type = t;
            bytes = new byte[byteArrayLength];
            ms = new MemoryStream(bytes);
            bw = new BinaryWriter(ms);
            lenMs = new MemoryStream(lenInBytes);
            lenBw = new BinaryWriter(lenMs);
        }
        public void ResetOjb()
        {
            Array.Clear(bytes, 0, bytes.Length);
            Array.Clear(lenInBytes, 0, lenInBytes.Length);
            ms.Seek(0, SeekOrigin.Begin);
            lenMs.Seek(0, SeekOrigin.Begin);
            length = 0;
        }
    }
    public class BinaryObjPool
    {
        /// <summary>
        /// 1k bytes buffer for Request and Response
        /// </summary>
        public static BinaryObjPool PoolForReq { get; } = new BinaryObjPool(RequestToMatching.TotalLength, BinaryObj.PresetType.RequestToMatching);
        public static BinaryObjPool PoolForResp { get; } = new BinaryObjPool(ProcessOrderResult.TotalLength, BinaryObj.PresetType.ProcessOrderResult);
        public static BinaryObjPool PoolForTx { get; } = new BinaryObjPool(Transaction.TotalLength, BinaryObj.PresetType.Transaction);
        public static void Checkin(BinaryObj binObj)
        {
            binObj.ResetOjb();
            if (binObj.type == BinaryObj.PresetType.RequestToMatching)
            {
                PoolForReq.Pool.Checkin(binObj);
            }
            else if (binObj.type == BinaryObj.PresetType.ProcessOrderResult)
            {
                PoolForResp.Pool.Checkin(binObj);
            }
            else if (binObj.type == BinaryObj.PresetType.Transaction)
            {
                PoolForTx.Pool.Checkin(binObj);
            }
        }
        public static BinaryObj Checkout(BinaryObj.PresetType type)
        {
            if (type == BinaryObj.PresetType.RequestToMatching)
            {
                return PoolForReq.Pool.Checkout();
            }
            else if (type == BinaryObj.PresetType.ProcessOrderResult)
            {
                return PoolForResp.Pool.Checkout();
            }
            else if (type == BinaryObj.PresetType.Transaction)
            {
                return PoolForTx.Pool.Checkout();
            }
            return null;
        }
        public BinaryObjPool(int byteArrayLength, BinaryObj.PresetType t = BinaryObj.PresetType.Unknown)
        {
            Pool = new objPool<BinaryObj>(() => new BinaryObj(byteArrayLength, t));
        }
        public objPool<BinaryObj> Pool { get; }
    }
}
