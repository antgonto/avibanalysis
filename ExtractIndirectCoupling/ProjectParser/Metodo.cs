using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    class Metodo
    {
        static int cantidadMetodos = 0;
        static int totalDeLOC = 0;
        static int currentId = 0;

        int id;
        string clase;
        string nombre;
        List<Llamada> listaLlamadas;
        List<Eslavon> listaEslavones;
        bool esLlamador;
        bool esLlamado;
        bool visitado;
        int cantidadLineasMetodo;
        int constanteK;
        int complejidadCiclomatica;

        bool banderaMinimoMaximoExport = false;
        bool banderaMinimoMaximoImport = false;

        int exportLOCSum = 0;
        float exportLOCAvg = 0;
        int exportLOCMin = 0;
        int exportLOCMax = 0;

        int exportCLYCLOSum = 0;
        float exportCLYCLOAvg = 0;
        int exportCLYCLOMin = 0;
        int exportCLYCLOMax = 0;

        int exportConstantSum = 0;
        float exportConstantAvg = 0;
        int exportConstantMin = 0;
        int exportConstantMax = 0;
        //------------------------------------------------------
        int importLOCSum = 0;
        float importLOCAvg = 0;
        int importLOCMin = 0;
        int importLOCMax = 0;

        int importCLYCLOSum = 0;
        float importCLYCLOAvg = 0;
        int importCLYCLOMin = 0;
        int importCLYCLOMax = 0;

        int importConstantSum = 0;
        float importConstantAvg = 0;
        int importConstantMin = 0;
        int importConstantMax = 0;


        SymbolKind tipo;
        

        public Metodo(string clase, string nombre, bool esLlamador, bool esLlamado, bool visitado, SymbolKind tipo, int cantidadLineas, int constante, int complejidad)
        {
            if (tipo == SymbolKind.Method)
            {
                CantidadMetodos++;
                TotalDeLOC += cantidadLineas;
                id = currentId++;
            }
            this.clase = clase;
            this.nombre = nombre;
            this.listaLlamadas = new List<Llamada>();
            this.esLlamador = esLlamador;
            this.esLlamado = esLlamado;
            this.visitado = visitado;
            this.Tipo = tipo;
            this.listaEslavones = new List<Eslavon>();
            cantidadLineasMetodo = cantidadLineas;
            ConstanteK = constante;
            ComplejidadCiclomatica = complejidad;
        }
        public void imprimirEslavones(System.IO.StreamWriter output)
        {
            for (int i = 0; i < listaEslavones.Count; i++)
            {
                listaEslavones[i].Cadena.imprimirCadena(output);
                output.WriteLine();
            }
        }

        public string Clase
        {
            get
            {
                return clase;
            }

            set
            {
                clase = value;
            }
        }

        public string Nombre
        {
            get
            {
                return nombre;
            }

            set
            {
                nombre = value;
            }
        }

        internal List<Llamada> ListaLlamadas
        {
            get
            {
                return listaLlamadas;
            }

            set
            {
                listaLlamadas = value;
            }
        }

        public bool EsLlamador
        {
            get
            {
                return esLlamador;
            }

            set
            {
                esLlamador = value;
            }
        }

        public bool EsLlamado
        {
            get
            {
                return esLlamado;
            }

            set
            {
                esLlamado = value;
            }
        }

        public bool Visitado
        {
            get
            {
                return visitado;
            }

            set
            {
                visitado = value;
            }
        }

        public SymbolKind Tipo
        {
            get
            {
                return tipo;
            }

            set
            {
                tipo = value;
            }
        }

        internal List<Eslavon> ListaEslavones
        {
            get
            {
                return listaEslavones;
            }

            set
            {
                listaEslavones = value;
            }
        }

        public int CantidadLineasMetodo
        {
            get
            {
                return cantidadLineasMetodo;
            }

            set
            {
                cantidadLineasMetodo = value;
            }
        }

        public int ComplejidadCiclomatica
        {
            get
            {
                return complejidadCiclomatica;
            }

            set
            {
                complejidadCiclomatica = value;
            }
        }

        public int ConstanteK
        {
            get
            {
                return constanteK;
            }

            set
            {
                constanteK = value;
            }
        }

        public int ExportLOCSum
        {
            get
            {
                return exportLOCSum;
            }

            set
            {
                exportLOCSum = value;
            }
        }

        public float ExportLOCAvg
        {
            get
            {
                return exportLOCAvg;
            }

            set
            {
                exportLOCAvg = value;
            }
        }

        public int ExportLOCMin
        {
            get
            {
                return exportLOCMin;
            }

            set
            {
                exportLOCMin = value;
            }
        }

        public int ExportLOCMax
        {
            get
            {
                return exportLOCMax;
            }

            set
            {
                exportLOCMax = value;
            }
        }

        public int ExportCLYCLOSum
        {
            get
            {
                return exportCLYCLOSum;
            }

            set
            {
                exportCLYCLOSum = value;
            }
        }

        public float ExportCLYCLOAvg
        {
            get
            {
                return exportCLYCLOAvg;
            }

            set
            {
                exportCLYCLOAvg = value;
            }
        }

        public int ExportCLYCLOMin
        {
            get
            {
                return exportCLYCLOMin;
            }

            set
            {
                exportCLYCLOMin = value;
            }
        }

        public int ExportCLYCLOMax
        {
            get
            {
                return exportCLYCLOMax;
            }

            set
            {
                exportCLYCLOMax = value;
            }
        }

        public int ExportConstantSum
        {
            get
            {
                return exportConstantSum;
            }

            set
            {
                exportConstantSum = value;
            }
        }

        public float ExportConstantAvg
        {
            get
            {
                return exportConstantAvg;
            }

            set
            {
                exportConstantAvg = value;
            }
        }

        public int ExportConstantMin
        {
            get
            {
                return exportConstantMin;
            }

            set
            {
                exportConstantMin = value;
            }
        }

        public int ExportConstantMax
        {
            get
            {
                return exportConstantMax;
            }

            set
            {
                exportConstantMax = value;
            }
        }


        public int ImportLOCSum
        {
            get
            {
                return importLOCSum;
            }

            set
            {
                importLOCSum = value;
            }
        }

        public float ImportLOCAvg
        {
            get
            {
                return importLOCAvg;
            }

            set
            {
                importLOCAvg = value;
            }
        }

        public int ImportLOCMin
        {
            get
            {
                return importLOCMin;
            }

            set
            {
                importLOCMin = value;
            }
        }

        public int ImportLOCMax
        {
            get
            {
                return importLOCMax;
            }

            set
            {
                importLOCMax = value;
            }
        }

        public int ImportCLYCLOSum
        {
            get
            {
                return importCLYCLOSum;
            }

            set
            {
                importCLYCLOSum = value;
            }
        }

        public float ImportCLYCLOAvg
        {
            get
            {
                return importCLYCLOAvg;
            }

            set
            {
                importCLYCLOAvg = value;
            }
        }

        public int ImportCLYCLOMin
        {
            get
            {
                return importCLYCLOMin;
            }

            set
            {
                importCLYCLOMin = value;
            }
        }

        public int ImportCLYCLOMax
        {
            get
            {
                return importCLYCLOMax;
            }

            set
            {
                importCLYCLOMax = value;
            }
        }

        public int ImportConstantSum
        {
            get
            {
                return importConstantSum;
            }

            set
            {
                importConstantSum = value;
            }
        }

        public float ImportConstantAvg
        {
            get
            {
                return importConstantAvg;
            }

            set
            {
                importConstantAvg = value;
            }
        }

        public int ImportConstantMin
        {
            get
            {
                return importConstantMin;
            }

            set
            {
                importConstantMin = value;
            }
        }

        public int ImportConstantMax
        {
            get
            {
                return importConstantMax;
            }

            set
            {
                importConstantMax = value;
            }
        }

        public bool BanderaMinimoMaximoExport
        {
            get
            {
                return banderaMinimoMaximoExport;
            }

            set
            {
                banderaMinimoMaximoExport = value;
            }
        }

        public bool BanderaMinimoMaximoImport
        {
            get
            {
                return banderaMinimoMaximoImport;
            }

            set
            {
                banderaMinimoMaximoImport = value;
            }
        }

        public static int CantidadMetodos
        {
            get
            {
                return cantidadMetodos;
            }

            set
            {
                cantidadMetodos = value;
            }
        }


        public static int TotalDeLOC
        {
            get
            {
                return totalDeLOC;
            }

            set
            {
                totalDeLOC = value;
            }
        }
    }
}
