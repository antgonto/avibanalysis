using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    class JsonChainStream
    {
        private string streamName;
        private BufferedStream chainStream;
        private JsonChainStream sourceStream;
        private long position;
        private int count;
        private long sourceStart;
        private int offset;
        private bool merged;
        private int level;
        private int index;
        private int threshold;
        private JsonSubchain nextChain;
        private IComparer<JsonSubchain> comparer;

        public string StreamName { get => streamName; set => streamName = value; }
        public BufferedStream ChainStream { get => chainStream; set => chainStream = value; }
        public long Position { get => position; set => position = value; }
        public int Count { get => count; set => count = value; }
        public int Offset { get => offset; set => offset = value; }
        public bool Merged { get => merged; set => merged = value; }
        public int Level { get => level; set => level = value; }
        public int Index { get => index; set => index = value; }
        public int Threshold { get => threshold; set => threshold = value; }
        public JsonChainStream SourceStream { get => sourceStream; set => sourceStream = value; }
        public long SourceStart { get => sourceStart; set => sourceStart = value; }
        internal JsonSubchain NextChain { get => nextChain; set => nextChain = value; }
        internal IComparer<JsonSubchain> Comparer { get => comparer; set => comparer = value; }

        public void Open(string filename, IComparer<JsonSubchain> comparer, bool delete = false)
        {
            StreamName = filename;
            Position = 0;
            Count = 0;
            SourceStart = 0;
            Offset = JsonSubchainRecord.ByteArrayLen;
            Merged = false;
            Level = 0;
            Index = 0;
            if (delete && File.Exists(StreamName))
            {
               try
                {
                    File.Delete(StreamName);
                }
                catch (Exception e)
                {
                    // ignore
                }
            }
            ChainStream = new BufferedStream(File.Open(StreamName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite), 512000 * Offset);
            SourceStream = this;
            Threshold = 1024000000 / JsonSubchainRecord.ByteArrayLen;
            Comparer = comparer;
        }

        public void Open(JsonChainStream stream, long sourceStart, int count, int level, bool delete = false)
        {
            StreamName = stream.StreamName + (++stream.Index).ToString();
            Position = 0;
            Count = count;
            SourceStart = sourceStart;
            Offset = stream.Offset;
            Merged = true;
            Level = level + 1;
            Index = stream.Index;
            if (delete && File.Exists(StreamName))
            {
                try
                {
                    File.Delete(StreamName);
                }
                catch (Exception e)
                {
                    // ignore
                }
            }
            ChainStream = new BufferedStream(File.Open(StreamName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite));
            SourceStream = stream.SourceStream;
            Threshold = stream.Threshold;
            Comparer = stream.Comparer;
        }

        public void Close(bool delete = false)
        {
            if (ChainStream != null)
            {
                ChainStream.Close();
                ChainStream = null;
            }

            try
            {
                if (delete && File.Exists(StreamName))
                    File.Delete(StreamName);
            }
            catch (Exception e)
            {
                // ignore
            }
        }

        public void Seek(long position)
        {
            ChainStream.Seek(position * Offset, SeekOrigin.Begin);
            Position = position;
        }

        public void Flush()
        {
            ChainStream.Flush();
        }

        public bool HasNext()
        {
            return Position < Count;
        }

        public void GetNext()
        {
            NextChain = Read();
        }

        public void Write(JsonSubchain chain)
        {
            ChainStream.Write(JsonSubchainRecord.ObjectToByteArray(new JsonSubchainRecord(chain)), 0, Offset);
            if (Position >= Count) Count++;
            Position++;
        }

        public void Write(JsonSubchain chain, long position)
        {
            ChainStream.Seek(position * Offset, SeekOrigin.Begin);
            ChainStream.Write(JsonSubchainRecord.ObjectToByteArray(new JsonSubchainRecord(chain)), 0, Offset);
            Position = position + 1;
        }

        public JsonSubchain Read()
        {
            byte[] bytes = new byte[Offset];
            //ChainStream.Seek(Position * Offset, SeekOrigin.Begin);
            ChainStream.Read(bytes, 0, Offset);
            Position++;
            return JsonSubchainRecord.ByteArrayToObject(bytes).GetJsonSubchain();
        }

        public JsonSubchain Read(long position)
        {
            byte[] bytes = new byte[Offset];
            ChainStream.Seek(position * Offset, SeekOrigin.Begin);
            ChainStream.Read(bytes, 0, Offset);
            Position = position + 1;
            return JsonSubchainRecord.ByteArrayToObject(bytes).GetJsonSubchain();
        }

        public JsonSubchain[] Read(long position, long count)
        {
            byte[] bytes = new byte[Offset];
            byte[] buffer = new byte[Offset * count];
            JsonSubchain[] chains = new JsonSubchain[count];
            ChainStream.Seek(position * Offset, SeekOrigin.Begin);
            ChainStream.Read(buffer, 0, Offset * (int)count);
            Position = position + count;
            for (long i = 0; i < count; i++)
            {
                Array.Copy(buffer, i * Offset, bytes, 0, Offset);
                chains[i] = JsonSubchainRecord.ByteArrayToObject(bytes).GetJsonSubchain();
            }
            return chains;
        }

        public void CheckSort()
        {
            Seek(0);
            long minK = (Count - 1) / Threshold;
            long maxK = (Count + Threshold - 1) / Threshold;
            bool init = true;
            int next;
            JsonSubchain subchain = null;
            JsonSubchain[] chains = new JsonSubchain[Threshold];

            for (int i = 0; i < minK; i++)
            {
                chains = Read(Position, Threshold);
                next = 0;
                if (init)
                {
                    init = false;
                    subchain = chains[0];
                    next++;
                }
                for (int j = next; j < Threshold; j++)
                {
                    if (Comparer.Compare(subchain, chains[j]) > 0)
                    {
                        bool stopHere = true;
                        Console.Write("****");
                    }
                    Console.WriteLine("from: {0} to: {1} initial: {2} final: {3}", 
                        subchain.From.ToString().PadLeft(4),
                        subchain.To.ToString().PadLeft(4),
                        subchain.Initial.ToString().PadRight(5), 
                        subchain.Final.ToString().PadRight(5));
                    subchain = chains[j];
                }
            }

            long q = Threshold - ((maxK * Threshold) - Count);
            if (q > 0)
            {
                chains = Read(Position, q);
                next = 0;
                if (init)
                {
                    subchain = chains[0];
                    next++;
                }
                for (int j = next; j < q; j++)
                {
                    if (Comparer.Compare(subchain, chains[j]) > 0)
                    {
                        bool stopHere = true;
                        Console.Write("****");
                    }
                    Console.WriteLine("from: {0} to: {1} initial: {2} final: {3}",
                        subchain.From.ToString().PadLeft(4),
                        subchain.To.ToString().PadLeft(4),
                        subchain.Initial.ToString().PadRight(5),
                        subchain.Final.ToString().PadRight(5));
                    subchain = chains[j];
                }
            }
        }

        public JsonChainStream MergeSort()
        {
            if (Count <= Threshold)
            {
                Sort();
            }
            else
            {
                JsonChainStream left = new JsonChainStream();
                left.Open(SourceStream, SourceStart, (Count + 1) / 2, Level + 1);
                left.MergeSort();

                JsonChainStream right = new JsonChainStream();
                right.Open(SourceStream, SourceStart + (Count + 1) / 2, Count - (Count + 1) / 2, Level + 1);
                right.MergeSort();

                Merge(this, left, right);

                left.Close(true);
                right.Close(true);
            }
            return this;
        }

        private void Sort()
        {
            JsonSubchain[] subchains = SourceStream.Read(SourceStart, Count);
            Array.Sort(subchains, Comparer);
            this.Seek(0);
            for (int i = 0; i < Count; i++)
            {
                this.Write(subchains[i]);
            }
        }

        private void Merge(JsonChainStream result, JsonChainStream left, JsonChainStream right)
        {
            left.Seek(0);
            right.Seek(0);
            result.Seek(0);

            if (left.HasNext() && right.HasNext())
            {
                left.GetNext();
                right.GetNext();

                while (true)
                {
                    if (Comparer.Compare(left.NextChain, right.NextChain) < 1)
                    {
                        result.Write(left.NextChain);
                        if (left.HasNext())
                            left.GetNext();
                        else
                        {
                            result.Write(right.NextChain);
                            while (right.HasNext())
                            {
                                right.GetNext();
                                result.Write(right.NextChain);
                            }
                            break;
                        }
                    }
                    else
                    {
                        result.Write(right.NextChain);
                        if (right.HasNext())
                            right.GetNext();
                        else
                        {
                            result.Write(left.NextChain);
                            while (left.HasNext())
                            {
                                left.GetNext();
                                result.Write(left.NextChain);
                            }
                            break;
                        }
                    }
                }
            }
        }
    }
}
