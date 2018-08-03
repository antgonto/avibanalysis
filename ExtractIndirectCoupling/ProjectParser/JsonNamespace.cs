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
        static int currentId = 0;
        static Dictionary<string, JsonNamespace> namespaces = new Dictionary<string, JsonNamespace>();
        static JsonProject project;
        int id;
        string name;
        List<JsonNamespace> childNamespaces = new List<JsonNamespace>();
        List<JsonClass> classes = new List<JsonClass>();

        public JsonNamespace(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        public static JsonProject Project { get => project; set => project = value; }
        [JsonProperty]
        public int Id { get => id; set => id = value; }
        [JsonProperty]
        public string Name { get => name; set => name = value; }
        [JsonProperty("Namespaces")]
        internal List<JsonNamespace> ChildNamespaces { get => childNamespaces; set => childNamespaces = value; }
        [JsonProperty("Classes")]
        public List<JsonClass> Clases { get => classes; set => classes = value; }

        public static JsonNamespace GetNamespace(string name)
        {
            JsonNamespace onamespace;

            if (!namespaces.TryGetValue(name, out onamespace))
            {
                onamespace = new JsonNamespace(currentId++, name);
                namespaces.Add(name, onamespace);
                //project.Namespaces.Add(onamespace);

                // Add namespaces hierarchy to project
                string[] namespaceParts = name.Split('.');
                List<JsonNamespace> nslist = project.Namespaces;
                string nameprefix = "";

                // Build/traverse namespaces hierarchy
                for (int i = 0; i < namespaceParts.Length - 1; i++)
                {
                    JsonNamespace ns = new JsonNamespace(currentId, nameprefix + namespaceParts[i]);
                    int idx = nslist.IndexOf(ns);
                    if (idx < 0)
                    {
                        currentId++;
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
                   name == other.name;
        }

        public override int GetHashCode()
        {
            return 363513814 + EqualityComparer<string>.Default.GetHashCode(name);
        }
    }
}

