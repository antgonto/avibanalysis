using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    [JsonObject(MemberSerialization.OptIn, Description = "Namespace")]
    class JsonPaquete
    {
        static int currentId = 0;
        static Dictionary<string, JsonPaquete> paquetes = new Dictionary<string, JsonPaquete>();
        static JsonProject project;
        int id;
        string name;
        List<JsonClase> clases = new List<JsonClase>();

        public JsonPaquete(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }

        public static JsonProject Project { get => project; set => project = value; }
        [JsonProperty]
        public int Id { get => id; set => id = value; }
        [JsonProperty]
        public string Name { get => name; set => name = value; }
        [JsonProperty("Classes")]
        public List<JsonClase> Clases { get => clases; set => clases = value; }

        public static JsonPaquete GetPaquete(string name)
        {
            JsonPaquete paquete;

            if (!paquetes.TryGetValue(name, out paquete))
            {
                paquete = new JsonPaquete(currentId++, name);
                paquetes.Add(name, paquete);
                project.Workspaces.Add(paquete);
            }

            return paquete;
        }
    }
}

