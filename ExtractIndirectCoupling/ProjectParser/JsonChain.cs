using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    class JsonChain
    {
        int id;
        int kon;
        int loc;
        int cyc;

        public JsonChain(int id)
        {
            this.id = id;
            this.kon = 0;
            this.loc = 0;
            this.cyc = 0;
        }

        public JsonChain(int id, int kon, int loc, int cyc)
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

        public int Id { get => id; set => id = value; }
        public int Kon { get => kon; set => kon = value; }
        public int Loc { get => loc; set => loc = value; }
        public int Cyc { get => cyc; set => cyc = value; }
    }
}
