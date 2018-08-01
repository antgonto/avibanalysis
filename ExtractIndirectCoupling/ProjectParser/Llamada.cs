using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    class Llamada
    {
        string clase;
        string metodo_atributo;
        SymbolKind tipo;

        public Llamada(string clase, string metodo_atributo, SymbolKind tipo)
        {
            this.clase = clase;
            this.metodo_atributo = metodo_atributo;
            this.Tipo = tipo;
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

        public string Metodo_atributo
        {
            get
            {
                return Metodo_atributo1;
            }

            set
            {
                Metodo_atributo1 = value;
            }
        }

        public string Metodo_atributo1
        {
            get
            {
                return metodo_atributo;
            }

            set
            {
                metodo_atributo = value;
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
    }
}
