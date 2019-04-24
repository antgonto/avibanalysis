using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneracionDeGrafos
{
    class Metodo
    {
        public int identificador { get; set; }
        public List<Metodo> metodosInvocados { get; set; } = new List<Metodo>();
        public static int cantidadDeMetodos { get; set; }

        public Metodo() {
            identificador = cantidadDeMetodos;
            cantidadDeMetodos++;
        }
    }
}