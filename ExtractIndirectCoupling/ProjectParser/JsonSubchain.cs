using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    class JsonSubchain : IEquatable<JsonSubchain>, IComparable<JsonSubchain>
    {
        private int from;
        private int to;
        private JsonMethod[] chain;
        private bool initial; // 0: true, 1: false
        private bool final;   // 0: true, 1: false

        public JsonSubchain(JsonMethod m)
        {
            From = m.Id;
            To = m.Id;
            Chain = new JsonMethod[1];
            Chain[0] = m;
            Initial = (m.CalledBy.Count == 0);
            Final = (m.Calls.Count == 0);
        }

        public JsonSubchain(int from, JsonMethod to, JsonMethod[] chain)
        {
            From = from;
            To = to.Id;
            Chain = new JsonMethod[chain.Length + 1];
            chain.CopyTo(Chain, 0);
            LastMethod = to;
            Initial = (Chain[0].CalledBy.Count == 0);
            Final = (to.Calls.Count == 0);
        }

        public int From { get => from; set => from = value; }
        public int To { get => to; set => to = value; }
        public JsonMethod[] Chain { get => chain; set => chain = value; }
        public bool Initial { get => initial; set => initial = value; }
        public bool Final { get => final; set => final = value; }
        public JsonMethod LastMethod { get => chain[chain.Length-1]; set => chain[chain.Length-1] = value; }

        public override bool Equals(object obj)
        {
            return Equals(obj as JsonSubchain);
        }

        public bool Equals(JsonSubchain other)
        {
            return other != null &&
                   from == other.from &&
                   to == other.to;
        }

        public override int GetHashCode()
        {
            var hashCode = -1951484959;
            hashCode = hashCode * -1521134295 + from.GetHashCode();
            hashCode = hashCode * -1521134295 + to.GetHashCode();
            return hashCode;
        }

        int IComparable<JsonSubchain>.CompareTo(JsonSubchain other)
        {
            if (this.from > other.from) return 1;
            if (this.from < other.from) return -1;
            if (this.to > other.to) return 1;
            if (this.to < other.to) return -1;
            return 0;
        }

        // Usage: Array.Sort(json_subchains, JsonSubchain.SortToFromAscending())

        public static IComparer<JsonSubchain> SortFromToAscending()
        {
            return (IComparer<JsonSubchain>)new SortFromToAscendingHelper();
        }

        public static IComparer<JsonSubchain> SortToFromAscending()
        {
            return (IComparer<JsonSubchain>)new SortToFromAscendingHelper();
        }

        public static IComparer<JsonSubchain> SortFromToFinalAscending()
        {
            return (IComparer<JsonSubchain>)new SortFromToFinalAscendingHelper();
        }

        public static IComparer<JsonSubchain> SortToFromInitialAscending()
        {
            return (IComparer<JsonSubchain>)new SortToFromInitialAscendingHelper();
        }

        private class SortFromToAscendingHelper : IComparer<JsonSubchain>
        {
            int IComparer<JsonSubchain>.Compare(JsonSubchain a, JsonSubchain b)
            {
                if (a.from > b.from) return 1;
                if (a.from < b.from) return -1;
                if (a.to > b.to) return 1;
                if (a.to < b.to) return -1;
                return 0;
            }
        }

        private class SortToFromAscendingHelper : IComparer<JsonSubchain>
        {
            int IComparer<JsonSubchain>.Compare(JsonSubchain a, JsonSubchain b)
            {
                if (a.to > b.to) return 1;
                if (a.to < b.to) return -1;
                if (a.from > b.from) return 1;
                if (a.from < b.from) return -1;
                return 0;
            }
        }

        private class SortFromToFinalAscendingHelper : IComparer<JsonSubchain>
        {
            int IComparer<JsonSubchain>.Compare(JsonSubchain a, JsonSubchain b)
            {
                if (a.from > b.from) return 1;
                if (a.from < b.from) return -1;
                if (a.Final && !b.Final) return 1;
                if (!a.Final && b.Final) return -1;
                if (a.to > b.to) return 1;
                if (a.to < b.to) return -1;
                return 0;
            }
        }

        private class SortToFromInitialAscendingHelper : IComparer<JsonSubchain>
        {
            int IComparer<JsonSubchain>.Compare(JsonSubchain a, JsonSubchain b)
            {
                if (a.to > b.to) return 1;
                if (a.to < b.to) return -1;
                if (a.Initial && !b.Initial) return 1;
                if (!a.Initial && b.Initial) return -1;
                if (a.from > b.from) return 1;
                if (a.from < b.from) return -1;
                return 0;
            }
        }
    }
}
