using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    class PairMetrics
    {
        string method1;
        string method2;
        ForwardMetrics k = new ForwardMetrics();
        ForwardMetrics l = new ForwardMetrics();
        ForwardMetrics c = new ForwardMetrics();

        public ForwardMetrics K { get => k; set => k = value; }
        public ForwardMetrics L { get => l; set => l = value; }
        public ForwardMetrics C { get => c; set => c = value; }
        public string Method1 { get => method1; set => method1 = value; }
        public string Method2 { get => method2; set => method2 = value; }
    }
}
