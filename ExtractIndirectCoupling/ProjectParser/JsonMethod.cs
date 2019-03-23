using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectParser
{
    [JsonObject(MemberSerialization.OptIn, Description = "Method")]
    public class JsonMethod : IEquatable<JsonMethod>
    {
        static public int cantidadMetodos = 0;
        static public int totalDeLOC = 0;
        static public int numOfChains = 0;
        static public int maxChainLength = 0;
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

        static int count = 0;
        static int avgdepth = 0;

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

        // Metrics values
        Metrics kon_metrics = new Metrics();
        Metrics loc_metrics = new Metrics();
        Metrics cyc_metrics = new Metrics();

        // SCC Metric Values
        Dictionary<int, Dictionary<int, List<ForwardMetrics>>> sccForward;
        Dictionary<int, Dictionary<int, List<BackwardMetrics>>> sccBackward;

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
        }

        public JsonMethod(int id, string name, JsonClass clase, JsonNamespace @namespace, int loc, int kon, int cyc)
        {
            this.id = id;
            this.name = name;
            this.fullname = clase.Fullname + "." + name;
            this.oclass = clase;
            this.onamespace = @namespace;
            this.Loc = loc;
            this.Kon = kon;
            this.Cyc = cyc;
        }

        public static JsonMethod GetMethod(string name, string oclass, string onamespace, bool isInterface, int loc, int kon, int cyc)
        {
            JsonMethod method;

            if (!methods.TryGetValue(onamespace + "." + oclass + "." + name, out method))
            {
                JsonClass c = JsonClass.GetClass(oclass, onamespace, isInterface);
                method = new JsonMethod(JsonProject.Nextid++, name, c, JsonNamespace.GetNamespace(onamespace), loc, kon, cyc);
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
            method.Kon_metrics = new Metrics();
            method.Loc_metrics = new Metrics();
            method.Cyc_metrics = new Metrics();
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
                mc.Kon_metrics.Fmax = Math.Max(mc.Kon_metrics.Fmax, mc.Kon_metrics.Fsum);
                mc.Kon_metrics.Fmin = Math.Min(mc.Kon_metrics.Fmin, mc.Kon_metrics.Fsum);
                mc.Kon_metrics.Favg += mc.Kon_metrics.Fsum;
                mc.Kon_metrics.Fcnt++;
            }

            // loc values
            mc.Loc_metrics.Fsum = m.Loc_metrics.Fsum + mc.Loc;
            if (mc.isSccOut)
            {
                mc.Loc_metrics.Fmax = Math.Max(mc.Loc_metrics.Fmax, mc.Loc_metrics.Fsum);
                mc.Loc_metrics.Fmin = Math.Min(mc.Loc_metrics.Fmin, mc.Loc_metrics.Fsum);
                mc.Loc_metrics.Favg += mc.Loc_metrics.Fsum;
                mc.Loc_metrics.Fcnt++;
            }

            // cyc values
            mc.Cyc_metrics.Fsum = m.Cyc_metrics.Fsum + mc.Cyc;
            if (mc.isSccOut)
            {
                mc.Cyc_metrics.Fmax = Math.Max(mc.Cyc_metrics.Fmax, mc.Cyc_metrics.Fsum);
                mc.Cyc_metrics.Fmin = Math.Min(mc.Cyc_metrics.Fmin, mc.Cyc_metrics.Fsum);
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
                mc.Kon_metrics.Bmax = Math.Max(mc.Kon_metrics.Bmax, mc.Kon_metrics.Bsum);
                mc.Kon_metrics.Bmin = Math.Min(mc.Kon_metrics.Bmin, mc.Kon_metrics.Bsum);
                mc.Kon_metrics.Bavg += mc.Kon_metrics.Bsum;
                mc.Kon_metrics.Bcnt++;
            }

            // loc values
            mc.Loc_metrics.Bsum = m.Loc_metrics.Bsum + mc.Loc;
            if (mc.isSccIn)
            {
                mc.Loc_metrics.Bmax = Math.Max(mc.Loc_metrics.Bmax, mc.Loc_metrics.Bsum);
                mc.Loc_metrics.Bmin = Math.Min(mc.Loc_metrics.Bmin, mc.Loc_metrics.Bsum);
                mc.Loc_metrics.Bavg += mc.Loc_metrics.Bsum;
                mc.Loc_metrics.Bcnt++;
            }

            // cyc values
            mc.Cyc_metrics.Bsum = m.Cyc_metrics.Bsum + mc.Cyc;
            if (mc.isSccIn)
            {
                mc.Cyc_metrics.Bmax = Math.Max(mc.Cyc_metrics.Bmax, mc.Cyc_metrics.Bsum);
                mc.Cyc_metrics.Bmin = Math.Min(mc.Cyc_metrics.Bmin, mc.Cyc_metrics.Bsum);
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

            // Watch out - Sync needed!!!
            Parallel.ForEach(startList, m => CollectMetricsUsingDfsThread(m));
            //Parallel.ForEach(startList, new ParallelOptions { MaxDegreeOfParallelism = 32 }, m => CollectMetricsUsingDfsThread(m));

            // No Sync needed!!!
            Parallel.ForEach(allList, m => { AvgMetrics(m); SumMetrics(m); });
            //Parallel.ForEach(allList, new ParallelOptions { MaxDegreeOfParallelism = 32 }, m => { AvgMetrics(m); SumMetrics(m); });

            List<Tuple<JsonMethod, JsonMethod>> pairList = new List<Tuple<JsonMethod, JsonMethod>>();
            foreach (JsonMethod m1 in allList)
                foreach (JsonMethod m2 in allList)
                    if (m1.Id != m2.Id && m1.IsCollapsed == false && m2.IsCollapsed == false)
                        pairList.Add(new Tuple<JsonMethod, JsonMethod>(m1, m2));

            // Watch out - Sync needed!!!
            Parallel.ForEach(pairList, p => { CollectPairMetrics(p); });
            //Parallel.ForEach(pairList, new ParallelOptions { MaxDegreeOfParallelism = 32 }, p => { CollectPairMetrics(p); });
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

        /*
        maxMethods = JsonProject.Nextid;
        chainsCnt = 0;
        chainMethodsCnt = new List<long>((int) maxMethods);
        chainMethods = new SparseMatrix<JsonMethod>(maxChains, maxMethods);
        methodChainsCnt = new List<long>((int) maxMethods);
        methodChains = new SparseMatrix<JsonChain>(maxChains, maxMethods);
        */

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

                JsonMethod.numOfChains++;
                JsonMethod.maxChainLength = Math.Max(JsonMethod.maxChainLength, mCnt);
            }
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

        static void AcumPairMetrics(PairMetrics p)
        {
            p.K.Favg += p.K.Fsum;
            p.K.Fmin = Math.Min(p.K.Fmin, p.K.Fsum);
            p.K.Fmax = Math.Max(p.K.Fmax, p.K.Fsum);
            p.K.Fsum = 0;
            p.K.Fcnt++;

            p.L.Favg += p.L.Fsum;
            p.L.Fmin = Math.Min(p.L.Fmin, p.L.Fsum);
            p.L.Fmax = Math.Max(p.L.Fmax, p.L.Fsum);
            p.L.Fsum = 0;
            p.L.Fcnt++;

            p.C.Favg += p.C.Fsum;
            p.C.Fmin = Math.Min(p.C.Fmin, p.C.Fsum);
            p.C.Fmax = Math.Max(p.C.Fmax, p.C.Fsum);
            p.C.Fsum = 0;
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
            // TODO: this method needs to be completed, using SUM by now

            //MarkSccEntranceAndExit();

            foreach (JsonMethod scc in JsonMethod.SccList)
            {
                /*
                scc.SccMethods.ForEach(delegate (JsonMethod c) { c.Visited = false; });
                scc.sccForward = new Dictionary<long, Dictionary<long, List<ForwardMetrics>>>();
                
                foreach (JsonMethod m in scc.SccMethods.FindAll(i => i.isSccIn == true))
                {
                    scc.SccMethods.ForEach(delegate (JsonMethod c) { ResetMetricValues(c); });
                    InitForwardMetricValues(m);
                    SccMetricsForwardDFS(m);
                    Dictionary<long, List<ForwardMetrics>> outputs = new Dictionary<long, List<ForwardMetrics>>();
                    scc.sccForward.Add(m.Id, outputs);
                    foreach (JsonMethod o in scc.SccMethods.FindAll(x => x.isSccOut == true))
                    {
                        AvgDfsForwardMetricValues(o);
                        List<ForwardMetrics> list = new List<ForwardMetrics>();
                        list.Add(o.kon_metrics.GetForwardMetrics());
                        list.Add(o.loc_metrics.GetForwardMetrics());
                        list.Add(o.cyc_metrics.GetForwardMetrics());
                        outputs.Add(o.Id, list);
                    }
                }

                scc.sccBackward = new Dictionary<long, Dictionary<long, List<BackwardMetrics>>>();

                foreach (JsonMethod m in scc.SccMethods.FindAll(i => i.isSccOut == true))
                {
                    scc.SccMethods.ForEach(delegate (JsonMethod c) { ResetMetricValues(c); });
                    InitBackwardMetricValues(m);
                    SccMetricsBackwardDFS(m);
                    Dictionary<long, List<BackwardMetrics>> inputs = new Dictionary<long, List<BackwardMetrics>>();
                    scc.sccBackward.Add(m.Id, inputs);
                    foreach (JsonMethod i in scc.SccMethods.FindAll(x => x.isSccIn == true))
                    {
                        AvgDfsBackwardMetricValues(i);
                        List<BackwardMetrics> list = new List<BackwardMetrics>();
                        list.Add(i.kon_metrics.GetBackwardMetrics());
                        list.Add(i.loc_metrics.GetBackwardMetrics());
                        list.Add(i.cyc_metrics.GetBackwardMetrics());
                        inputs.Add(i.Id, list);
                    }
                }
                */

                scc.Kon = 0;
                scc.Loc = 0;
                scc.Cyc = 0;
                foreach (JsonMethod m in scc.SccMethods)
                {
                    scc.Kon += m.Kon;
                    scc.Loc += m.Loc;
                    scc.Cyc += m.Cyc;
                }
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
                if (m.Value.CalledBy.Count == 0)
                {
                    list.Add(m.Value);
                }
            }

            // Watch out - Sync needed!!!
            Parallel.ForEach(list, new ParallelOptions { MaxDegreeOfParallelism = 32 }, m => CountDFS(m, 1));

            if (count > 0)
            {
                avgdepth = avgdepth / count;
                Console.WriteLine("# of Chains: " + count.ToString() + ", AVG Length: " + avgdepth.ToString());
                Console.Read();
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
                if (depth > 2)
                {
                    IncreaseCount(depth);
                }
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
            m.FSUMKON = kon_metrics.Bsum;
            m.FSUMLOC = loc_metrics.Bsum;
            m.FSUMCYC = cyc_metrics.Bsum;
            m.RSUMKON = kon_metrics.Fsum;
            m.RSUMLOC = loc_metrics.Fsum;
            m.RSUMCYC = cyc_metrics.Fsum;

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
        public Metrics Kon_metrics { get => kon_metrics; set => kon_metrics = value; }
        public Metrics Loc_metrics { get => loc_metrics; set => loc_metrics = value; }
        public Metrics Cyc_metrics { get => cyc_metrics; set => cyc_metrics = value; }
        public bool IsRecursive { get => isRecursive; set => isRecursive = value; }
        public Dictionary<int, Dictionary<int, List<ForwardMetrics>>> SccForward { get => sccForward; set => sccForward = value; }
        public Dictionary<int, Dictionary<int, List<BackwardMetrics>>> SccBackward { get => sccBackward; set => sccBackward = value; }
        internal static SparseMatrix<PairMetrics> PairMetricsList { get => pairMetricsList; set => pairMetricsList = value; }
        public static int MaxMethods { get => maxMethods; set => maxMethods = value; }



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

}
