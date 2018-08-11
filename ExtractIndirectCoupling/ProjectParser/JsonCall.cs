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
        long id;
        string name;
        long classId;
        string className;
        long namespaceId;
        string namespaceName;
        JsonMethod method;

        public JsonCall(long id, string name, long classId, string className, long namespaceId, string namespaceName, JsonMethod method)
        {
            this.id = id;
            this.name = name;
            this.classId = classId;
            this.className = className;
            this.namespaceId = namespaceId;
            this.namespaceName = namespaceName;
            this.method = method;
        }

        [JsonProperty]
        public long Id { get => id; set => id = value; }
        [JsonProperty]
        public string Name { get => name; set => name = value; }
        [JsonProperty]
        public string Fullname { get => namespaceName + "." + className + "." + name; }
        [JsonProperty("ClassId")]
        public long ClassId { get => classId; set => classId = value; }
        [JsonProperty("Class")]
        public string ClassName { get => className; set => className = value; }
        [JsonProperty]
        public string FullClassname { get => namespaceName + "." + className; }
        [JsonProperty("NamespaceId")]
        public long NamespaceId { get => namespaceId; set => namespaceId = value; }
        [JsonProperty("FullNamespace")]
        public string FullNamespace { get => namespaceName; set => namespaceName = value; }
        public JsonMethod Method { get => method; set => method = value; }

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
                   string.Equals(name, other.name) &&
                   classId == other.classId &&
                   string.Equals(className, other.className) &&
                   namespaceId == other.namespaceId &&
                   string.Equals(namespaceName, other.namespaceName);
        }

        public override int GetHashCode()
        {
            var hashCode = 652166652;
            hashCode = hashCode * -1521134295 + id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            hashCode = hashCode * -1521134295 + classId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(className);
            hashCode = hashCode * -1521134295 + namespaceId.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(namespaceName);
            return hashCode;
        }
    }
}
