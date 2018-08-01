using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    [JsonObject(MemberSerialization.OptIn,Description = "Metodo")]
    class JsonMetodo
    {
        static int currentId = 0;

        int id;
        string name;
        JsonClase oClase;
        JsonPaquete oPaquete;
        static Dictionary<string, JsonMetodo> metodos = new Dictionary<string, JsonMetodo>();
        HashSet<JsonCall> calls = new HashSet<JsonCall>();
        HashSet<JsonCall> calledBy = new HashSet<JsonCall>();

        bool dfsFlag = false;


        public JsonMetodo(int id, string name, JsonClase clase, JsonPaquete paquete)
        {
            this.id = id;
            this.name = name;
            this.oClase = clase;
            this.oPaquete = paquete;
        }

        public static JsonMetodo GetMetodo(string name, string clase, string paquete)
        {
            JsonMetodo oMetodo;

            if (!metodos.TryGetValue(paquete + "." + clase + "." + name, out oMetodo))
            {
                JsonClase c = ProjectParser.JsonClase.GetClase(clase, paquete);
                oMetodo = new JsonMetodo(currentId++, name, c, JsonPaquete.GetPaquete(paquete));
                metodos.Add(paquete + "." + clase + "." + name, oMetodo);
                c.Metodos.Add(oMetodo);
            }

            return oMetodo;
        }

        public static void RunDFS(JsonProject project)
        {
            foreach (KeyValuePair<string, JsonMetodo> m in metodos)
            {
                if (m.Value.CalledBy.Count == 0)
                {
                    FindChainsUsingDFS(m.Value, new List<JsonCall>(), project);
                }
            }
        }

        static void FindChainsUsingDFS(JsonMetodo m, List<JsonCall> list, JsonProject project)
        {
            m.DfsFlag = true;
            list.Add(new JsonCall(m.Id, m.Name, m.ClaseId, m.ClaseName, m.PaqueteId, m.PaqueteName, m));
            if (m.Calls.Count == 0)
            {
                if (list.Count > 2)
                {
                    List<JsonCall> l = new List<JsonCall>(list);
                    project.Chains.Add(l);
                }
            }
            else
            {
                foreach (JsonCall c in m.Calls)
                {
                    if (c.Metodo.DfsFlag == false)
                    {
                        FindChainsUsingDFS(c.Metodo, list, project);
                    }
                }
            }
            list.RemoveAt(list.Count - 1);
            m.DfsFlag = false;
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
        internal HashSet<JsonCall> Calls { get => calls; set => calls = value; }
        [JsonProperty]
        internal HashSet<JsonCall> CalledBy { get => calledBy; set => calledBy = value; }
        public JsonClase GetClase { get => oClase; set => oClase = value; }
        public JsonPaquete GetPaquete { get => oPaquete; set => oPaquete = value; }
        public bool DfsFlag { get => dfsFlag; set => dfsFlag = value; }
    }
}
