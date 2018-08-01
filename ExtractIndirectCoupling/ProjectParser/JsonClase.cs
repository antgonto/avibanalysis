using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    [JsonObject(MemberSerialization.OptIn, Description = "Clase")]
    class JsonClase
    {
        static int currentId = 0;
        static Dictionary<string, JsonClase> clases = new Dictionary<string, JsonClase>();
        int id;
        string name, spell;
        JsonPaquete paquete;
        List<JsonAtributo> atributos = new List<JsonAtributo>();
        List<JsonMetodo> metodos = new List<JsonMetodo>();

        public JsonClase(int id, string name, JsonPaquete paquete)
        {
            this.id = id;
            this.name = name;
            this.paquete = paquete;
        }

        [JsonProperty]
        public int Id { get => id; set => id = value; }
        [JsonProperty]
        public string Name { get => name; set => name = value; }
        [JsonProperty("NamespaceId")]
        public int PaqueteId { get => paquete.Id; set => paquete.Id = value; }
        [JsonProperty("Namespace")]
        public string PaqueteName { get => paquete.Name; set => paquete.Name = value; }
        //[JsonProperty("Attributes")]
        public List<JsonAtributo> Atributos { get => atributos; set => atributos = value; }
        [JsonProperty("Methods")]
        public List<JsonMetodo> Metodos { get => metodos; set => metodos = value; }

        public JsonPaquete GetPaquete { get => paquete; set => paquete = value; }

        public static JsonClase GetClase(string name, string paquete)
        {
            JsonClase clase;

            if (!clases.TryGetValue(paquete + "." + name, out clase))
            {
                JsonPaquete p = JsonPaquete.GetPaquete(paquete);
                clase = new JsonClase(currentId++, name, p);
                clases.Add(paquete + "." + name, clase);
                p.Clases.Add(clase);
            }

            return clase;
        }

    }
}
