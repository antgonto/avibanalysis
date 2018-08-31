using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.CSharp.Symbols;

using System.Windows.Forms;
using System.Reflection;
using Newtonsoft.Json;
using Neo4j.Driver.V1;
using System.Diagnostics;

//Tools> Nugget>Console
// Install-Package Microsoft.CodeAnalysis
namespace ProjectParser
{
    public class Program
    {
        public static int calcularComplejidadCiclomatica(MethodDeclarationSyntax Nodo)
        {
            int cantIf = Nodo.DescendantNodes().OfType<IfStatementSyntax>().Count();
            int cantWhile = Nodo.DescendantNodes().OfType<WhileStatementSyntax>().Count();
            int cantFor = Nodo.DescendantNodes().OfType<ForStatementSyntax>().Count();
            int cantForEach = Nodo.DescendantNodes().OfType<ForEachStatementSyntax>().Count();
            int cantCase = Nodo.DescendantNodes().OfType<CaseSwitchLabelSyntax>().Count();
            int cantDefault = Nodo.DescendantNodes().OfType<DefaultSwitchLabelSyntax>().Count();
            int cantDoWhile = Nodo.DescendantNodes().OfType<DoStatementSyntax>().Count();

            return cantIf + cantWhile + cantFor + cantForEach + cantCase + cantDefault + cantDoWhile + 1;
        }

        [STAThread]
        public static void Main(string[] args)
        {
            //Metrics();
            JsonStructure();
        }

        private static SyntaxNode FindMethod(SyntaxNode obj)
        {
            while (!(obj is MethodDeclarationSyntax ||
                     obj is PropertyDeclarationSyntax ||
                     obj is ConstructorDeclarationSyntax ||
                     //obj is OperatorDeclarationSyntax || <--- Pendiente de procesar
                     //obj is IndexerDeclarationSyntax || <--- Pendiente de procesar
                     //obj is ConversionOperatorDeclarationSyntax|| <--- Pendiente de procesar
                     //obj is FieldDeclarationSyntax ||   <--- Pendiente de procesar
                     obj is DestructorDeclarationSyntax) && obj != null)
            {
                if (obj is ClassDeclarationSyntax || obj is NamespaceDeclarationSyntax)
                    break;
                obj = obj.Parent;
            }

            return (obj is ClassDeclarationSyntax || obj is NamespaceDeclarationSyntax) ? null : obj;
        }

        private static string FindMethodName(SyntaxNode obj)
        {
            if (obj is MethodDeclarationSyntax) return (obj as MethodDeclarationSyntax).Identifier.ToString();
            if (obj is PropertyDeclarationSyntax) return (obj as PropertyDeclarationSyntax).Identifier.ToString();
            if (obj is ConstructorDeclarationSyntax) return (obj as ConstructorDeclarationSyntax).Identifier.ToString();
            //if (obj is OperatorDeclarationSyntax) return (obj as OperatorDeclarationSyntax).GetText().ToString(); <--- Pendiente de procesar
            //if (obj is IndexerDeclarationSyntax) return (obj as IndexerDeclarationSyntax).GetText().ToString(); <--- Pendiente de procesar
            //if (obj is FieldDeclarationSyntax) return (obj as FieldDeclarationSyntax).GetText().ToString(); <--- Pendiente de procesar
            if (obj is DestructorDeclarationSyntax) return (obj as DestructorDeclarationSyntax).Identifier.ToString();
            //if (obj is ConversionOperatorDeclarationSyntax) return (obj as ConversionOperatorDeclarationSyntax).GetText().ToString(); <--- Pendiente de procesar
            return "";
        }

        private static SyntaxNode FindClass(SyntaxNode obj)
        {
            while (!(obj is ClassDeclarationSyntax ||
                     obj is InterfaceDeclarationSyntax ||
                     obj is StructDeclarationSyntax) && obj != null)
            {
                if (obj is NamespaceDeclarationSyntax)
                    break;
                obj = obj.Parent;
            }

            return (obj is NamespaceDeclarationSyntax) ? null : obj;
        }

        private static string FindClassName(SyntaxNode obj)
        {
            if (obj is ClassDeclarationSyntax) return (obj as ClassDeclarationSyntax).Identifier.ToString();
            if (obj is InterfaceDeclarationSyntax) return (obj as InterfaceDeclarationSyntax).Identifier.ToString();
            if (obj is StructDeclarationSyntax) return (obj as StructDeclarationSyntax).Identifier.ToString();
            return "";
        }

        private static NamespaceDeclarationSyntax FindNamespace(SyntaxNode obj)
        {
            while (!(obj is NamespaceDeclarationSyntax) && obj != null) obj = obj.Parent;

            return obj == null ? null : (obj as NamespaceDeclarationSyntax);
        }

        [STAThread]
        private static void ExtractGraphFromAST(JsonProject project, Compilation myCompilation, string path)
        {
            Dictionary<string, JsonMethod> nodoNamespacesNeo4J = new Dictionary<string, JsonMethod>();
            // Send output to a file
            System.IO.StreamWriter output = new System.IO.StreamWriter(@"" + path + @"\graph_info.txt");

            List<SemanticModel> semanticModels = new List<SemanticModel>();
            List<SyntaxNode> roots = new List<SyntaxNode>();
            project.Name = myCompilation.AssemblyName;
            
            foreach (SyntaxTree sourceTree in myCompilation.SyntaxTrees)//Loop para recorrer la lista de archivos
            {
                roots.Add(sourceTree.GetRoot());//Obtiene el root de cada árbol de clase
                semanticModels.Add(myCompilation.GetSemanticModel(sourceTree));//Se guarda los semantic models en el mismo orden de la lista
            }

            List<MethodDeclarationSyntax> declarationSyntax;
            List<ClassDeclarationSyntax> classDeclarationSyntax;
            List<FieldDeclarationSyntax> fieldDeclaration;
            List<InvocationExpressionSyntax> invocationExpressionSyntax;

            output.WriteLine("\n\nLista de Metodos:\n----------------------------------------------------------------\n\n");

            for (int i = 0; i < semanticModels.Count; i++)
            {
                //Obtengo todas las declaraciones de metodos en el modelo semantico.
                declarationSyntax = roots[i].DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

                //Recorro las declaraciones para obtener los metodos.
                foreach (MethodDeclarationSyntax declaracionDeMetodoActual in declarationSyntax)
                {
                    //Calculations of each method 
                    int start = 0, end = 0, lines = 0, cyclomatic = 0;
                    //output.WriteLine(declaracionDeMetodoActual.Identifier.ToString());

                    cyclomatic = calcularComplejidadCiclomatica(declaracionDeMetodoActual);
                    if (declaracionDeMetodoActual.Body != null)
                    {
                        start = declaracionDeMetodoActual.Body.SyntaxTree.GetLineSpan(declaracionDeMetodoActual.FullSpan).StartLinePosition.Line;
                        end = declaracionDeMetodoActual.Body.SyntaxTree.GetLineSpan(declaracionDeMetodoActual.FullSpan).EndLinePosition.Line;
                        lines = end - start;
                    }
                    else
                    {
                        lines = 1;
                        cyclomatic = 1;

                    }
                        SyntaxNode classDec = FindClass(declaracionDeMetodoActual);
                    NamespaceDeclarationSyntax namespaceDec = FindNamespace(classDec);

                    if (classDec != null && namespaceDec != null)
                    {
                        JsonMethod m = JsonMethod.GetMethod(
                            declaracionDeMetodoActual.Identifier.ToString(),
                            FindClassName(classDec),
                            namespaceDec.Name.ToString(),
                            lines,
                            1,
                            cyclomatic);

                        output.WriteLine(String.Format("{0,-150} {1,-150} {2,-150} {3,-15} {4,-15} {5,-15}",
                                                       m.Name, m.ClassName, m.NamespaceName, m.Id, m.ClassId, m.NamespaceId));
                    }
                }

                //Obtengo todas las classes del modelo.
                classDeclarationSyntax = roots[i].DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();

                //Recorro las classes para obtener sus attributes
                foreach (ClassDeclarationSyntax claseActual in classDeclarationSyntax)
                {
                    NamespaceDeclarationSyntax namespaceDec = FindNamespace(claseActual);

                    fieldDeclaration = claseActual.DescendantNodes().OfType<FieldDeclarationSyntax>().ToList();
                    foreach (FieldDeclarationSyntax field in fieldDeclaration)
                    {
                        VariableDeclarationSyntax declaracion = field.Declaration;
                        List<VariableDeclaratorSyntax> listaVariables = declaracion.DescendantNodes().OfType<VariableDeclaratorSyntax>().ToList();
                        foreach (VariableDeclaratorSyntax variable in listaVariables)
                        {
                            if (claseActual != null && namespaceDec != null)
                            {
                                JsonAttribute.GetAttribute(
                                variable.Identifier.ToString(),
                                claseActual.Identifier.ToString(),
                                namespaceDec == null ? "" : namespaceDec.Name.ToString());
                            }
                        }
                    }
                }
            }

            output.WriteLine("\n\nLista de Llamadas:\n----------------------------------------------------------------\n\n");

            for (int i = 0; i < semanticModels.Count; i++)
            {
                //Obtengo todas las invocaciones de metodos en el modelo semantico.
                invocationExpressionSyntax = roots[i].DescendantNodes().OfType<InvocationExpressionSyntax>().ToList();

                foreach (InvocationExpressionSyntax invocation in invocationExpressionSyntax)
                {
                    SyntaxNode methodDec = FindMethod(invocation);
                    SyntaxNode classDec = FindClass(methodDec);
                    NamespaceDeclarationSyntax namespaceDec = FindNamespace(classDec);
                    ISymbol symbol = semanticModels[i].GetSymbolInfo(invocation).Symbol;
                    if (methodDec != null && classDec != null && namespaceDec != null && symbol != null)
                    {
                        if (symbol is IMethodSymbol)
                        {
                            IMethodSymbol iSymbol = symbol as IMethodSymbol;
                            if (!iSymbol.MethodKind.ToString().Equals("ReducedExtension") &&
                                !iSymbol.MethodKind.ToString().Equals("LocalFunction"))
                            {
                                // 0 search only 
                                JsonMethod caller = JsonMethod.GetMethod(FindMethodName(methodDec),
                                    FindClassName(classDec), namespaceDec.Name.ToString(), 0, 0, 0);

                                string mname = iSymbol.Name;
                                string cname = iSymbol.ContainingSymbol.Name;
                                string nname = iSymbol.ContainingNamespace.ToString();
                                // 0 search only
                                JsonMethod callee = JsonMethod.GetMethod(mname, cname, nname, 0, 0, 0);

                                if (caller.Id == callee.Id)
                                {
                                    caller.IsRecursive = true;
                                }
                                else
                                {
                                    JsonCall callerEntry = new JsonCall(caller.Id, caller.Name, caller.ClassId, caller.ClassName, caller.NamespaceId, caller.FullNamespaceName, caller);
                                    JsonCall calleeEntry = new JsonCall(callee.Id, callee.Name, callee.ClassId, callee.ClassName, callee.NamespaceId, callee.FullNamespaceName, callee);

                                    if (!callee.CalledBy.Contains(callerEntry))
                                    {
                                        output.WriteLine(String.Format("{0,-150} {1,-150} {2,-150} {3,-150} {4,-150} {5,-150} {6,-15} {7,-15} {8,-15} {9,-15} {10,-15} {11,-15}",
                                                                       caller.Name, callee.Name,
                                                                       caller.ClassName, callee.ClassName,
                                                                       caller.FullNamespaceName, callee.FullNamespaceName,
                                                                       caller.Id, callee.Id,
                                                                       caller.ClassId, callee.ClassId,
                                                                       caller.NamespaceId, callee.NamespaceId));
                                    }

                                    if (!callee.CalledBy.Contains(callerEntry)) callee.CalledBy.Add(callerEntry);
                                    if (!caller.Calls.Contains(calleeEntry)) caller.Calls.Add(calleeEntry);
                                }
                            }
                        }
                    }
                }
            }

            output.Flush();
        }

