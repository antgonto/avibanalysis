using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CodeGenerator.Methods;
using CodeGenerator.Classes;
using CodeGenerator.Keywords;
using CodeGenerator.Parameters;
using Newtonsoft.Json;

namespace CodeGenerator
{
    class GrafoDeInvocaciones
    {
        public List<Method> metodos { get; set; } = new List<Method>();
        public List<Class> clases { get; set; } = new List<Class>();
        public List<Method> firstMethods { get; set; }
        List<List<Method>> cadenasBase { get; set; }
        public Configuracion configuracion { get; set; }

        public GrafoDeInvocaciones(Configuracion configuracion) {
            this.configuracion = configuracion;

            // Crear las cadenas base
            cadenasBase = crearCadenasBase();

            // Crear cruces base

            crearCruces(cadenasBase);

            // Distribuir los métodos en clases
            distribuirMetodosPorClase();

            // Obtener la clase principal
            // Obtener los métodos que inician todas las demás cadenas
            firstMethods = obtenerIniciosDeCadena(cadenasBase);
            // Obtener el método main, que invoca todas las cadenas iniciadoras
            StaticMethod mainMethod = getMainMethod(firstMethods);
            // Obtener una clase que contenga el método main
            SimpleClass mainClass = getMainClass(mainMethod);
            // Se almacena la clase principal.
            clases.Add(mainClass);
        }

        /* Obtiene una clase que posee el método principal, dado por parámetro.
         */
        public SimpleClass getMainClass(StaticMethod mainMethod) {

            SimpleClass mainClass = new SimpleClass();
            mainClass.metodos.Add(mainMethod);

            return mainClass;
        }

        /* Crea el método principal, invoca a los métodos que inician todas las cadenas, para crear un
         * punto en común estos métodos son obtenidos por parámetro.
         */
        public StaticMethod getMainMethod(List<Method> metodosInvocados) {

            StaticMethod mainMethod = new StaticMethod(AccessModifier.PUBLIC, Types.VOID, "Main");
            mainMethod.metodosInvocados.AddRange(metodosInvocados);
            mainMethod.parametros.Add(new Parameter(Types.ARRAY_STRING));

            return mainMethod;
        }

        /* Obtener todos los inicios de cadenas, por la estructura de generación,
         * son los primeros métodos de cada cadena base.
         */
        public List<Method> obtenerIniciosDeCadena(List<List<Method>> cadenasBase) {
            List<Method> iniciosDeCadena = new List<Method>();

            foreach (List<Method> cadena in cadenasBase) {
                iniciosDeCadena.Add(cadena[0]);
            }

            return iniciosDeCadena;
        }

        /* Se crearán tantas cadenas independientes como fue especificado en
         * parámetros, se almacenarán en una lista, siendo cada entrada de la
         * lista una lista que representa la cadena independiente original.
         */
        public List<List<Method>> crearCadenasBase() {

            // Lista para almacenar todas las cadenas base.
            List<List<Method>> cadenasBase = new List<List<Method>>(configuracion.CantidadDeCadenasIndependientes);

            // Se crean tantas cadenas base como se especificó en la configuración.
            for (int i = 0; i < configuracion.CantidadDeCadenasIndependientes; i++) {
                // Se obtiene un largo aleatorio dentro del rango especificado.
                int largoDeCadena = Utilidades.Random.Next(configuracion.MinimaLongitudDeCadena, configuracion.MaximaLongitudDeCadena);

                // Nueva lista que representa una cadena.
                List<Method> cadena = new List<Method>(largoDeCadena + 1);

                // Se crea el primer método de la cadena y se almacena en la lista general de métodos y su cadena.
                Method metodoActual = new SimpleMethod(AccessModifier.PUBLIC, Types.VOID);
                metodos.Add(metodoActual);
                cadena.Add(metodoActual);

                // Se agregan los restantes métodos para completar el largo de la cadena.
                for (int j = 0; j < largoDeCadena; j++) {

                    // Se crea un nuevo método y se almacena en la cadena y en la lista general de métodos.
                    Method nuevoMetodo = new SimpleMethod(AccessModifier.PUBLIC, Types.VOID);
                    metodos.Add(nuevoMetodo);
                    cadena.Add(nuevoMetodo);

                    // Se enlaza el método anterior para que invoque el método recién creado.
                    metodoActual.metodosInvocados.Add(nuevoMetodo);
                    metodoActual = nuevoMetodo;
                }

                // Se almacena la cadena general.
                cadenasBase.Add(cadena);
            }

            return cadenasBase;
        }

