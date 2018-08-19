using System;

namespace ProjectParser
{
    public class ForwardMetrics : ICloneable
    {
        double favg = 0;
        double fmax = 0;
        double fmin = double.MaxValue;
        double fcnt = 0;
        double fsum = 0;
        double fcntproc = 0;

        public ForwardMetrics()
        {
        }

        public void InitForward(double value)
        {
            this.Fsum = value;
            this.Fmax = value;
            this.Fmin = value;
            this.Favg = value;
            this.Fcnt = 1;
        }

        public void AddForward(double value, Metrics m)
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

        public double Favg { get => favg; set => favg = value; }
        public double Fmax { get => fmax; set => fmax = value; }
        public double Fmin { get => fmin; set => fmin = value; }
        public double Fcnt { get => fcnt; set => fcnt = value; }
        public double Fsum { get => fsum; set => fsum = value; }
        public double Fcntproc { get => fcntproc; set => fcntproc = value; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