        [STAThread]
        public static void JsonStructure()
        {
            JsonProject project = new JsonProject();
            JsonNamespace.Project = project;

            Compilation myCompilation = CreateTestCompilation();//Llama a la clase para crear la lista de archivos

            FolderBrowserDialog salida = new FolderBrowserDialog();
            salida.Description = @"Output folder";
            salida.SelectedPath = @"C:\Users\jnavas\source\repos\avibanalysis\ExtractIndirectCoupling\output";
            //salida.SelectedPath = @"C:\Users\Administrador\Documents\GitHub\avibanalysis\ExtractIndirectCoupling\output";
            //salida.SelectedPath = @"C:\Users\Steven\Desktop\output";
            if (salida.ShowDialog() == DialogResult.OK)
            {
                Stopwatch timer = new Stopwatch();
                Console.Write("Loading ASTs...");
                timer.Start();
                ExtractGraphFromAST(project, myCompilation, salida.SelectedPath);
                myCompilation = null;
                timer.Stop();
                Console.WriteLine(" (ellapsed time: " + (((double)timer.ElapsedMilliseconds) / 60000.0).ToString() + " min)");

                //connectNeo4J(project, salida.SelectedPath);

                Console.Write("Collapsing SCCs...");
                timer.Reset(); timer.Start();
                CollapseGraphSCC();
                timer.Stop();
                Console.WriteLine(" (ellapsed time: " + (((double)timer.ElapsedMilliseconds) / 60000.0).ToString() + " min)");

                Console.Write("Collecting SCC Metrics using DFS...");
                timer.Reset(); timer.Start();
                JsonMethod.CollectSccMetricsUsingDFS();
                timer.Stop();
                Console.WriteLine(" (ellapsed time: " + (((double)timer.ElapsedMilliseconds) / 60000.0).ToString() + " min)");

                Console.Write("Collecting PDG Metrics using Dfs...");
                timer.Reset(); timer.Start();
                JsonMethod.CollectMetricsUsingDfs();
                timer.Stop();
                Console.WriteLine(" (ellapsed time: " + (((double)timer.ElapsedMilliseconds) / 60000.0).ToString() + " min)");

                Console.Write("Saving graph with metrics in neo4j...");
                timer.Reset(); timer.Start();
                SaveNeo4JGraph(project);
                timer.Stop();
                Console.WriteLine(" (ellapsed time: " + (((double)timer.ElapsedMilliseconds) / 60000.0).ToString() + " min)");

                Console.Write("Saving metrics output to metrics.txt...");
                timer.Reset(); timer.Start();
                // Send output to a file
                System.IO.StreamWriter output = new System.IO.StreamWriter(@"" + salida.SelectedPath + @"\metrics.txt");

                output.WriteLine("\n=============================================================================================\n");

                // Print metrics for Methods/SCCs 

                foreach (KeyValuePair<string, JsonMethod> kv in JsonMethod.Methods)
                {
                    JsonMethod m = kv.Value;
                    if (m.IsCollapsed == true) continue;
                    output.WriteLine(String.Format("Method: {0}-{1}  K:{2}  L:{3}  C:{4}", m.Id, m.Name, m.Kon, m.Loc, m.Cyc));
                    output.WriteLine(String.Format("  FORWARD"));
                    output.WriteLine(String.Format("     Kon  Min:{0}  Max:{1}  Avg:{2}  Cnt:{3}  Sum:{4}", m.Kon_metrics.Fmin, m.Kon_metrics.Fmax, m.Kon_metrics.Favg, m.Kon_metrics.Fcnt, m.Kon_metrics.Fsum));
                    output.WriteLine(String.Format("     Loc  Min:{0}  Max:{1}  Avg:{2}  Cnt:{3}  Sum:{4}", m.Loc_metrics.Fmin, m.Loc_metrics.Fmax, m.Loc_metrics.Favg, m.Loc_metrics.Fcnt, m.Loc_metrics.Fsum));
                    output.WriteLine(String.Format("     Cyc  Min:{0}  Max:{1}  Avg:{2}  Cnt:{3}  Sum:{4}", m.Cyc_metrics.Fmin, m.Cyc_metrics.Fmax, m.Cyc_metrics.Favg, m.Cyc_metrics.Fcnt, m.Cyc_metrics.Fsum));
                    output.WriteLine(String.Format("  BACKWARD"));
                    output.WriteLine(String.Format("     Kon  Min:{0}  Max:{1}  Avg:{2}  Cnt:{3}  Sum:{4}", m.Kon_metrics.Bmin, m.Kon_metrics.Bmax, m.Kon_metrics.Bavg, m.Kon_metrics.Bcnt, m.Kon_metrics.Bsum));
                    output.WriteLine(String.Format("     Loc  Min:{0}  Max:{1}  Avg:{2}  Cnt:{3}  Sum:{4}", m.Loc_metrics.Bmin, m.Loc_metrics.Bmax, m.Loc_metrics.Bavg, m.Loc_metrics.Bcnt, m.Loc_metrics.Bsum));
                    output.WriteLine(String.Format("     Cyc  Min:{0}  Max:{1}  Avg:{2}  Cnt:{3}  Sum:{4}", m.Cyc_metrics.Bmin, m.Cyc_metrics.Bmax, m.Cyc_metrics.Bavg, m.Cyc_metrics.Bcnt, m.Cyc_metrics.Bsum));
                }
                foreach (JsonMethod m in JsonMethod.SccList)
                {
                    output.WriteLine(String.Format("SCC: {0}-{1}  K:{2}  L:{3}  C:{4}", m.Id, m.Name, m.Kon, m.Loc, m.Cyc));
                    output.WriteLine(String.Format("  FORWARD"));
                    output.WriteLine(String.Format("     Kon  Min:{0}  Max:{1}  Avg:{2}  Cnt:{3}  Sum:{4}", m.Kon_metrics.Fmin, m.Kon_metrics.Fmax, m.Kon_metrics.Favg, m.Kon_metrics.Fcnt, m.Kon_metrics.Fsum));
                    output.WriteLine(String.Format("     Loc  Min:{0}  Max:{1}  Avg:{2}  Cnt:{3}  Sum:{4}", m.Loc_metrics.Fmin, m.Loc_metrics.Fmax, m.Loc_metrics.Favg, m.Loc_metrics.Fcnt, m.Loc_metrics.Fsum));
                    output.WriteLine(String.Format("     Cyc  Min:{0}  Max:{1}  Avg:{2}  Cnt:{3}  Sum:{4}", m.Cyc_metrics.Fmin, m.Cyc_metrics.Fmax, m.Cyc_metrics.Favg, m.Cyc_metrics.Fcnt, m.Cyc_metrics.Fsum));
                    output.WriteLine(String.Format("  BACKWARD"));
                    output.WriteLine(String.Format("     Kon  Min:{0}  Max:{1}  Avg:{2}  Cnt:{3}  Sum:{4}", m.Kon_metrics.Bmin, m.Kon_metrics.Bmax, m.Kon_metrics.Bavg, m.Kon_metrics.Bcnt, m.Kon_metrics.Bsum));
                    output.WriteLine(String.Format("     Loc  Min:{0}  Max:{1}  Avg:{2}  Cnt:{3}  Sum:{4}", m.Loc_metrics.Bmin, m.Loc_metrics.Bmax, m.Loc_metrics.Bavg, m.Loc_metrics.Bcnt, m.Loc_metrics.Bsum));
                    output.WriteLine(String.Format("     Cyc  Min:{0}  Max:{1}  Avg:{2}  Cnt:{3}  Sum:{4}", m.Cyc_metrics.Bmin, m.Cyc_metrics.Bmax, m.Cyc_metrics.Bavg, m.Cyc_metrics.Bcnt, m.Cyc_metrics.Bsum));
                }

                output.WriteLine("\n=============================================================================================\n");

                // Print metrics for Pairs of methods

                for (int m1 = 0; m1 < JsonMethod.PairMetricsList.Width; m1++)
                    for (int m2 = 0; m2 < JsonMethod.PairMetricsList.Height; m2++)
                        if (JsonMethod.PairMetricsList.IsCellPresent(m1, m2))
                        {
                            PairMetrics m = JsonMethod.PairMetricsList[m1, m2];
                            output.WriteLine(String.Format("From Method: {0} ==> To Method: {1}", m1, m2));
                            output.WriteLine(String.Format("     Kon  Min:{0}  Max:{1}  Avg:{2}  Cnt:{3}  Sum:{4}", m.K.Fmin, m.K.Fmax, m.K.Favg, m.K.Fcnt, m.K.Fsum));
                            output.WriteLine(String.Format("     Loc  Min:{0}  Max:{1}  Avg:{2}  Cnt:{3}  Sum:{4}", m.L.Fmin, m.L.Fmax, m.L.Favg, m.L.Fcnt, m.L.Fsum));
                            output.WriteLine(String.Format("     Cyc  Min:{0}  Max:{1}  Avg:{2}  Cnt:{3}  Sum:{4}", m.C.Fmin, m.C.Fmax, m.C.Favg, m.C.Fcnt, m.C.Fsum));
                        }

                output.WriteLine("\n=============================================================================================\n");
                output.Flush();
                output.Close();

                timer.Stop();
                Console.WriteLine(" (ellapsed time: " + (((double)timer.ElapsedMilliseconds) / 60000.0).ToString() + " min)");


                /*
                JsonSerializer serializer = new JsonSerializer();

                using (StreamWriter sw = new StreamWriter(@"" + salida.SelectedPath + @"\" + project.Name + @".json"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(writer, project);
                }
                */
            }

            Console.WriteLine("Process finished successfully!");
            Console.Read();

        }