        /* Crea los cruces entre los métodos, los objetos Metodo implicados
         * son actualizados en la lista general de métodos para representar
         * el cruce.
         */
        public void crearCruces(List<List<Method>> cadenasBase) {

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

        public void realizarCruce1(List<List<Method>> cadenasBase) {

            // Obtener la primer cadena.
            int primerCadenaIndex = Utilidades.Random.Next(cadenasBase.Count);
            var primerCadena = cadenasBase[primerCadenaIndex];

            // Obtengo el método de la primer cadena
            int primerMetodoIndex = Utilidades.Random.Next(primerCadena.Count);
            Method primerMetodo = primerCadena[primerMetodoIndex];

            // Obtengo la segunda cadena.
            int segundaCadenaIndex = Utilidades.Random.Next(cadenasBase.Count);
            var segundaCadena = cadenasBase[segundaCadenaIndex];

            // El método de la segunda cadena puede ir desde el índice del primer método + 1, hasta el 
            // máximo índice de la segunda cadena, incluyendo a este.
            // Esto ya que siempre, la conexión a un nuevo método debe tener un índice mayor. Para evitar
            // ciclos.
            // Por lo tanto, la segunda cadena debe ser más larga que el índice del primer método + 1.
            // De momento se evita que la primer cadena y la segunda no sean la misma.
            while ((primerMetodoIndex + 1) >= segundaCadena.Count || (primerCadenaIndex == segundaCadenaIndex)) {
                //Obtener la primer cadena.
                primerCadenaIndex = Utilidades.Random.Next(cadenasBase.Count);
                primerCadena = cadenasBase[primerCadenaIndex];

                //Obtengo el método de la primer cadena
                primerMetodoIndex = Utilidades.Random.Next(primerCadena.Count);
                primerMetodo = primerCadena[primerMetodoIndex];

                //Obtengo la segunda cadena.
                segundaCadenaIndex = Utilidades.Random.Next(cadenasBase.Count);
                segundaCadena = cadenasBase[segundaCadenaIndex];
            }

            // Obtengo el segundo método.
            // Al mínimo del random se le suma uno, para que el menor sea exclusivo.
            int segundoMetodoIndex = Utilidades.Random.Next(primerMetodoIndex + 1, segundaCadena.Count);
            Method segundoMetodo = segundaCadena[segundoMetodoIndex];

            // Realizo el cruce, del primero vamos al segundo.
            primerMetodo.metodosInvocados.Add(segundoMetodo);

        }

        public void realizarCruce2(List<List<Method>> cadenasBase) {

        }

        /* Distribuye los métodos creados en la cantidad de clases indicadas en la configuración,
         * son distribuidos según un rango, también de la consiguración. Los métodos restantes
         * serán asignados uno a uno en clases aleatorias
         */
        public void distribuirMetodosPorClase() {
        
            //Permutar la lista de métodos para una distribución aleatoria
            metodos.Shuffle();

            //Indice general sobre todos los métodos
            int indexMetodos = 0;

            //Agregar la cantidad base de métodos a cada clase.
            for (int i = 0; i < configuracion.CantidadDeClases; i++) {
                SimpleClass nuevaClase = new SimpleClass();
                clases.Add(nuevaClase);
                int metodosPorAgregarIndices = Utilidades.Random.Next(configuracion.MinimaCantidadDeMetodosPorClase, configuracion.MaximaCantidadDeMetodosPorClase);

                var metodosPorAgregar = metodos.GetRange(indexMetodos, metodosPorAgregarIndices);

                foreach (Method metodo in metodosPorAgregar)
                    metodo.claseContenedora = nuevaClase;
                nuevaClase.metodos.AddRange(metodosPorAgregar);
                indexMetodos += metodosPorAgregarIndices;
            }

            //Reasignar el resto de métodos de manera aleatoria al resto de las clases.
            for (; indexMetodos < metodos.Count; indexMetodos++) {
                int claseContenedora = Utilidades.Random.Next(clases.Count);

                Method metodoPorAsignar = metodos[indexMetodos];

                metodoPorAsignar.claseContenedora = clases[claseContenedora];
                clases[claseContenedora].metodos.Add(metodoPorAsignar);
            }
            
        }

        /* Inicia la escritura de los archivos, le indica a todas las clases que deben escribirse,
         * cada clase conoce como debe escribirse y a su vez cada método conoce como escribirse.
         */
        public void escribirCodigo() {
            DirectoryInfo dir = Directory.CreateDirectory(configuracion.DirectorioDelCodigo);
            
            foreach (var clase in clases) {
                clase.WriteClass(configuracion.DirectorioDelCodigo);
            }

        }

        /* Los grafos creados tienen una estructura similar a un árbol, por lo que no hay ciclos.
         * Se recorren todos los caminos por profundidad hasta llegar a un método que sea final,
         * es decir, que no tenga más invocaciones.
         */
        public void escribirCadenas() {

            // Iniciar el escritor en formato JSON
            StringBuilder stringBuilder = new StringBuilder();
            StringWriter stringWriter = new StringWriter(stringBuilder);
            JsonTextWriter jsonWriter = new JsonTextWriter(stringWriter);
            jsonWriter.Formatting = Formatting.Indented;

            // Iniciar el arreglo que tiene todas las cadenas
            jsonWriter.WritePropertyName("Chains");
            jsonWriter.WriteStartArray();

            foreach (Method method in firstMethods) {

                // Crear una pila para almacenar el camino
                Stack<Method> chainPath = new Stack<Method>();

                // Recorrer el grafo desde el método actual
                recorrerGrafoPorProfundidad(method, chainPath, jsonWriter);


            }

            // Cerrar el arreglo que contiene las cadenas
            jsonWriter.WriteEndArray();

            // Obtener un archivo para guardar las cadenas
            string fileName = Path.Combine(configuracion.DirectorioPrincipal, "Chains.json");
            FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fileStream);

            // Escribir las cadenas
            writer.WriteLine(stringBuilder.ToString());

            // Cerrar los flujos
            writer.Close();
            fileStream.Close();

        }


