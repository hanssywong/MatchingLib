using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchingLib
{
    public interface IMethodResult
    {
        bool Success { get; set; }
        string Comment { get; set; }
    }

    public class MethodResult : IMethodResult
    {
        public string Comment { get; set; } = string.Empty;

        public bool Success { get; set; } = false;
    }
}
