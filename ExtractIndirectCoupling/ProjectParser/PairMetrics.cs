using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    class PairMetrics
    {
        ForwardMetrics k = new ForwardMetrics();
        ForwardMetrics l = new ForwardMetrics();
        ForwardMetrics c = new ForwardMetrics();

        public ForwardMetrics K { get => k; set => k = value; }
        public ForwardMetrics L { get => l; set => l = value; }
        public ForwardMetrics C { get => c; set => c = value; }
    }
}
