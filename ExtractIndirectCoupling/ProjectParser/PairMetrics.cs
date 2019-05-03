using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    class PairMetrics
    {
        ForwardMetrics<int, long> k = new ForwardMetrics<int, long>();
        ForwardMetrics<int, long> l = new ForwardMetrics<int, long>();
        ForwardMetrics<int, long> c = new ForwardMetrics<int, long>();

        public ForwardMetrics<int, long> K { get => k; set => k = value; }
        public ForwardMetrics<int, long> L { get => l; set => l = value; }
        public ForwardMetrics<int, long> C { get => c; set => c = value; }
    }
}