        public static void Metrics()
        { 
            int cantidadClases = 0;
            //***********************************************
            //* Aca comienza el paso 1 del pseudocódigo     *
            //*                                             *
            //***********************************************
            List<Metodo> listaMetodos = new System.Collections.Generic.List<Metodo>();
            Compilation myCompilation = CreateTestCompilation();//Llama a la clase para crear la lista de archivos

            List<SemanticModel> semanticModels = new List<SemanticModel>();
            List<SyntaxNode> roots = new List<SyntaxNode>();
            string nombreProyecto = myCompilation.AssemblyName;

            foreach (SyntaxTree sourceTree in myCompilation.SyntaxTrees)//Loop para recorrer la lista de archivos
            {
                roots.Add(sourceTree.GetRoot());//Obtiene el root de cada árbol de clase
                semanticModels.Add(myCompilation.GetSemanticModel(sourceTree));//Se guarda los semantic models en el mismo orden de la lista
            }

            List<MethodDeclarationSyntax> declarationSyntax = new List<MethodDeclarationSyntax>();
            List<ClassDeclarationSyntax> classDeclarationSyntax = new List<ClassDeclarationSyntax>();
            List<FieldDeclarationSyntax> fieldDeclaration = new List<FieldDeclarationSyntax>();
            for (int i = 0; i < semanticModels.Count; i++)
            {
                //Obtengo todas las declaraciones de metodos en el modelo semantico.
                //Obtengo todas las classes del modelo.
                declarationSyntax = roots[i].DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
                classDeclarationSyntax = roots[i].DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
                cantidadClases += classDeclarationSyntax.Count;


                fieldDeclaration = roots[i].DescendantNodes().OfType<FieldDeclarationSyntax>().ToList();

                //Recorro las declaraciones para obtener los metodos.
                foreach (MethodDeclarationSyntax declaracionDeMetodoActual in declarationSyntax)
                {
                    int inicio = 0, fin = 0, lineas = 0, ciclomatico = 0;
                    Console.WriteLine(declaracionDeMetodoActual.Identifier.ToString());

                    ciclomatico = calcularComplejidadCiclomatica(declaracionDeMetodoActual);
                    if (declaracionDeMetodoActual.Body != null)
                    {
                        inicio = declaracionDeMetodoActual.Body.SyntaxTree.GetLineSpan(declaracionDeMetodoActual.FullSpan).StartLinePosition.Line;
                        fin = declaracionDeMetodoActual.Body.SyntaxTree.GetLineSpan(declaracionDeMetodoActual.FullSpan).EndLinePosition.Line;
                        lineas = fin - inicio;
                    }
                    else
                    {
                        lineas = 1;
                        bool flag = false;
                        foreach (SyntaxToken modifier in declaracionDeMetodoActual.Modifiers)
                        {
                            if (modifier.Value == "extern")
                            { flag = true; break; }
                        }
                        if (flag)
                        {
                            Console.WriteLine("");//Detecta un extern del proyecto

                        }
                        if (declaracionDeMetodoActual.Parent.IsKind(SyntaxKind.InterfaceDeclaration))
                        {
                            Console.Write("");
                        }
                        else
                        {
                            List<SyntaxToken> syntaxTokens = declaracionDeMetodoActual.DescendantTokens().OfType<SyntaxToken>().ToList();
                            flag = false;
                            foreach (SyntaxToken token in syntaxTokens)
                            {
                                if (token.IsKind(SyntaxKind.AbstractKeyword))
                                {

                                    Console.Write("");
                                    flag = true;
                                    break;
                                }
                            }
                            if (!flag)
                            {
                                if (declaracionDeMetodoActual.ExpressionBody.IsKind(SyntaxKind.ArrowExpressionClause))
                                {


                                }
                                else
                                {
                                    Console.WriteLine("");
                                }
                            }
                        }
                        lineas = 1;
                        ciclomatico = 1;

                    }
                    foreach (ClassDeclarationSyntax claseActual in classDeclarationSyntax)
                    {

                        //Busco la clase correspondiente a la declaracion de metodo actual
                        if (claseActual.Contains(declaracionDeMetodoActual))
                        {

                            //Se agrega a la lista de Metodos
                            Metodo newMetodo = new Metodo(claseActual.Identifier.ToString(),
                                declaracionDeMetodoActual.Identifier.ToString(),
                                false,
                                false,
                                false,
                                SymbolKind.Method,
                                lineas,
                                1,
                                ciclomatico);
                            listaMetodos.Add(newMetodo);
                            break;
                        }
                    }
                }
                //Recorro las classes para obtener sus attributes
                foreach (ClassDeclarationSyntax claseActual in classDeclarationSyntax)
                {

                    fieldDeclaration = claseActual.DescendantNodes().OfType<FieldDeclarationSyntax>().ToList();
                    foreach (FieldDeclarationSyntax field in fieldDeclaration)
                    {
                        VariableDeclarationSyntax declaracion = field.Declaration;
                        List<VariableDeclaratorSyntax> listaVariables = declaracion.DescendantNodes().OfType<VariableDeclaratorSyntax>().ToList();
                        foreach (VariableDeclaratorSyntax variable in listaVariables)
                        {

                            Metodo newMetodo = new Metodo(claseActual.Identifier.ToString(),
                                variable.Identifier.ToString(),
                                false,
                                false,
                                false,
                                SymbolKind.Field,
                                0,
                                1,
                                0);
                            listaMetodos.Add(newMetodo);
                        }
                    }
                }
            }



            //***********************************************
            //* Aca comienza el paso 2 del pseudocódigo     *
            //*                                             *
            //***********************************************
            int contadorSemanticModel = 0;
            foreach (SemanticModel currentSemanticModel in semanticModels)//Recorre los modelos semanticos de cada arbol
            {
                declarationSyntax = roots[contadorSemanticModel].DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
                var invocationSyntax = roots[contadorSemanticModel].DescendantNodes().OfType<MemberAccessExpressionSyntax>();
                classDeclarationSyntax = roots[contadorSemanticModel].DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
                foreach (MemberAccessExpressionSyntax expression in invocationSyntax)
                {
                    Metodo llamado = null;

                    ISymbol val = currentSemanticModel.GetSymbolInfo(expression).Symbol;
                    if (val == null)
                    {
                        MethodDeclarationSyntax declaracionThis = declarationSyntax.Find(x => x.Contains(expression));
                        if (declaracionThis != null)
                        {
                            List<ParameterSyntax> listaParametros = declaracionThis.ParameterList.DescendantNodes().OfType<ParameterSyntax>().ToList();
                            foreach (ParameterSyntax parametro in listaParametros)
                            {

                                if (parametro.Identifier.ValueText == expression.Expression.ToString())
                                {
                                    Console.WriteLine(parametro.Type.ToString());
                                    Console.WriteLine(expression.Name.ToString());
                                    llamado = listaMetodos.Find(x => (x.Clase == parametro.Type.ToString() &&
                                                                       x.Nombre == expression.Name.ToString())
                                                                                 );
                                    Console.Write("");
                                }
                            }
                        }
                        else
                        {
                            if (expression.Parent.IsKind(SyntaxKind.SimpleLambdaExpression) || expression.Parent.Parent.IsKind(SyntaxKind.ParenthesizedLambdaExpression))
                            {
                                Console.Write("");
                            }
                        }

                    }
                    else
                    {
                        var nombre = currentSemanticModel.GetSymbolInfo(expression).Symbol.Name;
                        var clase = currentSemanticModel.GetSymbolInfo(expression).Symbol.ContainingType;
                        if (clase == null)
                        {
                            MethodDeclarationSyntax declaracionThis = declarationSyntax.Find(x => x.Contains(expression));
                            if (declaracionThis != null)
                            {
                                List<ParameterSyntax> listaParametros = declaracionThis.ParameterList.DescendantNodes().OfType<ParameterSyntax>().ToList();
                                foreach (ParameterSyntax parametro in listaParametros)
                                {
                                    //Revisar esto 
                                    if (parametro.Identifier.ValueText == expression.Expression.ToString())
                                    {
                                        Console.WriteLine(parametro.Type.ToString());
                                        Console.WriteLine(expression.Name.ToString());
                                        llamado = listaMetodos.Find(x => (x.Clase == parametro.Type.ToString() &&
                                                                          x.Nombre == expression.Name.ToString())
                                                                                     );
                                        Console.Write("");
                                    }
                                }
                            }
                            else
                            {
                                Console.Write("");
                            }
                        }
                        else
                        {
                            llamado = listaMetodos.Find(x => (x.Clase == clase.Name && x.Nombre == nombre));
                        }//Llamado
                    }



                    if (val != null && (llamado != null || currentSemanticModel.GetSymbolInfo(expression).Symbol.Kind == SymbolKind.Field))
                    {

                        for (int i = 0; i < declarationSyntax.Count; i++)
                        {
                            if (llamado != null)
                            {
                                llamado.EsLlamado = true;
                            }
                            if (declarationSyntax.ElementAt(i).Contains(expression))
                            {
                                foreach (ClassDeclarationSyntax classSyntax in classDeclarationSyntax)
                                {
                                    if (classSyntax.Contains(declarationSyntax.ElementAt(i)))
                                    {
                                        Metodo llamador = listaMetodos.Find(x => (x.Clase == classSyntax.Identifier.ToString() &&
                                                                             x.Nombre == declarationSyntax.ElementAt(i).Identifier.ToString())
                                                                            );//Llamador
                                        if (llamador != null)
                                        {
                                            llamador.EsLlamador = true;

                                            Llamada nuevaLlamada = new Llamada(currentSemanticModel.GetSymbolInfo(expression).Symbol.ContainingType.Name
                                                                                , currentSemanticModel.GetSymbolInfo(expression).Symbol.Name
                                                                                , currentSemanticModel.GetSymbolInfo(expression).Symbol.Kind);

                                            llamador.ListaLlamadas.Add(nuevaLlamada);
                                        }

                                        /*Console.WriteLine("JsonClass: " + classSyntax.Identifier);
                                        Console.WriteLine("Metodo que invoca: " + declarationSyntax.ElementAt(i).Identifier);
                                        Console.WriteLine("JsonClass invocada: " + currentSemanticModel.GetSymbolInfo(expression).Symbol.ContainingType.Name);
                                        Console.WriteLine("Metodo invocado: " + currentSemanticModel.GetSymbolInfo(expression).Symbol.Name);
                                        Console.WriteLine("--------------------------------------");*/

                                    }
                                }
                            }

                        }
                    }

                }
                contadorSemanticModel++;
            }
            //Imprimir lista:
            /*
             if (listaMetodos !=null) {
                 foreach (Metodo invocacion in listaMetodos)
                 {
                     Console.WriteLine("JsonClass: "+invocacion.JsonClass);
                    Console.WriteLine("Tipo: " + invocacion.Tipo);
                    Console.WriteLine("Método/Atributo: "+invocacion.Nombre);
                     Console.WriteLine("Cantidad de metodos/attributes que llama: "+invocacion.ListaLlamadas.Count());
                    Console.WriteLine("\t----------------------------------------------------------");
                    for (int i = 0; i < invocacion.ListaLlamadas.Count(); i++)
                     {
                        Console.WriteLine("\t JsonClass: " + invocacion.ListaLlamadas.ElementAt(i).JsonClass);
                        Console.WriteLine("\t Método: " + invocacion.ListaLlamadas.ElementAt(i).Metodo_atributo);
                        Console.WriteLine("\t Tipo: " + invocacion.ListaLlamadas.ElementAt(i).Tipo);
                        Console.WriteLine("\t----------------------------------------------------------");
                    }

                    Console.WriteLine("\t----------------------------------------------------------");
                    Console.WriteLine("Es llamador: " + invocacion.EsLlamador);
                    Console.WriteLine("Es llamado: " + invocacion.EsLlamado);
                    Console.WriteLine("******************************************************************");
                }

             }
             */
            // grafo.imprimirMatrizDistancias();
            //***********************************************
            //* Aca comienza el paso 3 del pseudocódigo     *
            //*                                             *
            //***********************************************

            Grafo grafo = new Grafo(listaMetodos);
            //grafo.imprimirMatrizAdyacencia();
            Console.WriteLine("\n\n");
            //grafo.WarshallTransitiveClousure();
            //grafo.imprimirMatrizAdyacencia();
            grafo.recorrerDFS();
           // grafo.imprimirCadenas();
            Console.WriteLine("\n\n");
            Console.WriteLine("******************************************METRICAS DE EXPORT******************************************");
            //***********************************************
            //* Aca comienza el paso de aplicar metricas    *
            //*                                             *
            //***********************************************
            MetricCalculator metrics = new MetricCalculator(listaMetodos, grafo.ListaDeCadenas);
            metrics.exportCalculator();
            metrics.importCalculator();
            metrics.calculateAVG();
            metrics.calculatePairs();

            FolderBrowserDialog salida = new FolderBrowserDialog();
            System.IO.StreamWriter output;
            //System.IO.StreamWriter output = new System.IO.StreamWriter(@"C:\Users\Steven\Documents\GitHub\AnalizadorDFSMaster\AnalizadorDFS\output.txt");
            salida.SelectedPath = @"C:\Users\jnavas\source\repos\avibanalysis\ExtractIndirectCoupling\output";

            if (salida.ShowDialog() == DialogResult.OK)
            {
                output = new System.IO.StreamWriter(@"" + salida.SelectedPath + "\\output.txt");


                // Send output to a file
                //System.IO.StreamWriter output = new System.IO.StreamWriter(@"C:\Users\jnavas\source\repos\ExtractIndirectCoupling\output.txt");
                //System.IO.StreamWriter output = new System.IO.StreamWriter(@"C:\Users\jnavas\source\repos\AnalizadorDFS\AnalizadorDFS\output.txt");

                grafo.imprimirCadenas(output);

                /*foreach (Metodo invocacion in listaMetodos)
                {
                    if (invocacion.EsLlamador || invocacion.EsLlamado)
                    {
                        output.WriteLine("JsonClass: " + invocacion.JsonClass);
                        output.WriteLine("Tipo: " + invocacion.Tipo);
                        output.WriteLine("Método/Atributo: " + invocacion.Nombre);
                        output.WriteLine("CYCLO: " + invocacion.ComplejidadCiclomatica);
                        output.WriteLine("LOC:   " + invocacion.CantidadLineasMetodo);

                        output.WriteLine("...........  EXPORTS  ...............................");
                        output.WriteLine("             Loc  Cyclo Const");
                        output.WriteLine(String.Format("ExportSum: {0,5}  {1,5} {1,5}", invocacion.ExportLOCSum, invocacion.ExportCLYCLOSum, invocacion.ExportConstantSum));
                        output.WriteLine(String.Format("ExportMin: {0,5}  {1,5} {1,5}", invocacion.ExportLOCMin, invocacion.ExportCLYCLOMin, invocacion.ExportConstantMin));
                        output.WriteLine(String.Format("ExportMax: {0,5}  {1,5} {1,5}", invocacion.ExportLOCMax, invocacion.ExportCLYCLOMax, invocacion.ExportConstantMax));
                        output.WriteLine(String.Format("ExportAVG: {0,5:N2}  {1,5:N2} {1,5:N2}", invocacion.ExportLOCAvg, invocacion.ExportCLYCLOAvg, invocacion.ExportConstantAvg));

                        output.WriteLine("...........  IMPORTS  ...............................");
                        output.WriteLine("             Loc  Cyclo Const");
                        output.WriteLine(String.Format("ImportSum: {0,5}  {1,5} {1,5}", invocacion.ImportLOCSum, invocacion.ImportCLYCLOSum, invocacion.ImportConstantSum));
                        output.WriteLine(String.Format("ImportMin: {0,5}  {1,5} {1,5}", invocacion.ImportLOCMin, invocacion.ImportCLYCLOMin, invocacion.ImportConstantMin));
                        output.WriteLine(String.Format("ImportMax: {0,5}  {1,5} {1,5}", invocacion.ImportLOCMax, invocacion.ImportCLYCLOMax, invocacion.ImportConstantMax));
                        output.WriteLine(String.Format("ImportAVG: {0,5:N2}  {1,5:N2} {1,5:N2}", invocacion.ImportLOCAvg, invocacion.ImportCLYCLOAvg, invocacion.ImportConstantAvg));
                        output.WriteLine("******************************************************************");
                    }

                }*/

                // sort methods in descending order by their Maximum Export Cyclomatic Complexity
                listaMetodos.Sort(delegate (Metodo m_i, Metodo m_j)
                {
                    if (!m_i.EsLlamado && !m_i.EsLlamador && !m_j.EsLlamado && !m_j.EsLlamador) return 0; // both are not part of any chain
                else if (!m_i.EsLlamado && !m_i.EsLlamador) return 1; // isolated methods are less
                else if (!m_j.EsLlamado && !m_j.EsLlamador) return -1; // same as before
                else return m_j.ExportCLYCLOMax.CompareTo(m_i.ExportCLYCLOMax);
                });

                output.WriteLine("\n\n*");
                output.WriteLine("*");
                output.WriteLine("* 10 con mayor Rigidez Cyclomática Máxima (EXPORT)");
                output.WriteLine("*");
                output.WriteLine(String.Format("* CXmin={0} CXmax={1}", metrics.ExportNormal.CycloMaxMin, metrics.ExportNormal.CycloMaxMax));
                output.WriteLine("");
                output.WriteLine(String.Format("{0,-6} {1,10:N2} {2,10} {3,10} {4,10} {5,10:N2} {6,10:N2} {7,10:N2} {8,10} {9,10} {10,10} {11,10} {12,10} {13,10}",
                                               "Método", "*NCX", "LS", "CS", "KS", "LA", "CA", "KA", "LI", "CI", "KI", "LX", "*CX", "KX"));
                for (int i = 0; i < 10; i++)
                {
                    Metodo invocacion = listaMetodos[i];
                    if (invocacion.EsLlamador || invocacion.EsLlamado)
                    {
                        output.WriteLine(String.Format("{0,-6} {1,10:N2} {2,10} {3,10} {4,10} {5,10:N2} {6,10:N2} {7,10:N2} {8,10} {9,10} {10,10} {11,10} {12,10} {13,10}",
                                        i + 2, //invocacion.JsonClass + "."+ invocacion.Nombre, 
                                        metrics.ExportNormal.NormalizeCycloMax(invocacion.ExportCLYCLOMax),
                                        invocacion.ExportLOCSum, invocacion.ExportCLYCLOSum, invocacion.ExportConstantSum,
                                        invocacion.ExportLOCAvg, invocacion.ExportCLYCLOAvg, invocacion.ExportConstantAvg,
                                        invocacion.ExportLOCMin, invocacion.ExportCLYCLOMin, invocacion.ExportConstantMin,
                                        invocacion.ExportLOCMax, invocacion.ExportCLYCLOMax, invocacion.ExportConstantMax));
                    }
                }

                // sort methods in descending order by their Total Import Cyclomatic Complexity
                listaMetodos.Sort(delegate (Metodo m_i, Metodo m_j)
                {
                    if (!m_i.EsLlamado && !m_i.EsLlamador && !m_j.EsLlamado && !m_j.EsLlamador) return 0; // both are not part of any chain
                else if (!m_i.EsLlamado && !m_i.EsLlamador) return 1; // isolated methods are less
                else if (!m_j.EsLlamado && !m_j.EsLlamador) return -1; // same as before
                else return m_j.ImportCLYCLOMax.CompareTo(m_i.ImportCLYCLOMax);
                });

                output.WriteLine("\n\n*");
                output.WriteLine("*");
                output.WriteLine("* 10 con mayor Fragilidad Cyclomática Máxima (IMPORT)");
                output.WriteLine("*");
                output.WriteLine(String.Format("* CXmin={0} CXmax={1}", metrics.ImportNormal.CycloMaxMin, metrics.ImportNormal.CycloMaxMax));
                output.WriteLine("");
                output.WriteLine(String.Format("{0,-6} {1,10:N2} {2,10} {3,10} {4,10} {5,10:N2} {6,10:N2} {7,10:N2} {8,10} {9,10} {10,10} {11,10} {12,10} {13,10}",
                                               "Método", "*NCX", "LS", "CS", "KS", "LA", "CA", "KA", "LI", "CI", "KI", "LX", "*CX", "KX"));
                for (int i = 0; i < 10; i++)
                {
                    Metodo invocacion = listaMetodos[i];
                    if (invocacion.EsLlamador || invocacion.EsLlamado)
                    {
                        output.WriteLine(String.Format("{0,-6} {1,10:N2} {2,10} {3,10} {4,10} {5,10:N2} {6,10:N2} {7,10:N2} {8,10} {9,10} {10,10} {11,10} {12,10} {13,10}",
                                                       i + 1, //invocacion.JsonClass + "." + invocacion.Nombre, 
                                                       metrics.ImportNormal.NormalizeCycloMax(invocacion.ImportCLYCLOMax),
                                                       invocacion.ImportLOCSum, invocacion.ImportCLYCLOSum, invocacion.ImportConstantSum,
                                                       invocacion.ImportLOCAvg, invocacion.ImportCLYCLOAvg, invocacion.ImportConstantAvg,
                                                       invocacion.ImportLOCMin, invocacion.ImportCLYCLOMin, invocacion.ImportConstantMin,
                                                       invocacion.ImportLOCMax, invocacion.ImportCLYCLOMax, invocacion.ImportConstantMax));
                    }
                }

                List<Pair> listaPares = metrics.obtainMaxPairs();

                //foreach (Pair pair in metrics.ListaPares)
                //    output.WriteLine(pair.ToString());

                output.WriteLine("\n\n*");
                output.WriteLine("*");
                output.WriteLine("* 10 pares de métodos con mayor Fuerza de Acoplamiento Cyclomática Maxima");
                output.WriteLine("*");
                output.WriteLine(String.Format("* CXmin={0} CXmax={1}", metrics.PairNormal.CycloMaxMin, metrics.PairNormal.CycloMaxMax));
                output.WriteLine("");
                output.WriteLine(String.Format("{0,-6} {1,-6} {2,10} {3,10} {4,10:N2} {5,10} {6,10:N2} {7,10:N2} {8,10:N2} {9,10} {10,10} {11,10} {12,10} {13,10} {14,10}",
                                               "Caller", "Callee", "LS", "CS", "*NCX", "KS", "LA", "CA", "KA", "LI", "CI", "KI", "LX", "*CX", "KX"));
                for (int i = 0; i < 10; i++)
                {
                    Pair invocacion = listaPares[i];
                    if (invocacion != null)
                    {
                        output.WriteLine(String.Format("{0,-6} {1,-6} {2,10} {3,10} {4,10:N2} {5,10} {6,10:N2} {7,10:N2} {8,10:N2} {9,10} {10,10} {11,10} {12,10} {13,10} {14,10}",
                                                       i + 1, //invocacion.NombreI,
                                                       " ", //invocacion.NombreD,
                                                       invocacion.LocSum, invocacion.CycloSum, metrics.PairNormal.NormalizeCycloMax(invocacion.CycloMax), invocacion.ConstantSum,
                                                       invocacion.LocAvg, invocacion.CycloAvg, invocacion.ConstantAvg,
                                                       invocacion.LocMin, invocacion.CycloMin, invocacion.ConstantMin,
                                                       invocacion.LocMax, invocacion.CycloMax, invocacion.ConstantMax));
                    }
                }
                output.WriteLine("\n\nCantidad de LOC: " + Metodo.TotalDeLOC);
                output.WriteLine("Cantidad de métodos: " + Metodo.CantidadMetodos);
                output.WriteLine("Cantidad de classes: " + cantidadClases);
                output.WriteLine("Cadena más larga: " + grafo.CantidadMayorMetodosEnCadena);
                output.WriteLine("Cantidad de cadenas: " + grafo.ListaDeCadenas.Count);

                output.Flush();

                Console.Read();
            }
        }

