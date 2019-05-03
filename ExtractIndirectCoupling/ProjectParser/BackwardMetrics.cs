using System;

namespace ProjectParser
{
    public class BackwardMetrics<T, U> : ICloneable
    {
        U bavg;
        T bmax;
        T bmin;
        T bcnt;
        U bsum;
        T bnet;
        T bacc;
        T bcntproc;

        public U Bavg { get => bavg; set => bavg = value; }
        public T Bmax { get => bmax; set => bmax = value; }
        public T Bmin { get => bmin; set => bmin = value; }
        public T Bcnt { get => bcnt; set => bcnt = value; }
        public U Bsum { get => bsum; set => bsum = value; }
        public T Bnet { get => bnet; set => bnet = value; }
        public T Bacc { get => bacc; set => bacc = value; }
        public T Bcntproc { get => bcntproc; set => bcntproc = value; }

        public BackwardMetrics()
        {
            T value = default(T);

            this.Bavg = default(U);
            this.Bmax = default(T);
            this.Bmin = MaxValue((dynamic)value);
            this.Bcnt = default(T);
            this.Bsum = default(U);
            this.Bnet = default(T);
            this.Bacc = default(T);
            this.Bcntproc = default(T);
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

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
