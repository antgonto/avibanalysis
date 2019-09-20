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
        private int position;
        private int count;
        private int sourceStart;
        private int offset;
        private bool merged;
        private int level;
        private int index;
        private int threshold;
        private JsonSubchain nextChain;
        private IComparer<JsonSubchain> comparer;

        public string StreamName { get => streamName; set => streamName = value; }
        public BufferedStream ChainStream { get => chainStream; set => chainStream = value; }
        public int Position { get => position; set => position = value; }
        public int Count { get => count; set => count = value; }
        public int Offset { get => offset; set => offset = value; }
        public bool Merged { get => merged; set => merged = value; }
        public int Level { get => level; set => level = value; }
        public int Index { get => index; set => index = value; }
        public int Threshold { get => threshold; set => threshold = value; }
        public JsonChainStream SourceStream { get => sourceStream; set => sourceStream = value; }
        public int SourceStart { get => sourceStart; set => sourceStart = value; }
        internal JsonSubchain NextChain { get => nextChain; set => nextChain = value; }
        internal IComparer<JsonSubchain> Comparer { get => comparer; set => comparer = value; }

        public void Open(string filename, IComparer<JsonSubchain> comparer)
        {
            StreamName = filename;
            Position = 0;
            Count = 0;
            SourceStart = 0;
            Offset = JsonSubchainRecord.ByteArrayLen;
            Merged = false;
            Level = 0;
            Index = 0;
            ChainStream = new BufferedStream(File.Open(StreamName, FileMode.Create), 4096 * Offset);
            SourceStream = this;
            Threshold = 100000;
            Comparer = comparer;
        }

        public void Open(JsonChainStream stream, int sourceStart, int count, int level)
        {
            StreamName = stream.StreamName + (++stream.Index).ToString();
            Position = 0;
            Count = count;
            SourceStart = sourceStart;
            Offset = stream.Offset;
            Merged = true;
            Level = level + 1;
            Index = stream.Index;
            ChainStream = new BufferedStream(File.Open(StreamName, FileMode.Create));
            SourceStream = stream.SourceStream;
            Threshold = stream.Threshold;
            Comparer = stream.Comparer;
        }

        public void Close()
        {
            if (ChainStream != null)
            {
                ChainStream.Close();
                ChainStream = null;
            }

            if (File.Exists(StreamName))
                File.Delete(StreamName);
        }

        public void Seek(int position)
        {
            ChainStream.Seek(position * Offset, SeekOrigin.Begin);
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
            Position++;
            Count++;
        }

        public void Write(JsonSubchain chain, int position)
        {
            ChainStream.Seek(position * Offset, SeekOrigin.Begin);
            ChainStream.Write(JsonSubchainRecord.ObjectToByteArray(new JsonSubchainRecord(chain)), 0, Offset);
            Position = position + 1;
        }

        public JsonSubchain Read()
        {
            byte[] bytes = new byte[Offset];
            ChainStream.Read(bytes, 0, Offset);
            Position++;
            return JsonSubchainRecord.ByteArrayToObject(bytes).GetJsonSubchain();
        }

        public JsonSubchain Read(int position)
        {
            byte[] bytes = new byte[Offset];
            ChainStream.Seek(position * Offset, SeekOrigin.Begin);
            ChainStream.Read(bytes, 0, Offset);
            Position = position + 1;
            return JsonSubchainRecord.ByteArrayToObject(bytes).GetJsonSubchain();
        }

        public JsonSubchain[] Read(int position, int count)
        {
            byte[] bytes = new byte[Offset];
            byte[] buffer = new byte[Offset * count];
            JsonSubchain[] chains = new JsonSubchain[count];
            ChainStream.Seek(position * Offset, SeekOrigin.Begin);
            ChainStream.Read(buffer, 0, Offset * count);
            Position = position + count;
            for (int i = 0; i < count; i++)
            {
                Array.Copy(buffer, i * Offset, bytes, 0, Offset);
                chains[i] = JsonSubchainRecord.ByteArrayToObject(bytes).GetJsonSubchain();
            }
            return chains;
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

                left.Close();
                right.Close();
            }
            return this;
        }

        public void Sort()
        {
            JsonSubchain[] subchains = SourceStream.Read(SourceStart, Count);
            Array.Sort(subchains, Comparer);
            this.Seek(0);
            for (int i = 0; i < Count; i++)
            {
                this.Write(subchains[i]);
            }
        }

        public void Merge(JsonChainStream result, JsonChainStream left, JsonChainStream right)
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
                    if (Comparer.Compare(left.NextChain, right.NextChain) > -1)
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
