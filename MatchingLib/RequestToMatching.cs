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
    /// RequestType | DateTime | ExecutionType | Direction | Symbol len | Symbol   | Price len | Price    | Volume  | UserID len | User ID  | OrderID len | Order ID | Filled Volume |
    /// 1 byte      | 8 bytes  | 1 byte        | 1 byte    | 1 byte     | 30 bytes | 1 byte    | 51 bytes | 8 bytes | 1 byte     | 50 bytes | 1 byte      | 60 bytes | 8 bytes       |
    /// Max: 222 bytes
    /// </summary>
    public abstract class RequestToMatching
    {
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
        static int OrderSymbolLenSize { get; } = 1;
        /// <summary>
        /// 30 bytes - content
        /// </summary>
        static int OrderSymbolMaxSize { get; } = 30;
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
        static int OrderPriceMaxSize { get; } = 51;
        /// <summary>
        /// 8 bytes
        /// </summary>
        static int OrderVolumeSize { get; } = 8;
        /// <summary>
        /// 1 byte
        /// </summary>
        static int OrderUserIdLenSize { get; } = 1;
        /// <summary>
        /// 50 bytes - content
        /// </summary>
        static int OrderUserIdMaxSize { get; } = 50;
        /// <summary>
        /// 1 byte
        /// </summary>
        static int OrderIdLenSize { get; } = 1;
        /// <summary>
        /// 60 bytes
        /// </summary>
        static int OrderIdMaxSize { get; } = 60;
        /// <summary>
        /// 8 bytes
        /// </summary>
        static int OrderFilledVolumeSize { get; } = 8;
        /// <summary>
        /// Structure Max binary length
        /// </summary>
        public static int TotalLength { get; } = RequestTypeSize + DateTimeSize + OrderExecutionTypeSize + OrderDirectionSize + OrderSymbolLenSize + OrderSymbolMaxSize + OrderPriceLenSize + OrderPriceMaxSize +
            OrderVolumeSize + OrderUserIdLenSize + OrderUserIdMaxSize + OrderIdLenSize + OrderIdMaxSize + OrderFilledVolumeSize;

        /// <summary>
        ///  we don't support edit
        /// </summary>
        public enum RequestType
        {
            TradeOrder,
            CancelOrder,
            IsOrderExist,
        }
        public DateTime dt { get; set; }
        public RequestType type { get; set; }
        public Order order { get; set; }
        public virtual void FromBytes(byte[] bytes)
        {
            // Request Type
            int RequestTypePos = 0;
            int DateTimePos = RequestTypePos + RequestTypeSize;
            int OrderExecutionTypePos = DateTimePos + DateTimeSize;
            int OrderDirectionPos = OrderExecutionTypePos + OrderExecutionTypeSize;

            int OrderSymbolLenPos = OrderDirectionPos + OrderDirectionSize;
            int OrderSymbolPos = OrderSymbolLenPos + OrderSymbolLenSize;
            int OrderSymbolActualSize = bytes[OrderSymbolLenPos];

            int OrderPriceLenPos = OrderSymbolPos + OrderSymbolActualSize;
            int OrderPricePos = OrderPriceLenPos + OrderPriceLenSize;
            int OrderPriceActualSize = bytes[OrderPriceLenPos];

            int OrderVolumePos = OrderPricePos + OrderPriceActualSize;

            int OrderUserIdLenPos = OrderVolumePos + OrderVolumeSize;
            int OrderUserIdPos = OrderUserIdLenPos + OrderUserIdLenSize;
            int OrderUserIdActualSize = bytes[OrderUserIdLenPos];

            int OrderIdLenPos = OrderUserIdPos + OrderUserIdActualSize;
            int OrderIdPos = OrderIdLenPos + OrderIdLenSize;
            int OrderIdActualSize = bytes[OrderIdLenPos];

            int OrderFilledVolumePos = OrderIdPos + OrderIdActualSize;

            this.type = (RequestType)bytes[RequestTypePos];
            long _dt = BitConverter.ToInt64(bytes, DateTimePos);
            this.dt = DateTime.FromBinary(_dt);
            if (order == null) throw new Exception("Order isn't initialized!");
            this.order.et = (Order.ExecutionType)bytes[OrderExecutionTypePos];
            this.order.t = (Order.OrderType)bytes[OrderDirectionPos];
            this.order.s = Encoding.UTF8.GetString(bytes, OrderSymbolPos, OrderSymbolActualSize).Trim();
            string price = Encoding.UTF8.GetString(bytes, OrderPricePos, OrderPriceActualSize).Trim();
            double p = 0.0;
            if (double.TryParse(price, out p))
            {
                this.order.p = p;
            }
            this.order.v = BitConverter.ToInt64(bytes, OrderVolumePos);
            this.order.u = Encoding.UTF8.GetString(bytes, OrderUserIdPos, OrderUserIdActualSize).Trim();
            this.order.id = Encoding.UTF8.GetString(bytes, OrderIdPos, OrderIdActualSize).Trim();
            this.order.fv= BitConverter.ToInt64(bytes, OrderFilledVolumePos);
        }
        public virtual BinaryObj ToBytes()
        {
            BinaryObj buffer = BinaryObjPool.PoolForReq.Pool.CheckoutMT();
            buffer.bw.Write((byte)this.type);
            buffer.bw.Write(this.dt.ToBinary());
            buffer.bw.Write((byte)this.order.et);
            buffer.bw.Write((byte)this.order.t);
            if (this.order.s.Length > OrderSymbolMaxSize) throw new Exception(string.Format("Symbol exceed max length:{0}", OrderSymbolMaxSize));
            buffer.bw.Write(this.order.s);
            string price = this.order.p.ToString();
            //if (price.Length > OrderPriceMaxSize) throw new Exception(string.Format("Price exceed max length:{0}", OrderPriceMaxSize));
            buffer.bw.Write(price);
            buffer.bw.Write(this.order.v);
            if (this.order.u.Length > OrderUserIdMaxSize) throw new Exception(string.Format("User ID exceed max length:{0}", OrderUserIdMaxSize));
            buffer.bw.Write(this.order.u);
            if (this.order.id.Length > OrderIdMaxSize) throw new Exception(string.Format("Order ID exceed max length:{0}", OrderIdMaxSize));
            buffer.bw.Write(this.order.id);
            buffer.bw.Write(this.order.fv);
            return buffer;
        }
        public static void CheckIn(BinaryObj buffer)
        {
            BinaryObjPool.PoolForReq.Pool.Checkin(buffer);
        }
    }
}
