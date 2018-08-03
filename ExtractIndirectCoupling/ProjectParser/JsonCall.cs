using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    [JsonObject(MemberSerialization.OptIn, Description = "Method")]
    class JsonCall : IEquatable<JsonCall>, ICloneable
    {
        int id;
        string name;
        int classId;
        string className;
        int workspaceId;
        string workspaceName;
        JsonMethod method;

        public JsonCall(int id, string name, int classId, string className, int workspaceId, string workspaceName, JsonMethod method)
        {
            this.id = id;
            this.name = name;
            this.classId = classId;
            this.className = className;
            this.workspaceId = workspaceId;
            this.workspaceName = workspaceName;
            this.method = method;
        }

        [JsonProperty]
        public int Id { get => id; set => id = value; }
        [JsonProperty]
        public string Name { get => name; set => name = value; }
        [JsonProperty("ClassId")]
        public int ClassId { get => classId; set => classId = value; }
        [JsonProperty("Class")]
        public string ClassName { get => className; set => className = value; }
        [JsonProperty("NamespaceId")]
        public int WorkspaceId { get => workspaceId; set => workspaceId = value; }
        [JsonProperty("Namespace")]
        public string WorkspaceName { get => workspaceName; set => workspaceName = value; }
        public JsonMethod Metodo { get => method; set => method = value; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as JsonCall);
        }

        public bool Equals(JsonCall other)
        {
            return other != null &&
                   id == other.id &&
                   name == other.name &&
                   classId == other.classId &&
                   className == other.className &&
                   workspaceId == other.workspaceId &&
                   workspaceName == other.workspaceName;
        }

        public override int GetHashCode()
        {
            var hashCode = 652166652;
            hashCode = hashCode * -1521134295 + id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            hashCode = hashCode * -1521134295 + classId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(className);
            hashCode = hashCode * -1521134295 + workspaceId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(workspaceName);
            return hashCode;
        }
    }
}
