using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    [JsonObject(MemberSerialization.OptIn, Description = "Namespace")]
    public class JsonNamespace : IEquatable<JsonNamespace>
    {
        static Dictionary<string, JsonNamespace> namespaces = new Dictionary<string, JsonNamespace>();
        static JsonProject project;
        long id;
        string name;
        string fullname;
        long parentid;
        List<JsonNamespace> childNamespaces = new List<JsonNamespace>();
        List<JsonClass> classes = new List<JsonClass>();

        public JsonNamespace(long id, string name, string fullname, long parentid)
        {
            this.Id = id;
            this.Name = name;
            this.fullname = fullname;
            this.parentid = parentid;
        }

        public static JsonProject Project { get => project; set => project = value; }
        [JsonProperty]
        public long Id { get => id; set => id = value; }
        [JsonProperty]
        public string Name { get => name; set => name = value; }
        [JsonProperty]
        public string Fullname { get => fullname; set => fullname = value; }
        [JsonProperty]
        public long ParentId { get => parentid; set => parentid = value; }
        [JsonProperty("Namespaces")]
        internal List<JsonNamespace> ChildNamespaces { get => childNamespaces; set => childNamespaces = value; }
        [JsonProperty("Classes")]
        public List<JsonClass> Classes { get => classes; set => classes = value; }

        public static JsonNamespace GetNamespace(string name)
        {
            JsonNamespace onamespace;

            if (!namespaces.TryGetValue(name, out onamespace))
            {
                // Add namespaces hierarchy to project
                string[] namespaceParts = name.Split('.');
                List<JsonNamespace> nslist = project.Namespaces;
                List<JsonNamespace> childlist = nslist;
                string nameprefix = "";
                long parent = -1;

                // Build/traverse namespaces hierarchy
                for (int i = 0; i < namespaceParts.Length; i++)
                {
                    JsonNamespace ns = new JsonNamespace(JsonProject.Nextid, namespaceParts[i], nameprefix + namespaceParts[i], parent);
                    int idx = childlist.IndexOf(ns);
                    if (idx < 0)
                    {
                        JsonProject.Nextid++;
                        childlist.Add(ns);
                    }
                    else
                    {
                        ns = childlist[idx];
                    }
                    onamespace = ns;
                    childlist = ns.childNamespaces;
                    parent = ns.Id;
                    nameprefix = nameprefix + namespaceParts[i] + ".";
                }

                // Add leaf namespace to project namespaces hierarchy
                namespaces.Add(name, onamespace);
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
                   string.Equals(fullname,other.fullname);
        }

        public override int GetHashCode()
        {
            return 363513814 + EqualityComparer<string>.Default.GetHashCode(fullname);
        }
    }
}