        [STAThread]
        private static Compilation CreateTestCompilation()//JsonClass para la creacion de los árboles de sintaxis
        {
            FolderBrowserDialog entrada = new FolderBrowserDialog();
            //entrada.SelectedPath = @"C:\Users\Administrador\Documents\repos\roslyn";
            entrada.SelectedPath = @"C:\Users\jnavas\source\repos\avibanalysis\ExtractIndirectCoupling\Ejemplo";
            //entrada.SelectedPath = @"C:\Users\jnavas\source\repos";
            //entrada.SelectedPath = @"C:\Users\Steven\Desktop\Sources\";
            entrada.Description = @"Input folder";
            if (entrada.ShowDialog() == DialogResult.OK)
            {
                Console.WriteLine(entrada.SelectedPath);
                // creation of the syntax tree for every file    
                //String programPath = @"C:\Users\Steven\Desktop\Código prueba\shadowsocks-windows-master";
                //  String programPath = @"C:\Users\Steven\Desktop\Código prueba\mongo-csharp-driver-master\mongo-csharp-driver-master\src";//         <--------------------------- DIRECTORIO DONDE ESTÁN LOS ARCHIVOS .cs
                //String programPath = @"C:\Users\jnavas\source\repos\AnalizadorDFS\AnalizadorDFS\Ejemplo";//         <--------------------------- DIRECTORIO DONDE ESTÁN LOS ARCHIVOS .cs
                //String programPath = @""+ entrada.SelectedPath;
                // String programPath = @"" + entrada.SelectedPath;
                //String programPath = @"C:\Users\jnavas\source\repos\netmf";
                //String programPath = @"C:\Users\jnavas\source\repos\mongo";

                //String programPath = @"C:\Users\jnavas\source\repos\roslyn";
                String programPath = @"" + entrada.SelectedPath;
                //String programPath = @"C:\Users\Steven\Documents\GitHub\AnalizadorDFS\AnalizadorDFS\Ejemplo";//         <--------------------------- DIRECTORIO DONDE ESTÁN LOS ARCHIVOS .cs
                //String programPath = @"C:\Users\jnavas\source\repos\AnalizadorDFS\AnalizadorDFS\Ejemplo";//         <--------------------------- DIRECTORIO DONDE ESTÁN LOS ARCHIVOS .cs

                string nombreDelProyecto = Path.GetFileName(programPath);
                var csFiles = Directory.EnumerateFiles(programPath, "*.cs", SearchOption.AllDirectories);//Crea una coleccion de directorios de los archivos que encuentre

                List<SyntaxTree> sourceTrees = new List<SyntaxTree>();//Lista para almacenar los SyntaxTrees que se van a crear


                foreach (string currentFile in csFiles)
                {//Loop que recorre toda la coleccion de archivos


                    String programText = File.ReadAllText(currentFile);//Lee el archivo y lo guarda en un string
                    SyntaxTree programTree = CSharpSyntaxTree.ParseText(programText).WithFilePath(currentFile);//Crea el SyntaxTree para el archivo actual con el string 


                    sourceTrees.Add(programTree);//Guarda el archivo ya parseado dentro de la lista

                }
                // gathering the assemblies
                MetadataReference mscorlib = MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location);
                MetadataReference codeAnalysis = MetadataReference.CreateFromFile(typeof(SyntaxTree).GetTypeInfo().Assembly.Location);
                MetadataReference csharpCodeAnalysis = MetadataReference.CreateFromFile(typeof(CSharpSyntaxTree).GetTypeInfo().Assembly.Location);
                MetadataReference[] references = { mscorlib, codeAnalysis, csharpCodeAnalysis };

                // compilation
                return CSharpCompilation.Create(nombreDelProyecto,
                                 sourceTrees,
                                 references,
                                 new CSharpCompilationOptions(OutputKind.ConsoleApplication));

            }
            return null;
        }

