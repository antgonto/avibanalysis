using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CodeGenerator.Classes;
using CodeGenerator.Keywords;
using CodeGenerator;

namespace CodeGenerator.Methods
{
    class SimpleMethod : Method
    {

        public SimpleMethod(string accessModifier, string returnType) : base(accessModifier, returnType) { }


        /* Método para invocar un método normal, sin ningún tipo de modificador, desde una clase externa.
         */
        protected override string GetOuterInvocation(int indentationLevel = 3) {
            string indentation = new string('\t', indentationLevel);

            //Name of class instance
            string instanceName = string.Format(Templates.nameInstaceClass, cantidadInvocaciones);

            //Invocation
            string invocation = "";
            invocation += string.Format(indentation + Templates.instanceSimpleClass, claseContenedora.Name, instanceName);
            invocation += System.Environment.NewLine;
            invocation += string.Format(indentation + Templates.invokeSimpleMethodFromOuterClass, instanceName, name, GetParameters());

            return invocation;
        }

        /* Método para invocar un método normal, sin ningún tipo de modificador, desde la misma clase contenedora.
         */
        protected override string GetInnerInvocation(int indentationLevel = 3) {
            string indentation = new string('\t', indentationLevel);

            //Invocation
            string invocation = "";
            invocation += string.Format(indentation + Templates.invokeSimpleMethodFromInnerClass, name, GetParameters());

            return invocation;
        }

        /* Método para escribir la firma de un método normal
         */
        protected override string GetSignature() {
            //Signature
            string signature = string.Format(Templates.simpleMethodHead, accessModifier, returnType, name, GetParameters());
            return signature;
        }
        
    }
}