using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    [JsonObject(MemberSerialization.OptIn, Description = "Project")]
    public class JsonProject
    {
        static long nextid = 0;
        string name;
        List<JsonNamespace> namespaces = new List<JsonNamespace>();
        List<List<JsonCall>> chains = new List<List<JsonCall>>();

        [JsonProperty]
        public string Name { get => name; set => name = value; }
        [JsonProperty]
        public List<JsonNamespace> Namespaces { get => namespaces; set => namespaces = value; }
        // Disabled until chains collection runtime issue is solved
        //[JsonProperty]
        public List<List<JsonCall>> Chains { get => chains; set => chains = value; }
        public static long Nextid { get => nextid; set => nextid = value; }
    }
}
