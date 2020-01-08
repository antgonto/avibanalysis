using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace CodeGenerator
{
    public sealed class Configuration
    {
        public static Configuration Instancia { get; private set; } = new Configuration();

        public int CantidadDeClases { get; set; }
        public int MinimaCantidadDeMetodosPorClase { get; set; }
        public int MaximaCantidadDeMetodosPorClase { get; set; }
        public int MinimaLongitudDeCadena { get; set; }
        public int MaximaLongitudDeCadena { get; set; }
        public int CantidadDeCadenasIndependientes { get; set; }
        public int CantidadDeCrucezPorRealizar { get; set; }
        public double PorcentajeDeRealizarCruce1 { get; set; }
        public double PorcentajeDeRealizarCruce2 { get; set; }
        public double PorcentajeDeRecubrimientoConSnippets { get; set; }
        public bool ValidarCompilacionDelCodigo { get; set; }
        public string DirectorioPrincipal { get; set; }
        public string NombreDelProyecto { get; set; }
        public string DirectorioDelCodigo { get; set; }

        private Configuration() {}

        public static void write(string path) {
            // Create a file
            FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fileStream);

            // Serialize the object.
            string jsonConfiguration = JsonConvert.SerializeObject(Instancia, Formatting.Indented);

            // Write and close the file
            writer.WriteLine(jsonConfiguration);
            writer.Close();
            fileStream.Close();
        } 

        public static void load(string path) {
            // Open the file
            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            StreamReader reader = new StreamReader(fileStream);

            // Read the json object.
            string jsonConfiguration = reader.ReadToEnd();

            // Deserialize the json object and set it as the new instance.
            Instancia = JsonConvert.DeserializeObject<Configuration>(jsonConfiguration);

            // Close the file
            reader.Close();
            fileStream.Close();
        }
        
        /* Validar que por ejemplo la cantidad de cadenas independientes no
         * sea posible de realizar según la cantidad límite de métodos
         * establecidos según en otros parámetros.
         * También existe un límite teórico para realizar los crucez.
         * ¿Validar cosas así?
         * Validar que la cantidad de métodos creados no sea inferior 
         * a la cantidad de métodos a asignar por clase.
         */
        /*
        public string validarConfiguracion() {
            
        }
        */

    }
}
