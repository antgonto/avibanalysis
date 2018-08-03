using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    [JsonObject(MemberSerialization.OptIn, Description = "Namespace")]
    class JsonNamespace : IEquatable<JsonNamespace>
    {
        static Dictionary<string, JsonNamespace> namespaces = new Dictionary<string, JsonNamespace>();
        static JsonProject project;
        long id;
        string name;
        string fullname;
        List<JsonNamespace> childNamespaces = new List<JsonNamespace>();
        List<JsonClass> classes = new List<JsonClass>();

        public JsonNamespace(long id, string name, string fullname)
        {
            this.Id = id;
            this.Name = name;
            this.fullname = fullname;
        }

        public static JsonProject Project { get => project; set => project = value; }
        [JsonProperty]
        public long Id { get => id; set => id = value; }
        [JsonProperty]
        public string Name { get => name; set => name = value; }
        [JsonProperty]
        public string Fullname { get => fullname; set => fullname = value; }
        [JsonProperty("Namespaces")]
        internal List<JsonNamespace> ChildNamespaces { get => childNamespaces; set => childNamespaces = value; }
        [JsonProperty("Classes")]
        public List<JsonClass> Classes { get => classes; set => classes = value; }

        public static JsonNamespace GetNamespace(string name)
        {
            JsonNamespace onamespace;

            if (!namespaces.TryGetValue(name, out onamespace))
            {
                onamespace = new JsonNamespace(JsonProject.Nextid++, name.Substring(name.LastIndexOf('.')+1), name);
                namespaces.Add(name, onamespace);

                // Add namespaces hierarchy to project
                string[] namespaceParts = name.Split('.');
                List<JsonNamespace> nslist = project.Namespaces;
                string nameprefix = "";

                // Build/traverse namespaces hierarchy
                for (int i = 0; i < namespaceParts.Length - 1; i++)
                {
                    JsonNamespace ns = new JsonNamespace(JsonProject.Nextid, namespaceParts[i], nameprefix + namespaceParts[i]);
                    int idx = nslist.IndexOf(ns);
                    if (idx < 0)
                    {
                        JsonProject.Nextid++;
                        nslist.Add(ns);
                    }
                    else
                    {
                        ns = nslist[idx];
                    }
                    nslist = ns.childNamespaces;
                    nameprefix = nameprefix + namespaceParts[i] + ".";
                }

                // Add leaf namespace to project namespaces hierarchy
                nslist.Add(onamespace);
            }

            return onamespace;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as JsonNamespace);
        }

        public bool Equals(JsonNamespace other)
        {
            return other != null &&
                   fullname == other.fullname;
        }

        public override int GetHashCode()
        {
            return 363513814 + EqualityComparer<string>.Default.GetHashCode(fullname);
        }
    }
}

