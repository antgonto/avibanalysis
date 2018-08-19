using System;

namespace ProjectParser
{
    public class BackwardMetrics : ICloneable
    {
        int bavg = 0;
        int bmax = 0;
        int bmin = int.MaxValue;
        int bcnt = 0;
        int bsum = 0;
        int bcntproc = 0;

        public BackwardMetrics()
        {
        }
        
        public int Bavg { get => bavg; set => bavg = value; }
        public int Bmax { get => bmax; set => bmax = value; }
        public int Bmin { get => bmin; set => bmin = value; }
        public int Bcnt { get => bcnt; set => bcnt = value; }
        public int Bsum { get => bsum; set => bsum = value; }
        public int Bcntproc { get => bcntproc; set => bcntproc = value; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
