using MatchingLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchingLib
{
    /// <summary>
    /// Binary Structure
    /// Symbol, Price, UserID, OrderID are variable length
    /// Msg length | RequestType | DateTime | ExecutionType | Direction | Price len | Price    | Volume  | ReqID len | Req ID   | OrderID len | Order ID | Filled Volume |
    /// 2 bytes    | 1 byte      | 8 bytes  | 1 byte        | 1 byte    | 1 byte    | 41 bytes | 8 bytes | 1 byte    | 40 bytes | 1 byte      | 40 bytes | 8 bytes       |
    /// Max: 153 bytes
    /// </summary>
    public abstract class RequestToMatching : IBinaryProcess
    {
        static int MsgLenSize { get; } = 2;
        static int RequestTypePos { get; } = 2;
        /// <summary>
        /// 1 byte
        /// </summary>
        static int RequestTypeSize { get; } = 1;
        /// <summary>
        /// 8 bytes
        /// </summary>
        static int DateTimeSize { get; } = 8;
        /// <summary>
        /// 1 byte
        /// </summary>
        static int OrderExecutionTypeSize { get; } = 1;
        /// <summary>
        /// 1 byte
        /// </summary>
        static int OrderDirectionSize { get; } = 1;
        /// <summary>
        /// 1 byte
        /// </summary>
        //static int OrderSymbolLenSize { get; } = 1;
        /// <summary>
        /// 30 bytes - content
        /// </summary>
        //static int OrderSymbolMaxSize { get; } = 30;
        /// <summary>
        /// 1 byte
        /// </summary>
        static int OrderPriceLenSize { get; } = 1;
        /// <summary>
        /// 30 bytes
        /// dot 1 byte
        /// decimal place 20 bytes
        /// 51 bytes - content
        /// </summary>
        //static int OrderPriceMaxSize { get; } = 51;
        static int OrderPriceMaxSize { get; } = 41;
        /// <summary>
        /// 8 bytes
        /// </summary>
        static int OrderVolumeSize { get; } = 8;
        /// <summary>
        /// 1 byte
        /// </summary>
        static int OrderReqIdLenSize { get; } = 1;
        /// <summary>
        /// 50 bytes - content
        /// </summary>
        static int OrderReqIdMaxSize { get; } = 40;
        /// <summary>
        /// 1 byte
        /// </summary>
        static int OrderIdLenSize { get; } = 1;
        /// <summary>
        /// 60 bytes
        /// </summary>
        //static int OrderIdMaxSize { get; } = 60;
        static int OrderIdMaxSize { get; } = 40;
        /// <summary>
        /// 8 bytes
        /// </summary>
        static int OrderFilledVolumeSize { get; } = 8;
        /// <summary>
        /// Structure Max binary length
        /// Max: 153 bytes
        /// </summary>
        public static int TotalLength { get; } = MsgLenSize + RequestTypeSize + DateTimeSize + OrderExecutionTypeSize + OrderDirectionSize + /*OrderSymbolLenSize + OrderSymbolMaxSize +*/ OrderPriceLenSize + OrderPriceMaxSize +
            OrderVolumeSize + OrderReqIdLenSize + OrderReqIdMaxSize + OrderIdLenSize + OrderIdMaxSize + OrderFilledVolumeSize;

        /// <summary>
        ///  we don't support edit
        /// </summary>
        public enum RequestType
        {
            TradeOrder,
            CancelOrder,
            IsOrderExist,
        }
        public string id { get; set; }
        public DateTime dt { get; set; }
        public RequestType type { get; set; }
        public Order order { get; set; }
        public void FromBytes(byte[] bytes)
        {
            int DateTimePos = RequestTypePos + RequestTypeSize;
            int OrderExecutionTypePos = DateTimePos + DateTimeSize;
            int OrderDirectionPos = OrderExecutionTypePos + OrderExecutionTypeSize;

            //int OrderSymbolLenPos = OrderDirectionPos + OrderDirectionSize;
            //int OrderSymbolPos = OrderSymbolLenPos + OrderSymbolLenSize;
            //int OrderSymbolActualSize = bytes[OrderSymbolLenPos];

            //int OrderPriceLenPos = OrderSymbolPos + OrderSymbolActualSize;
            int OrderPriceLenPos = OrderDirectionPos + OrderDirectionSize;
            int OrderPricePos = OrderPriceLenPos + OrderPriceLenSize;
            int OrderPriceActualSize = bytes[OrderPriceLenPos];

            int OrderVolumePos = OrderPricePos + OrderPriceActualSize;

            int OrderReqIdLenPos = OrderVolumePos + OrderVolumeSize;
            int OrderReqIdPos = OrderReqIdLenPos + OrderReqIdLenSize;
            int OrderReqIdActualSize = bytes[OrderReqIdLenPos];

            int OrderIdLenPos = OrderReqIdPos + OrderReqIdActualSize;
            int OrderIdPos = OrderIdLenPos + OrderIdLenSize;
            int OrderIdActualSize = bytes[OrderIdLenPos];

            int OrderFilledVolumePos = OrderIdPos + OrderIdActualSize;

            this.type = (RequestType)bytes[RequestTypePos];
            long _dt = BitConverter.ToInt64(bytes, DateTimePos);
            this.dt = DateTime.FromBinary(_dt);
            if (order == null) throw new Exception("Order isn't initialized!");
            this.order.et = (Order.ExecutionType)bytes[OrderExecutionTypePos];
            this.order.t = (Order.OrderType)bytes[OrderDirectionPos];
            //this.order.s = Encoding.UTF8.GetString(bytes, OrderSymbolPos, OrderSymbolActualSize).Trim();
            string price = Encoding.UTF8.GetString(bytes, OrderPricePos, OrderPriceActualSize).Trim();
            double p = 0.0;
            if (double.TryParse(price, out p))
            {
                this.order.p = p;
            }
            this.order.v = BitConverter.ToInt64(bytes, OrderVolumePos);
            this.order.u = Encoding.UTF8.GetString(bytes, OrderReqIdPos, OrderReqIdActualSize).Trim();
            this.order.id = Encoding.UTF8.GetString(bytes, OrderIdPos, OrderIdActualSize).Trim();
            this.order.fv = BitConverter.ToInt64(bytes, OrderFilledVolumePos);
        }
        public BinaryObj ToBytes()
        {
            BinaryObj buffer = BinaryObjPool.Checkout(BinaryObj.PresetType.RequestToMatching);
            buffer.bw.Write((short)0);
            buffer.bw.Write((byte)this.type);
            buffer.bw.Write(this.dt.ToBinary());
            buffer.bw.Write((byte)this.order.et);
            buffer.bw.Write((byte)this.order.t);
            //if (this.order.s.Length > OrderSymbolMaxSize) throw new Exception(string.Format("Symbol exceed max length:{0}", OrderSymbolMaxSize));
            //buffer.bw.Write(this.order.s);
            string price = this.order.p.ToString();
            //if (price.Length > OrderPriceMaxSize) throw new Exception(string.Format("Price exceed max length:{0}", OrderPriceMaxSize));
            buffer.bw.Write(price);
            buffer.bw.Write(this.order.v);
            if (this.order.u.Length > OrderReqIdMaxSize) throw new Exception(string.Format("User ID exceed max length:{0}", OrderReqIdMaxSize));
            buffer.bw.Write(this.order.u);
            if (this.order.id.Length > OrderIdMaxSize) throw new Exception(string.Format("Order ID exceed max length:{0}", OrderIdMaxSize));
            buffer.bw.Write(this.order.id);
            buffer.bw.Write(this.order.fv);
            buffer.lenBw.Write((short)buffer.ms.Position);
            buffer.length = (int)buffer.ms.Position;
            Array.Copy(buffer.lenInBytes, 0, buffer.bytes, 0, 2);
            return buffer;
        }
    }
}
