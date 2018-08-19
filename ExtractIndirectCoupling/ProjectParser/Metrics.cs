using System;

namespace ProjectParser
{
    public class Metrics : ICloneable
    {
        double favg = 0;
        double fmax = 0;
        double fmin = double.MaxValue;
        double fcnt = 0;
        double fsum = 0;
        double fcntproc = 0;
        double bavg = 0;
        double bmax = 0;
        double bmin = double.MaxValue;
        double bcnt = 0;
        double bsum = 0;
        double bcntproc = 0;

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
            this.Bavg /= Bcnt;
        }

        public void InitBackward(double value)
        {
            this.Bsum = value;
            this.Bmax = value;
            this.Bmin = value;
            this.Bavg = value;
            this.Bcnt = 1;
        }

        public void AddBackward(double value, Metrics m)
        {
            this.Bsum = m.Bsum + value;
            this.Bmax = Math.Max(this.Bmax, this.Bsum);
            this.Bmin = Math.Min(this.Bmin, this.Bsum);
            this.Bavg += this.Bsum;
            this.Bcnt++;
        }

        public double Favg { get => favg; set => favg = value; }
        public double Fmax { get => fmax; set => fmax = value; }
        public double Fmin { get => fmin; set => fmin = value; }
        public double Fcnt { get => fcnt; set => fcnt = value; }
        public double Fsum { get => fsum; set => fsum = value; }
        public double Bavg { get => bavg; set => bavg = value; }
        public double Bmax { get => bmax; set => bmax = value; }
        public double Bmin { get => bmin; set => bmin = value; }
        public double Bcnt { get => bcnt; set => bcnt = value; }
        public double Bsum { get => bsum; set => bsum = value; }
        public double Fcntproc { get => fcntproc; set => fcntproc = value; }
        public double Bcntproc { get => bcntproc; set => bcntproc = value; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
