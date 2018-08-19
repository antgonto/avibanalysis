using System;

namespace ProjectParser
{
    public class Metrics : ICloneable
    {
        int favg = 0;
        int fmax = 0;
        int fmin = int.MaxValue;
        int fcnt = 0;
        int fsum = 0;
        int fcntproc = 0;
        int bavg = 0;
        int bmax = 0;
        int bmin = int.MaxValue;
        int bcnt = 0;
        int bsum = 0;
        int bcntproc = 0;

        public Metrics()
        {
        }

        public ForwardMetrics GetForwardMetrics()
        {
            ForwardMetrics m = new ForwardMetrics();

            m.Favg = this.Favg;
            m.Fmin = this.Fmin;
            m.Fmax = this.Fmax;
            m.Fcnt = this.Fcnt;
            m.Fsum = this.Fsum;
            m.Fcntproc = this.Fcntproc;

            return m;
        }

        public BackwardMetrics GetBackwardMetrics()
        {
            BackwardMetrics m = new BackwardMetrics();

            m.Bavg = this.Bavg;
            m.Bmin = this.Bmin;
            m.Bmax = this.Bmax;
            m.Bcnt = this.Bcnt;
            m.Bsum = this.Bsum;
            m.Bcntproc = this.Bcntproc;

            return m;
        }

        public void InitForward(int value)
        {
            this.Fsum = value;
            this.Fmax = value;
            this.Fmin = value;
            this.Favg = value;
            this.Fcnt = 1;
        }

        public void AddForward(int value, Metrics m)
        {
            this.Fsum = m.Fsum + value;
            this.Fmax = Math.Max(this.Fmax, this.Fsum);
            this.Fmin = Math.Min(this.Fmin, this.Fsum);
            this.Favg += this.Fsum;
            this.Fcnt++;
        }

        public void AvgMetrics()
        {
            this.Favg /= Fcnt;
            this.Bavg /= Bcnt;
        }

        public void InitBackward(int value)
        {
            this.Bsum = value;
            this.Bmax = value;
            this.Bmin = value;
            this.Bavg = value;
            this.Bcnt = 1;
        }

        public void AddBackward(int value, Metrics m)
        {
            this.Bsum = m.Bsum + value;
            this.Bmax = Math.Max(this.Bmax, this.Bsum);
            this.Bmin = Math.Min(this.Bmin, this.Bsum);
            this.Bavg += this.Bsum;
            this.Bcnt++;
        }

        public int Favg { get => favg; set => favg = value; }
        public int Fmax { get => fmax; set => fmax = value; }
        public int Fmin { get => fmin; set => fmin = value; }
        public int Fcnt { get => fcnt; set => fcnt = value; }
        public int Fsum { get => fsum; set => fsum = value; }
        public int Bavg { get => bavg; set => bavg = value; }
        public int Bmax { get => bmax; set => bmax = value; }
        public int Bmin { get => bmin; set => bmin = value; }
        public int Bcnt { get => bcnt; set => bcnt = value; }
        public int Bsum { get => bsum; set => bsum = value; }
        public int Fcntproc { get => fcntproc; set => fcntproc = value; }
        public int Bcntproc { get => bcntproc; set => bcntproc = value; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
