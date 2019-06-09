using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneracionDeGrafos
{
    static class Utilidades
    {
        /* Utilizar una única instancia de Random para evitar generar randoms con el mismo timestamp */
        public static Random Random { get; set; } = new Random();

        /* Shuffle a list. Método Fisher-Yates */
        public static void Shuffle<T>(this IList<T> list) {
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = Random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

    }
}
