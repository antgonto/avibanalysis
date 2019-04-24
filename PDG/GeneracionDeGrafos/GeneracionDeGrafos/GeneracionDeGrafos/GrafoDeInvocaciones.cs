using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneracionDeGrafos
{
    class GrafoDeInvocaciones
    {
        public List<Metodo> metodos { get; set; } = new List<Metodo>();

        /* Se crearán tantas cadenas independientes como fue especificado en
         * parámetros, se almacenarán en una lista, siendo cada entrada de la
         * lista una lista que representa la cadena independiente original.
         */
        public List<List<Metodo>> crearCadenasBase() {
            Configuracion configuracion = Configuracion.Instacia;

            List<List<Metodo>> cadenasBase = new List<List<Metodo>>(configuracion.CantidadDeCadenasIndependientes);

            for (int i = 0; i < configuracion.CantidadDeCadenasIndependientes; i++) {
                int largoDeCadena = Utilidades.Random.Next(configuracion.MinimaLongitudDeCadena, configuracion.MaximaLongitudDeCadena);

                List<Metodo> cadena = new List<Metodo>(largoDeCadena + 1);

                Metodo metodoActual = new Metodo();
                metodos.Add(metodoActual);
                cadena.Add(metodoActual);

                for (int j = 0; j < largoDeCadena; j++) {
                    Metodo nuevoMetodo = new Metodo();
                    metodos.Add(nuevoMetodo);
                    cadena.Add(metodoActual);
                    metodoActual.metodosInvocados.Add(nuevoMetodo);
                    metodoActual = nuevoMetodo;
                }

                cadenasBase.Add(cadena);
            }

            return cadenasBase;
        }

        /* Crea los cruces entre los métodos, los objetos Metodo implicados
         * son actualizados en la lista general de métodos para representar
         * el cruce.
         */
        public void crearCruces(List<List<Metodo>> cadenasBase) {
            Configuracion configuracion = Configuracion.Instacia;

            double probabilidadDeCruce1 = configuracion.PorcentajeDeRealizarCruce1;
            double probabilidadDeCruce2 = configuracion.PorcentajeDeRealizarCruce1 + configuracion.PorcentajeDeRealizarCruce2;

            for (int i = 0; i < configuracion.CantidadDeCrucezPorRealizar; i++) {
                double probabilidad = Utilidades.Random.NextDouble();

                if (probabilidad < probabilidadDeCruce1)
                    realizarCruce1(cadenasBase);
                else if (probabilidad < probabilidadDeCruce2)
                    realizarCruce2(cadenasBase);
            }
        }

        public void realizarCruce1(List<List<Metodo>> cadenasBase) {

            //Obtener la primer cadena.
            int primerCadenaIndex = Utilidades.Random.Next(cadenasBase.Count);
            var primerCadena = cadenasBase[primerCadenaIndex];

            //Obtengo el método de la primer cadena
            int primerMetodoIndex = Utilidades.Random.Next(primerCadena.Count);
            Metodo primerMetodo = primerCadena[primerMetodoIndex];

            //Obtengo la segunda cadena.
            int segundaCadenaIndex = Utilidades.Random.Next(cadenasBase.Count);
            var segundaCadena = cadenasBase[segundaCadenaIndex];

            //El método de la segunda cadena
            //Puede ir desde el indice del primer método al máximo de la segunda cadena.
            //Por lo que, la segunda cadena debe ser más larga que el indice escogido del primer método
            //De no ser así, necesito otras cadenas.
            //También es necesario no escoger la misma cadena
            while (primerMetodoIndex > segundaCadena.Count || primerCadenaIndex == segundaCadenaIndex) {
                //Obtener la primer cadena.
                primerCadenaIndex = Utilidades.Random.Next(cadenasBase.Count);
                primerCadena = cadenasBase[primerCadenaIndex];

                //Obtengo el método de la primer cadena
                primerMetodoIndex = Utilidades.Random.Next(primerCadena.Count);

                //Obtengo la segunda cadena.
                segundaCadenaIndex = Utilidades.Random.Next(cadenasBase.Count);
                segundaCadena = cadenasBase[segundaCadenaIndex];
            }

            //Obtengo el segundo método
            int segundoMetodoIndex = Utilidades.Random.Next(primerMetodoIndex, segundaCadena.Count);
            Metodo segundoMetodo = segundaCadena[segundoMetodoIndex];

            //Realizo el cruce, del primero vamos al segundo.
            primerMetodo.metodosInvocados.Add(segundoMetodo);

        }

        public void realizarCruce2(List<List<Metodo>> cadenasBase) {

        }

        /* Crea una matriz de adyacencia para representar el grago de
         * invocaciones, el índice de la matriz coincide con el identificador
         * del método.
         */
        public int[,] crearMatrizDeAdyacencias(List<Metodo> metodos) {
            int[,] grafo = new int[Metodo.cantidadDeMetodos, Metodo.cantidadDeMetodos];

            for (int i = 0; i < Metodo.cantidadDeMetodos; i++)
                for (int j = 0; j < metodos[i].metodosInvocados.Count; j++)
                    grafo[i, metodos[i].metodosInvocados[j].identificador] = 1;

            return grafo;
        }
    }
}