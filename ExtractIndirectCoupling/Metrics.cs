using System;

namespace ProjectParser
{
    public class Metrics
    {
        double favg = 0;
        long fmax = long.MinValue;
        long fmin = long.MaxValue;
        long fcnt = 0;
        long fsum = 0;
        long fcntproc = 0;
        double bavg = 0;
        long bmax = long.MinValue;
        long bmin = long.MaxValue;
        long bcnt = 0;
        long bsum = 0;
        long bcntproc = 0;

        public Metrics()
        {
        }

        public double Favg { get => favg; set => favg = value; }
        public long Fmax { get => fmax; set => fmax = value; }
        public long Fmin { get => fmin; set => fmin = value; }
        public long Fcnt { get => fcnt; set => fcnt = value; }
        public long Fsum { get => fsum; set => fsum = value; }
        public double Bavg { get => bavg; set => bavg = value; }
        public long Bmax { get => bmax; set => bmax = value; }
        public long Bmin { get => bmin; set => bmin = value; }
        public long Bcnt { get => bcnt; set => bcnt = value; }
        public long Bsum { get => bsum; set => bsum = value; }
        public long Fcntproc { get => fcntproc; set => fcntproc = value; }
        public long Bcntproc { get => bcntproc; set => bcntproc = value; }
    }
}

