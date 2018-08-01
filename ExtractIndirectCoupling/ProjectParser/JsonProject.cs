using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    [JsonObject(MemberSerialization.OptIn, Description = "Project")]
    class JsonProject
    {
        string name;
        List<JsonPaquete> workspaces = new List<JsonPaquete>();
        List<List<JsonCall>> chains = new List<List<JsonCall>>();

        [JsonProperty]
        public string Name { get => name; set => name = value; }
        [JsonProperty]
        public List<JsonPaquete> Workspaces { get => workspaces; set => workspaces = value; }
        [JsonProperty]
        public List<List<JsonCall>> Chains { get => chains; set => chains = value; }
    }
}
