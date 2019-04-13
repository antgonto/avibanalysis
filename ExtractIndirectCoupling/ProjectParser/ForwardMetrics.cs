using System;

namespace ProjectParser
{
    public class ForwardMetrics : ICloneable
    {
        long favg = 0;
        int fmax = 0;
        int fmin = int.MaxValue;
        int fcnt = 0;
        long fsum = 0;
        int fnet = 0;
        int facc = 0;
        int fcntproc = 0;

        public ForwardMetrics()
        {
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
        }

        public long Favg { get => favg; set => favg = value; }
        public int Fmax { get => fmax; set => fmax = value; }
        public int Fmin { get => fmin; set => fmin = value; }
        public int Fcnt { get => fcnt; set => fcnt = value; }
        public long Fsum { get => fsum; set => fsum = value; }
        public int Fcntproc { get => fcntproc; set => fcntproc = value; }
        public int Fnet { get => fnet; set => fnet = value; }
        public int Facc { get => facc; set => facc = value; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
