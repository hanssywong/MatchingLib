using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatchingLib
{
    interface ICouchbase
    {
        string classType { get; }
        string GetCbKey();
    }
}