        private void recorrerGrafoPorProfundidad(Method actual, Stack<Method> chainPath, JsonWriter jsonWriter) {

            // Marcar el nodo actual
            chainPath.Push(actual);

            // Verificar si el actual es un fin de cadena, no invoca más métodos.
            // Si no, continua con el corrido.
            if (actual.metodosInvocados.Count == 0) {
                writeChainAsJSON(chainPath, jsonWriter);
            }
            else {
                foreach(Method method in actual.metodosInvocados) {
                    recorrerGrafoPorProfundidad(method, chainPath, jsonWriter);
                }
            }

            // Desmarcar el nodo actual
            chainPath.Pop();

        }


        private void writeChainAsJSON(Stack<Method> chainPath, JsonWriter jsonWriter) {

            // Write an array for the chain.
            jsonWriter.WriteStartArray();

            foreach (Method method in chainPath) {

                // Write an object to containt the method
                jsonWriter.WriteStartObject();

                // Write the container class for the method and it's value.
                jsonWriter.WritePropertyName("Class");
                jsonWriter.WriteValue(method.claseContenedora.name);
                // Write the method name and it's value.
                jsonWriter.WritePropertyName("Method");
                jsonWriter.WriteValue(method.name);

                // Close the object for the method.
                jsonWriter.WriteEndObject();
            }

            // Close the array for the chain.
            jsonWriter.WriteEndArray();
        }



        /* Crea una matriz de adyacencia para representar el grago de
         * invocaciones, el índice de la matriz coincide con el identificador
         * del método.
         */
        public int[,] crearMatrizDeAdyacencias() {
            int[,] grafo = new int[Method.cantidadDeMetodos, Method.cantidadDeMetodos];

            for (int i = 0; i < Method.cantidadDeMetodos; i++)
                for (int j = 0; j < metodos[i].metodosInvocados.Count; j++)
                    grafo[i, metodos[i].metodosInvocados[j].identificador] = 1;

            return grafo;
        }
    }
}