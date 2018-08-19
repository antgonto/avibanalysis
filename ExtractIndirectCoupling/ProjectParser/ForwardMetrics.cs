using System;

namespace ProjectParser
{
    public class ForwardMetrics : ICloneable
    {
        int favg = 0;
        int fmax = 0;
        int fmin = int.MaxValue;
        int fcnt = 0;
        int fsum = 0;
        int fcntproc = 0;

        public ForwardMetrics()
        {
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
        }

        public int Favg { get => favg; set => favg = value; }
        public int Fmax { get => fmax; set => fmax = value; }
        public int Fmin { get => fmin; set => fmin = value; }
        public int Fcnt { get => fcnt; set => fcnt = value; }
        public int Fsum { get => fsum; set => fsum = value; }
        public int Fcntproc { get => fcntproc; set => fcntproc = value; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
