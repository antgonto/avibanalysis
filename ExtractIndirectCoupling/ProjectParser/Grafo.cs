using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    class Grafo
    {
        List<Metodo> metodos;
        List<Chains> listaDeCadenas;
        Chains cadenaMásLarga;
        bool[,] matrizAdyacencia;
        bool[,] matrizAdyacenciaOriginal;
        bool[] visited;
        int max = 99999;
        int length;
        int cantidadMayorMetodosEnCadena;

        internal List<Chains> ListaDeCadenas
        {
            get
            {
                return listaDeCadenas;
            }

            set
            {
                listaDeCadenas = value;
            }
        }

        
public int CantidadMayorMetodosEnCadena
        {
            get
            {
                return cantidadMayorMetodosEnCadena;
            }

            set
            {
                cantidadMayorMetodosEnCadena = value;
            }
        }

        public Grafo(List<Metodo> metodos)
        {
            this.ListaDeCadenas = new List<Chains>();
            this.metodos = metodos;
            length = metodos.Count;
            matrizAdyacencia = new bool[metodos.Count, metodos.Count];
            matrizAdyacenciaOriginal = new bool[metodos.Count, metodos.Count];
            visited = new bool[metodos.Count];
            //Setear valores a matris de distancias
            for (int i = 0; i < metodos.Count; i++)
            {

                foreach (Llamada llamada in metodos[i].ListaLlamadas)
                {
                    int indiceLlamada = metodos.FindIndex(x => (x.Clase == llamada.Clase && x.Nombre ==
                     llamada.Metodo_atributo));
                    if (indiceLlamada != -1)
                    {
                        matrizAdyacencia[i, indiceLlamada] = true;
                        matrizAdyacenciaOriginal[i, indiceLlamada] = true;
                    }
                }
            }

        }
        public void imprimirMatrizAdyacencia()
        {

            Console.WriteLine("**************************************************************MATRIZ ADYACENCIA*****************************************************************");
            string space = "";
            Console.Write(space.PadLeft(12));
            for (int i = 0; i < length; i++)
            {
                Console.Write(metodos.ElementAt(i).Nombre.PadLeft(12));
            }
            Console.WriteLine();
            for (int i = 0; i < length; i++)
            {
                Console.Write(metodos.ElementAt(i).Nombre.PadLeft(12));
                for (int j = 0; j < length; j++)
                {
                    if (matrizAdyacencia[i, j])
                    {
                        if (matrizAdyacenciaOriginal[i, j])
                        {
                            String r = "AD";
                            Console.Write(r.PadLeft(12));
                        }
                        else
                        {
                            String r = "AI";
                            Console.Write(r.PadLeft(12));
                        }
                    }
                    else
                    {
                        String r = "-";
                        Console.Write(r.PadLeft(12));
                    }
                }
                Console.WriteLine();
            }
        }
        public void recorrerDFS()
        {
            for (int i = 0; i < length; i++)
            {
                if ((!metodos[i].EsLlamado) && (metodos[i].EsLlamador))
                {
                    Chains newChain = new Chains();
                    DFS(i, metodos[i], newChain);
                }
            }

            for (int i = 0; i < metodos.Count; i++)
            {
                if (metodos[i].EsLlamador && !metodos[i].EsLlamado)
                {
                    foreach(Eslavon eslavon in metodos[i].ListaEslavones)
                    {
                        if (CantidadMayorMetodosEnCadena < eslavon.Cadena.CadenaDeMetodos.Count)
                        {
                            CantidadMayorMetodosEnCadena = eslavon.Cadena.CadenaDeMetodos.Count;
                        }
                        listaDeCadenas.Add(eslavon.Cadena);
                    }
                }
            }
        }
        private void DFS(int node,Metodo metodo, Chains cadena)
        {
            visited[node] = true;
       
            int i;
            cadena.addEslavon(metodos[node]);
            for (i = 0; i < length; i++)
            {
                if (matrizAdyacenciaOriginal[node,i] && !visited[i])
                {

                    DFS(i,metodo,cadena);
                    if (!metodos[i].EsLlamador) 
                    {
                        Chains cadenaNueva = cadena.Clone();
                        for (int j = 0; j < cadenaNueva.CadenaDeMetodos.Count; j++)
                        {
                            cadenaNueva.CadenaDeMetodos[j].ListaEslavones.Add(new Eslavon(cadenaNueva, j , cadenaNueva.ID1));
                        
                        }
                    }
                    cadena.eliminarEslavon();

                }

            }
            visited[node] = false;
            //visited = new bool[metodos.Count];//Reinicia los visitados
        }
        public void WarshallTransitiveClousure()
        {
            for (int k = 0; k < length; k++)
            {
                for (int i = 0; i < length; i++)
                {
                    for (int j = 0; j < length; j++)
                    {
                       matrizAdyacencia[i, j] = matrizAdyacencia[i, j] || (matrizAdyacencia[i, k] && matrizAdyacencia[k, j]);
                    }
                }
            }
        }
        public void imprimirCadenas(System.IO.StreamWriter output)
        {
            for (int i = 0; i < metodos.Count; i++)
            {
                if (metodos[i].EsLlamador && !metodos[i].EsLlamado)
                    metodos[i].imprimirEslavones(output);
            }
        }
       
    }
}