        private static void CollapseGraphSCC()
        {
            foreach (KeyValuePair<string, JsonMethod> entry in JsonMethod.Methods)
            {
                JsonMethod n = entry.Value;
                if (n.Visited == false)
                {
                    GabowDFS(n, 0);
                }
            }

            /*
            long maxLen = 0;
            long avgLen = 0;
            long count = 0;
            foreach (JsonMethod m in JsonMethod.SccList)
            {
                long c = 0;
                Console.Write("SCC" + m.SccId + " (" + m.SccMethods.Count + "): ");
                foreach (JsonMethod s in m.SccMethods)
                {
                    c++;
                    Console.Write(s.Name + ", ");
                }
                Console.WriteLine();
                maxLen = Math.Max(maxLen, c);
                avgLen += c;
                count++;
            }
            Console.WriteLine("Count: " + count.ToString() + ", Max: " + maxLen.ToString() + ", Avg: " + (avgLen / count).ToString());
            Console.Read();
            */
        }

        private static void GabowDFS(JsonMethod n, int depth)
        {
            Stack<JsonMethod> P = JsonMethod.P;
            Stack<JsonMethod> R = JsonMethod.R;

            n.Visited = true;
            n.Pre = JsonMethod.Prev++;

            P.Push(n);
            R.Push(n);

            foreach (JsonCall n_m in n.Calls)
            {
                JsonMethod m = n_m.Method;

                if (m.Visited == false)
                {
                    GabowDFS(m, depth+1);
                }
                else if (m.SccId == -1)
                {
                    while (R.Count > 0 && R.Peek().Pre > m.Pre)
                    {
                        R.Pop();
                    }
                }
            }

            if (R.Count > 0 && R.Peek().Id == n.Id)
            {
                R.Pop();

                if (P.Count > 0 && P.Peek().Id == n.Id)
                {
                    P.Peek().SccId = P.Peek().Id;
                    P.Pop();
                }
                else
                {
                    JsonMethod o, scc;

                    scc = new JsonMethod(JsonProject.Nextid, "SCC" + JsonProject.Nextid);
                    JsonProject.Nextid++;
                    scc.SccId = scc.Id;
                    scc.IsMethod = false;
                    scc.IsScc = true;

                    JsonMethod.SccList.Add(scc);

                    do
                    {
                        o = P.Pop();
                        o.SccId = scc.Id;
                        o.Scc = scc;
                        o.IsCollapsed = true;

                        scc.SccMethods.Add(o);
                        scc.Calls.AddRange(o.Calls);
                        scc.CalledBy.AddRange(o.CalledBy);

                        //JsonMethod.count_gabow_methods++;

                    } while (P.Count > 0 && o.Id != n.Id);

                    //JsonMethod.count_gabow_calls += scc.Calls.FindAll(c => c.Method.SccId == scc.SccId).Count();
                    //JsonMethod.count_gabow_calls += scc.CalledBy.FindAll(c => c.Method.SccId == scc.SccId).Count();

                    // Remove self references
                    scc.Calls.RemoveAll(c => c.Method.SccId == scc.SccId);
                    scc.CalledBy.RemoveAll(c => c.Method.SccId == scc.SccId);

                    // Remove duplicate references
                    scc.Calls = scc.Calls.Distinct().ToList();
                    scc.CalledBy = scc.CalledBy.Distinct().ToList();
                }
            }
        }

