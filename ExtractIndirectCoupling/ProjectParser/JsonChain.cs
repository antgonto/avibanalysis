using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    class JsonChain
    {
        long id;
        double kon;
        double loc;
        double cyc;

        public JsonChain(long id)
        {
            this.id = id;
            this.kon = 0;
            this.loc = 0;
            this.cyc = 0;
        }

        public JsonChain(long id, double kon, double loc, double cyc)
        {
            this.id = id;
            this.kon = kon;
            this.loc = loc;
            this.cyc = cyc;
        }

        public int CollectChainWeights(List<JsonMethod> list)
        {
            foreach (JsonMethod m in list)
            {
                Kon += m.Kon;
                Loc += m.Loc;
                Cyc += m.Cyc;
            }
            return list.Count;
        }

        public long Id { get => id; set => id = value; }
        public double Kon { get => kon; set => kon = value; }
        public double Loc { get => loc; set => loc = value; }
        public double Cyc { get => cyc; set => cyc = value; }
    }
}
