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

            Configuracion configuracion = Configuracion.Instancia;

            GrafoDeInvocaciones grafo = new GrafoDeInvocaciones(configuracion);
            grafo.escribirCodigo();
            grafo.escribirCadenas();

            if (configuracion.ValidarCompilacionDelCodigo) {
                if (compileCode())
                    Console.WriteLine("El código generado fue compilado correctamente, no se encontraron errores.");
                else
                    Console.WriteLine(":(");
            }

            /*
            int[,] grafito = grafo.crearMatrizDeAdyacencias();

            int Nodos = (int)(Math.Sqrt(grafito.LongLength));

            for(int x = 0; x < Nodos; x++) {
                for (int y = 0; y < Nodos; y++) {
                    Console.Write(grafito[x, y]);
                    Console.Write(' ');
                }
                Console.WriteLine();
            }
            */

            Console.ReadKey();
        }

        /* Verificar si los archivos creados compilan
         */
        static public bool compileCode() {

            //Configuración
            Configuracion configuracion = Configuracion.Instancia;

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


            /*
            foreach (string filePath in filePaths) {

                compilerParameters.OutputAssembly = (filePath + ".exe");

                CompilerResults results = provider.CompileAssemblyFromFile(compilerParameters, filePath);

                if (results.Errors.Count > 0) {
                    Console.WriteLine("Error de compilación en el archivo: \"{0}\"", filePath);

                    // Display compilation errors.
                    Console.WriteLine("Errors building {0} into {1}",
                        filePath, results.PathToAssembly);
                    foreach (CompilerError ce in results.Errors) {
                        Console.WriteLine("  {0}", ce.ToString());
                        Console.WriteLine();
                    }

                    canCompile = false;
                }

            }*/
        }
    }
}
