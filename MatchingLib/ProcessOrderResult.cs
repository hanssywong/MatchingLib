using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchingLib
{
    /// <summary>
    /// Binary Structure
    /// Comment, Symbol, Price, UserID, OrderID are variable length
    /// Success | DateTime | ErrorType | Filled Volume |
    /// 1 byte  | 8 bytes  | 1 byte    | 8 bytes       |
    /// Max: 223 bytes
    /// </summary>
    public abstract class ProcessOrderResult : IMethodResult, IBinaryProcess
    {
        public enum ErrorType
        {
            Error,
            Success,
            OrderExists,
            SystemBusy,
            Cancelled,
            OrderIsMissing
        }
        static int MsgLenSize { get; } = 2;
        static int SuccessPos { get; } = 2;
        /// <summary>
        /// 1 byte
        /// </summary>
        static int SuccessSize { get; } = 1;
        /// <summary>
        /// 8 bytes
        /// </summary>
        static int DateTimeSize { get; } = 8;
        /// <summary>
        /// 1 byte
        /// </summary>
        static int ErrorTypeSize { get; } = 1;
        ///// <summary>
        ///// 1 byte
        ///// </summary>
        //static int OrderExecutionTypeSize { get; } = 1;
        ///// <summary>
        ///// 1 byte
        ///// </summary>
        //static int OrderDirectionSize { get; } = 1;
        ///// <summary>
        ///// 1 byte
        ///// </summary>
        //static int OrderSymbolLenSize { get; } = 1;
        ///// <summary>
        ///// 30 bytes - content
        ///// </summary>
        //static int OrderSymbolMaxSize { get; } = 30;
        ///// <summary>
        ///// 1 byte
        ///// </summary>
        //static int OrderPriceLenSize { get; } = 1;
        ///// <summary>
        ///// 30 bytes
        ///// dot 1 byte
        ///// decimal place 20 bytes
        ///// 51 bytes - content
        ///// </summary>
        //static int OrderPriceMaxSize { get; } = 51;
        ///// <summary>
        ///// 8 bytes
        ///// </summary>
        //static int OrderVolumeSize { get; } = 8;
        ///// <summary>
        ///// 1 byte
        ///// </summary>
        //static int OrderUserIdLenSize { get; } = 1;
        ///// <summary>
        ///// 50 bytes - content
        ///// </summary>
        //static int OrderUserIdMaxSize { get; } = 50;
        ///// <summary>
        ///// 1 byte
        ///// </summary>
        //static int OrderIdLenSize { get; } = 1;
        ///// <summary>
        ///// 60 bytes
        ///// </summary>
        //static int OrderIdMaxSize { get; } = 60;
        /// <summary>
        /// 8 bytes
        /// </summary>
        static int OrderFilledVolumeSize { get; } = 8;
        /// <summary>
        /// Structure Max binary length
        /// Max: 223 bytes
        /// </summary>
        public static int TotalLength { get; } = MsgLenSize + SuccessSize + DateTimeSize + ErrorTypeSize/* + OrderExecutionTypeSize + OrderDirectionSize + OrderSymbolLenSize + OrderSymbolMaxSize + OrderPriceLenSize + OrderPriceMaxSize +
            OrderVolumeSize + OrderUserIdLenSize + OrderUserIdMaxSize + OrderIdLenSize + OrderIdMaxSize*/ + OrderFilledVolumeSize;

        public bool Success { get; set; } = false;
        public DateTime dt { get; set; } = DateTime.Now;
        public ErrorType errorType { get; set; } = ErrorType.Error;
        public Order order { get; set; }
        public string Comment { get; set; } = string.Empty;
        public void FromBytes(byte[] bytes)
        {
            int DateTimePos = SuccessPos + SuccessSize;
            int ErrorTypePos = DateTimePos + DateTimeSize;
            //int OrderExecutionTypePos = ErrorTypePos + ErrorTypeSize;
            //int OrderDirectionPos = OrderExecutionTypePos + OrderExecutionTypeSize;

            //int OrderSymbolLenPos = OrderDirectionPos + OrderDirectionSize;
            //int OrderSymbolPos = OrderSymbolLenPos + OrderSymbolLenSize;
            //int OrderSymbolActualSize = bytes[OrderSymbolLenPos];

            //int OrderPriceLenPos = OrderSymbolPos + OrderSymbolActualSize;
            //int OrderPricePos = OrderPriceLenPos + OrderPriceLenSize;
            //int OrderPriceActualSize = bytes[OrderPriceLenPos];

            //int OrderVolumePos = OrderPricePos + OrderPriceActualSize;

            //int OrderUserIdLenPos = OrderVolumePos + OrderVolumeSize;
            //int OrderUserIdPos = OrderUserIdLenPos + OrderUserIdLenSize;
            //int OrderUserIdActualSize = bytes[OrderUserIdLenPos];

            //int OrderIdLenPos = OrderUserIdPos + OrderUserIdActualSize;
            //int OrderIdPos = OrderIdLenPos + OrderIdLenSize;
            //int OrderIdActualSize = bytes[OrderIdLenPos];

            //int OrderFilledVolumePos = OrderIdPos + OrderIdActualSize;
            int OrderFilledVolumePos = ErrorTypePos + ErrorTypeSize;

            this.Success = BitConverter.ToBoolean(bytes, SuccessPos);
            long _dt = BitConverter.ToInt64(bytes, DateTimePos);
            this.dt = DateTime.FromBinary(_dt);
            //this.errorType = (ErrorType)bytes[ErrorTypePos];
            //if (order == null) throw new Exception("Order isn't initialized!");
            //this.order.et = (Order.ExecutionType)bytes[OrderExecutionTypePos];
            //this.order.t = (Order.OrderType)bytes[OrderDirectionPos];
            //this.order.s = Encoding.UTF8.GetString(bytes, OrderSymbolPos, OrderSymbolActualSize).Trim();
            //string price = Encoding.UTF8.GetString(bytes, OrderPricePos, OrderPriceActualSize).Trim();
            //double p = 0.0;
            //if (double.TryParse(price, out p))
            //{
            //    this.order.p = p;
            //}
            //this.order.v = BitConverter.ToInt64(bytes, OrderVolumePos);
            //this.order.u = Encoding.UTF8.GetString(bytes, OrderUserIdPos, OrderUserIdActualSize).Trim();
            //this.order.id = Encoding.UTF8.GetString(bytes, OrderIdPos, OrderIdActualSize).Trim();
            this.order.fv = BitConverter.ToInt64(bytes, OrderFilledVolumePos);
        }
        /// <summary>
        /// Thread safe
        /// </summary>
        /// <returns></returns>
        public BinaryObj ToBytes()
        {
            BinaryObj buffer = BinaryObjPool.Checkout(BinaryObj.PresetType.ProcessOrderResult);
            buffer.bw.Write((short)0);
            buffer.bw.Write(this.Success);
            buffer.bw.Write(this.dt.ToBinary());
            buffer.bw.Write((byte)this.errorType);
            //buffer.bw.Write((byte)this.order.et);
            //buffer.bw.Write((byte)this.order.t);
            //if (this.order.s.Length > OrderSymbolMaxSize) throw new Exception(string.Format("Symbol exceed max length:{0}", OrderSymbolMaxSize));
            //buffer.bw.Write(this.order.s);
            //string price = this.order.p.ToString();
            //if (price.Length > OrderPriceMaxSize) throw new Exception(string.Format("Price exceed max length:{0}", OrderPriceMaxSize));
            //buffer.bw.Write(price);
            //buffer.bw.Write(this.order.v);
            //if (this.order.u.Length > OrderUserIdMaxSize) throw new Exception(string.Format("User ID exceed max length:{0}", OrderUserIdMaxSize));
            //buffer.bw.Write(this.order.u);
            //if (this.order.id.Length > OrderIdMaxSize) throw new Exception(string.Format("Order ID exceed max length:{0}", OrderIdMaxSize));
            //buffer.bw.Write(this.order.id);
            buffer.bw.Write(this.order.fv);
            buffer.lenBw.Write((short)buffer.ms.Position);
            buffer.length = (int)buffer.ms.Position;
            Array.Copy(buffer.lenInBytes, 0, buffer.bytes, 0, 2);
            return buffer;
        }
        /// <summary>
        /// Thread safe
        /// </summary>
        /// <returns></returns>
        public static BinaryObj ConstructRejectBuffer()
        {
            BinaryObj buffer = BinaryObjPool.Checkout(BinaryObj.PresetType.ProcessOrderResult);
            try
            {
                buffer.bw.Write(false);
                buffer.bw.Write(DateTime.Now.ToBinary());
                buffer.bw.Write((byte)ErrorType.SystemBusy);
                buffer.bw.Write((long)0);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return buffer;
        }
    }
}
