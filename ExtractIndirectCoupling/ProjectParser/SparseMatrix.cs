using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    public class SparseMatrix<T>
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public long Size { get; private set; }

        private Dictionary<long, T> _cells = new Dictionary<long, T>();

        public SparseMatrix(int w, int h)
        {
            this.Width = w;
            this.Height = h;
            this.Size = (long)w * (long)h;
        }

        public void IncreaseHeight(int h)
        {
            if (h < this.Height) throw new InvalidOperationException("Matrix height cannot be shrunk");
            this.Height = h;
            this.Size = (long)this.Width * (long)h;
        }

        public bool IsCellPresent(int row, int col)
        {
            if (col >= this.Width) throw new IndexOutOfRangeException("Column index is out of range");
            if (row >= this.Height)
                IncreaseHeight((int)(row * 1.25));

            long index = (long)row * (long)Width + (long)col;
            return _cells.ContainsKey(index);
        }

        public T this[int row, int col]
        {
            get
            {
                if (col >= this.Width) throw new IndexOutOfRangeException("Column index is out of range");

                long index = (long)row * (long)Width + (long)col;
                T result;
                _cells.TryGetValue(index, out result);
                return result;
            }
            set
            {
                if (col >= this.Width) throw new IndexOutOfRangeException("Column index is out of range");
                if (row >= this.Height)
                    IncreaseHeight((int)(row * 1.25));

                long index = (long)row * (long)Width + (long)col;
                _cells[index] = value;
            }
        }
    }
}
