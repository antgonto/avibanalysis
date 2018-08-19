using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    [JsonObject(MemberSerialization.OptIn, Description = "Class")]
    public class JsonClass
    {
        static Dictionary<string, JsonClass> classes = new Dictionary<string, JsonClass>();
        int id;
        string name, fullname;
        JsonNamespace onamespace;
        List<JsonAttribute> attributes = new List<JsonAttribute>();
        List<JsonMethod> methods = new List<JsonMethod>();

        public JsonClass(int id, string name, string fullname, JsonNamespace onamespace)
        {
            this.id = id;
            this.name = name;
            this.fullname = fullname;
            this.onamespace = onamespace;
        }

        public static Dictionary<string, JsonClass> Classes { get => classes; set => classes = value; }
        [JsonProperty]
        public int Id { get => id; set => id = value; }
        [JsonProperty]
        public string Name { get => name; set => name = value; }
        [JsonProperty]
        public string Fullname { get => fullname; set => fullname = value; }
        [JsonProperty("NamespaceId")]
        public int NamespaceId { get => onamespace.Id; set => onamespace.Id = value; }
        [JsonProperty("Namespace")]
        public string NamespaceName { get => onamespace.Name; set => onamespace.Name = value; }
        [JsonProperty("Fullnamespace")]
        public string FullNamespaceName { get => onamespace.Fullname; set => onamespace.Fullname = value; }
        //[JsonProperty("Attributes")]
        public List<JsonAttribute> Attributes { get => attributes; set => attributes = value; }
        [JsonProperty("Methods")]
        public List<JsonMethod> Methods { get => methods; set => methods = value; }

        public JsonNamespace GetNamespace { get => onamespace; set => onamespace = value; }

        public static JsonClass GetClass(string name, string onamespace)
        {
            JsonClass oclass;

            if (!classes.TryGetValue(onamespace + "." + name, out oclass))
            {
                JsonNamespace p = JsonNamespace.GetNamespace(onamespace);
                oclass = new JsonClass(JsonProject.Nextid++, name, onamespace + "." + name, p);
                classes.Add(onamespace + "." + name, oclass);
                p.Classes.Add(oclass);
            }

            return oclass;
        }

    }
}
