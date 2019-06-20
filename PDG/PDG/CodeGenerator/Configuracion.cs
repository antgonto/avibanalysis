using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CodeGenerator
{
    public sealed class Configuracion
    {
        private static readonly Configuracion _instancia = new Configuracion();

        public int CantidadDeClases { get; set; }
        public int MinimaCantidadDeMetodosPorClase { get; set; }
        public int MaximaCantidadDeMetodosPorClase { get; set; }
        public int MinimaLongitudDeCadena { get; set; }
        public int MaximaLongitudDeCadena { get; set; }
        public int CantidadDeCadenasIndependientes { get; set; }
        public int CantidadDeCrucezPorRealizar { get; set; }
        public double PorcentajeDeRealizarCruce1 { get; set; }
        public double PorcentajeDeRealizarCruce2 { get; set; }
        public bool ValidarCompilacionDelCodigo { get; set; }
        public string DirectorioPrincipal { get; set; }
        public string NombreDelProyecto { get; set; }
        public string DirectorioDelCodigo { get; set; }

        private Configuracion() {
            CantidadDeClases = 5;
            MinimaCantidadDeMetodosPorClase = 3;
            MaximaCantidadDeMetodosPorClase = 6;
            MinimaLongitudDeCadena = 5;
            MaximaLongitudDeCadena = 7;
            CantidadDeCadenasIndependientes = 5;
            CantidadDeCrucezPorRealizar = 10;
            PorcentajeDeRealizarCruce1 = .99;
            PorcentajeDeRealizarCruce2 = .01;
            ValidarCompilacionDelCodigo = true;
            DirectorioPrincipal = "C:\\ProyectoAVIB\\Pruebas";
            NombreDelProyecto = "FirstTest";

            DirectorioDelCodigo = Path.Combine(DirectorioPrincipal, NombreDelProyecto);
        }

        public static Configuracion Instancia {
            get { return _instancia; }
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
