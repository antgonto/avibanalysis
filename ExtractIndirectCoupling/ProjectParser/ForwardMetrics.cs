using System;

namespace ProjectParser
{
    public class ForwardMetrics<T, U> : ICloneable
    {
        U favg;
        T fmax;
        T fmin;
        T fcnt;
        U fsum;
        T fnet;
        T facc;
        T fcntproc;

        public ForwardMetrics()
        {
            T value = default(T);
            this.favg = default(U);
            this.fmax = default(T);
            this.fmin = MaxValue((dynamic)value);
            this.fcnt = default(T);
            this.fsum = default(U);
            this.fnet = default(T);
            this.facc = default(T);
            this.fcntproc = default(T);
        }

        public int MaxValue(int dummy)
        {
            return int.MaxValue;
        }

        public long MaxValue(long dummy)
        {
            return long.MaxValue;
        }

        public double MaxValue(double dummy)
        {
            return double.MaxValue;
        }

        public float MaxValue(float dummy)
        {
            return float.MaxValue;
        }

        public object MaxValue(object dummy)
        {
            throw new NotSupportedException(dummy.GetType().Name);
        }

        public void InitForward(T value)
        {
            this.Fsum = (U)(dynamic)value;
            this.Fnet = value;
            this.Facc = value;
            this.Fmax = value;
            this.Fmin = value;
            this.Favg = (U)(dynamic)value;
            this.Fcnt = (dynamic)1;
        }

        public void AddForward(T value, Metrics<T, U> m)
        {
            this.Fsum = m.Fsum + (dynamic)value;
            this.Facc = m.Facc + (dynamic)value;
            this.Fmax = Math.Max((dynamic)this.Fmax, (dynamic)this.Fsum);
            this.Fmin = Math.Min((dynamic)this.Fmin, (dynamic)this.Fsum);
            this.Favg += (dynamic)this.Fsum;
            this.Fcnt += (dynamic)1;
        }

        public void AvgMetrics()
        {
            this.Favg /= (dynamic)Fcnt;
        }

        public U Favg { get => favg; set => favg = value; }
        public T Fmax { get => fmax; set => fmax = value; }
        public T Fmin { get => fmin; set => fmin = value; }
        public T Fcnt { get => fcnt; set => fcnt = value; }
        public U Fsum { get => fsum; set => fsum = value; }
        public T Fcntproc { get => fcntproc; set => fcntproc = value; }
        public T Fnet { get => fnet; set => fnet = value; }
        public T Facc { get => facc; set => facc = value; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
