using Microsoft.CSharp;
using System;
using System.IO;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator
{
    class Program
    {
        static void Main(string[] args) {

            // Temp, hardcoded load configuration.
            Configuration.load("C:\\ProyectoAVIB\\SampleConfiguration.json");
            // Get the instance.
            Configuration configuracion = Configuration.Instancia;

            // Optional, save the configuration after running.
            Configuration.write(Path.Combine(configuracion.DirectorioPrincipal, "Configuration(" + configuracion.NombreDelProyecto + ").json"));

            // Create a graph based in the configuration.
            GrafoDeInvocaciones grafo = new GrafoDeInvocaciones(configuracion);
            // Write the code (all clases).
            grafo.escribirCodigo();
            // Write the chains.
            grafo.WriteChains();

            // Validate the created code, compile it and get the errors if required.
            if (configuracion.ValidarCompilacionDelCodigo) {
                if (compileCode())
                    Console.WriteLine("El código generado fue compilado correctamente, no se encontraron errores.");
                else
                    Console.WriteLine(":(");
            }

            // Wait until the user press a key.
            Console.ReadKey();
        }

        /* Verificar si los archivos creados compilan
         */
        static public bool compileCode() {

            //Configuración
            Configuration configuracion = Configuration.Instancia;

            //Compilador
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");

            //Parámetros para el compilador
            CompilerParameters compilerParameters = new CompilerParameters();
            //Para generar un ejecutable, no una librería, solo estas dos opciones existen.
            compilerParameters.GenerateExecutable = true;
            //En memoria, no necesito el archivo de ensamblaje.
            compilerParameters.GenerateInMemory = true;
            //Directorio para los archivos temporales, borrarlos luego de la compilación.
            compilerParameters.TempFiles = new TempFileCollection(configuracion.DirectorioPrincipal, false);
            //Nombre del ejecutable
            compilerParameters.OutputAssembly = ("TemporalExecutable.exe");

            //Abrir la carpeta del código y obtener todos los archivos de c# en el directorio de código.
            string[] filePaths = Directory.GetFiles(configuracion.DirectorioDelCodigo, "*.cs", SearchOption.TopDirectoryOnly);

            //Bandera de compilación
            bool canCompile = true;

            //Compilar el directorio
            CompilerResults results = provider.CompileAssemblyFromFile(compilerParameters, filePaths);

            //Validar errores
            if (results.Errors.Count > 0) {
                canCompile = false;

                foreach (CompilerError ce in results.Errors) {
                    Console.WriteLine("  {0}", ce.ToString());
                    Console.WriteLine();
                }
            }

            return canCompile;
        }
    }
}
