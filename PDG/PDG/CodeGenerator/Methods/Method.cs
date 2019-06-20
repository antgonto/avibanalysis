using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenerator.Classes;
using CodeGenerator.Keywords;
using CodeGenerator;
using CodeGenerator.Parameters;
using System.IO;

namespace CodeGenerator.Methods
{
    abstract class Method
    {
        public Class claseContenedora { get; set; }
        public int identificador { get; set; }
        public string accessModifier { get; set; }
        public string returnType { get; set; }
        public string name { get; set; }


        public List<Method> metodosInvocados { get; set; } = new List<Method>();
        public List<Parameter> parametros { get; set; } = new List<Parameter>();

        public static int cantidadDeMetodos { get; set; }
        private static int _cantidadInvocaciones;
        public static int cantidadInvocaciones {
            get { return _cantidadInvocaciones++; }
        }

        public Method(string accessModifier, string returnType) {
            this.accessModifier = accessModifier;
            this.returnType = returnType;

            identificador = cantidadDeMetodos;
            cantidadDeMetodos++;

            name = "method" + identificador.ToString();
        }

        public Method(string accessModifier, string returnType, string name) {
            this.accessModifier = accessModifier;
            this.returnType = returnType;
            this.name = name;

            identificador = cantidadDeMetodos;
            cantidadDeMetodos++;
        }

        //TODO,

        public void WriteMethod(StreamWriter writer) {

            //Head of the method, the signature, method access and parameters.
            WriteSignature(writer);

            //TODO
            //Body of the method, the invocations to another methods.
            foreach (SimpleMethod metodo in metodosInvocados) //{
                // Si el método a invocar pertenence a la misma clase que la clase contenedora
                // este método, su llamada debería ser diferente.
                //if (metodo.claseContenedora == claseContenedora)
                //    metodo.WriteInnerInvocation(writer);
                //else
                //metodo.WriteOuterInvocation();
                metodo.WriteInvocation(writer);
            //}

            //Close de the method.
            writer.WriteLine(Templates.simpleMethodTail);
            writer.WriteLine();

        }

        protected abstract void WriteSignature(StreamWriter writer);

        protected abstract void WriteInvocation(StreamWriter writer);

        //TODO
        //protected abstract void WriteInnerInvocation(StreamWriter writer);
        //protected abstract void WriteOuterInvocation(StreamWriter writer);

        protected string GetParameters() {
            return Parameter.formatParameters(parametros);
        }
    }

}
