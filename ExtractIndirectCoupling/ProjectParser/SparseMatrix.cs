using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    public class SparseMatrix<T>
    {
        public long Width { get; private set; }
        public long Height { get; private set; }
        public long Size { get; private set; }

        private Dictionary<long, T> _cells = new Dictionary<long, T>();

        public SparseMatrix(long w, long h)
        {
            this.Width = w;
            this.Height = h;
            this.Size = w * h;
        }

        public void IncreaseHeight(long h)
        {
            if (h < this.Height) throw new InvalidOperationException("Matrix height cannot be shrunk");
            this.Height = h;
            this.Size = this.Width * h;
        }

        public bool IsCellPresent(long row, long col)
        {
            if (col >= this.Width) throw new IndexOutOfRangeException("Column index is out of range");
            if (row >= this.Height)
                IncreaseHeight((long)(row * 1.25));

            long index = row * Width + col;
            return _cells.ContainsKey(index);
        }

        public T this[long row, long col]
        {
            get
            {
                if (col >= this.Width) throw new IndexOutOfRangeException("Column index is out of range");

                long index = row * Width + col;
                T result;
                _cells.TryGetValue(index, out result);
                return result;
            }
            set
            {
                if (col >= this.Width) throw new IndexOutOfRangeException("Column index is out of range");
                if (row >= this.Height)
                    IncreaseHeight((long)(row * 1.25));

                long index = row * Width + col;
                _cells[index] = value;
            }
        }
    }
}
