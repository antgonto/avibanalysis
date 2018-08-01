using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    class Eslavon
    {
        Chains cadena;
        int indice;
        int ID;
        int locExport;
        int cycloExport;
        int constantExport;
        int locImport;
        int cycloImport;
        int constantImport;

        public Eslavon(Chains cadena, int indice, int ID)
        {
            this.cadena = cadena;
            this.indice = indice;
            this.ID = ID;
        }

        public int Indice
        {
            get
            {
                return indice;
            }

            set
            {
                indice = value;
            }
        }

        internal Chains Cadena
        {
            get
            {
                return cadena;
            }

            set
            {
                cadena = value;
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

        public int LocExport
        {
            get
            {
                return locExport;
            }

            set
            {
                locExport = value;
            }
        }

        public int CycloExport
        {
            get
            {
                return cycloExport;
            }

            set
            {
                cycloExport = value;
            }
        }

        public int ConstantExport
        {
            get
            {
                return constantExport;
            }

            set
            {
                constantExport = value;
            }
        }

        public int LocImport
        {
            get
            {
                return locImport;
            }

            set
            {
                locImport = value;
            }
        }

        public int CycloImport
        {
            get
            {
                return cycloImport;
            }

            set
            {
                cycloImport = value;
            }
        }

        public int ConstantImport
        {
            get
            {
                return constantImport;
            }

            set
            {
                constantImport = value;
            }
        }
    }
}
