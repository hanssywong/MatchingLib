﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchingLib
{
    /// Binary Structure
    /// Comment, Symbol, Price, UserID, OrderID are variable length
    /// Msg length | DateTime | Price len | Price    | Volume  | Buy Order len | Buy Order ID  | Sell Order len | Sell Order ID  | Tx ID len | Tx ID    | Initiator |
    /// 2 bytes    | 8 bytes  | 1 byte    | 41 bytes | 8 bytes | 1 byte        | 40 bytes      | 1 byte         | 40 bytes       | 1 byte    | 40 bytes | 1 byte    |
    /// Max: 182 bytes
    public abstract class Transaction : IBinaryProcess
    {
        static int MsgLenSize { get; } = 2;
        static int DateTimePos { get; } = 2;
        /// <summary>
        /// 8 bytes
        /// </summary>
        static int DateTimeSize { get; } = 8;
        /// <summary>
        /// 1 byte
        /// </summary>
        //static int SymbolLenSize { get; } = 1;
        /// <summary>
        /// 30 bytes - content
        /// </summary>
        //static int SymbolMaxSize { get; } = 30;
        /// <summary>
        /// 1 byte
        /// </summary>
        static int PriceLenSize { get; } = 1;
        /// <summary>
        /// 41 bytes
        /// </summary>
        static int PriceMaxSize { get; } = 41;
        /// <summary>
        /// 8 bytes
        /// </summary>
        static int VolumeSize { get; } = 8;
        /// <summary>
        /// 1 byte
        /// </summary>
        static int BuyOrderIdLenSize { get; } = 1;
        /// <summary>
        /// 40 bytes
        /// </summary>
        static int BuyOrderIdMaxSize { get; } = 40;
        /// <summary>
        /// 1 byte
        /// </summary>
        static int SellOrderIdLenSize { get; } = 1;
        /// <summary>
        /// 40 bytes
        /// </summary>
        static int SellOrderIdMaxSize { get; } = 40;
        /// <summary>
        /// 1 byte
        /// </summary>
        static int TxIdLenSize { get; } = 1;
        /// <summary>
        /// 40 bytes
        /// </summary>
        static int TxIdMaxSize { get; } = 40;
        /// <summary>
        /// 1 byte
        /// </summary>
        static int InitiatorSize { get; } = 1;

        /// <summary>
        /// Structure Max binary length
        /// Max: 182 bytes
        /// </summary>
        public static int TotalLength { get; } = MsgLenSize + DateTimeSize /*+ SymbolLenSize + SymbolMaxSize*/ + PriceLenSize + PriceMaxSize + VolumeSize + BuyOrderIdLenSize + BuyOrderIdMaxSize +
            SellOrderIdLenSize + SellOrderIdMaxSize + TxIdLenSize + TxIdMaxSize + InitiatorSize;

        public enum InitiatorType
        {
            Buy,
            Sell
        }
        /// <summary>
        /// Symbol
        /// </summary>
        public string s { get; set; } = string.Empty;
        /// <summary>
        /// Transaction ID
        /// </summary>
        public string id { get; set; } = string.Empty;
        /// <summary>
        /// price
        /// </summary>
        public double p { set; get; } = 0;
        /// <summary>
        /// volume
        /// </summary>
        public long v { set; get; } = 0;
        /// <summary>
        /// buy side order id(ticket number)
        /// </summary>
        public string bt { set; get; } = string.Empty;
        /// <summary>
        /// sell side order id(ticket number)
        /// </summary>
        public string st { set; get; } = string.Empty;
        /// <summary>
        /// Tx created time
        /// </summary>
        public DateTime dt { get; set; } = DateTime.Now;
        /// <summary>
        /// Initiator side
        /// </summary>
        public InitiatorType init { get; set; } = InitiatorType.Buy;
        public void ResetObj()
        {
            this.bt = string.Empty;
            this.dt = new DateTime();
            this.id = string.Empty;
            this.p = 0;
            this.s = string.Empty;
            this.st = string.Empty;
            this.v = 0;
        }

        public BinaryObj ToBytes()
        {
            var buffer = BinaryObjPool.Checkout(BinaryObj.PresetType.Transaction);
            buffer.bw.Write((short)0);
            buffer.bw.Write(this.dt.ToBinary());
            //if (this.s.Length > SymbolMaxSize) throw new Exception(string.Format("Symbol exceed max length:{0}", SymbolMaxSize));
            //buffer.bw.Write(this.s);
            buffer.bw.Write(this.p.ToString());
            buffer.bw.Write(this.v);
            if (this.bt.Length > BuyOrderIdMaxSize) throw new Exception(string.Format("Buy Order ID exceed max length:{0}", BuyOrderIdMaxSize));
            buffer.bw.Write(this.bt);
            if (this.st.Length > SellOrderIdMaxSize) throw new Exception(string.Format("Sell Order ID exceed max length:{0}", SellOrderIdMaxSize));
            buffer.bw.Write(this.st);
            if (this.id.Length > TxIdMaxSize) throw new Exception(string.Format("Tx ID exceed max length:{0}", TxIdMaxSize));
            buffer.bw.Write(this.id);
            buffer.bw.Write((byte)this.init);
            buffer.lenBw.Write((short)buffer.ms.Position);
            buffer.length = (int)buffer.ms.Position;
            Array.Copy(buffer.lenInBytes, 0, buffer.bytes, 0, 2);
            return buffer;
        }
        public void FromBytes(byte[] bytes)
        {
            //int SymbolLenPos = DateTimePos + DateTimeSize;
            //int SymbolPos = SymbolLenPos + SymbolLenSize;
            //int SymbolActualSize = bytes[SymbolLenPos];

            //int PriceLenPos = SymbolPos + SymbolActualSize;
            int PriceLenPos = DateTimePos + DateTimeSize;
            int PricePos = PriceLenPos + PriceLenSize;
            int PriceActualSize = bytes[PriceLenPos];

            int VolumePos = PricePos + PriceActualSize;

            int BuyOrderIdLenPos = VolumePos + VolumeSize;
            int BuyOrderIdPos = BuyOrderIdLenPos + BuyOrderIdLenSize;
            int BuyOrderIdActualSize = bytes[BuyOrderIdLenPos];

            int SellOrderIdLenPos = BuyOrderIdPos + BuyOrderIdActualSize;
            int SellOrderIdPos = SellOrderIdLenPos + SellOrderIdLenSize;
            int SellOrderIdActualSize = bytes[SellOrderIdLenPos];

            int TxIdLenPos = SellOrderIdPos + SellOrderIdActualSize;
            int TxIdPos = TxIdLenPos + TxIdLenSize;
            int TxIdActualSize = bytes[TxIdLenPos];

            int InitiatorPos = TxIdPos + TxIdActualSize;

            long _dt = BitConverter.ToInt64(bytes, DateTimePos);
            this.dt = DateTime.FromBinary(_dt);
            //this.s = Encoding.UTF8.GetString(bytes, SymbolPos, SymbolActualSize).Trim();
            string price = Encoding.UTF8.GetString(bytes, PricePos, PriceActualSize).Trim();
            double p = 0.0;
            if (double.TryParse(price, out p))
            {
                this.p = p;
            }
            this.v = BitConverter.ToInt64(bytes, VolumePos);
            this.bt = Encoding.UTF8.GetString(bytes, BuyOrderIdPos, BuyOrderIdActualSize).Trim();
            this.st = Encoding.UTF8.GetString(bytes, SellOrderIdPos, SellOrderIdActualSize).Trim();
            this.id = Encoding.UTF8.GetString(bytes, TxIdPos, TxIdActualSize).Trim();
            this.init = (InitiatorType)bytes[TxIdLenPos];
        }
    }
}
