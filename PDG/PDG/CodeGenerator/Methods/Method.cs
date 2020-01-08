using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenerator.Classes;
using CodeGenerator.Keywords;
using CodeGenerator;
using CodeGenerator.Parameters;
using CodeGenerator.Snippets;
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
        public List<Snippet> snippets { get; set; } = new List<Snippet>();
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

        public void WriteMethod(StreamWriter writer) {

            //Write the head of the method, the signature, method access and parameters.
            writer.WriteLine(GetSignature());

            //Control the used methods.
            int usedMethods = 0;

            //Use all the snippets for this method first.
            foreach (Snippet snippet in snippets) {
                //Get the required method calls o build the snippet.
                int requiredMethods = snippet.RequiredMethods;
                string[] invokedMethods = new string[requiredMethods];

                //Buil the invocation for each required method call.
                for (int i = 0; i < requiredMethods; i++) {
                    //Get the method to use in the snippet.
                    Method method = metodosInvocados[usedMethods];
                    //Verify if the method is in the same class where the method that calls it is.
                    if (method.claseContenedora == claseContenedora)
                        invokedMethods[i] = method.GetInnerInvocation(snippet.IdentationLevels[i]);
                    else
                        invokedMethods[i] = method.GetOuterInvocation(snippet.IdentationLevels[i]);

                    //Aument the used methods for the snippets.
                    usedMethods++;
                }

                //Write the snippet with the required methods.
                snippet.WriteSnippet(writer, invokedMethods);
            }

            //Write simple calls for the rest of the methods to invoke.
            for (int i = usedMethods; i < metodosInvocados.Count; i++) {
                //Get the method to use in the snippet.
                Method method = metodosInvocados[i];
                //Verify if the method is in the same class where the method that calls it is.
                if (method.claseContenedora == claseContenedora)
                    writer.WriteLine(method.GetInnerInvocation());
                else
                    writer.WriteLine(method.GetOuterInvocation());
            }

            //Close de the method.
            writer.WriteLine(Templates.simpleMethodTail);
            writer.WriteLine();
        }

        protected abstract string GetSignature();

        protected abstract string GetInnerInvocation(int indentationLevel = 3);

        protected abstract string GetOuterInvocation(int indentationLevel = 3);

        protected string GetParameters() {
            return Parameter.formatParameters(parametros);
        }
    }

}
