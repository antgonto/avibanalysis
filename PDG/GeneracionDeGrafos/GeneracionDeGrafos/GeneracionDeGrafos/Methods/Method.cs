using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GeneracionDeGrafos.Classes;
using GeneracionDeGrafos.Keywords;
using GeneracionDeGrafos;
using System.IO;

namespace GeneracionDeGrafos.Methods
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

        public void write(StreamWriter writer) {

            //Head of the method, the signature, method access and parameters.
            writeSignature(writer);

            //Body of the method, the invocations to another methods.
            foreach (SimpleMethod metodo in metodosInvocados)
                metodo.writeInvocation(writer);

            //Close de the method.
            writer.WriteLine(Templates.simpleMethodTail);
            writer.WriteLine();

        }

        protected abstract void writeSignature(StreamWriter writer);

        protected abstract void writeInvocation(StreamWriter writer);

        protected string getParameters() {
            return Parameter.formatParameters(parametros);
        }
    }
}
