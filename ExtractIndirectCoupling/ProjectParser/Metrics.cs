using System;
using System.Collections.Generic;

namespace ProjectParser
{
    public class Metrics<T, U> : ICloneable
    {
        U favg;
        T fmax;
        T fmin;
        T fcnt;
        U fsum;
        T fnet;
        T facc;
        T fcntproc;

        U bavg;
        T bmax;
        T bmin;
        T bcnt;
        U bsum;
        T bnet;
        T bacc;             
        T bcntproc;

        public Metrics()
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
            
            this.bavg = default(U);
            this.bmax = default(T);
            this.bmin = MaxValue((dynamic)value);
            this.bcnt = default(T);
            this.bsum = default(U);
            this.bnet = default(T);
            this.bacc = default(T);
            this.bcntproc = default(T);
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

        public ForwardMetrics<T, U> GetForwardMetrics()
        {
            ForwardMetrics<T, U> m = new ForwardMetrics<T, U>();

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

        public BackwardMetrics<T, U> GetBackwardMetrics()
        {
            BackwardMetrics<T, U> m = new BackwardMetrics<T, U>();

            m.Bavg = this.Bavg;
            m.Bmin = this.Bmin;
            m.Bmax = this.Bmax;
            m.Bcnt = this.Bcnt;
            m.Bsum = this.Bsum;
            m.Bnet = this.Bnet;
            m.Bacc = this.Bacc;
            m.Bcntproc = this.Bcntproc;

            return m;
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
            this.Bavg /= (dynamic)Bcnt;
        }

        public void InitBackward(T value)
        {
            this.Bsum = (U)(dynamic)value;
            this.Bnet = value;
            this.Bmax = value;
            this.Bmin = value;
            this.Bavg = (U)(dynamic)value;
            this.Bcnt = (dynamic)1;
        }

        public void AddBackward(T value, Metrics<T, U> m)
        {
            this.Bsum = m.Bsum + (dynamic)value;
            this.Bmax = Math.Max((dynamic)this.Bmax, (dynamic)this.Bsum);
            this.Bmin = Math.Min((dynamic)this.Bmin, (dynamic)this.Bsum);
            this.Bavg += (dynamic)this.Bsum;
            this.Bcnt += (dynamic)1;
        }

        public void AddForwardMetrics(T s, T n)
        {
            fmax = Math.Max((dynamic)s, (dynamic)fmax);
            fmin = Math.Min((dynamic)s, (dynamic)fmin);
            fsum += (dynamic)s;
            favg += (dynamic)s;
            fnet += (dynamic)n;
            fcnt += (dynamic)1;
        }

        public U Favg { get => favg; set => favg = value; }
        public T Fmax { get => fmax; set => fmax = value; }
        public T Fmin { get => fmin; set => fmin = value; }
        public T Fcnt { get => fcnt; set => fcnt = value; }
        public U Fsum { get => fsum; set => fsum = value; }
        public T Fcntproc { get => fcntproc; set => fcntproc = value; }
        public T Fnet { get => fnet; set => fnet = value; }
        public T Facc { get => facc; set => facc = value; }
        public U Bavg { get => bavg; set => bavg = value; }
        public T Bmax { get => bmax; set => bmax = value; }
        public T Bmin { get => bmin; set => bmin = value; }
        public T Bcnt { get => bcnt; set => bcnt = value; }
        public U Bsum { get => bsum; set => bsum = value; }
        public T Bcntproc { get => bcntproc; set => bcntproc = value; }
        public T Bnet { get => bnet; set => bnet = value; }
        public T Bacc { get => bacc; set => bacc = value; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
