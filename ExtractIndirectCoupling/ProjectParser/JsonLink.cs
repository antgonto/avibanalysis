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
        int chainId;
        int methodIdx;
        int chainIdx;

        public JsonLink(JsonMethod method, int chainId, int methodIdx, int chainIdx)
        {
            this.method = method;
            this.chainId = chainId;
            this.methodIdx = methodIdx;
            this.chainIdx = chainIdx;
        }

        public JsonMethod Method { get => method; set => method = value; }
        public int MethodIdx { get => methodIdx; set => methodIdx = value; }
        public int ChainIdx { get => chainIdx; set => chainIdx = value; }
        public int ChainId { get => chainId; set => chainId = value; }
    }
}
