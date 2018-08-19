using System;

namespace ProjectParser
{
    public class BackwardMetrics : ICloneable
    {
        double bavg = 0;
        double bmax = 0;
        double bmin = double.MaxValue;
        double bcnt = 0;
        double bsum = 0;
        double bcntproc = 0;

        public BackwardMetrics()
        {
        }
        
        public double Bavg { get => bavg; set => bavg = value; }
        public double Bmax { get => bmax; set => bmax = value; }
        public double Bmin { get => bmin; set => bmin = value; }
        public double Bcnt { get => bcnt; set => bcnt = value; }
        public double Bsum { get => bsum; set => bsum = value; }
        public double Bcntproc { get => bcntproc; set => bcntproc = value; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
