using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GeneracionDeGrafos.Classes;
using GeneracionDeGrafos.Keywords;
using GeneracionDeGrafos;

namespace GeneracionDeGrafos.Methods
{
    class SimpleMethod : Method
    {

        public SimpleMethod(string accessModifier, string returnType) : base(accessModifier, returnType) { }


        /* Método para invocar un método normal, sin ningún tipo de modificador.
         */
        protected override void WriteInvocation(StreamWriter writer) {
            //Name of class instance
            string instanceName = string.Format(Templates.nameInstaceClass, cantidadInvocaciones);

            //invocation
            writer.WriteLine(Templates.instanceSimpleClass, claseContenedora.name, instanceName);
            writer.WriteLine(Templates.invokeSimpleMethodFromClass, instanceName, name, GetParameters());
        }

        /* Método para escribir la firma de un método normal
         */
        protected override void WriteSignature(StreamWriter writer) {
            writer.WriteLine(Templates.simpleMethodHead, accessModifier, returnType, name, GetParameters());
        }
        
    }
}