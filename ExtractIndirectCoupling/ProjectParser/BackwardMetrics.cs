using System;

namespace ProjectParser
{
    public class BackwardMetrics : ICloneable
    {
        long bavg = 0;
        int bmax = 0;
        int bmin = int.MaxValue;
        int bcnt = 0;
        long bsum = 0;
        int bnet = 0;
        int bcntproc = 0;

        public BackwardMetrics()
        {
        }
        
        public long Bavg { get => bavg; set => bavg = value; }
        public int Bmax { get => bmax; set => bmax = value; }
        public int Bmin { get => bmin; set => bmin = value; }
        public int Bcnt { get => bcnt; set => bcnt = value; }
        public long Bsum { get => bsum; set => bsum = value; }
        public int Bcntproc { get => bcntproc; set => bcntproc = value; }
        public int Bnet { get => bnet; set => bnet = value; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
