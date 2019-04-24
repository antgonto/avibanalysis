using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneracionDeGrafos
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

        private Configuracion() {
            CantidadDeClases = 10;
            MinimaCantidadDeMetodosPorClase = 5;
            MaximaCantidadDeMetodosPorClase = 10;
            MinimaLongitudDeCadena = 3;
            MaximaLongitudDeCadena = 6;
            CantidadDeCadenasIndependientes = 4;
            CantidadDeCrucezPorRealizar = 10;
            PorcentajeDeRealizarCruce1 = .99;
            PorcentajeDeRealizarCruce2 = .01;
        }

        public static Configuracion Instacia {
            get { return _instancia; }
        }
        
        /* Validar que por ejemplo la cantidad de cadenas independientes no
         * sea posible de realizar según la cantidad límite de métodos
         * establecidos según en otros parámetros.
         * También existe un límite teórico para realizar los crucez.
         * ¿Validar cosas así?
         */
        /*
        public string validarConfiguracion() {
            
        }
        */

    }
}
