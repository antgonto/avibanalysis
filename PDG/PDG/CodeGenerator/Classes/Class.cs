using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using CodeGenerator.Methods;
using CodeGenerator;

namespace CodeGenerator.Classes
{
    abstract class Class
    {
        public int identificador { get; set; }
        public string name { get; set; }
        public int complejidadCiclomatica { get; set; }
        public List<Method> metodos { get; set; } = new List<Method>();
        public static int cantidadDeClases { get; set; }
        

        public Class() {
            identificador = cantidadDeClases;
            cantidadDeClases++;

            name = "Class" + identificador.ToString();
        }

        public void WriteClass(string path) {

            // Crear el objeto necesario para escribir archivos.
            string pathClase = Path.Combine(path, (name + ".cs"));
            FileStream fileStream = new FileStream(pathClase, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fileStream);

            // Abrir el namespace.
            writer.WriteLine(Templates.namespaceHead);

            // Abre la definición de la clase, modificadores, nombre, herencias.
            WriteClassHead(writer);

            // Escribir la definición de cada método.
            foreach (var metodo in metodos) {
                metodo.WriteMethod(writer);
            }

            // Cerrar la definición de la clase.
            writer.WriteLine(Templates.simpleClassTail);

            // Cerrar la definición del namespace.
            writer.WriteLine(Templates.namespaceTail);

            // Cerrar el flujo de datos del escritor.
            writer.Close();
            fileStream.Close();
        }

        protected abstract void WriteClassHead(StreamWriter writer);
        
    }
}
