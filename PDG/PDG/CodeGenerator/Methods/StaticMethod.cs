using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator.Methods
{
    class StaticMethod : Method
    {

        public StaticMethod(string accessModifier, string returnType) : base(accessModifier, returnType) { }

        public StaticMethod(string accessModifier, string returnType, string name) : base(accessModifier, returnType, name) { }

        protected override string GetInnerInvocation(int indentationLevel = 3) {
            throw new NotImplementedException();
        }

        protected override string GetOuterInvocation(int indentationLevel = 3) {
            throw new NotImplementedException();
        }

        /* Método para escribir la firma de un método estático.
         */
        protected override string GetSignature() {
            //Signature
            string signature = string.Format(Templates.staticMethodHead, accessModifier, returnType, name, GetParameters());
            return signature;
        }
    }
}
