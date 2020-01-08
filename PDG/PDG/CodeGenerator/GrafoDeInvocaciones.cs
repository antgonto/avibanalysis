using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using CodeGenerator.Snippets;
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
        public List<Snippet> Snippets { get; set; } = new List<Snippet>();
        public List<List<Method>> Chains { get; set; } = new List<List<Method>>();
        public Configuration Configuration { get; set; }

        public GrafoDeInvocaciones(Configuration configuration) {

            // Set the current configuration
            Configuration = configuration;

            /* Build the graph of invocations */
            // Create the base chains.
            List<List<Method>> cadenasBase = crearCadenasBase();
            // Make crosses among the base chains.
            MakeCrosses(cadenasBase);

            /* Get all the chains created by the crosses. */
            // Get the methods that are initiator of any chain.
            List<Method> firstMethods = obtenerIniciosDeCadena(cadenasBase);
            // Get all the posible chains from any initiating method.
            Chains = GetChains(firstMethods);

            /* Generate the Project Structure */
            // Assign snippets to each method.
            loadHardCodedSnippets();
            AssignRandomSnippets();
            // Distribute the methods among the classes.
            DistributeMethods();
            // Distribute the classes among the namespaces.
            DistributeClasses();
            // Add a main method in project to trigger all the chains from the its initiating methods.
            // Create a main method that invoke all the initiating methods.
            StaticMethod mainMethod = GetMainMethod(firstMethods);
            // Create a class for the static method.
            SimpleClass mainClass = getMainClass(mainMethod);
            // Store the main class.
            clases.Add(mainClass);
        }

        public void DistributeClasses() {

        }

        public void loadHardCodedSnippets() {
            ConcreteSnippet snippet1 = new ConcreteSnippet(1,
                "\t\t\tfor (int i = 0; i < 5; i++) {{" + System.Environment.NewLine +
                "{0}" + System.Environment.NewLine +
                "\t\t\t}}");
            snippet1.IdentationLevels.Add(4);
            ConcreteSnippet snippet2 = new ConcreteSnippet(1,
                "\t\t\tif (5 < 10) {{" + System.Environment.NewLine +
                "{0}" + System.Environment.NewLine +
                "\t\t\t}}");
            snippet2.IdentationLevels.Add(4);
            ConcreteSnippet snippet3 = new ConcreteSnippet(2,
                "\t\t\tif (3 < 3) {{" + System.Environment.NewLine +
                "{0}" + System.Environment.NewLine +
                "\t\t\t}} else {{" + System.Environment.NewLine +
                "{1}" + System.Environment.NewLine +
                "\t\t\t}}");
            snippet3.IdentationLevels.Add(4);
            snippet3.IdentationLevels.Add(4);
            ConcreteSnippet snippet4 = new ConcreteSnippet(2,
                "\t\t\twhile (true) {{" + System.Environment.NewLine +
                "{0}" + System.Environment.NewLine +
                "{1}" + System.Environment.NewLine +
                "\t\t\t\tbreak;" + System.Environment.NewLine +
                "\t\t\t}}");
            snippet4.IdentationLevels.Add(4);
            snippet4.IdentationLevels.Add(4);
            Snippets.Add(snippet1);
            Snippets.Add(snippet2);
            Snippets.Add(snippet3);
            Snippets.Add(snippet4);
        }

        public void AssignRandomSnippets() {
            // Add snippets for all the methods created.
            foreach (Method method in metodos) {
                // Get the number of invoked methods of the method to get covered by a snippet.
                int requiredMethods = (int)(Configuration.PorcentajeDeRecubrimientoConSnippets * method.metodosInvocados.Count);
                // Get a random number of snippets to cover all the required methods.
                int coveredMethods = 0;
                while (coveredMethods < requiredMethods) {
                    // Get a random snippet, uniform probability.
                    int snippetIndex = Utilidades.Random.Next(Snippets.Count);
                    Snippet snippet = Snippets[snippetIndex];
                    // If the snippet cover more than the required methods, choose another one.
                    if (coveredMethods + snippet.RequiredMethods <= requiredMethods) {
                        method.snippets.Add(snippet);
                        coveredMethods += snippet.RequiredMethods;
                    }
                }
            }
        }

        /* Obtiene una clase que posee el método principal, dado por parámetro.
         */
        public SimpleClass getMainClass(StaticMethod mainMethod) {

            SimpleClass mainClass = new SimpleClass("MainClass");
            mainClass.metodos.Add(mainMethod);

            return mainClass;
        }

        /* Crea el método principal, invoca a los métodos que inician todas las cadenas, para crear un
         * punto en común estos métodos son obtenidos por parámetro.
         */
        public StaticMethod GetMainMethod(List<Method> metodosInvocados) {

            StaticMethod mainMethod = new StaticMethod(AccessModifier.PUBLIC, Types.VOID, "Main");
            mainMethod.metodosInvocados.AddRange(metodosInvocados);
            mainMethod.parametros.Add(new Parameter(Types.ARRAY_STRING, 0));

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
            List<List<Method>> cadenasBase = new List<List<Method>>(Configuration.CantidadDeCadenasIndependientes);

            // Se crean tantas cadenas base como se especificó en la configuración.
            for (int i = 0; i < Configuration.CantidadDeCadenasIndependientes; i++) {
                // Se obtiene un largo aleatorio dentro del rango especificado.
                int largoDeCadena = Utilidades.Random.Next(Configuration.MinimaLongitudDeCadena, Configuration.MaximaLongitudDeCadena);

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
        public void MakeCrosses(List<List<Method>> cadenasBase) {

            double probabilidadDeCruce1 = Configuration.PorcentajeDeRealizarCruce1;
            double probabilidadDeCruce2 = Configuration.PorcentajeDeRealizarCruce1 + Configuration.PorcentajeDeRealizarCruce2;

            for (int i = 0; i < Configuration.CantidadDeCrucezPorRealizar; i++) {
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
        public void DistributeMethods() {

            //Permutar la lista de métodos para una distribución aleatoria
            metodos.Shuffle();

            //Indice general sobre todos los métodos
            int indexMetodos = 0;

            //Agregar la cantidad base de métodos a cada clase.
            for (int i = 0; i < Configuration.CantidadDeClases; i++) {
                SimpleClass nuevaClase = new SimpleClass();
                clases.Add(nuevaClase);
                int metodosPorAgregarIndices = Utilidades.Random.Next(Configuration.MinimaCantidadDeMetodosPorClase, Configuration.MaximaCantidadDeMetodosPorClase);

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
            DirectoryInfo dir = Directory.CreateDirectory(Configuration.DirectorioDelCodigo);

            foreach (var clase in clases) {
                clase.WriteClass(Configuration.DirectorioDelCodigo);
            }

        }

        private List<List<Method>> GetChains(List<Method> InitiatingMethods) {

            // Create a list to store the chains.
            List<List<Method>> chains = new List<List<Method>>();

            // Get all of the possible chains from any initiating method.
            foreach (Method initiatingMethod in InitiatingMethods) {
                
                // Temporal stack to manage the chains.
                Stack<Method> actualChain = new Stack<Method>();

                // Get all the chains from the current method and store it in the list of chains.
                GetChainsAux(initiatingMethod, actualChain, chains);
            }

            return chains;
        }

        private void GetChainsAux(Method actualMethod, Stack<Method> actualChain, List<List<Method>> chains) {

            // Add the current method in the stack to build the chain.
            actualChain.Push(actualMethod);

            // If the actual method don't have any more methods to invoke this method is the end of the chain.
            // save a copy of the current stack as a list in the general chains list.
            if (actualMethod.metodosInvocados.Count == 0) {
                List<Method> chain = actualChain.ToList();
                chain.Reverse();
                chains.Add(chain);
            }
            else {
                foreach (Method method in actualMethod.metodosInvocados) {
                    GetChainsAux(method, actualChain, chains);
                }
            }

            // Remove the current method of the stack. Back to the previous method.
            actualChain.Pop();
        }

        /* Los grafos creados tienen una estructura similar a un árbol, por lo que no hay ciclos.
         * Se recorren todos los caminos por profundidad hasta llegar a un método que sea final,
         * es decir, que no tenga más invocaciones.
         */
        public void WriteChains() {

            // Get a JSON writer.
            StringBuilder stringBuilder = new StringBuilder();
            StringWriter stringWriter = new StringWriter(stringBuilder);
            JsonTextWriter jsonWriter = new JsonTextWriter(stringWriter) {
                Formatting = Formatting.Indented
            };

            // Write the generic information about the chains.
            // ...
            // Write the chains objects
            WriteChainsInfo(Chains, jsonWriter);

            // Get a file to store the created JSON.
            string fileName = Path.Combine(Configuration.DirectorioPrincipal, "Chains.json");
            FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fileStream);

            // Write the chains and close the streams.
            writer.WriteLine(stringBuilder.ToString());
            writer.Close();
            fileStream.Close();
        }

        private void WriteChainsInfo(List<List<Method>> chains, JsonTextWriter jsonWriter) {

            // Write properties about the chains in general, like max length, min length, mean and standard deviation.
            List<int> lengths = GetListOfLengths(chains);
            jsonWriter.WritePropertyName("Mean");
            jsonWriter.WriteValue(lengths.Average());
            jsonWriter.WritePropertyName("Variance");
            jsonWriter.WriteValue(Utilidades.Variance(lengths));
            jsonWriter.WritePropertyName("Max");
            jsonWriter.WriteValue(lengths.Max());
            jsonWriter.WritePropertyName("Min");
            jsonWriter.WriteValue(lengths.Min());

            // Write a "Chains" property for the chains, it contains as a value an array of chains objects.
            jsonWriter.WritePropertyName("Chains");
            jsonWriter.WriteStartArray();

            // Write all of the chains in the current array value of the "Chains" property.
            foreach (List<Method> chain in chains) {

                // Write the chain as a json object.
                WriteChainAsJSON(chain, jsonWriter);
            }

            // Close the array value of the "Chains property".
            jsonWriter.WriteEndArray();
        }

        private List<int> GetListOfLengths(List<List<Method>> chains) {
            List<int> lengths = new List<int>();

            foreach (List<Method> chain in chains) {
                lengths.Add(chain.Count);
            }

            return lengths;
        }

        private void WriteChainAsJSON(List<Method> chain, JsonWriter jsonWriter) {

            // Write an object to contain the chain attributes and the array for the methods.
            jsonWriter.WriteStartObject();

            // Write the length of the chain.
            jsonWriter.WritePropertyName("Length");
            jsonWriter.WriteValue(chain.Count);

            // Write an array for the chain.
            jsonWriter.WritePropertyName("Chain");
            jsonWriter.WriteStartArray();

            foreach (Method method in chain) {

                // Write an object to containt the method.
                jsonWriter.WriteStartObject();

                // Write the container class for the method and it's value.
                jsonWriter.WritePropertyName("Class");
                jsonWriter.WriteValue(method.claseContenedora.Name);
                // Write the method name and it's value.
                jsonWriter.WritePropertyName("Method");
                jsonWriter.WriteValue(method.name);

                // Close the object for the method.
                jsonWriter.WriteEndObject();
            }

            // Close the array for the chain.
            jsonWriter.WriteEndArray();

            // Close the object for the chain.
            jsonWriter.WriteEndObject();
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