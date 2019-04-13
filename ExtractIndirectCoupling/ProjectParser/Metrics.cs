using System;

namespace ProjectParser
{
    public class Metrics : ICloneable
    {
        long favg = 0;
        int fmax = 0;
        int fmin = int.MaxValue;
        int fcnt = 0;
        long fsum = 0;
        int fnet = 0;
        int facc = 0;
        int fcntproc = 0;
        long bavg = 0;
        int bmax = 0;
        int bmin = int.MaxValue;
        int bcnt = 0;
        long bsum = 0;
        int bnet = 0;
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
            m.Fnet = this.Fnet;
            m.Facc = this.Facc;
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
            m.Bnet = this.Bnet;
            m.Bcntproc = this.Bcntproc;

            return m;
        }

        public void InitForward(int value)
        {
            this.Fsum = value;
            this.Fnet = value;
            this.Facc = value;
            this.Fmax = value;
            this.Fmin = value;
            this.Favg = value;
            this.Fcnt = 1;
        }

        public void AddForward(int value, Metrics m)
        {
            this.Fsum = m.Fsum + value;
            this.Facc = m.Facc + value;
            this.Fmax = Math.Max(this.Fmax, (int)this.Fsum);
            this.Fmin = Math.Min(this.Fmin, (int)this.Fsum);
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
            this.Bnet = value;
            this.Bmax = value;
            this.Bmin = value;
            this.Bavg = value;
            this.Bcnt = 1;
        }

        public void AddBackward(int value, Metrics m)
        {
            this.Bsum = m.Bsum + value;
            this.Bmax = Math.Max(this.Bmax, (int)this.Bsum);
            this.Bmin = Math.Min(this.Bmin, (int)this.Bsum);
            this.Bavg += this.Bsum;
            this.Bcnt++;
        }

        public long Favg { get => favg; set => favg = value; }
        public int Fmax { get => fmax; set => fmax = value; }
        public int Fmin { get => fmin; set => fmin = value; }
        public int Fcnt { get => fcnt; set => fcnt = value; }
        public long Fsum { get => fsum; set => fsum = value; }
        public long Bavg { get => bavg; set => bavg = value; }
        public int Bmax { get => bmax; set => bmax = value; }
        public int Bmin { get => bmin; set => bmin = value; }
        public int Bcnt { get => bcnt; set => bcnt = value; }
        public long Bsum { get => bsum; set => bsum = value; }
        public int Fcntproc { get => fcntproc; set => fcntproc = value; }
        public int Bcntproc { get => bcntproc; set => bcntproc = value; }
        public int Fnet { get => fnet; set => fnet = value; }
        public int Bnet { get => bnet; set => bnet = value; }
        public int Facc { get => facc; set => facc = value; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
