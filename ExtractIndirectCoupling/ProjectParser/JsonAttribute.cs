using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    [JsonObject(MemberSerialization.OptIn, Description = "Attribute")]
    public class JsonAttribute
    {
        int id;
        string name;
        JsonClass oClass;
        JsonNamespace oNamespace;
        static Dictionary<string, JsonAttribute> attributes = new Dictionary<string, JsonAttribute>();
        List<JsonCall> calledBy = new List<JsonCall>();
        public JsonAttribute(int id, string name, JsonClass clase, JsonNamespace workspace)
        {
            this.id = id;
            this.name = name;
            this.oClass = clase;
            this.oNamespace = workspace;
        }

        public static JsonAttribute GetAttribute(string name, string clase, string workspace)
        {
            JsonAttribute oAttribute;

            if (!attributes.TryGetValue(workspace + "." + clase + "." + name, out oAttribute))
            {
                JsonClass c = ProjectParser.JsonClass.GetClass(clase, workspace, false);
                oAttribute = new JsonAttribute(JsonProject.Nextid++, name, c, JsonNamespace.GetNamespace(workspace));
                attributes.Add(workspace + "." + clase + "." + name, oAttribute);
                c.Attributes.Add(oAttribute);
            }

            return oAttribute;
        }

        [JsonProperty]
        public int Id { get => id; set => id = value; }
        [JsonProperty]
        public string Name { get => name; set => name = value; }
        [JsonProperty("ClassId")]
        public int ClaseId { get => oClass.Id; set => oClass.Id = value; }
        [JsonProperty("Class")]
        public string ClaseName { get => oClass.Name; set => oClass.Name = value; }
        [JsonProperty("NamespaceId")]
        public int NamespaceId { get => oNamespace.Id; set => oNamespace.Id = value; }
        [JsonProperty("Namespace")]
        public string NamespaceName { get => oNamespace.Name; set => oNamespace.Name = value; }
        [JsonProperty]
        internal List<JsonCall> CalledBy { get => calledBy; set => calledBy = value; }
        internal JsonClass OClass { get => oClass; set => oClass = value; }
        internal JsonNamespace ONamespace { get => oNamespace; set => oNamespace = value; }
        
    }
}
