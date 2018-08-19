using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    class JsonLink
    {
        JsonMethod method;
        JsonChain chain;
        int methodIdx;
        int chainIdx;

        public JsonLink(JsonMethod method, JsonChain chain, int methodIdx, int chainIdx)
        {
            this.method = method;
            this.chain = chain;
            this.methodIdx = methodIdx;
            this.chainIdx = chainIdx;
        }

        public JsonMethod Method { get => method; set => method = value; }
        public int MethodIdx { get => methodIdx; set => methodIdx = value; }
        public int ChainIdx { get => chainIdx; set => chainIdx = value; }
        internal JsonChain Chain { get => chain; set => chain = value; }
    }
}