        private static void SaveNeo4JGraph(JsonProject project)
        {
            SaveGraphCSV(project);
            UploadCSVtoNeo4J();

            // queries utiles
            //
            // obtener los objetos en un namespace por nombre (namespace, clases y metodos)
            //      match ((n {qualifiedname:'Microsoft.CodeAnalysis.CSharp.DesignerAttributes'})-->(c)) return n,c
            // borrar todos los objetos
            //      match(n: Scc) detach delete n
            //
            // match (m:Method) create(s: Scc { scc: m.scc})-[:COLLAPSES]->(m)
        }

        private static void UploadCSVtoNeo4J()
        {
            var driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "123"));
            using (var session = driver.Session(AccessMode.Write))
            {
                // CANNOT USE --> USING PERIODIC COMMIT 500

                session.WriteTransaction(tx => tx.Run(@"LOAD CSV FROM ""file:///project.csv"" as f
                                                        CREATE(:Project { id: f[0] })"));
                session.WriteTransaction(tx => tx.Run(@"CREATE CONSTRAINT ON (p:Project) ASSERT p.id IS UNIQUE"));

                session.WriteTransaction(tx => tx.Run(@"LOAD CSV FROM ""file:///namespaces.csv"" as f
                                                        MERGE (p:Project { id: f[3] })
                                                        CREATE (n:Namespace { id: f[0], name: f[1], qualifiedname: f[2] })
                                                        CREATE (p)-[:HAS_NAMESPACE]->(n)"));
                session.WriteTransaction(tx => tx.Run(@"CREATE CONSTRAINT ON (n:Namespace) ASSERT n.id IS UNIQUE"));

                session.WriteTransaction(tx => tx.Run(@"LOAD CSV FROM ""file:///hierarchy.csv"" as f
                                                        MERGE (n1:Namespace { id: f[0] })
                                                        MERGE (n2:Namespace { id: f[1] })
                                                        CREATE (n1)-[:CONTAINS_NAMESPACE]->(n2)"));

                session.WriteTransaction(tx => tx.Run(@"LOAD CSV FROM ""file:///classes.csv"" as f
                                                        MERGE (p:Project { id: f[4] })
                                                        MERGE (n:Namespace { id: f[3] })
                                                        CREATE (c:Class { id: f[0], name: f[1], qualifiedname: f[2] })
                                                        CREATE (p)-[:HAS_CLASS]->(c)
                                                        CREATE (n)-[:CONTAINS_CLASS]->(c)"));
                session.WriteTransaction(tx => tx.Run(@"CREATE CONSTRAINT ON (c:Class) ASSERT c.id IS UNIQUE"));

                session.WriteTransaction(tx => tx.Run(@"LOAD CSV FROM ""file:///methods.csv"" as f
                                                        MERGE (p:Project { id: f[4] })
                                                        MERGE (n:Namespace { id: f[3] })
                                                        MERGE (c:Class { id: f[2] })
                                                        CREATE (m:Method { id: f[0], name: f[1], 
                                                            lines:   toInt(f[ 5]), cyclomatic: toInt(f[6]),  constant: toInt(f[7]),
                                                            icrlmin: toInt(f[ 8]),    icrlmax: toInt(f[9]),  icrlavg: toInt(f[10]), icrlsum: toInt(f[11]),
                                                            icflmin: toInt(f[12]),    icflmax: toInt(f[13]), icflavg: toInt(f[14]), icflsum: toInt(f[15]),
                                                            icrcmin: toInt(f[16]),    icrcmax: toInt(f[17]), icrcavg: toInt(f[18]), icrcsum: toInt(f[19]),
                                                            icfcmin: toInt(f[20]),    icfcmax: toInt(f[21]), icfcavg: toInt(f[22]), icfcsum: toInt(f[23]),
                                                            icrkmin: toInt(f[24]),    icrkmax: toInt(f[25]), icrkavg: toInt(f[26]), icrksum: toInt(f[27]),
                                                            icfkmin: toInt(f[28]),    icfkmax: toInt(f[29]), icfkavg: toInt(f[30]), icfksum: toInt(f[31]),
                                                            ismethod: toInt(f[31]),   iscollapsed: toInt(f[32]), isrecursive: toInt(f[33])
                                                         })
                                                        CREATE (p)-[:HAS_METHOD]->(m)
                                                        CREATE (n)-[:CONTAINS_METHOD]->(m)
                                                        CREATE (c)-[:OWNS_METHOD]->(m)"));
                session.WriteTransaction(tx => tx.Run(@"LOAD CSV FROM ""file:///sccs.csv"" as f
                                                        MERGE (p:Project { id: f[4] })
                                                        CREATE (m:Method { id: f[0], name: f[1], 
                                                            lines:   toInt(f[ 5]), cyclomatic: toInt(f[6]),  constant: toInt(f[7]),
                                                            icrlmin: toInt(f[ 8]),    icrlmax: toInt(f[9]),  icrlavg: toInt(f[10]), icrlsum: toInt(f[11]),
                                                            icflmin: toInt(f[12]),    icflmax: toInt(f[13]), icflavg: toInt(f[14]), icflsum: toInt(f[15]),
                                                            icrcmin: toInt(f[16]),    icrcmax: toInt(f[17]), icrcavg: toInt(f[18]), icrcsum: toInt(f[19]),
                                                            icfcmin: toInt(f[20]),    icfcmax: toInt(f[21]), icfcavg: toInt(f[22]), icfcsum: toInt(f[23]),
                                                            icrkmin: toInt(f[24]),    icrkmax: toInt(f[25]), icrkavg: toInt(f[26]), icrksum: toInt(f[27]),
                                                            icfkmin: toInt(f[28]),    icfkmax: toInt(f[29]), icfkavg: toInt(f[30]), icfksum: toInt(f[31]),
                                                            ismethod: toInt(f[31]),   iscollapsed: toInt(f[32]), isrecursive: toInt(f[33])
                                                         })
                                                        CREATE (p)-[:HAS_METHOD]->(m)"));
                session.WriteTransaction(tx => tx.Run(@"CREATE CONSTRAINT ON (m:Method) ASSERT m.id IS UNIQUE"));

                session.WriteTransaction(tx => tx.Run(@"LOAD CSV FROM ""file:///calls.csv"" as f
                                                        MERGE (m1:Method { id: f[0] })
                                                        MERGE (m2:Method { id: f[1] })
                                                        CREATE (m1)-[:CALLS]->(m2)"));

                session.WriteTransaction(tx => tx.Run(@"LOAD CSV FROM ""file:///scccalls.csv"" as f
                                                        MERGE (m1:Method { id: f[0] })
                                                        MERGE (m2:Method { id: f[1] })
                                                        CREATE (m1)-[:CALLS]->(m2)"));

                session.WriteTransaction(tx => tx.Run(@"LOAD CSV FROM ""file:///collapses.csv"" as f
                                                        MERGE (m1:Method { id: f[0] })
                                                        MERGE (m2:Method { id: f[1] })
                                                        CREATE (m1)-[:COLLAPSES]->(m2)"));

                session.WriteTransaction(tx => tx.Run(@"LOAD CSV FROM ""file:///ics.csv"" as f
                                                        MERGE (m1:Method { id: f[0] })
                                                        MERGE (m2:Method { id: f[1] })
                                                        CREATE (m1)-[:ICS { 
                                                            icslmin: toInt(f[ 2]), icslmax: toInt(f[ 3]), icslavg: toInt(f[ 4]), icslsum: toInt(f[ 5]),
                                                            icscmin: toInt(f[ 6]), icscmax: toInt(f[ 7]), icscavg: toInt(f[ 8]), icscsum: toInt(f[ 9]),
                                                            icskmin: toInt(f[10]), icskmax: toInt(f[11]), icskavg: toInt(f[12]), icsksum: toInt(f[13])
                                                        }]->(m2)"));

                session.WriteTransaction(tx => tx.Run(@"DROP CONSTRAINT ON (p:Project) ASSERT p.id IS UNIQUE"));
                session.WriteTransaction(tx => tx.Run(@"DROP CONSTRAINT ON (n:Namespace) ASSERT n.id IS UNIQUE"));
                session.WriteTransaction(tx => tx.Run(@"DROP CONSTRAINT ON (c:Class) ASSERT c.id IS UNIQUE"));
                session.WriteTransaction(tx => tx.Run(@"DROP CONSTRAINT ON (m:Method) ASSERT m.id IS UNIQUE"));
                session.WriteTransaction(tx => tx.Run(@"MATCH (x) WHERE x:Namespace OR x:Class OR x:Method REMOVE x.id"));
            }
        }


        private static void SaveGraphCSV(JsonProject project)
        {
            string import_path = Environment.ExpandEnvironmentVariables(@"%NEO4J_HOME%\import\");
            System.IO.StreamWriter projectSW = new System.IO.StreamWriter(String.Format(@"{0}{1}", import_path, @"project.csv"), false);
            System.IO.StreamWriter namespacesSW = new System.IO.StreamWriter(String.Format(@"{0}{1}", import_path, @"namespaces.csv"), false);
            System.IO.StreamWriter hierarchySW = new System.IO.StreamWriter(String.Format(@"{0}{1}", import_path, @"hierarchy.csv"), false);
            System.IO.StreamWriter classesSW = new System.IO.StreamWriter(String.Format(@"{0}{1}", import_path, @"classes.csv"), false);
            System.IO.StreamWriter methodsSW = new System.IO.StreamWriter(String.Format(@"{0}{1}", import_path, @"methods.csv"), false);
            System.IO.StreamWriter sccsSW = new System.IO.StreamWriter(String.Format(@"{0}{1}", import_path, @"sccs.csv"), false);
            System.IO.StreamWriter callsSW = new System.IO.StreamWriter(String.Format(@"{0}{1}", import_path, @"calls.csv"), false);
            System.IO.StreamWriter scccallsSW = new System.IO.StreamWriter(String.Format(@"{0}{1}", import_path, @"scccalls.csv"), false);
            System.IO.StreamWriter collapsesSW = new System.IO.StreamWriter(String.Format(@"{0}{1}", import_path, @"collapses.csv"), false);
            System.IO.StreamWriter icsSW = new System.IO.StreamWriter(String.Format(@"{0}{1}", import_path, @"ics.csv"), false);

            projectSW.WriteLine(String.Format(@"{0}", project.Name));
            projectSW.Flush();
            projectSW.Close();
            projectSW.Dispose();

            foreach (JsonNamespace ns in project.Namespaces)
            {
                namespacesSW.WriteLine(String.Format(@"{0}{1},{2},{3},{0}", project.Name, ns.Id, ns.Name, ns.Fullname));
                if (ns.ParentId != -1)
                {
                    hierarchySW.WriteLine(String.Format(@"{0}{1},{0}{2}", project.Name, ns.ParentId, ns.Id));
                }
                SaveNamespacesCSV(project.Name, ns.ChildNamespaces, namespacesSW, hierarchySW);
            }
            namespacesSW.Flush();
            namespacesSW.Close();
            namespacesSW.Dispose();
            hierarchySW.Flush();
            hierarchySW.Close();
            hierarchySW.Dispose();

            foreach (KeyValuePair<string, JsonClass> entry in JsonClass.Classes)
            {
                JsonClass c = entry.Value;
                classesSW.WriteLine(String.Format(@"{0}{1},{2},{3},{0}{4},{0}", project.Name, c.Id, c.Name, c.Fullname, c.NamespaceId));
            }
            classesSW.Flush();
            classesSW.Close();
            classesSW.Dispose();

            foreach (KeyValuePair<string, JsonMethod> entry in JsonMethod.Methods)
            {
                JsonMethod m = entry.Value;
                methodsSW.WriteLine(String.Format(@"{0}{1},{2},{0}{3},{0}{4},{0},{5},{6},{7}," +
                    " {8}, {9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19}," +
                    "{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31}" +
                    "{32},{33},{34}", 
                    project.Name, m.Id, m.Fullname, m.ClassId, m.NamespaceId, m.Loc, m.Cyc, m.Kon,
                    m.Loc_metrics.Fmin, m.Loc_metrics.Fmax, m.Loc_metrics.Favg, m.Loc_metrics.Fsum,
                    m.Loc_metrics.Bmin, m.Loc_metrics.Bmax, m.Loc_metrics.Bavg, m.Loc_metrics.Bsum,
                    m.Cyc_metrics.Fmin, m.Cyc_metrics.Fmax, m.Cyc_metrics.Favg, m.Cyc_metrics.Fsum,
                    m.Cyc_metrics.Bmin, m.Cyc_metrics.Bmax, m.Cyc_metrics.Bavg, m.Cyc_metrics.Bsum,
                    m.Kon_metrics.Fmin, m.Kon_metrics.Fmax, m.Kon_metrics.Favg, m.Kon_metrics.Fsum,
                    m.Kon_metrics.Bmin, m.Kon_metrics.Bmax, m.Kon_metrics.Bavg, m.Kon_metrics.Bsum,
                    m.IsMethod?"1":"0", m.IsCollapsed?"1":"0", m.IsRecursive?"1":"0"));

                foreach (JsonCall c in m.Calls)
                {
                    callsSW.WriteLine(String.Format(@"{0}{1},{0}{2}", project.Name, m.Id, c.Id));
                }
            }

            foreach (JsonMethod m in JsonMethod.SccList)
            {
                sccsSW.WriteLine(String.Format(@"{0}{1},{2},{3},{4},{0},{5},{6},{7}," +
                    " {8}, {9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19}," +
                    "{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31}" +
                    "{32},{33},{34}",
                    project.Name, m.Id, m.Fullname, "", "", m.Loc, m.Cyc, m.Kon,
                    m.Loc_metrics.Fmin, m.Loc_metrics.Fmax, m.Loc_metrics.Favg, m.Loc_metrics.Fsum,
                    m.Loc_metrics.Bmin, m.Loc_metrics.Bmax, m.Loc_metrics.Bavg, m.Loc_metrics.Bsum,
                    m.Cyc_metrics.Fmin, m.Cyc_metrics.Fmax, m.Cyc_metrics.Favg, m.Cyc_metrics.Fsum,
                    m.Cyc_metrics.Bmin, m.Cyc_metrics.Bmax, m.Cyc_metrics.Bavg, m.Cyc_metrics.Bsum,
                    m.Kon_metrics.Fmin, m.Kon_metrics.Fmax, m.Kon_metrics.Favg, m.Kon_metrics.Fsum,
                    m.Kon_metrics.Bmin, m.Kon_metrics.Bmax, m.Kon_metrics.Bavg, m.Kon_metrics.Bsum,
                    m.IsMethod ? "1" : "0", m.IsCollapsed ? "1" : "0", m.IsRecursive ? "1" : "0"));

                foreach (JsonCall c in m.Calls)
                {
                    scccallsSW.WriteLine(String.Format(@"{0}{1},{0}{2}", project.Name, m.Id, c.Id));
                }
                foreach (JsonMethod d in m.SccMethods)
                {
                    collapsesSW.WriteLine(String.Format(@"{0}{1},{0}{2}", project.Name, m.Id, d.Id));
                }
            }
            methodsSW.Flush();
            methodsSW.Close();
            methodsSW.Dispose();
            callsSW.Flush();
            callsSW.Close();
            callsSW.Dispose();
            sccsSW.Flush();
            sccsSW.Close();
            sccsSW.Dispose();
            scccallsSW.Flush();
            scccallsSW.Close();
            scccallsSW.Dispose();
            collapsesSW.Flush();
            collapsesSW.Close();
            collapsesSW.Dispose();

            for (int m1 = 0; m1 < JsonMethod.MaxMethods; m1++)
                for (int m2 = 0; m2 < JsonMethod.MaxMethods; m2++)
                    if (m1 != m2 && JsonMethod.PairMetricsList.IsCellPresent(m1, m2))
                    {
                        PairMetrics pair = JsonMethod.PairMetricsList[m1, m2];
                        icsSW.WriteLine(String.Format(@"{0}{1},{0}{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}",
                            project.Name, m1, m2,
                            pair.L.Fmin, pair.L.Fmax, pair.L.Favg, pair.L.Fsum,
                            pair.C.Fmin, pair.C.Fmax, pair.C.Favg, pair.C.Fsum,
                            pair.K.Fmin, pair.K.Fmax, pair.K.Favg, pair.K.Fsum));
                    }
            icsSW.Flush();
            icsSW.Close();
            icsSW.Dispose();
        }

        private static void SaveNamespacesCSV(string projectName, List<JsonNamespace> childNamespaces, System.IO.StreamWriter namespacesSW, System.IO.StreamWriter hierarchySW)
        {
            foreach (JsonNamespace ns in childNamespaces)
            {
                namespacesSW.WriteLine(String.Format(@"{0}{1},{2},{3},{0}", projectName, ns.Id, ns.Name, ns.Fullname));
                if (ns.ParentId != -1)
                {
                    hierarchySW.WriteLine(String.Format(@"{0}{1},{0}{2}", projectName, ns.ParentId, ns.Id));
                }
                SaveNamespacesCSV(projectName, ns.ChildNamespaces, namespacesSW, hierarchySW);
            }
        }

        private static void connectNeo4J(JsonProject project, string path)
        {
            System.IO.StreamWriter output = new System.IO.StreamWriter(path + @"\queryNeo4J.txt");

            var driver = GraphDatabase.Driver("bolt://localhost:7687", AuthTokens.Basic("neo4j", "123"));
            var session = driver.Session();
            string query = "";
            query += "CREATE(project:Project {nameProject:" + "'" + project.Name + "'" + "}) \n";
            //Se armar toda la estructura del proyecto en el Query.
            foreach (JsonNamespace namespaces in project.Namespaces)
            {
                query += "CREATE (namespaces" + namespaces.Id + ":Namespace {nameNamespaces:" + "'" + namespaces.Name + "'"
                        + ", idLocal:" + "'" + namespaces.Id + "'})\n";
                query += "CREATE (namespaces" + namespaces.Id + ")-[:ParentOfWorkspace]->(project) \n";
                foreach (JsonClass classes in namespaces.Classes)
                {
                    
                       query += "CREATE (class" + classes.Id + ":Clase {nameClass:" + "'" + classes.Name + "'"
                        + ", idLocal:" + "'" + namespaces.Id + "'})\n";
                    query += "CREATE (class" + classes.Id + ")-[:ParentOfClass]->("+ "namespaces" + namespaces.Id + ") \n";
                    foreach (JsonMethod method in classes.Methods)
                    {
                        query += "CREATE (method" + method.Id + ":Method {nameMethod:" + "'" + method.Name + "'" +
                            ", class:" + "'" + method.ClassName + "'" + "}) \n";
                        query += "CREATE (method" + method.Id + ")-[:ParentOfMethod]->(" + "class" + classes.Id + ") \n";

                    }
                }
            }
            //Se arman las relaciones de callers
            foreach (JsonNamespace namespaces in project.Namespaces)
            {
                foreach (JsonClass classes in namespaces.Classes)
                {
                    foreach (JsonMethod method in classes.Methods)
                    {
                        var listOfCalls = method.Calls.ToList();
                        foreach (JsonCall call in listOfCalls)
                        {
                            query += "CREATE (method" + method.Id + ")-[:Call]->(" + "method" + call.Id + ") \n";
                        }
                        var listOfCalledBy = method.CalledBy.ToList();
                       /* foreach (JsonCall call in listOfCalledBy)
                        {
                            query += "CREATE (method" + method.Id + ")-[:CalledBy]->(" + "method" + call.Id + ") \n";
                        }*/

                    }
                }
            }
            var result = session.Run(query);
            output.Write(query);
            output.Flush();
            Console.WriteLine("Finish");
            Console.ReadLine();

        }
    }

}
