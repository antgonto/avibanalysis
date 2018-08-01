using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    [JsonObject(MemberSerialization.OptIn, Description = "Atributo")]
    class JsonAtributo
    {
        static int currentId = 0;

        int id;
        string name;
        JsonClase oClase;
        JsonPaquete oPaquete;
        static Dictionary<string, JsonAtributo> atributos = new Dictionary<string, JsonAtributo>();
        List<JsonCall> calledBy = new List<JsonCall>();
        public JsonAtributo(int id, string name, JsonClase clase, JsonPaquete paquete)
        {
            this.id = id;
            this.name = name;
            this.oClase = clase;
            this.oPaquete = paquete;
        }

        public static JsonAtributo GetAtributo(string name, string clase, string paquete)
        {
            JsonAtributo oAtributo;

            if (!atributos.TryGetValue(paquete + "." + clase + "." + name, out oAtributo))
            {
                JsonClase c = ProjectParser.JsonClase.GetClase(clase, paquete);
                oAtributo = new JsonAtributo(currentId++, name, c, JsonPaquete.GetPaquete(paquete));
                atributos.Add(paquete + "." + clase + "." + name, oAtributo);
                c.Atributos.Add(oAtributo);
            }

            return oAtributo;
        }

        [JsonProperty]
        public int Id { get => id; set => id = value; }
        [JsonProperty]
        public string Name { get => name; set => name = value; }
        [JsonProperty("ClassId")]
        public int ClaseId { get => oClase.Id; set => oClase.Id = value; }
        [JsonProperty("Class")]
        public string ClaseName { get => oClase.Name; set => oClase.Name = value; }
        [JsonProperty("NamespaceId")]
        public int PaqueteId { get => oPaquete.Id; set => oPaquete.Id = value; }
        [JsonProperty("Namespace")]
        public string PaqueteName { get => oPaquete.Name; set => oPaquete.Name = value; }
        [JsonProperty]
        internal List<JsonCall> CalledBy { get => calledBy; set => calledBy = value; }
        internal JsonClase OClase { get => oClase; set => oClase = value; }
        internal JsonPaquete OPaquete { get => oPaquete; set => oPaquete = value; }
    }
}
