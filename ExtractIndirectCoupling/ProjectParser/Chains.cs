using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    class Chains : ICloneable
    {
        static int IDGeneral = 0;
        int ID = 0;
        List<Metodo> cadenaDeMetodos;
        int cantidadMetodos;

        int pesoLOC;
        int pesoConstant;
        int pesoCYCLO;

        public Chains()
        {
            IDGeneral++;
            ID1 = IDGeneral;
            cadenaDeMetodos = new List<Metodo>();
        }
        public void addEslavon(Metodo metodo)
        {
            cadenaDeMetodos.Add(metodo);
        }
        public void eliminarEslavon()
        {
            cadenaDeMetodos.RemoveAt(cadenaDeMetodos.Count - 1);
        }
        public void imprimirCadena(System.IO.StreamWriter output)
        {
            for (int i = 0; i < cadenaDeMetodos.Count; i++)
            {
                output.Write(" - > "+cadenaDeMetodos[i].Clase + "." + cadenaDeMetodos[i].Nombre + " (C=" + cadenaDeMetodos[i].ComplejidadCiclomatica + ")");
            }
        }

        public Chains Clone()
        {
            Chains x = new Chains();
            foreach (Metodo metodo in this.cadenaDeMetodos)
            {
                x.addEslavon(metodo);
            }
            return x;
        }

        object ICloneable.Clone()
        {
            throw new NotImplementedException();
        }

        internal List<Metodo> CadenaDeMetodos
        {
            get
            {
                return cadenaDeMetodos;
            }

            set
            {
                cadenaDeMetodos = value;
            }
        }

        public int ID1
        {
            get
            {
                return ID;
            }

            set
            {
                ID = value;
            }
        }

       

        public int PesoLOC
        {
            get
            {
                return pesoLOC;
            }

            set
            {
                pesoLOC = value;
            }
        }

        public int PesoConstant
        {
            get
            {
                return pesoConstant;
            }

            set
            {
                pesoConstant = value;
            }
        }

        public int PesoCYCLO
        {
            get
            {
                return pesoCYCLO;
            }

            set
            {
                pesoCYCLO = value;
            }
        }
    }
}
