using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        string name, fullname, filepath;
        JsonNamespace onamespace;
        List<JsonAttribute> attributes = new List<JsonAttribute>();
        List<JsonMethod> methods = new List<JsonMethod>();

        JsonClass parent = null;
        List<JsonClass> interfaces = new List<JsonClass>();
        List<JsonClass> children = new List<JsonClass>();
        List<String> usings = new List<String>();
        bool isInterface = false;
        List<String> types= new List<string>();

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

        public dynamic JSerialize()
        {
            dynamic c = new JObject();
            c.Name = Name;
            c.Fullname = Fullname;
            c.Namespace = FullNamespaceName;
            c.NOM = 0;
            c.LOC = 0;
            c.CYC = 0;
            c.FSUMKON = 0;
            c.FSUMLOC = 0;
            c.FSUMCYC = 0;
            c.RSUMKON = 0;
            c.RSUMLOC = 0;
            c.RSUMCYC = 0;
            c.Methods = new JArray();
            foreach (JsonMethod m in Methods)
            {
                if (m.IsCollapsed == false)
                {
                    c.Methods.Add(m.JSerialize());
                    c.NOM += m.Kon;
                    c.LOC += m.Loc;
                    c.CYC += m.Cyc;
                    c.FSUMKON += m.Kon_metrics.Bsum;
                    c.FSUMLOC += m.Loc_metrics.Bsum;
                    c.FSUMCYC += m.Cyc_metrics.Bsum;
                    c.RSUMKON += m.Kon_metrics.Fsum;
                    c.RSUMLOC += m.Loc_metrics.Fsum;
                    c.RSUMCYC += m.Cyc_metrics.Fsum;
                }
            }
            return c;
        }

        public JsonNamespace GetNamespace { get => onamespace; set => onamespace = value; }
        public JsonClass Parent { get => parent; set => parent = value; }
        public List<JsonClass> Children { get => children; set => children = value; }
        public bool IsInterface { get => isInterface; set => isInterface = value; }
        public List<String> Types { get => types; set => types = value; }
        public List<JsonClass> Interfaces { get => interfaces; set => interfaces = value; }
        public List<string> Usings { get => usings; set => usings = value; }
        public string Filepath { get => filepath; set => filepath = value; }

        public static JsonClass GetClass(string name, string onamespace, bool isInterface, string filepath)
        {
            JsonClass oclass;

            if (!classes.TryGetValue(onamespace + "." + name, out oclass))
            {
                JsonNamespace p = JsonNamespace.GetNamespace(onamespace);
                oclass = new JsonClass(JsonProject.Nextid++, name, onamespace + "." + name, p);
                oclass.IsInterface = isInterface;
                oclass.Filepath = filepath;
                classes.Add(onamespace + "." + name, oclass);
                p.Classes.Add(oclass);
            }

            return oclass;
        }

        public static JsonClass FindClass(string name, string onamespace)
        {
            JsonClass oclass;

            String ns = (onamespace.Length == 0 ? "" : onamespace + ".");

            if (!classes.TryGetValue(ns + name, out oclass))
            {
                return null;
            }

            return oclass;
        }

        public static void ResolveHierarchy()
        {
            foreach (KeyValuePair<string, JsonClass> entry in Classes)
            {
                JsonClass c = entry.Value;
                
                if (c.Types.Count > 0)
                {
                    JsonClass p = FindClass(c.Types[0], c.FullNamespaceName);
                    if (p != null && p.Id != c.Id)
                    {
                        if (p.IsInterface)
                            c.Interfaces.Add(p);
                        else
                            c.Parent = p;
                        p.Children.Add(c);
                    }

                    for (int i = 1; i < c.Types.Count; i++)
                    {
                        p = FindClass(c.Types[i], c.FullNamespaceName);
                        if (p != null && p.Id != c.Id)
                        {
                            c.Interfaces.Add(p);
                            p.Children.Add(c);
                        }
                    }
                }
            }
        }

        public static void PropagateCall(JsonMethod caller, JsonMethod callee, JsonCall callerEntry, JsonClass calleeClass)
        {
            foreach (JsonClass c in calleeClass.Children)
            {
                JsonMethod m = JsonMethod.FindMethod(callee.Name, c.Name, c.FullNamespaceName);
                if (m != null)
                {
                    if (caller.Id != m.Id)
                    {
                        JsonCall calleeEntry = new JsonCall(m.Id, m.Name, m.ClassId, m.ClassName, m.NamespaceId, m.NamespaceName, m);
                        if (!m.CalledBy.Contains(callerEntry)) m.CalledBy.Add(callerEntry);
                        if (!caller.Calls.Contains(calleeEntry)) caller.Calls.Add(calleeEntry);
                    }
                }

                if (c.Children.Count > 0) PropagateCall(caller, callee, callerEntry, c);
            }
        }

    }
}
