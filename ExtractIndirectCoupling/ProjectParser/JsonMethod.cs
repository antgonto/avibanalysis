using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectParser
{
    [JsonObject(MemberSerialization.OptIn, Description = "Method")]
    public class JsonMethod : IEquatable<JsonMethod>
    {
        bool wasProcessed = false;

        static public int cantidadMetodos = 0;
        static public int totalDeLOC = 0;
        static public int numOfChains = 0;
        static public int maxChainLength = 0;
        static public int avgChainLength = 0;
        static int maxChains = 1000000;
        static int chainsCnt = 0;
        //static int[] chainMethodsCnt;
        static List<int> chainMethodsCnt;
        static SparseMatrix<JsonLink> chainMethods;
        static int maxMethods = 1000;
        static int[] methodChainsCnt;
        //static List<int> methodChainsCnt;
        static SparseMatrix<JsonLink> methodChains;
        static SparseMatrix<int> methodChainPairs;
        static SparseMatrix<PairMetrics> pairMetricsList;
        static List<Tuple<int, int, PairMetrics>> pairMetrics;

        static int count = 0;
        static int avgdepth = 0;
        static public int numMethods = 0;
        static public int maxCalledBy = 0;
        static public int maxCallsTo = 0;

        private static SystemStats stats = new SystemStats();

        int id;
        int sccId = -1;
        JsonMethod scc = null;
        bool isMethod = true;
        bool isScc = false;
        bool isCollapsed = false;
        bool isRecursive = false;
        bool isSccIn = false;
        bool isSccOut = false;
        string name;
        string fullname;
        JsonClass oclass;
        JsonNamespace onamespace;
        List<JsonMethod> sccMethods = new List<JsonMethod>();
        List<JsonCall> calls = new List<JsonCall>();
        List<JsonCall> calledBy = new List<JsonCall>();
        List<JsonCall> callsSCC = new List<JsonCall>();
        List<JsonCall> calledBySCC = new List<JsonCall>();
        long numChains = 0;
        long jumps = 0;
        long chainLength = 0;
        long maxChainLen = 0;

        // Metrics values
        Metrics<int, long> kon_metrics = new Metrics<int, long>();
        Metrics<int, long> loc_metrics = new Metrics<int, long>();
        Metrics<int, long> cyc_metrics = new Metrics<int, long>();
        Metrics<double, double> hal_metrics = new Metrics<double, double>();
        Metrics<double, double> midx_metrics = new Metrics<double, double>();
        Metrics<int, long> fanin_metrics = new Metrics<int, long>();
        Metrics<int, long> fanout_metrics = new Metrics<int, long>();

        // SCC Metric Values
        Dictionary<int, Dictionary<int, List<ForwardMetrics<int, long>>>> sccForward;
        Dictionary<int, Dictionary<int, List<BackwardMetrics<int, long>>>> sccBackward;

        static Dictionary<string, JsonMethod> methods = new Dictionary<string, JsonMethod>();
        static List<JsonMethod> sccList = new List<JsonMethod>();

        // Gabow's Algorithm
        bool visited = false;
        int pre;
        static int prev = 0;
        static Stack<JsonMethod> p = new Stack<JsonMethod>();
        static Stack<JsonMethod> r = new Stack<JsonMethod>();

        int loc;
        int kon;
        int cyc;
        double midx;
        IHalsteadMetrics hal;

        bool dfsFlag = false;

        public JsonMethod(int id, string name)
        {
            this.id = id;
            this.name = name;
            this.fullname = name;
            this.oclass = null;
            this.onamespace = null;
            this.Loc = 1;
            this.Kon = 1;
            this.Cyc = 1;
            this.Midx = 1;
        }

        public JsonMethod(int id, string name, JsonClass clase, JsonNamespace @namespace, int loc, int kon, int cyc, IHalsteadMetrics h, double mi)
        {
            this.id = id;
            this.name = name;
            this.fullname = clase.Fullname + "." + name;
            this.oclass = clase;
            this.onamespace = @namespace;
            this.Loc = loc;
            this.Kon = kon;
            this.Cyc = cyc;
            this.Hal = h;
            this.Midx = mi;
        }

        public static JsonMethod GetMethod(string name, string oclass, string onamespace, bool isInterface, int loc, int kon, int cyc, IHalsteadMetrics h, double mi)
        {
            JsonMethod method;

            if (!methods.TryGetValue(onamespace + "." + oclass + "." + name, out method))
            {
                JsonClass c = JsonClass.GetClass(oclass, onamespace, isInterface);
                method = new JsonMethod(JsonProject.Nextid++, name, c, JsonNamespace.GetNamespace(onamespace), loc, kon, cyc, h, mi);
                methods.Add(onamespace + "." + oclass + "." + name, method);
                c.Methods.Add(method);
                JsonMethod.cantidadMetodos++;
                JsonMethod.totalDeLOC += loc;
            }

            return method;
        }

        public static JsonMethod FindMethod(string name, string oclass, string onamespace)
        {
            JsonMethod method;

            if (!methods.TryGetValue(onamespace + "." + oclass + "." + name, out method))
            {
                method = null;
            }

            return method;
        }

        public static void InitForwardMetricValues(JsonMethod method)
        {
            // kon values
            method.Kon_metrics.Favg = method.kon;
            method.Kon_metrics.Fmax = method.kon;
            method.Kon_metrics.Fmin = method.kon;
            method.Kon_metrics.Fsum = method.kon;
            method.Kon_metrics.Fcnt = 1;

            // loc values
            method.Loc_metrics.Favg = method.loc;
            method.Loc_metrics.Fmax = method.loc;
            method.Loc_metrics.Fmin = method.loc;
            method.Loc_metrics.Fsum = method.loc;
            method.Loc_metrics.Fcnt = 1;

            // cyc values
            method.Cyc_metrics.Favg = method.cyc;
            method.Cyc_metrics.Fmax = method.cyc;
            method.Cyc_metrics.Fmin = method.cyc;
            method.Cyc_metrics.Fsum = method.cyc;
            method.Cyc_metrics.Fcnt = 1;
        }

        public static void InitBackwardMetricValues(JsonMethod method)
        {
            // kon values
            method.Kon_metrics.Bavg = method.kon;
            method.Kon_metrics.Bmax = method.kon;
            method.Kon_metrics.Bmin = method.kon;
            method.Kon_metrics.Bsum = method.kon;
            method.Kon_metrics.Bcnt = 1;

            // loc values
            method.Loc_metrics.Bavg = method.loc;
            method.Loc_metrics.Bmax = method.loc;
            method.Loc_metrics.Bmin = method.loc;
            method.Loc_metrics.Bsum = method.loc;
            method.Loc_metrics.Bcnt = 1;

            // cyc values
            method.Cyc_metrics.Bavg = method.cyc;
            method.Cyc_metrics.Bmax = method.cyc;
            method.Cyc_metrics.Bmin = method.cyc;
            method.Cyc_metrics.Bsum = method.cyc;
            method.Cyc_metrics.Bcnt = 1;
        }

        public static void ResetMetricValues(JsonMethod method)
        {
            method.Kon_metrics = new Metrics<int, long>();
            method.Loc_metrics = new Metrics<int, long>();
            method.Cyc_metrics = new Metrics<int, long>();
        }

        public static void AddBfsForwardMetricValues(JsonMethod m, JsonMethod mc)
        {
            // kon values
            mc.Kon_metrics.Fsum += m.Kon_metrics.Fsum + mc.Kon * m.Kon_metrics.Fcnt;
            mc.Kon_metrics.Fmax = Math.Max(mc.Kon_metrics.Fmax, m.Kon_metrics.Fmax + mc.Kon);
            mc.Kon_metrics.Fmin = Math.Min(mc.Kon_metrics.Fmin, m.Kon_metrics.Fmin + mc.Kon);
            mc.Kon_metrics.Fcnt += m.Kon_metrics.Fcnt;

            // loc values
            mc.Loc_metrics.Fsum += m.Loc_metrics.Fsum + mc.Loc * m.Loc_metrics.Fcnt;
            mc.Loc_metrics.Fmax = Math.Max(mc.Loc_metrics.Fmax, m.Loc_metrics.Fmax + mc.Loc);
            mc.Loc_metrics.Fmin = Math.Min(mc.Loc_metrics.Fmin, m.Loc_metrics.Fmin + mc.Loc);
            mc.Loc_metrics.Fcnt += m.Loc_metrics.Fcnt;

            // cyc values
            mc.Cyc_metrics.Fsum += m.Cyc_metrics.Fsum + mc.Cyc * m.Cyc_metrics.Fcnt;
            mc.Cyc_metrics.Fmax = Math.Max(mc.Cyc_metrics.Fmax, m.Cyc_metrics.Fmax + mc.Cyc);
            mc.Cyc_metrics.Fmin = Math.Min(mc.Cyc_metrics.Fmin, m.Cyc_metrics.Fmin + mc.Cyc);
            mc.Cyc_metrics.Fcnt += m.Cyc_metrics.Fcnt;

            mc.Kon_metrics.Fcntproc++;
        }

        public static void AddBfsBackwardMetricValues(JsonMethod m, JsonMethod mc)
        {
            // kon values
            mc.Kon_metrics.Bsum += m.Kon_metrics.Bsum + mc.Kon * m.Kon_metrics.Bcnt;
            mc.Kon_metrics.Bmax = Math.Max(mc.Kon_metrics.Bmax, m.Kon_metrics.Bmax + mc.Kon);
            mc.Kon_metrics.Bmin = Math.Min(mc.Kon_metrics.Bmin, m.Kon_metrics.Bmin + mc.Kon);
            mc.Kon_metrics.Bcnt += m.Kon_metrics.Bcnt;

            // loc values
            mc.Loc_metrics.Bsum += m.Loc_metrics.Bsum + mc.Loc * m.Loc_metrics.Bcnt;
            mc.Loc_metrics.Bmax = Math.Max(mc.Loc_metrics.Bmax, m.Loc_metrics.Bmax + mc.Loc);
            mc.Loc_metrics.Bmin = Math.Min(mc.Loc_metrics.Bmin, m.Loc_metrics.Bmin + mc.Loc);
            mc.Loc_metrics.Bcnt += m.Loc_metrics.Bcnt;

            // cyc values
            mc.Cyc_metrics.Bsum += m.Cyc_metrics.Bsum + mc.Cyc * m.Cyc_metrics.Bcnt;
            mc.Cyc_metrics.Bmax = Math.Max(mc.Cyc_metrics.Bmax, m.Cyc_metrics.Bmax + mc.Cyc);
            mc.Cyc_metrics.Bmin = Math.Min(mc.Cyc_metrics.Bmin, m.Cyc_metrics.Bmin + mc.Cyc);
            mc.Cyc_metrics.Bcnt += m.Cyc_metrics.Bcnt;

            mc.Kon_metrics.Bcntproc++;
        }

        public static void AddDfsForwardMetricValues(JsonMethod m, JsonMethod mc)
        {
            // kon values
            mc.Kon_metrics.Fsum = m.Kon_metrics.Fsum + mc.Kon;
            if (mc.isSccOut)
            {
                mc.Kon_metrics.Fmax = Math.Max(mc.Kon_metrics.Fmax, (int)mc.Kon_metrics.Fsum);
                mc.Kon_metrics.Fmin = Math.Min(mc.Kon_metrics.Fmin, (int)mc.Kon_metrics.Fsum);
                mc.Kon_metrics.Favg += mc.Kon_metrics.Fsum;
                mc.Kon_metrics.Fcnt++;
            }

            // loc values
            mc.Loc_metrics.Fsum = m.Loc_metrics.Fsum + mc.Loc;
            if (mc.isSccOut)
            {
                mc.Loc_metrics.Fmax = Math.Max(mc.Loc_metrics.Fmax, (int)mc.Loc_metrics.Fsum);
                mc.Loc_metrics.Fmin = Math.Min(mc.Loc_metrics.Fmin, (int)mc.Loc_metrics.Fsum);
                mc.Loc_metrics.Favg += mc.Loc_metrics.Fsum;
                mc.Loc_metrics.Fcnt++;
            }

            // cyc values
            mc.Cyc_metrics.Fsum = m.Cyc_metrics.Fsum + mc.Cyc;
            if (mc.isSccOut)
            {
                mc.Cyc_metrics.Fmax = Math.Max(mc.Cyc_metrics.Fmax, (int)mc.Cyc_metrics.Fsum);
                mc.Cyc_metrics.Fmin = Math.Min(mc.Cyc_metrics.Fmin, (int)mc.Cyc_metrics.Fsum);
                mc.Cyc_metrics.Favg += mc.Cyc_metrics.Fsum;
                mc.Cyc_metrics.Fcnt++;
            }

            mc.Kon_metrics.Fcntproc++;
        }

        public static void AddDfsBackwardMetricValues(JsonMethod m, JsonMethod mc)
        {
            // kon values
            mc.Kon_metrics.Bsum = m.Kon_metrics.Bsum + mc.Kon;
            if (mc.isSccIn)
            {
                mc.Kon_metrics.Bmax = Math.Max(mc.Kon_metrics.Bmax, (int)mc.Kon_metrics.Bsum);
                mc.Kon_metrics.Bmin = Math.Min(mc.Kon_metrics.Bmin, (int)mc.Kon_metrics.Bsum);
                mc.Kon_metrics.Bavg += mc.Kon_metrics.Bsum;
                mc.Kon_metrics.Bcnt++;
            }

            // loc values
            mc.Loc_metrics.Bsum = m.Loc_metrics.Bsum + mc.Loc;
            if (mc.isSccIn)
            {
                mc.Loc_metrics.Bmax = Math.Max(mc.Loc_metrics.Bmax, (int)mc.Loc_metrics.Bsum);
                mc.Loc_metrics.Bmin = Math.Min(mc.Loc_metrics.Bmin, (int)mc.Loc_metrics.Bsum);
                mc.Loc_metrics.Bavg += mc.Loc_metrics.Bsum;
                mc.Loc_metrics.Bcnt++;
            }

            // cyc values
            mc.Cyc_metrics.Bsum = m.Cyc_metrics.Bsum + mc.Cyc;
            if (mc.isSccIn)
            {
                mc.Cyc_metrics.Bmax = Math.Max(mc.Cyc_metrics.Bmax, (int)mc.Cyc_metrics.Bsum);
                mc.Cyc_metrics.Bmin = Math.Min(mc.Cyc_metrics.Bmin, (int)mc.Cyc_metrics.Bsum);
                mc.Cyc_metrics.Bavg += mc.Cyc_metrics.Bsum;
                mc.Cyc_metrics.Bcnt++;
            }

            mc.Kon_metrics.Bcntproc++;
        }

        public static void AvgBfsForwardMetricValues(JsonMethod m)
        {
            // kon values
            m.Kon_metrics.Favg = m.Kon_metrics.Fsum / m.Kon_metrics.Fcnt;
            m.Loc_metrics.Favg = m.Loc_metrics.Fsum / m.Loc_metrics.Fcnt;
            m.Cyc_metrics.Favg = m.Cyc_metrics.Fsum / m.Cyc_metrics.Fcnt;
        }

        public static void AvgBfsBackwardMetricValues(JsonMethod m)
        {
            // kon values
            m.Kon_metrics.Bavg = m.Kon_metrics.Bsum / m.Kon_metrics.Bcnt;
            m.Loc_metrics.Bavg = m.Loc_metrics.Bsum / m.Loc_metrics.Bcnt;
            m.Cyc_metrics.Bavg = m.Cyc_metrics.Bsum / m.Cyc_metrics.Bcnt;
        }

        public static void AvgDfsForwardMetricValues(JsonMethod m)
        {
            // kon values
            m.Kon_metrics.Favg /= m.Kon_metrics.Fcnt;
            m.Loc_metrics.Favg /= m.Loc_metrics.Fcnt;
            m.Cyc_metrics.Favg /= m.Cyc_metrics.Fcnt;
        }

        public static void AvgDfsBackwardMetricValues(JsonMethod m)
        {
            // kon values
            m.Kon_metrics.Bavg /= m.Kon_metrics.Bcnt;
            m.Loc_metrics.Bavg /= m.Loc_metrics.Bcnt;
            m.Cyc_metrics.Bavg /= m.Cyc_metrics.Bcnt;
        }

        public static void AccumSccMetricValues(JsonMethod scc, JsonMethod m)
        {
            // kon values
            scc.Kon_metrics.Bsum += m.Kon;
            scc.Kon_metrics.Bmax += m.Kon;
            scc.Kon_metrics.Bmin += m.Kon;
            scc.Kon_metrics.Bavg += m.Kon;
            scc.Kon_metrics.Bcnt++;

            // loc values
            scc.Loc_metrics.Bsum += m.Loc;
            scc.Loc_metrics.Bmax += m.Loc;
            scc.Loc_metrics.Bmin += m.Loc;
            scc.Loc_metrics.Bavg += m.Loc;
            scc.Loc_metrics.Bcnt++;

            // cyc values
            scc.Cyc_metrics.Bsum += m.Cyc;
            scc.Cyc_metrics.Bmax += m.Cyc;
            scc.Cyc_metrics.Bmin += m.Cyc;
            scc.Cyc_metrics.Bavg += m.Cyc;
            scc.Cyc_metrics.Bcnt++;
        }

        public static void CollectMetricsInParallel()
        {
            pairMetrics = new List<Tuple<int, int, PairMetrics>>();

            //int method_cnt = JsonMethod.SccList.Count;
            //foreach (KeyValuePair<string, JsonMethod> kv in JsonMethod.Methods) { if (kv.Value.IsCollapsed == false) method_cnt++; }

            List<JsonSubchain> list = new List<JsonSubchain>();

            //Console.WriteLine("    Collecting method list[" + method_cnt + "]...");
            Console.WriteLine("    Collecting method list...");
            foreach (KeyValuePair<string, JsonMethod> kv in JsonMethod.Methods)
            {
                JsonMethod m = kv.Value;
                if (m.IsCollapsed == false)
                {
                    list.Add(new JsonSubchain(m));
                }
            }
            foreach (JsonMethod m in JsonMethod.SccList)
            {
                list.Add(new JsonSubchain(m));
            }

            List<JsonSubchain> next_list = list;

            int minRecPerThread = 100000;
            int nthreads;

            int maxThreads = 0;
            int actualThreads = 0;

            Console.WriteLine("    Building all subchains ...");
            Console.Write("          ");
            while (next_list.Count > 0)
            {
                maxThreads = 0;
                actualThreads = 0;
                nthreads = Math.Min(50, (next_list.Count + minRecPerThread - 1) / minRecPerThread);

                //Console.WriteLine("        ...launching " + nthreads + " threads maximum...");

                List<JsonSubchain> new_list = new List<JsonSubchain>();
                List<Tuple<int, int>> chunks = SplitRecords(next_list.Count, nthreads);

                Parallel.ForEach(
                    chunks,
                    new ParallelOptions { MaxDegreeOfParallelism = nthreads },
                    chunk => {
                        var max = Interlocked.Increment(ref actualThreads);

                        if (maxThreads < max)
                        {
                            maxThreads = max;
                        }

                        Console.Write(".");

                        List<JsonSubchain> l = new List<JsonSubchain>();
                        for (var index = chunk.Item1; index <= chunk.Item2; index++)
                        {
                            List<JsonSubchain> result = CollectAllSubchains(next_list[index]);
                            l.AddRange(result);
                        }
                        AppendSubchains(l, new_list);

                        Interlocked.Decrement(ref actualThreads);
                    });
                //Console.WriteLine();
                //Console.WriteLine("           ->Max Threads=" + maxThreads + " new_list.Count=" + new_list.Count);

                list.AddRange(new_list);
                next_list = new_list;
            }
            Console.WriteLine();

            Console.WriteLine("    Copying list of all chains ...");
            JsonSubchain[] records = list.ToArray();
            list = null;
            next_list = null;

            Console.WriteLine("    Sorting for Riskiness ...");
            // Collect metrics for Rigidity (Fan-In)
            Array.Sort(records, JsonSubchain.SortToFromInitialAscending());

            nthreads = Math.Min(50, (records.Length + minRecPerThread - 1) / minRecPerThread);

            Console.WriteLine("    Splitting records ...");
            // Split records in nthreads slices to collect Rigidity (Fan-In)
            List<Tuple<int, int>> slices = SplitRecordsByTo(records, nthreads);

            maxThreads = 0;
            actualThreads = 0;

            Console.WriteLine("    Collecting Riskiness... slice size=" + slices[0].Item2);
            Console.Write("          ");
            Parallel.ForEach(
                slices,
                new ParallelOptions { MaxDegreeOfParallelism = slices.Count },
                s => {
                    var max = Interlocked.Increment(ref actualThreads);

                    if (maxThreads < max)
                    {
                        maxThreads = max;
                    }

                    Console.Write(".");

                    CollectRiskiness(records, s.Item1, s.Item2);

                    Interlocked.Decrement(ref actualThreads);
                });
            Console.WriteLine();
            Console.WriteLine("    Max Threads=" + maxThreads);

            // Collect metrics for Coupling Strength (All-Pairs)
            maxMethods = (int)JsonProject.Nextid;


            Console.WriteLine("    Splitting records ...");
            // Split records in nthreads slices to collect Coupling Strength (All-Pairs)
            slices = SplitRecordsByToFrom(records, nthreads);

            maxThreads = 0;
            actualThreads = 0;

            Console.WriteLine("    Collecting Coupling Strength... slice size=" + slices[0].Item2);
            Console.Write("          ");
            Parallel.ForEach(
                slices,
                new ParallelOptions { MaxDegreeOfParallelism = slices.Count },
                s => {
                    var max = Interlocked.Increment(ref actualThreads);

                    if (maxThreads < max)
                    {
                        maxThreads = max;
                    }

                    Console.Write(".");

                    List<Tuple<int, int, PairMetrics>> pairMetricsList = CollectCouplingStrength(records, s.Item1, s.Item2);
                    AppendPairMetrics(pairMetricsList, pairMetrics);

                    Interlocked.Decrement(ref actualThreads);
                });
            Console.WriteLine();
            Console.WriteLine("    Max Threads=" + maxThreads);

            Console.WriteLine("    Sorting for Fragility ...");
            // Collect metrics for Fragility (Fan-Out)
            Array.Sort(records, JsonSubchain.SortFromToFinalAscending());

            nthreads = Math.Min(50, (records.Length + minRecPerThread - 1) / minRecPerThread);

            Console.WriteLine("    Splitting records ...");
            // Split records in nthreads slices to collect Fragility (Fan-Out)
            slices = SplitRecordsByFrom(records, nthreads);

            maxThreads = 0;
            actualThreads = 0;

            Console.WriteLine("    Collecting Fragility... slice size=" + slices[0].Item2);
            Console.Write("          ");
            Parallel.ForEach(
                slices,
                new ParallelOptions { MaxDegreeOfParallelism = slices.Count },
                s => {
                    var max = Interlocked.Increment(ref actualThreads);

                    if (maxThreads < max)
                    {
                        maxThreads = max;
                    }

                    Console.Write(".");

                    CollectFragility(records, s.Item1, s.Item2);

                    Interlocked.Decrement(ref actualThreads);
                });
            Console.WriteLine();
            Console.WriteLine("    Max Threads=" + maxThreads);
        }

        private static void CollectRiskiness(JsonSubchain[] records, int i, int j)
        {
            int idx = i;

            JsonSubchain s = records[idx];
            int chlen = s.Chain.Length;
            int to = s.To;
            JsonMethod m = s.Chain[chlen - 1];
            HashSet<int> h = new HashSet<int>();
            Program.HalsteadMetrics hal_sum = new Program.HalsteadMetrics(0, 0, 0, 0);
            Program.HalsteadMetrics hal_net = new Program.HalsteadMetrics(0, 0, 0, 0);

            bool enteredIf = false;
            
            while (idx <= j)
            {
                s = records[idx];
                chlen = s.Chain.Length;

                if (s.To != to)
                {
                    AvgForwardMetrics(m, ref hal_sum, ref hal_net);

                    m.WasProcessed = enteredIf;
                    enteredIf = false;

                    to = s.To;
                    m = s.Chain[chlen - 1];
                    h = new HashSet<int>();
                    hal_sum = new Program.HalsteadMetrics(0, 0, 0, 0);
                    hal_net = new Program.HalsteadMetrics(0, 0, 0, 0);
                }

                if (s.Initial == false && s.Chain[0].CalledBy.Count == 0)
                {
                    Console.WriteLine("Chain from method " + s.Chain[0].Fullname + " to method " + s.LastMethod.Fullname + " should be Initial");
                }

                if (s.Initial)
                {
                    enteredIf = true;

                    AddForwardMetrics(m, s.Chain, ref hal_sum, ref hal_net, h);
                }

                idx++;
            }

            AvgForwardMetrics(m, ref hal_sum, ref hal_net);

            m.WasProcessed = enteredIf;
        }

        private static void AddForwardMetrics(JsonMethod m, JsonMethod[] ch, ref Program.HalsteadMetrics halSum, ref Program.HalsteadMetrics halNet, HashSet<int> h)
        {
            int kon_sum = 0;
            int kon_net = 0;
            int loc_sum = 0;
            int loc_net = 0;
            int cyc_sum = 0;
            int cyc_net = 0;
            Program.HalsteadMetrics hal_sum = new Program.HalsteadMetrics(0, 0, 0, 0);
            Program.HalsteadMetrics hal_net = new Program.HalsteadMetrics(0, 0, 0, 0);
            int fanin_sum = 0;
            int fanin_net = 0;
            int fanout_sum = 0;
            int fanout_net = 0;

            foreach (JsonMethod n in ch)
            {
                kon_sum += n.Kon;
                loc_sum += n.Loc;
                cyc_sum += n.Cyc;
                hal_sum = hal_sum.Merge(n.Hal) as Program.HalsteadMetrics;
                fanin_sum += n.CalledBy.Count;
                fanout_sum += n.Calls.Count;
                if (h.Contains(n.Id) == false)
                {
                    h.Add(n.Id);
                    kon_net += n.Kon;
                    loc_net += n.Loc;
                    cyc_net += n.Cyc;
                    hal_net = hal_net.Merge(n.Hal) as Program.HalsteadMetrics;
                    fanin_net += n.CalledBy.Count;
                    fanout_net += n.Calls.Count;
                }
            }
            m.Kon_metrics.AddForwardMetrics(kon_sum, kon_net);
            m.Loc_metrics.AddForwardMetrics(loc_sum, loc_net);
            m.Cyc_metrics.AddForwardMetrics(cyc_sum, cyc_net);
            halSum = halSum.Merge(hal_sum) as Program.HalsteadMetrics;
            halNet = halNet.Merge(hal_net) as Program.HalsteadMetrics;
            m.Hal_metrics.AddForwardMetrics(hal_sum.GetVolume(), hal_net.GetVolume());
            m.Fanin_metrics.AddForwardMetrics(fanin_sum, fanin_net);
            m.Fanout_metrics.AddForwardMetrics(fanout_sum, fanout_net);
        }

        private static void AvgForwardMetrics(JsonMethod m, ref Program.HalsteadMetrics halSum, ref Program.HalsteadMetrics halNet)
        {
            if (m.Kon_metrics.Fcnt > 0) m.Kon_metrics.Favg /= m.Kon_metrics.Fcnt;
            if (m.Loc_metrics.Fcnt > 0) m.Loc_metrics.Favg /= m.Loc_metrics.Fcnt;
            if (m.Cyc_metrics.Fcnt > 0) m.Cyc_metrics.Favg /= m.Cyc_metrics.Fcnt;
            m.Hal_metrics.Fsum = halSum.GetVolume();
            m.Hal_metrics.Fnet = halNet.GetVolume();
            if (m.Hal_metrics.Fcnt > 0) m.Hal_metrics.Favg /= m.Hal_metrics.Fcnt;
            m.Midx_metrics.Fmax = Program.CalculateMaintainablityIndex(m.Cyc_metrics.Fmax, m.Loc_metrics.Fmax, m.Hal_metrics.Fmax);
            m.Midx_metrics.Fmin = Program.CalculateMaintainablityIndex(m.Cyc_metrics.Fmin, m.Loc_metrics.Fmin, m.Hal_metrics.Fmin);
            m.Midx_metrics.Favg = Program.CalculateMaintainablityIndex(m.Cyc_metrics.Favg, m.Loc_metrics.Favg, m.Hal_metrics.Favg);
            m.Midx_metrics.Fsum = Program.CalculateMaintainablityIndex(m.Cyc_metrics.Fsum, m.Loc_metrics.Fsum, m.Hal_metrics.Fsum);
            m.Midx_metrics.Fnet = Program.CalculateMaintainablityIndex(m.Cyc_metrics.Fnet, m.Loc_metrics.Fnet, m.Hal_metrics.Fnet);
            m.Midx_metrics.Fcnt = m.Hal_metrics.Fcnt;
            if (m.Fanin_metrics.Fcnt > 0) m.Fanin_metrics.Favg /= m.Fanin_metrics.Fcnt;
            if (m.Fanout_metrics.Fcnt > 0) m.Fanout_metrics.Favg /= m.Fanout_metrics.Fcnt;
        }

        private static List<Tuple<int, int, PairMetrics>> CollectCouplingStrength(JsonSubchain[] records, int i, int j)
        {
            int idx = i;
            List<Tuple<int, int, PairMetrics>> metric_list = new List<Tuple<int, int, PairMetrics>>();

            JsonSubchain s = records[idx];
            int from = s.From;
            int to = s.To;
            HashSet<int> h = new HashSet<int>();
            PairMetrics metrics = new PairMetrics();

            while (idx <= j)
            {
                s = records[idx];

                if (s.From != from || s.To != to)
                {
                    if (from != to)
                    {
                        AvgPairMetrics(metrics);
                        metric_list.Add(new Tuple<int, int, PairMetrics>(from, to, metrics));
                    }

                    from = s.From;
                    to = s.To;
                    h = new HashSet<int>();
                    metrics = new PairMetrics();
                }

                if (from != to)
                {
                    foreach (JsonMethod n in s.Chain)
                    {
                        AddPairMetricsNet(metrics, n, (h.Contains(n.Id) == false));
                        if (h.Contains(n.Id) == false) h.Add(n.Id);
                    }

                    AcumPairMetrics(metrics);
                }

                idx++;
            }

            if (from != to)
            {
                AvgPairMetrics(metrics);
                metric_list.Add(new Tuple<int, int, PairMetrics>(from, to, metrics));
            }

            return metric_list;
        }

        // Sync
        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        static void AppendPairMetrics(List<Tuple<int, int, PairMetrics>> partial, List<Tuple<int, int, PairMetrics>> complete)
        {
            complete.AddRange(partial);
        }

        // Sync
        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        static void AppendSubchains(List<JsonSubchain> list, List<JsonSubchain> subchains)
        {
            subchains.AddRange(list);
        }

        private static void CollectFragility(JsonSubchain[] records, int i, int j)
        {
            int idx = i;

            JsonSubchain s = records[idx];
            int chlen = s.Chain.Length;
            int from = s.From;
            JsonMethod m = s.Chain[0];
            HashSet<int> h = new HashSet<int>();
            Program.HalsteadMetrics hal_sum = new Program.HalsteadMetrics(0, 0, 0, 0);
            Program.HalsteadMetrics hal_net = new Program.HalsteadMetrics(0, 0, 0, 0);

            while (idx <= j)
            {
                s = records[idx];
                chlen = s.Chain.Length;

                if (s.From != from)
                {
                    AvgBackwardMetrics(m, ref hal_sum, ref hal_net);

                    from = s.From;
                    m = s.Chain[0];
                    h = new HashSet<int>();
                    hal_sum = new Program.HalsteadMetrics(0, 0, 0, 0);
                    hal_net = new Program.HalsteadMetrics(0, 0, 0, 0);
                }

                if (s.Final)
                {
                    AddBackwardMetrics(m, s.Chain, ref hal_sum, ref hal_net, h);
                }

                idx++;
            }

            AvgBackwardMetrics(m, ref hal_sum, ref hal_net);
        }

        private static void AddBackwardMetrics(JsonMethod m, JsonMethod[] ch, ref Program.HalsteadMetrics halSum, ref Program.HalsteadMetrics halNet, HashSet<int> h)
        {
            int kon_sum = 0;
            int kon_net = 0;
            int loc_sum = 0;
            int loc_net = 0;
            int cyc_sum = 0;
            int cyc_net = 0;
            Program.HalsteadMetrics hal_sum = new Program.HalsteadMetrics(0, 0, 0, 0);
            Program.HalsteadMetrics hal_net = new Program.HalsteadMetrics(0, 0, 0, 0);
            int fanin_sum = 0;
            int fanin_net = 0;
            int fanout_sum = 0;
            int fanout_net = 0;

            foreach (JsonMethod n in ch)
            {
                kon_sum += n.Kon;
                loc_sum += n.Loc;
                cyc_sum += n.Cyc;
                hal_sum = hal_sum.Merge(n.Hal) as Program.HalsteadMetrics;
                fanin_sum += n.CalledBy.Count;
                fanout_sum += n.Calls.Count;
                if (h.Contains(n.Id) == false)
                {
                    h.Add(n.Id);
                    kon_net += n.Kon;
                    loc_net += n.Loc;
                    cyc_net += n.Cyc;
                    hal_net = hal_net.Merge(n.Hal) as Program.HalsteadMetrics;
                    fanin_net += n.CalledBy.Count;
                    fanout_net += n.Calls.Count;
                }
            }
            m.Kon_metrics.AddBackwardMetrics(kon_sum, kon_net);
            m.Loc_metrics.AddBackwardMetrics(loc_sum, loc_net);
            m.Cyc_metrics.AddBackwardMetrics(cyc_sum, cyc_net);
            halSum = halSum.Merge(hal_sum) as Program.HalsteadMetrics;
            halNet = halNet.Merge(hal_net) as Program.HalsteadMetrics;
            m.Hal_metrics.AddBackwardMetrics(hal_sum.GetVolume(), hal_net.GetVolume());
            m.Fanin_metrics.AddBackwardMetrics(fanin_sum, fanin_net);
            m.Fanout_metrics.AddBackwardMetrics(fanout_sum, fanout_net);
        }

        private static void AvgBackwardMetrics(JsonMethod m, ref Program.HalsteadMetrics halSum, ref Program.HalsteadMetrics halNet)
        {
            if (m.Kon_metrics.Bcnt > 0) m.Kon_metrics.Bavg /= m.Kon_metrics.Bcnt;
            if (m.Loc_metrics.Bcnt > 0) m.Loc_metrics.Bavg /= m.Loc_metrics.Bcnt;
            if (m.Cyc_metrics.Bcnt > 0) m.Cyc_metrics.Bavg /= m.Cyc_metrics.Bcnt;
            m.Hal_metrics.Bsum = halSum.GetVolume();
            m.Hal_metrics.Bnet = halNet.GetVolume();
            if (m.Hal_metrics.Bcnt > 0) m.Hal_metrics.Bavg /= m.Hal_metrics.Bcnt;
            m.Midx_metrics.Bmax = Program.CalculateMaintainablityIndex(m.Cyc_metrics.Bmax, m.Loc_metrics.Bmax, m.Hal_metrics.Bmax);
            m.Midx_metrics.Bmin = Program.CalculateMaintainablityIndex(m.Cyc_metrics.Bmin, m.Loc_metrics.Bmin, m.Hal_metrics.Bmin);
            m.Midx_metrics.Bavg = Program.CalculateMaintainablityIndex(m.Cyc_metrics.Bavg, m.Loc_metrics.Bavg, m.Hal_metrics.Bavg);
            m.Midx_metrics.Bsum = Program.CalculateMaintainablityIndex(m.Cyc_metrics.Bsum, m.Loc_metrics.Bsum, m.Hal_metrics.Bsum);
            m.Midx_metrics.Bnet = Program.CalculateMaintainablityIndex(m.Cyc_metrics.Bnet, m.Loc_metrics.Bnet, m.Hal_metrics.Bnet);
            m.Midx_metrics.Bcnt = m.Hal_metrics.Bcnt;
            if (m.Fanin_metrics.Bcnt > 0) m.Fanin_metrics.Bavg /= m.Fanin_metrics.Bcnt;
            if (m.Fanout_metrics.Bcnt > 0) m.Fanout_metrics.Bavg /= m.Fanout_metrics.Bcnt;
        }

        private static List<Tuple<int, int>> SplitRecords(int len, int maxslices)
        {
            List<Tuple<int, int>> indexes = new List<Tuple<int, int>>();
            int offset = (len + maxslices - 1) / maxslices;
            int first = 0;
            int last = Math.Min(first + offset - 1, len - 1);

            while (first < len)
            {
                indexes.Add(new Tuple<int, int>(first, last));
                first = last + 1;
                last = Math.Min(first + offset - 1, len - 1);
            }

            return indexes;
        }

        private static List<Tuple<int, int>> SplitRecordsByTo(JsonSubchain[] records, int maxslices)
        {
            int len = records.Length;
            List<Tuple<int, int>> indexes = new List<Tuple<int, int>>();
            int offset = (len + maxslices -1) / maxslices;
            int first = 0;
            int last = Math.Min(first + offset - 1, len - 1);

            while (first < len)
            {
                while (last < (len - 1) && records[last].To == records[last + 1].To) last++;
                indexes.Add(new Tuple<int, int>(first, last));
                first = last + 1;
                last = Math.Min(first + offset - 1, len - 1);
            }

            return indexes;
        }

        private static List<Tuple<int, int>> SplitRecordsByFrom(JsonSubchain[] records, int maxslices)
        {
            int len = records.Length;
            List<Tuple<int, int>> indexes = new List<Tuple<int, int>>();
            int offset = (len + maxslices - 1) / maxslices;
            int first = 0;
            int last = Math.Min(first + offset - 1, len - 1);

            while (first < len)
            {
                while (last < (len - 1) && records[last].From == records[last + 1].From) last++;
                indexes.Add(new Tuple<int, int>(first, last));
                first = last + 1;
                last = Math.Min(first + offset - 1, len - 1);
            }

            return indexes;
        }

        private static List<Tuple<int, int>> SplitRecordsByToFrom(JsonSubchain[] records, int maxslices)
        {
            int len = records.Length;
            List<Tuple<int, int>> indexes = new List<Tuple<int, int>>();
            int offset = (len + maxslices - 1) / maxslices;
            int first = 0;
            int last = Math.Min(first + offset - 1, len - 1);

            while (first < len)
            {
                while (last < (len - 1) && 
                    records[last].To == records[last + 1].To &&
                    records[last].From == records[last + 1].From) last++;
                indexes.Add(new Tuple<int, int>(first, last));
                first = last + 1;
                last = Math.Min(first + offset - 1, len - 1);
            }

            return indexes;
        }

        private static JsonSubchain[] MergeLists(List<JsonSubchain[]> list)
        {
            int listSize = 0;

            foreach (JsonSubchain[] s in list) listSize += s.Length;

            JsonSubchain[] records = new JsonSubchain[listSize];
            int pos = 0;

            foreach (JsonSubchain[] s in list)
            {
                s.CopyTo(records, pos);
                pos += s.Length;
            }

            return records;
        }

        private static List<JsonSubchain> CollectAllSubchains(JsonSubchain s)
        {
            List<JsonSubchain> list = new List<JsonSubchain>();
            foreach (JsonCall c in s.LastMethod.Calls)
            {
                JsonSubchain sch = new JsonSubchain(s.From, c.Method, s.Chain);
                list.Add(sch);
            }

            return list;
        }

        public static void CollectMetricsUsingDfs()
        {
            maxMethods = (int)JsonProject.Nextid;
            chainsCnt = 0;
            //chainMethodsCnt = new int[maxMethods];
            chainMethodsCnt = new List<int>();
            chainMethods = new SparseMatrix<JsonLink>(maxChains, maxMethods);
            //methodChainsCnt = new List<int>();
            methodChainsCnt = new int[maxMethods];
            methodChains = new SparseMatrix<JsonLink>(maxChains, maxMethods);
            methodChainPairs = new SparseMatrix<int>(maxChains, maxMethods);
            pairMetricsList = new SparseMatrix<PairMetrics>(maxMethods, maxMethods);

            List<JsonMethod> startList = new List<JsonMethod>();
            List<JsonMethod> allList = new List<JsonMethod>();

            Console.WriteLine("     ... collecting starting methods...");

            foreach (KeyValuePair<string, JsonMethod> kv in methods)
            {
                if (kv.Value.IsCollapsed == false)
                {
                    allList.Add(kv.Value);
                    if (kv.Value.CalledBy.Count == 0)
                    {
                        startList.Add(kv.Value);
                        InitForwardMetrics(kv.Value);
                    }
                    if (kv.Value.Calls.Count == 0)
                    {
                        InitBackwardMetrics(kv.Value);
                    }
                }
            }

            Console.WriteLine("     ... collecting starting SCCs...");
            foreach (JsonMethod m in sccList)
            {
                allList.Add(m);
                if (m.CalledBy.Count == 0)
                {
                    startList.Add(m);
                    InitForwardMetrics(m);
                }
                if (m.Calls.Count == 0)
                {
                    InitBackwardMetrics(m);
                }
            }

            JsonMethod.numMethods = allList.Count;
            JsonMethod.maxCalledBy = 0;
            JsonMethod.maxCallsTo = 0;

            foreach (JsonMethod m in allList)
            {
                if (m.calledBy.Count > JsonMethod.maxCalledBy) JsonMethod.maxCalledBy = m.calledBy.Count;
                if (m.calls.Count > JsonMethod.maxCallsTo) JsonMethod.maxCallsTo = m.calls.Count;
            }

            Console.WriteLine("     ... collecting metrics using DFS thread ...");

            // Watch out - Sync needed!!!
            Parallel.ForEach(startList, m => CollectMetricsUsingDfsThread(m));
            //Parallel.ForEach(startList, new ParallelOptions { MaxDegreeOfParallelism = 32 }, m => CollectMetricsUsingDfsThread(m));

            Console.WriteLine("     ... collecting average and sum metrics ...");

            /**/
            // No Sync needed!!!
            Parallel.ForEach(allList, m => { AvgMetrics(m); SumMetrics(m); });
            //Parallel.ForEach(allList, new ParallelOptions { MaxDegreeOfParallelism = 32 }, m => { AvgMetrics(m); SumMetrics(m); });

            Console.WriteLine("     ... collecting pair list ...");

            List<Tuple<JsonMethod, JsonMethod>> pairList = new List<Tuple<JsonMethod, JsonMethod>>();
            foreach (JsonMethod m1 in allList)
                foreach (JsonMethod m2 in allList)
                    if (m1.Id != m2.Id && m1.IsCollapsed == false && m2.IsCollapsed == false)
                        pairList.Add(new Tuple<JsonMethod, JsonMethod>(m1, m2));

            Console.WriteLine("     ... collecting pair metrics...");

            // Watch out - Sync needed!!!
            Parallel.ForEach(pairList, p => { CollectPairMetrics(p); });
            //Parallel.ForEach(pairList, new ParallelOptions { MaxDegreeOfParallelism = 32 }, p => { CollectPairMetrics(p); });
            /**/
        }

        static void CollectMetricsUsingDfsThread(JsonMethod m)
        {
            List<JsonMethod> list = new List<JsonMethod>();

            CollectChainMetricsUsingDfs(m, list);
        }

        static void CollectChainMetricsUsingDfs(JsonMethod m, List<JsonMethod> list)
        {
            if (m.IsCollapsed == true) m = m.Scc;

            list.Add(m);

            if (m.Calls.Count == 0)
                CollectNewChain(list);
            else
                foreach (JsonCall c in m.Calls)
                    CollectChainMetricsUsingDfs(c.Method, list);

            list.RemoveAt(list.Count - 1);
        }

        static void CollectPairMetrics(Tuple<JsonMethod, JsonMethod> p)
        {
            JsonMethod m1 = p.Item1;
            JsonMethod m2 = p.Item2;
            PairMetrics metrics = new PairMetrics();
            HashSet<JsonMethod> methodSet = new HashSet<JsonMethod>();
            bool hasData = false;

            for (int cidx1 = 0; cidx1 < methodChainsCnt[(int)m1.Id]; cidx1++)
            {
                JsonLink link1 = methodChains[cidx1, m1.Id];
                int chainId = link1.ChainId;
                if (methodChainPairs.IsCellPresent(chainId, m2.Id))
                {
                    int m2idx = methodChainPairs[chainId, m2.Id];
                    if (chainMethods.IsCellPresent(chainId, m2idx))
                    {
                        JsonLink link2 = chainMethods[chainId, m2idx];
                        if (link1.MethodIdx < link2.MethodIdx)
                        {
                            hasData = true;
                            for (int midx = link1.MethodIdx; midx <= link2.MethodIdx; midx++)
                            {
                                JsonMethod m = chainMethods[chainId, midx].Method;
                                methodSet.Add(m);
                                AddPairMetrics(metrics, m);
                            }

                            AcumPairMetrics(metrics);
                        }
                    }
                }
            }

            if (hasData)
            {
                AvgPairMetrics(metrics);

                SetPairMetricsSum(metrics, methodSet);

                CollectNewPair(m1.Id, m2.Id, metrics);
            }
        }

        // Sync
        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        static void CollectNewChain(List<JsonMethod> list)
        {
            int chainId = chainsCnt++;
            JsonChain ch = new JsonChain(chainId);
            int mCnt = ch.CollectChainWeights(list);
            chainMethodsCnt.Add(mCnt);

            int cidx = methodChainsCnt[(int)list[0].Id]++;
            JsonLink link = new JsonLink(list[0], chainId, 0, cidx);
            chainMethods[chainId, 0] = link;
            methodChains[cidx, list[0].Id] = link;

            methodChainPairs[chainId, list[0].Id] = 0;

            for (int midx = 1; midx < mCnt; midx++)
            {
                JsonMethod m = list[midx];
                cidx = methodChainsCnt[(int)m.Id]++;

                AddForwardMetrics(list[midx - 1], list[midx]);
                AddBackwardMetrics(list[mCnt - midx - 1], list[mCnt - midx]);

                link = new JsonLink(m, chainId, midx, cidx);

                chainMethods[chainId, midx] = link;
                methodChains[cidx, m.Id] = link;

                methodChainPairs[chainId, m.Id] = midx;
            }

            JsonMethod.numOfChains++;
            JsonMethod.maxChainLength = Math.Max(JsonMethod.maxChainLength, mCnt);
            JsonMethod.avgChainLength += mCnt;
        }

        // Sync
        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        static void CollectNewPair(int m1Id, int m2Id, PairMetrics p)
        {
            pairMetricsList[m1Id, m2Id] = p;
        }

        static void AddPairMetrics(PairMetrics p, JsonMethod m)
        {
            p.K.Fsum += m.Kon;
            p.L.Fsum += m.Loc;
            p.C.Fsum += m.Cyc;
        }

        static void AddPairMetricsNet(PairMetrics p, JsonMethod m, bool isNet)
        {
            p.K.Facc += m.Kon;
            p.L.Facc += m.Loc;
            p.C.Facc += m.Cyc;

            p.K.Fsum += m.Kon;
            p.L.Fsum += m.Loc;
            p.C.Fsum += m.Cyc;

            if (isNet)
            {
                p.K.Fnet += m.Kon;
                p.L.Fnet += m.Loc;
                p.C.Fnet += m.Cyc;
            }
        }

        static void AcumPairMetrics(PairMetrics p)
        {
            p.K.Favg += p.K.Facc;
            p.K.Fmin = Math.Min(p.K.Fmin, p.K.Facc);
            p.K.Fmax = Math.Max(p.K.Fmax, p.K.Facc);
            p.K.Facc = 0;
            p.K.Fcnt++;

            p.L.Favg += p.L.Facc;
            p.L.Fmin = Math.Min(p.L.Fmin, p.L.Facc);
            p.L.Fmax = Math.Max(p.L.Fmax, p.L.Facc);
            p.L.Facc = 0;
            p.L.Fcnt++;

            p.C.Favg += p.C.Facc;
            p.C.Fmin = Math.Min(p.C.Fmin, p.C.Facc);
            p.C.Fmax = Math.Max(p.C.Fmax, p.C.Facc);
            p.C.Facc = 0;
            p.C.Fcnt++;
        }

        static void AvgPairMetrics(PairMetrics p)
        {
            p.K.Favg /= p.K.Fcnt;
            p.L.Favg /= p.L.Fcnt;
            p.C.Favg /= p.C.Fcnt;
        }

        static void SetPairMetricsSum(PairMetrics p, HashSet<JsonMethod> methodSet)
        {
            p.K.Fsum = 0;
            p.L.Fsum = 0;
            p.C.Fsum = 0;
            foreach (JsonMethod m in methodSet)
            {
                p.K.Fsum += m.Kon;
                p.L.Fsum += m.Loc;
                p.C.Fsum += m.Cyc;
            }
        }

        static void InitForwardMetrics(JsonMethod m)
        {
            m.Kon_metrics.InitForward(m.Kon);
            m.Loc_metrics.InitForward(m.Loc);
            m.Cyc_metrics.InitForward(m.Cyc);
        }

        static void AddForwardMetrics(JsonMethod m, JsonMethod mc)
        {
            mc.Kon_metrics.AddForward(mc.Kon, m.Kon_metrics);
            mc.Loc_metrics.AddForward(mc.Loc, m.Loc_metrics);
            mc.Cyc_metrics.AddForward(mc.Cyc, m.Cyc_metrics);
        }

        static void InitBackwardMetrics(JsonMethod m)
        {
            m.Kon_metrics.InitBackward(m.Kon);
            m.Loc_metrics.InitBackward(m.Loc);
            m.Cyc_metrics.InitBackward(m.Cyc);
        }

        static void AddBackwardMetrics(JsonMethod m, JsonMethod mc)
        {
            m.Kon_metrics.AddBackward(m.Kon, mc.Kon_metrics);
            m.Loc_metrics.AddBackward(m.Loc, mc.Loc_metrics);
            m.Cyc_metrics.AddBackward(m.Cyc, mc.Cyc_metrics);
        }

        static void AvgMetrics(JsonMethod m)
        {
            m.Kon_metrics.AvgMetrics();
            m.Loc_metrics.AvgMetrics();
            m.Cyc_metrics.AvgMetrics();
        }

        static void SetForwardMetricsSum(JsonMethod m, HashSet<JsonMethod> forwardSet)
        {
            m.Kon_metrics.Fsum = m.Kon;
            m.Loc_metrics.Fsum = m.Loc;
            m.Cyc_metrics.Fsum = m.Cyc;
            foreach (JsonMethod fm in forwardSet)
            {
                m.Kon_metrics.Fsum += fm.Kon;
                m.Loc_metrics.Fsum += fm.Loc;
                m.Cyc_metrics.Fsum += fm.Cyc;
            }
        }

        static void SetBackwardMetricsSum(JsonMethod m, HashSet<JsonMethod> backwardSet)
        {
            m.Kon_metrics.Bsum = m.Kon;
            m.Loc_metrics.Bsum = m.Loc;
            m.Cyc_metrics.Bsum = m.Cyc;
            foreach (JsonMethod fm in backwardSet)
            {
                m.Kon_metrics.Bsum += fm.Kon;
                m.Loc_metrics.Bsum += fm.Loc;
                m.Cyc_metrics.Bsum += fm.Cyc;
            }
        }

        static void SumMetrics(JsonMethod m)
        {
            HashSet<JsonMethod> forwardSet = new HashSet<JsonMethod>();
            HashSet<JsonMethod> backwardSet = new HashSet<JsonMethod>();
            int mId = (int)m.Id;
            int chId;

            for (int cidx = 0; cidx < methodChainsCnt[mId]; cidx++)
            {
                JsonLink link = methodChains[cidx, mId];
                chId = (int)link.ChainId;
                for (int midx = 0; midx < chainMethodsCnt[chId]; midx++)
                {
                    if (link.MethodIdx > chainMethods[chId, midx].MethodIdx)
                        forwardSet.Add(chainMethods[chId, midx].Method);
                    if (link.MethodIdx < chainMethods[chId, midx].MethodIdx)
                        backwardSet.Add(chainMethods[chId, midx].Method);
                }
            }

            SetForwardMetricsSum(m, forwardSet);
            SetBackwardMetricsSum(m, backwardSet);
        }

        public static void CountChainsUsingBFS()
        {
            List<JsonMethod> list = new List<JsonMethod>();
            foreach (KeyValuePair<string, JsonMethod> m in methods)
            {
                JsonMethod method = m.Value;

                if (method.IsCollapsed == false)
                {
                    if (method.CalledBy.Count == 0)
                    {
                        method.NumChains = 1;
                        method.ChainLength = 1;
                        method.MaxChainLen = 1;
                        list.Add(method);
                    }

                    Stats.numMetodos++;
                    Stats.numLOC += method.Loc;
                    if (Stats.maxCalls < method.Calls.Count)
                        Stats.maxCallsToName = method.Fullname;
                    Stats.maxCalls = Math.Max(Stats.maxCalls, method.Calls.Count);
                    if (Stats.maxCalledby < method.CalledBy.Count)
                        Stats.maxCalledByName = method.Fullname;
                    Stats.maxCalledby = Math.Max(Stats.maxCalledby, method.CalledBy.Count);
                    Stats.promCalls += method.Calls.Count;
                    Stats.promCalledby += method.CalledBy.Count;
                }
            }
            foreach (JsonMethod m in sccList)
            {
                JsonMethod method = m;

                if (method.CalledBy.Count == 0)
                {
                    method.NumChains = 1;
                    method.ChainLength = 1;
                    method.MaxChainLen = 1;
                    list.Add(method);
                }

                Stats.numMetodos++;
                Stats.numLOC += method.Loc;
                if (Stats.maxCalls < method.Calls.Count)
                    Stats.maxCallsToName = method.Fullname;
                Stats.maxCalls = Math.Max(Stats.maxCalls, method.Calls.Count);
                if (Stats.maxCalledby < method.CalledBy.Count)
                    Stats.maxCalledByName = method.Fullname;
                Stats.maxCalledby = Math.Max(Stats.maxCalledby, method.CalledBy.Count);
                Stats.promCalls += method.Calls.Count;
                Stats.promCalledby += method.CalledBy.Count;
            }

            Stats.numClases = JsonClass.Classes.Count;
            Stats.numMetodosPorClase = Stats.numMetodos / Stats.numClases;
            Stats.promCalls /= Stats.numMetodos;
            Stats.promCalledby /= Stats.numMetodos;

            CountChainsWithdBFS(list);

        }

        static void CountChainsWithdBFS(List<JsonMethod> methods)
        {
            List<JsonMethod> list = methods;
            List<JsonMethod> nextlist;

            while (list.Count > 0)
            {
                nextlist = new List<JsonMethod>();
                foreach (JsonMethod method in list)
                {
                    if (method.Calls.Count == 0)
                    {
                        Stats.numCadenas += method.NumChains;
                        Stats.promLargoCadena += method.ChainLength;
                        Stats.maxLargoCadena = Math.Max(Stats.maxLargoCadena, method.MaxChainLen);
                    }
                    else
                    {
                        foreach (JsonCall c in method.Calls)
                        {
                            c.Method.Jumps++;
                            c.Method.NumChains += method.NumChains;
                            c.Method.ChainLength += method.ChainLength + method.NumChains;
                            c.Method.MaxChainLen = Math.Max(c.Method.MaxChainLen, method.MaxChainLen + 1);
                            if (c.Method.CalledBy.Count == c.Method.Jumps)
                            {
                                nextlist.Add(c.Method);
                            }
                        }
                    }
                }
                list = nextlist;
            }

            Stats.promLargoCadena /= Stats.numCadenas;
        }

        public static void CollectMetricsUsingBFS()
        {
            List<JsonMethod> list = new List<JsonMethod>();

            Console.WriteLine("\n========== Metricas Forward =============\n");

            // Forward pass from methods
            foreach (KeyValuePair<string, JsonMethod> m in methods)
            {
                JsonMethod method = m.Value;

                if (method.IsCollapsed == true)
                {
                    method = method.Scc;
                }
                if (method.CalledBy.Count == 0)
                {
                    InitForwardMetricValues(method);
                    PrintMethodForwardMetrics(method);
                    list.Add(method);
                }
            }

            // Forward pass from scc
            foreach (JsonMethod m in sccList)
            {
                JsonMethod method = m;

                if (method.CalledBy.Count == 0)
                {
                    InitForwardMetricValues(method);
                    PrintMethodForwardMetrics(method);
                    list.Add(method);
                }
            }

            MetricsForwardBFS(list);

            list.Clear();

            Console.WriteLine("\n========== Metricas Backward =============\n");

            // Backward pass from methods
            foreach (KeyValuePair<string, JsonMethod> m in methods)
            {
                JsonMethod method = m.Value;

                if (method.IsCollapsed == true)
                {
                    method = method.Scc;
                }
                if (method.Calls.Count == 0)
                {
                    InitBackwardMetricValues(method);
                    PrintMethodBackwardMetrics(method);
                    list.Add(method);
                }
            }

            // Backward pass from scc
            foreach (JsonMethod m in sccList)
            {
                JsonMethod method = m;

                if (method.Calls.Count == 0)
                {
                    InitBackwardMetricValues(method);
                    PrintMethodBackwardMetrics(method);
                    list.Add(method);
                }
            }

            MetricsBackwardBFS(list);

            Console.WriteLine("\n========================================\n");
        }

        static void MetricsForwardBFS(List<JsonMethod> methods)
        {
            List<JsonMethod> list = methods;
            List<JsonMethod> nextlist;

            while (list.Count > 0)
            {
                nextlist = new List<JsonMethod>();
                foreach (JsonMethod m in list)
                {
                    //List<long> processed = new List<long>();
                    JsonMethod method = m;
                    foreach (JsonCall c in method.Calls)
                    {
                        JsonMethod mc = c.Method;
                        if (mc.IsCollapsed == true)
                        {
                            mc = mc.Scc;
                        }

                        AddBfsForwardMetricValues(m, mc);
                        if (mc.CalledBy.Count == mc.Kon_metrics.Fcntproc)
                        {
                            AvgBfsForwardMetricValues(mc);
                            PrintMethodForwardMetrics(mc);
                            nextlist.Add(mc);
                        }
                    }
                }
                list = nextlist;
            }
        }

        static void MetricsBackwardBFS(List<JsonMethod> methods)
        {
            List<JsonMethod> list = methods;
            List<JsonMethod> nextlist;

            while (list.Count > 0)
            {
                nextlist = new List<JsonMethod>();
                foreach (JsonMethod m in list)
                {
                    //List<long> processed = new List<long>();
                    JsonMethod method = m;
                    foreach (JsonCall c in method.CalledBy)
                    {
                        JsonMethod mc = c.Method;

                        if (mc.IsCollapsed == true)
                        {
                            mc = mc.Scc;
                        }
                        AddBfsBackwardMetricValues(m, mc);
                        if (mc.Calls.Count == mc.Kon_metrics.Bcntproc)
                        {
                            AvgBfsBackwardMetricValues(mc);
                            PrintMethodBackwardMetrics(mc);
                            nextlist.Add(mc);
                        }
                    }
                }
                list = nextlist;
            }
        }

        public static void CollectSccMetricsUsingDFS()
        {
            foreach (JsonMethod scc in JsonMethod.SccList)
            {
                scc.Kon = 0;
                scc.Loc = 0;
                scc.Cyc = 0;
                scc.Hal = new Program.HalsteadMetrics(0, 0, 0, 0);
                foreach (JsonMethod m in scc.SccMethods)
                {
                    scc.Kon += m.Kon;
                    scc.Loc += m.Loc;
                    scc.Cyc += m.Cyc;
                    scc.Hal = scc.Hal.Merge(m.Hal);
                }
                scc.Midx = Program.CalculateMaintainablityIndex(scc.Cyc, scc.Loc, scc.Hal.GetVolume());
            }
        }

        static void MarkSccEntranceAndExit()
        {
            // Mark entrance and exit methods in SCCs
            foreach (JsonMethod scc in SccList)
            {
                foreach (JsonCall cb in scc.CalledBy)
                {
                    foreach (JsonCall c in cb.Method.Calls)
                    {
                        // is entrance method?
                        c.Method.isSccIn = c.Method.IsCollapsed;
                    }
                }
                foreach (JsonCall c in scc.Calls)
                {
                    foreach (JsonCall cb in c.Method.CalledBy)
                    {
                        // is exit method?
                        cb.Method.isSccOut = cb.Method.IsCollapsed;
                    }
                }
            }
        }

        static void SccMetricsForwardDFS(JsonMethod m)
        {
            m.Visited = true;

            foreach (JsonCall c in m.Calls.FindAll(x => x.Method.IsCollapsed == true && x.Method.SccId == m.SccId && x.Method.Visited == false))
            {
                JsonMethod mc = c.Method;
                AddDfsForwardMetricValues(m, mc);
                SccMetricsForwardDFS(mc);
            }

            m.Visited = false;
        }

        static void SccMetricsBackwardDFS(JsonMethod m)
        {
            m.Visited = true;

            foreach (JsonCall c in m.CalledBy.FindAll(x => x.Method.IsCollapsed == true && x.Method.SccId == m.SccId && x.Method.Visited == false))
            {
                JsonMethod mc = c.Method;
                AddDfsBackwardMetricValues(m, mc);
                SccMetricsBackwardDFS(mc);
            }

            m.Visited = false;
        }

        public static void PrintMethodForwardMetrics(JsonMethod m)
        {
            //JsonMethod.count_processed++;
            //return;
            Console.WriteLine(m.Name + " - K=" + m.Kon + ", L=" + m.Loc + ", C=" + m.Cyc + "  (Fcont=" + m.Loc_metrics.Fcnt + ", Fcntproc=" + m.Kon_metrics.Fcntproc + ")");
            Console.WriteLine(
                  " FKavg: " + m.Kon_metrics.Favg
                + " FKmax: " + m.Kon_metrics.Fmax
                + " FKmin: " + m.Kon_metrics.Fmin);
            Console.WriteLine(
                  " FLavg: " + m.Loc_metrics.Favg
                + " FLmax: " + m.Loc_metrics.Fmax
                + " FLmin: " + m.Loc_metrics.Fmin);
            Console.WriteLine(
                  " FCavg: " + m.Cyc_metrics.Favg
                + " FCmax: " + m.Cyc_metrics.Fmax
                + " FCmin: " + m.Cyc_metrics.Fmin);
        }

        public static void PrintMethodBackwardMetrics(JsonMethod m)
        {
            //JsonMethod.count_processed++;
            //return;
            Console.WriteLine(m.Name + " - K=" + m.Kon + ", L=" + m.Loc + ", C=" + m.Cyc + "  (Bcont=" + m.Loc_metrics.Bcnt + ", Bcntproc=" + m.Kon_metrics.Bcntproc + ")");
            Console.WriteLine(
                  " BKavg: " + m.Kon_metrics.Bavg
                + " BKmax: " + m.Kon_metrics.Bmax
                + " BKmin: " + m.Kon_metrics.Bmin);
            Console.WriteLine(
                  " BLavg: " + m.Loc_metrics.Bavg
                + " BLmax: " + m.Loc_metrics.Bmax
                + " BLmin: " + m.Loc_metrics.Bmin);
            Console.WriteLine(
                  " BCavg: " + m.Cyc_metrics.Bavg
                + " BCmax: " + m.Cyc_metrics.Bmax
                + " BCmin: " + m.Cyc_metrics.Bmin);
        }

        public static void CountChainsUsingDFS()
        {
            List<JsonMethod> list = new List<JsonMethod>();

            foreach (KeyValuePair<string, JsonMethod> m in methods)
            {
                if (m.Value.CalledBy.Count == 0 && m.Value.IsCollapsed == false)
                {
                    list.Add(m.Value);
                }
            }

            foreach (JsonMethod s in sccList)
            {
                if (s.CalledBy.Count == 0)
                {
                    list.Add(s);
                }
            }

            // Watch out - Sync needed!!!
            Parallel.ForEach(list, m => CountDFS(m, 1));
            //Parallel.ForEach(list, new ParallelOptions { MaxDegreeOfParallelism = 32 }, m => CountDFS(m, 1));

            if (count > 0)
            {
                avgdepth = avgdepth / count;
                Console.WriteLine("# of Chains: " + count.ToString() + ", AVG Length: " + avgdepth.ToString());
                //Console.Read();
            }
        }

        static void CountDFS(JsonMethod m, int depth)
        {
            if (m.IsCollapsed)
            {
                m = m.Scc;
            }
            //m.DfsFlag = true; // there are no cycles
            if (m.Calls.Count == 0)
            {
                //if (depth > 2)
                //{
                    IncreaseCount(depth);
                //}
            }
            else
            {
                foreach (JsonCall c in m.Calls)
                {
                    //if (c.Method.DfsFlag == false)
                    //{
                        CountDFS(c.Method, depth + 1);
                    //}
                }
            }
            //m.DfsFlag = false;
        }

        // Sync
        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        static void IncreaseCount(int depth)
        {
            avgdepth += depth;
            count++;
        }

        public static void CollectChainsUsingDFS(JsonProject project)
        {
            foreach (KeyValuePair<string, JsonMethod> m in methods)
            {
                if (m.Value.CalledBy.Count == 0)
                {
                    CollectDFS(m.Value, new List<JsonCall>(), project);
                }
            }

            /*
            foreach (List<JsonCall> ch in project.Chains)
            {
                string chain = ">> ";
                foreach (JsonCall c in ch)
                {
                    chain = chain + c.ClassName + "." + c.Name + " > ";
                }

                output.WriteLine(chain);
            }

            output.Flush();
            */
        }

        static void CollectDFS(JsonMethod m, List<JsonCall> list, JsonProject project)
        {
            //if (nprint == 0) return;

            m.DfsFlag = true;
            list.Add(new JsonCall(m.Id, m.Name, m.ClassId, m.ClassName, m.NamespaceId, m.NamespaceName, m));
            if (m.Calls.Count == 0)
            {
                if (list.Count > 2)
                {
                    List<JsonCall> l = new List<JsonCall>(list);
                    project.Chains.Add(l);
                    //nprint--;
                }
            }
            else
            {
                foreach (JsonCall c in m.Calls)
                {
                    if (c.Method.DfsFlag == false)
                    {
                        CollectDFS(c.Method, list, project);
                    }
                }
            }
            list.RemoveAt(list.Count - 1);
            m.DfsFlag = false;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as JsonMethod);
        }

        public bool Equals(JsonMethod other)
        {
            return other != null &&
                   id == other.id;
        }

        public override int GetHashCode()
        {
            return 1877310944 + id.GetHashCode();
        }

        [JsonProperty]
        public int Id { get => id; set => id = value; }
        [JsonProperty]
        public string Name { get => name; set => name = value; }
        [JsonProperty]
        public string Fullname { get => fullname; set => fullname = value; }
        [JsonProperty("ClassId")]
        public int ClassId { get => oclass.Id; set => oclass.Id = value; }
        [JsonProperty("Class")]
        public string ClassName { get => oclass.Name; set => oclass.Name = value; }
        [JsonProperty("FullClassname")]
        public string FullClassname { get => oclass.Fullname; set => oclass.Fullname = value; }
        [JsonProperty("NamespaceId")]
        public int NamespaceId { get => onamespace.Id; set => onamespace.Id = value; }
        [JsonProperty("Namespace")]
        public string NamespaceName { get => onamespace.Name; set => onamespace.Name = value; }
        [JsonProperty("FullNamespace")]
        public string FullNamespaceName { get => onamespace.Fullname; set => onamespace.Fullname = value; }
        [JsonProperty]
        public List<JsonCall> Calls { get => calls; set => calls = value; }
        [JsonProperty]
        internal List<JsonCall> CalledBy { get => calledBy; set => calledBy = value; }
        public JsonClass GetClass { get => oclass; set => oclass = value; }
        public JsonNamespace GetNamespace { get => onamespace; set => onamespace = value; }
        public bool DfsFlag { get => dfsFlag; set => dfsFlag = value; }
        public static Dictionary<string, JsonMethod> Methods { get => methods; set => methods = value; }

        public dynamic JSerialize()
        {
            dynamic m = new JObject();
            m.Name = Name;
            m.Fullname = Fullname;
            m.Class = ClassName;
            m.FullClass = FullClassname;
            m.Namespace = NamespaceName;
            m.FullNamespace = FullNamespaceName;
            m.KON = kon;
            m.LOC = loc;
            m.CYC = cyc;
            m.FSUMKON = Kon_metrics.Bsum;
            m.FSUMLOC = Loc_metrics.Bsum;
            m.FSUMCYC = Cyc_metrics.Bsum;
            m.RSUMKON = Kon_metrics.Fsum;
            m.RSUMLOC = Loc_metrics.Fsum;
            m.RSUMCYC = Cyc_metrics.Fsum;

            return m;
        }

        // For Gabo's Algorithm
        public bool Visited { get => visited; set => visited = value; }
        public int Pre { get => pre; set => pre = value; }
        public static int Prev { get => prev; set => prev = value; }
        internal static Stack<JsonMethod> P { get => p; set => p = value; }
        internal static Stack<JsonMethod> R { get => r; set => r = value; }
        public int SccId { get => sccId; set => sccId = value; }
        public JsonMethod Scc { get => scc; set => scc = value; }
        public bool IsMethod { get => isMethod; set => isMethod = value; }
        public bool IsScc { get => isScc; set => isScc = value; }
        public bool IsCollapsed { get => isCollapsed; set => isCollapsed = value; }
        public List<JsonMethod> SccMethods { get => sccMethods; set => sccMethods = value; }
        public static List<JsonMethod> SccList { get => sccList; set => sccList = value; }

        public int Loc { get => loc; set => loc = value; }
        public int Kon { get => kon; set => kon = value; }
        public int Cyc { get => cyc; set => cyc = value; }
        public bool IsRecursive { get => isRecursive; set => isRecursive = value; }
        public Dictionary<int, Dictionary<int, List<ForwardMetrics<int, long>>>> SccForward { get => sccForward; set => sccForward = value; }
        public Dictionary<int, Dictionary<int, List<BackwardMetrics<int, long>>>> SccBackward { get => sccBackward; set => sccBackward = value; }
        internal static SparseMatrix<PairMetrics> PairMetricsList { get => pairMetricsList; set => pairMetricsList = value; }
        public static int MaxMethods { get => maxMethods; set => maxMethods = value; }
        public List<JsonCall> CallsSCC { get => callsSCC; set => callsSCC = value; }
        public List<JsonCall> CalledBySCC { get => calledBySCC; set => calledBySCC = value; }
        public static SystemStats Stats { get => stats; set => stats = value; }
        public long NumChains { get => numChains; set => numChains = value; }
        public long ChainLength { get => chainLength; set => chainLength = value; }
        public long Jumps { get => jumps; set => jumps = value; }
        public long MaxChainLen { get => maxChainLen; set => maxChainLen = value; }
        internal static List<Tuple<int, int, PairMetrics>> PairMetrics { get => pairMetrics; set => pairMetrics = value; }
        public bool WasProcessed { get => wasProcessed; set => wasProcessed = value; }
        public double Midx { get => midx; set => midx = value; }
        public IHalsteadMetrics Hal { get => hal; set => hal = value; }
        public Metrics<int, long> Kon_metrics { get => kon_metrics; set => kon_metrics = value; }
        public Metrics<int, long> Loc_metrics { get => loc_metrics; set => loc_metrics = value; }
        public Metrics<int, long> Cyc_metrics { get => cyc_metrics; set => cyc_metrics = value; }
        public Metrics<double, double> Hal_metrics { get => hal_metrics; set => hal_metrics = value; }
        public Metrics<double, double> Midx_metrics { get => midx_metrics; set => midx_metrics = value; }
        public Metrics<int, long> Fanin_metrics { get => fanin_metrics; set => fanin_metrics = value; }
        public Metrics<int, long> Fanout_metrics { get => fanout_metrics; set => fanout_metrics = value; }

        static BsonJavaScript map = new BsonJavaScript(@"
            function() {
		            var node_key;
		            var node_value;

		            // retrieve original method
		            for (var idx = 0; idx < this.value.list.length; idx++) {
			            item = this.value.list[idx];
			            if (item.node == true) {
				            node_key = item.id;
				            node_value = item;
				            item.iteration = item.iteration + 1;
				            break;
			            }
		            }

		            var key;
		            var value;

		            // starting point
		            if (node_value.hasOwnProperty('unwind')) { 

			            delete node_value['unwind'];
			            for (var idx = 0; idx < node_value.calls.length; idx++) {
				            key = node_value.calls[idx];
				            value = {
							            id: key,
							            calls: [node_key],
							            weight: node_value.weight,
							            node: false,
							            iteration: node_value.iteration
						             };
				            emit(key, { list: [value] });
			            }
		            }
		            else { // progressive map reduce

			            // accummulate to original method and forward methods
			            for (var idx = 0; idx < this.value.list.length; idx++) {
				            item = this.value.list[idx];
				            if (item.node == false) {
					            caller_key = item.calls[0];
					            if (node_value.summed.includes(caller_key) == false) {
						            node_value.weight += item.weight;
						            node_value.summed.push(caller_key);
						            for (var jdx = 0; jdx < node_value.calls.length; jdx++) {
							            callee_key = node_value.calls[jdx];
							            value = {
										            id: callee_key,
										            calls: [caller_key],
										            weight: item.weight,
										            node: false,
										            iteration: node_value.iteration
									             };
							            emit(callee_key, { list: [value] });
						            }
					            }
				            }
			            }
		            }

		            // preserve original method with calculated metrics
		            emit(node_key, { list: [node_value] });
	            };
        ");

        static BsonJavaScript reduce = new BsonJavaScript(@"
            function(key, values) {
                reduced_value = { list:[] };
                for (var idx = 0; idx < values.length; idx++)
                    for (var jdx = 0; jdx < values[idx].list.length; jdx++)
                        reduced_value.list.push(values[idx].list[jdx]);
                return reduced_value;
            };
        ");

        static BsonJavaScript finalize = new BsonJavaScript(@"
	        function (key, reduced_value) {
               return reduced_value;
            };
        ");


        static BsonJavaScript mapChains = new BsonJavaScript(@"
	        function() {

		        var item = this.value;

		        if (item.type == 'Node') {

			        if (item.forward == true) {
				
				        item.forward = false;

				        for (var idx = 0; idx < item.to.length; idx++) {
					        var to = item.to[idx];
					        emit(to, { type: 'Path',
						               chains: [
						       		        {
						       			        to: to,
						       			        methods: [ { method: item.from, weight: item.weight, forward: item.weight }],
						       			        weight: item.weight,
						       			        count: 1
						       		        }
						               ] });
				        }
			        }

			        emit('N-'+item.from, item); // preserve the node

			        item.type = 'Chain';
			        item.chains = [];
			        emit(item.from, item);      // and create first paths
		        }
		        else if (item.type == 'Chain') { // emit links
			        if (item.forward == true) {
				        var to = item.chains[0].to;
				        var chains = [];

				        for (var idx = 0; idx < item.chains.length; idx++) {
					        chain = item.chains[idx];
					        if (chain.to == to) {
						        chains.push(chain);
					        }
					        else {
						        emit(to, { type: 'Path', chains: chains });
						        to = chain.to;
						        chains = [ chain ];
					        }
				        }
				        emit(to, { type: 'Path', chains: chains });
			        }
			        else {
				        // Just forward chain
				        emit(item.key, item);
			        }
		        }
		        else if (item.type == 'FinalChain') {
			        emit(item.key, item);
		        }
	        }
        ");

        static BsonJavaScript reduceChains = new BsonJavaScript(@"
	        function(key, values) {

		        var node_value;
		        var found = false;

		        // retrieve chain node
		        for (var idx = 0; idx < values.length; idx++) {
			        item = values[idx];

			        if (item.type == 'Chain') {
				        node_value = item;
				        node_value.forward = false;
				        found = true;
				        break;
			        }

			        // if not found, it will on a later partial reduce action !!!
		        }

		        var chains = [];

		        // retrieve chain links
		        if (found) {
			        for (var idx = 0; idx < values.length; idx++) {
				        item = values[idx];

				        if (item.type == 'Path') {
					        for (var jdx = 0; jdx < item.chains.length; jdx++) {
						        chain = item.chains[jdx];
						        forward_value = node_value.weight + chain.methods[chain.methods.length-1].forward;
						        chain.methods.push({ method: node_value.from, weight: node_value.weight, forward: forward_value });
						        chain.weight += node_value.weight;
						        chain.count++;
						        chains.push(chain);
					        }
				        }
			        }
			        if (chains.length > 0) {
				        if (node_value.to.length > 0) {
					        for (var jdx = 0; jdx < node_value.to.length; jdx++) {
						        for (var idx = 0; idx < chains.length; idx++) {
							        chain = chains[idx];
							        chain.to = node_value.to[jdx];
							        node_value.chains.push(JSON.parse(JSON.stringify(chain)));
						        }
					        }
					        node_value.forward = true;
				        }
				        else {
					        node_value.type = 'FinalChain';
					        item.key = ObjectId();
					        node_value.chains = chains;
				        }
				        /*
				        for (var jdx = 0; jdx < node_value.to.length; jdx++) {
					        for (var idx = 0; idx < chains.length; idx++) {
						        chain = chains[idx];
						        chain.to = node_value.to[jdx];
						        node_value.chains.push(JSON.parse(JSON.stringify(chain)));
					        }
				        }
				        if (node_value.to.length > 0) {
					        node_value.forward = true;
				        }*/
			        }
		        }
		        else { // forward to later partial reduce action !!!
			        for (var idx = 0; idx < values.length; idx++) {
				        item = values[idx];

				        if (item.type == 'Path') {
					        for (var jdx = 0; jdx < item.chains.length; jdx++) {
						        chain = item.chains[jdx];
						        chains.push(chain);
					        }
				        }
			        }
			        node_value = { type: 'Path', chains: chains };
		        }

                return node_value;
            }
        ");


        public static void MapReduceMetrics(MetricType metric, MagnitudeFunctionType v, WeightFunctionType w)
        {
            var client = new MongoClient("mongodb://192.168.100.16:27017");
            IMongoDatabase database = client.GetDatabase("mapreduce");

            switch (metric)
            {
                case MetricType.icr:
                    {
                        switch (v)
                        {
                            case MagnitudeFunctionType.sum:
                                {
                                    switch (w)
                                    {
                                        case WeightFunctionType.kon:
                                            {

                                            }
                                            break;
                                        case WeightFunctionType.loc:
                                            {

                                            }
                                            break;
                                        /*
                                        case WeightFunctionType.cyc:
                                            {
                                                //var results = collection.MapReduce<>
                                                // database.DropCollection("riskiness_sum");
                                                IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("riskiness_sum");
                                                collection.DeleteMany(FilterDefinition<BsonDocument>.Empty);

                                                foreach (KeyValuePair<string, JsonMethod> kv in methods)
                                                {
                                                    BsonArray calls = new BsonArray();
                                                    JsonMethod m = kv.Value;
                                                    foreach (JsonCall c in m.Calls)
                                                    {
                                                        calls.Add(c.Id);
                                                    }
                                                    
                                                    BsonDocument document =
                                                        new BsonDocument {
                                                            { "_id", m.Id },
                                                            { "value", new BsonDocument {
                                                                { "list", new BsonArray {
                                                                    new BsonDocument {
                                                                        { "id", m.Id },
                                                                        { "name", m.Name },
                                                                        { "calls", calls },
                                                                        { "cyc", m.Cyc },
                                                                        { "weight", m.Cyc },
                                                                        { "summed", new BsonArray { m.Id } },
                                                                        { "node", true },
                                                                        { "iteration", 0 },
                                                                        { "unwind", true }
                                                                    }
                                                                } }
                                                            } }
                                                        };

                                                    collection.InsertOne(document);
                                                }
                                                
                                                var options = new MapReduceOptions<BsonDocument, BsonDocument>
                                                {
                                                    OutputOptions = MapReduceOutputOptions.Replace("riskiness_sum")
                                                };

                                                int repeat = 0;
                                                List<BsonDocument> resultAsBsonDocumentList = null;

                                                do
                                                {
                                                    resultAsBsonDocumentList = collection.MapReduce(map, reduce, options).ToList();

                                                    BsonDocument project = new BsonDocument("$project", new BsonDocument("items", new BsonDocument("$size", "$value.list")));
                                                    BsonDocument match   = new BsonDocument("$match",   new BsonDocument("items", new BsonDocument("$gt",   1)));
                                                    BsonDocument count   = new BsonDocument("$count",   "items");

                                                    BsonDocument[] stages = { project, match, count };

                                                    try
                                                    {
                                                        repeat = collection.Aggregate<BsonDocument>(PipelineDefinition<BsonDocument, BsonDocument>.Create(stages)).Single().GetValue("items").AsInt32;
                                                    }
                                                    catch (InvalidOperationException ex)
                                                    {
                                                        repeat = 0;
                                                    }
                                                } while (repeat > 0);
                                            }*/
                                        case WeightFunctionType.cyc:
                                            {
                                                //var results = collection.MapReduce<>
                                                // database.DropCollection("riskiness_sum");
                                                IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("chainmetrics");
                                                collection.DeleteMany(FilterDefinition<BsonDocument>.Empty);

                                                foreach (KeyValuePair<string, JsonMethod> kv in methods)
                                                {
                                                    BsonArray calls = new BsonArray();
                                                    JsonMethod m = kv.Value;
                                                    foreach (JsonCall c in m.Calls)
                                                    {
                                                        calls.Add(c.Id);
                                                    }

                                                    BsonDocument document =
                                                        new BsonDocument {
                                                            { "_id", "N-"+m.Id },
                                                            { "value", new BsonDocument {
                                                                { "from", m.Id },
                                                                { "to", new BsonArray(calls) },
                                                                { "type", "Node" },
                                                                { "weight", m.Cyc },
                                                                { "forward", m.Calls.Count > 0 ? true : false }
                                                            } }
                                                        };

                                                    collection.InsertOne(document);
                                                }

                                                var options = new MapReduceOptions<BsonDocument, BsonDocument>
                                                {
                                                    OutputOptions = MapReduceOutputOptions.Replace("chainmetrics")
                                                };

                                                int repeat = 0;
                                                List<BsonDocument> resultAsBsonDocumentList = null;

                                                do
                                                {
                                                    resultAsBsonDocumentList = collection.MapReduce(mapChains, reduceChains, options).ToList();

                                                    BsonDocument project = new BsonDocument("$project", new BsonDocument("forward", "$value.forward"));
                                                    BsonDocument match = new BsonDocument("$match", new BsonDocument("forward", true));
                                                    BsonDocument count = new BsonDocument("$count", "forward");

                                                    BsonDocument[] stages = { project, match, count };

                                                    try
                                                    {
                                                        repeat = collection.Aggregate<BsonDocument>(PipelineDefinition<BsonDocument, BsonDocument>.Create(stages)).Single().GetValue("forward").AsInt32;

                                                    }
                                                    catch (InvalidOperationException ex)
                                                    {
                                                        repeat = 0;
                                                    }
                                                } while (repeat > 0);

                                                BsonDocument project1 = new BsonDocument("$project", new BsonDocument{ { "_id", 0 }, { "chain", "$value.chains" } } );
                                                BsonDocument unwind = new BsonDocument("$unwind", "$chain");
                                                BsonDocument project2 = new BsonDocument("$project", new BsonDocument { { "chain", "$chain.methods" }, { "weight", "$chain.weight" }, { "count", "$chain.count" } }); 
                                                BsonDocument outs = new BsonDocument("$out", "metrics");

                                                BsonDocument[] stages2 = { project1, unwind, project2, outs };

                                                try
                                                {
                                                    repeat = collection.Aggregate<BsonDocument>(PipelineDefinition<BsonDocument, BsonDocument>.Create(stages2)).Single().GetValue("items").AsInt32;
                                                }
                                                catch (InvalidOperationException ex)
                                                {
                                                    repeat = 0;
                                                }
                                            }
                                            break;
                                    }
                                }
                                break;
                            case MagnitudeFunctionType.avg:
                                {

                                }
                                break;
                            case MagnitudeFunctionType.min:
                                {

                                }
                                break;
                            case MagnitudeFunctionType.max:
                                {

                                }
                                break;
                        }
                    }
                    break;
                case MetricType.icf:
                    {

                    }
                    break;
                case MetricType.ics:
                    {

                    }
                    break;
            }
        }
    }

    public enum MetricType
    {
        icr, icf, ics
    }

    public enum MagnitudeFunctionType
    {
        sum, avg, min, max
    }

    public enum WeightFunctionType
    {
        kon, loc, cyc
    }

    public class SystemStats
    {
        public System.Int64 numClases = 0;
        public System.Int64 numMetodos = 0;
        public System.Int64 numMetodosPorClase = 0;
        public System.Int64 numCadenas = 0;
        public System.Int64 maxLargoCadena = 0;
        public System.Int64 promLargoCadena = 0;
        public System.Int64 numLOC = 0;
        public System.Int64 maxCalls = 0;
        public System.Int64 promCalls = 0;
        public System.Int64 maxCalledby = 0;
        public System.Int64 promCalledby = 0;
        public string maxCallsToName = "";
        public string maxCalledByName = "";
    }



    public interface IHalsteadMetrics
    {
        /// <summary>
        /// Gets the number of operands.
        /// </summary>
        int NumberOfOperands { get; }

        /// <summary>
        /// Gets the number of operators.
        /// </summary>
        int NumberOfOperators { get; }

        /// <summary>
        /// Gets the number of unique operands.
        /// </summary>
        int NumberOfUniqueOperands { get; }

        /// <summary>
        /// Gets the number of unique operators.
        /// </summary>
        int NumberOfUniqueOperators { get; }

        /// <summary>
        /// Gets the number of expected bugs in the underlying source code.
        /// </summary>
        /// <returns>The expected number of bugs as an <see cref="int"/>.</returns>
        int GetBugs();

        /// <summary>
        /// Gets the difficulty of the underlying source code.
        /// </summary>
        /// <returns>The calculated difficulty of the underlying source code as a <see cref="double"/> value.</returns>
        double GetDifficulty();

        /// <summary>
        /// Gets the estimated time to write the underlying source code.
        /// </summary>
        /// <returns>The estimated time as a <see cref="TimeSpan"/>.</returns>
        TimeSpan GetEffort();

        /// <summary>
        /// Gets the length of the underlying souce code.
        /// </summary>
        /// <returns>The length as an <see cref="int"/>.</returns>
        int GetLength();

        /// <summary>
        /// Gets the size of vocabulary of the underlying source code.
        /// </summary>
        /// <returns>The vocabulary size as an <see cref="int"/>.</returns>
        int GetVocabulary();

        /// <summary>
        /// Gets the volume of the underlying source code.
        /// </summary>
        /// <returns>The volume as a <see cref="double"/>.</returns>
        double GetVolume();

        /// <summary>
        /// Creates a new instance of an <see cref="IHalsteadMetrics"/> by merging another instance into the current.
        /// </summary>
        /// <param name="metrics">The other instance to merge.</param>
        /// <returns>The new <see cref="IHalsteadMetrics"/> instance from the merged sources.</returns>
        IHalsteadMetrics Merge(IHalsteadMetrics metrics);
    }

}
