using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneracionDeGrafos
{
    class Program
    {
        static void Main(string[] args) {

            GrafoDeInvocaciones grafo = new GrafoDeInvocaciones();

            var cadenasBase = grafo.crearCadenasBase();
            grafo.crearCruces(cadenasBase);

            int[,] grafito = grafo.crearMatrizDeAdyacencias(grafo.metodos);

            int Nodos = (int)(Math.Sqrt(grafito.LongLength));

            for(int x = 0; x < Nodos; x++) {
                for (int y = 0; y < Nodos; y++) {
                    Console.Write(grafito[x, y]);
                    Console.Write(' ');
                }
                Console.WriteLine();
            }

            Console.ReadKey();
        }
    }
}
