using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneracionDeGrafos.Methods
{
    class StaticMethod : Method
    {

        public StaticMethod(string accessModifier, string returnType) : base(accessModifier, returnType) { }

        public StaticMethod(string accessModifier, string returnType, string name) : base(accessModifier, returnType, name) { }

        /* Método para invocar un método estático.
         */
        protected override void writeInvocation(StreamWriter writer) {
            throw new NotImplementedException();
        }

        /* Método para escribir la firma de un método estático.
         */
        protected override void writeSignature(StreamWriter writer) {
            writer.WriteLine(Templates.staticMethodHead, accessModifier, returnType, name, getParameters());
        }
    }
}
