using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchingLib
{
    public class Order : ICouchbase
    {
        public enum ExecutionType
        {
            Limit,
            IoC, // Immediate Or Cancel, IOC order may fill completely or partially, or it may not fill at all. In any event, the order will not stay in the system.
            AoN // All or nothing, AoN order may fill completely or it may not fill at all. In any event, the order will not stay in the system.
        }
        public enum OrderType
        {
            Buy,
            Sell
        }
        /// <summary>
        /// Symbol
        /// </summary>
        public string s { get; set; } = string.Empty;
        /// <summary>
        /// Execution Type
        /// </summary>
        public ExecutionType et { get; set; } = ExecutionType.Limit;
        /// <summary>
        /// price
        /// </summary>
        public double p { set; get; } = 0;
        /// <summary>
        /// volume
        /// </summary>
        public long v { set; get; } = 0;
        /// <summary>
        /// filled volume
        /// </summary>
        public long fv { set; get; } = 0;
        /// <summary>
        /// user id
        /// </summary>
        public string u { set; get; } = string.Empty;
        /// <summary>
        /// order id(ticket number)
        /// </summary>
        public string id { set; get; } = string.Empty;
        /// <summary>
        /// <para>Buy or Sell</para>
        /// <para>0 - Buy</para>
        /// <para>1 - Sell</para>
        /// </summary>
        public OrderType t { set; get; } = OrderType.Buy;
        /// <summary>
        /// Created time
        /// </summary>
        public DateTime dt { get; set; } = DateTime.Now;
        /// <summary>
        /// Last update time
        /// </summary>
        public DateTime ldt { get; set; } = DateTime.Now;

        public string classType
        {
            get
            {
                return "O";
            }
        }

        public string GetCbKey()
        {
            return GenerateKey(id);
        }

        /// <summary>
        /// Generate Couchbase Key
        /// </summary>
        /// <param name="ID">order id(ticket number)</param>
        /// <returns></returns>
        public string GenerateKey(string ID)
        {
            return string.Format("{0}_{1}_{2}", classType, s, ID);
        }

        public void ResetObj()
        {
            this.dt = new DateTime();
            this.et = ExecutionType.Limit;
            this.fv = 0;
            this.id = string.Empty;
            this.p = 0;
            this.t = OrderType.Buy;
            this.u = string.Empty;
            this.v = 0;
            this.s = string.Empty;
            this.ldt = new DateTime();
        }
    }
}
