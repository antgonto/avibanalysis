using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    [JsonObject(MemberSerialization.OptIn,Description = "Method")]
    class JsonMethod
    {
        long id;
        string name;
        string fullname;
        JsonClass oclass;
        JsonNamespace onamespace;
        static Dictionary<string, JsonMethod> methods = new Dictionary<string, JsonMethod>();
        HashSet<JsonCall> calls = new HashSet<JsonCall>();
        HashSet<JsonCall> calledBy = new HashSet<JsonCall>();

        // Send output to a file
        //static System.IO.StreamWriter output = new System.IO.StreamWriter(@"C:\Users\jnavas\source\repos\avibanalysis\ExtractIndirectCoupling\some_chains.txt");
        //static int nprint = 10000;

        bool dfsFlag = false;


        public JsonMethod(long id, string name, JsonClass clase, JsonNamespace @namespace)
        {
            this.id = id;
            this.name = name;
            this.fullname = clase.Fullname + "." + name;
            this.oclass = clase;
            this.onamespace = @namespace;
        }

        public static JsonMethod GetMethod(string name, string oclass, string onamespace)
        {
            JsonMethod method;

            if (!methods.TryGetValue(onamespace + "." + oclass + "." + name, out method))
            {
                JsonClass c = ProjectParser.JsonClass.GetClass(oclass, onamespace);
                method = new JsonMethod(JsonProject.Nextid++, name, c, JsonNamespace.GetNamespace(onamespace));
                methods.Add(onamespace + "." + oclass + "." + name, method);
                c.Methods.Add(method);
            }

            return method;
        }

        public static void CountChainsUsingDFS(JsonProject project)
        {
            ulong count = 0;
            ulong avgdepth = 0;

            List<JsonMethod> list = new List<JsonMethod>();

            int method_count = methods.Count;
            int non_called_count = 0;

            foreach (KeyValuePair<string, JsonMethod> m in methods)
            {
                if (m.Value.CalledBy.Count == 0)
                {
                    list.Add(m.Value);
                    non_called_count++;
                }
            }

            foreach (JsonMethod m in list)
            {
                CountDFS(m, 1, ref avgdepth, ref count, project);
                non_called_count--;
            }

            if (count > 0) avgdepth = avgdepth / count;
        }

        static void CountDFS(JsonMethod m, ulong depth, ref ulong avgdepth, ref ulong count, JsonProject project)
        {
            m.DfsFlag = true;
            if (m.Calls.Count == 0)
            {
                if (depth > 2)
                {
                    avgdepth += depth;
                    count++;
                }
            }
            else
            {
                foreach (JsonCall c in m.Calls)
                {
                    if (c.Metodo.DfsFlag == false)
                    {
                        CountDFS(c.Metodo, depth+1, ref avgdepth, ref count, project);
                    }
                }
            }
            m.DfsFlag = false;
        }

        public static void CollectChainsUsingDFS(JsonProject project)
        {
            foreach (KeyValuePair<string, JsonMethod> m in methods)
            {
                if (m.Value.CalledBy.Count == 0)
                {
                    CollectDFS(m.Value, new List<JsonCall>(), project);
                }
            }

            /*
            foreach (List<JsonCall> ch in project.Chains)
            {
                string chain = ">> ";
                foreach (JsonCall c in ch)
                {
                    chain = chain + c.ClassName + "." + c.Name + " > ";
                }

                output.WriteLine(chain);
            }

            output.Flush();
            */
        }

        static void CollectDFS(JsonMethod m, List<JsonCall> list, JsonProject project)
        {
            //if (nprint == 0) return;

            m.DfsFlag = true;
            list.Add(new JsonCall(m.Id, m.Name, m.ClassId, m.ClassName, m.NamespaceId, m.NamespaceName, m));
            if (m.Calls.Count == 0)
            {
                if (list.Count > 2)
                {
                    List<JsonCall> l = new List<JsonCall>(list);
                    project.Chains.Add(l);
                    //nprint--;
                }
            }
            else
            {
                foreach (JsonCall c in m.Calls)
                {
                    if (c.Metodo.DfsFlag == false)
                    {
                        CollectDFS(c.Metodo, list, project);
                    }
                }
            }
            list.RemoveAt(list.Count - 1);
            m.DfsFlag = false;
        }

        [JsonProperty]
        public long Id { get => id; set => id = value; }
        [JsonProperty]
        public string Name { get => name; set => name = value; }
        [JsonProperty]
        public string Fullname { get => fullname; set => fullname = value; }
        [JsonProperty("ClassId")]
        public long ClassId { get => oclass.Id; set => oclass.Id = value; }
        [JsonProperty("Class")]
        public string ClassName { get => oclass.Name; set => oclass.Name = value; }
        [JsonProperty("FullClassname")]
        public string FullClassname { get => oclass.Fullname; set => oclass.Fullname = value; }
        [JsonProperty("NamespaceId")]
        public long NamespaceId { get => onamespace.Id; set => onamespace.Id = value; }
        [JsonProperty("Namespace")]
        public string NamespaceName { get => onamespace.Name; set => onamespace.Name = value; }
        [JsonProperty("FullNamespace")]
        public string FullNamespaceName { get => onamespace.Fullname; set => onamespace.Fullname = value; }
        [JsonProperty]
        public HashSet<JsonCall> Calls { get => calls; set => calls = value; }
        [JsonProperty]
        internal HashSet<JsonCall> CalledBy { get => calledBy; set => calledBy = value; }
        public JsonClass GetClass { get => oclass; set => oclass = value; }
        public JsonNamespace GetNamespace { get => onamespace; set => onamespace = value; }
        public bool DfsFlag { get => dfsFlag; set => dfsFlag = value; }
        public static Dictionary<string, JsonMethod> Methods { get => methods; set => methods = value; }
    }
}
