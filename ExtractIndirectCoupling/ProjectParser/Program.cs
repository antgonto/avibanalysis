﻿using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using System.Threading;

using ArchiMetrics.Analysis;

using System.Windows.Forms;
using System.Reflection;
using Newtonsoft.Json;
using Neo4j.Driver.V1;
using System.Diagnostics;
using ArchiMetrics.Analysis.Common;
using System.Threading.Tasks;

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
        static int cantidadClases = 0;

        private static async Task Run()
        {
            Console.WriteLine("Loading Solution");
            var solutionProvider = new SolutionProvider();
            var solution = await solutionProvider.Get(@"C:\Users\jnavas\source\repos\DCI-SLN\DCI_SIA\DCI_SIA.sln");
            Console.WriteLine("Solution loaded");

            var projects = solution.Projects.ToList();

            Console.WriteLine("Loading metrics, wait it may take a while.");
            var metricsCalculator = new CodeMetricsCalculator();
            var calculateTasks = projects.Select(p => metricsCalculator.Calculate(p, solution));
            var metrics = (await Task.WhenAll(calculateTasks)).SelectMany(nm => nm);
            foreach (var metric in metrics)
                Console.WriteLine("{0} => {1}", metric.Name, metric.CyclomaticComplexity);
        }

        [STAThread]
        public static void Main(string[] args)
        {
            //Metrics();

            /*
            var task = Run();
            task.Wait();
            Console.ReadKey();
            */

            
            if (args.Length == 0)
                LoadProject(null, null);
            else if (args.Length == 2)
                LoadProject(args[0], args[1]);
            
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

        private static double CalculateMaintainablityIndex(double cyclomaticComplexity, double linesOfCode, IHalsteadMetrics halsteadMetrics)
        {
            if (linesOfCode.Equals(0.0) || halsteadMetrics.NumberOfOperands.Equals(0) || halsteadMetrics.NumberOfOperators.Equals(0))
            {
                return 100.0;
            }

            var num = Math.Log(halsteadMetrics.GetVolume());
            var mi = ((171 - (5.2 * num) - (0.23 * cyclomaticComplexity) - (16.2 * Math.Log(linesOfCode))) * 100) / 171;

            return Math.Max(0.0, mi);
        }

        [STAThread]
        private static void ExtractGraphFromAST(JsonProject project, Compilation myCompilation, string path)
        {
            Dictionary<string, JsonMethod> nodoNamespacesNeo4J = new Dictionary<string, JsonMethod>();
            bool debug = false;
            // Send output to a file
            System.IO.StreamWriter output = null;
            if (debug) output = new System.IO.StreamWriter(@"" + path + @"\graph_info.txt");

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

            if (debug) output.WriteLine("\n\nLista de Metodos:\n----------------------------------------------------------------\n\n");

            for (int i = 0; i < semanticModels.Count; i++)
            {
                //Obtengo todas las declaraciones de metodos en el modelo semantico.
                declarationSyntax = roots[i].DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

                //Recorro las declaraciones para obtener los metodos.
                foreach (MethodDeclarationSyntax declaracionDeMetodoActual in declarationSyntax)
                {
                    //Calculations of each method 
                    //int start = 0, end = 0;
                    int lines = 0, cyclomatic = 0;
                    IHalsteadMetrics halstead;
                    double mi;
                    //output.WriteLine(declaracionDeMetodoActual.Identifier.ToString());

                    CyclomaticComplexityCounter cycCounter = new CyclomaticComplexityCounter();
                    cyclomatic = cycCounter.Calculate(declaracionDeMetodoActual, semanticModels[i]);

                    LinesOfCodeCalculator locCounter = new LinesOfCodeCalculator();
                    lines = locCounter.Calculate(declaracionDeMetodoActual);

                    HalsteadAnalyzer analyzer = new HalsteadAnalyzer();
                    halstead = analyzer.Calculate(declaracionDeMetodoActual);
                    mi = CalculateMaintainablityIndex(cyclomatic, lines, halstead);

                    //cyclomatic = calcularComplejidadCiclomatica(declaracionDeMetodoActual);
                    //if (declaracionDeMetodoActual.Body != null)
                    //{
                    //    start = declaracionDeMetodoActual.Body.SyntaxTree.GetLineSpan(declaracionDeMetodoActual.FullSpan).StartLinePosition.Line;
                    //    end = declaracionDeMetodoActual.Body.SyntaxTree.GetLineSpan(declaracionDeMetodoActual.FullSpan).EndLinePosition.Line;
                    //    lines = end - start;
                    //}
                    //else
                    //{
                    //    lines = 1;
                    //    cyclomatic = 1;
                    //
                    //}

                    SyntaxNode classDec = FindClass(declaracionDeMetodoActual);
                    NamespaceDeclarationSyntax namespaceDec = FindNamespace(classDec);

                    if (classDec != null && namespaceDec != null)
                    {
                        JsonMethod m = JsonMethod.GetMethod(
                            declaracionDeMetodoActual.Identifier.ToString() + GetMethodSignature(declaracionDeMetodoActual),
                            FindClassName(classDec),
                            namespaceDec.Name.ToString(),
                            classDec is InterfaceDeclarationSyntax,
                            lines,
                            1,
                            cyclomatic,
                            halstead,
                            mi);

                        //if (m.Name.StartsWith("UsingImplicitSession(") && m.ClassName.Equals("MongoClient"))
                        //{
                        //    string stop = GetMethodSignature(declaracionDeMetodoActual);
                        //    stop = "continue";
                        //}

                        if (debug) output.WriteLine(String.Format("{0,-150} {1,-150} {2,-150} {3,-15} {4,-15} {5,-15}",
                                                       m.Name, m.ClassName, m.NamespaceName, m.Id, m.ClassId, m.NamespaceId));
                    }
                }

                //Obtengo todas las classes del modelo.
                classDeclarationSyntax = roots[i].DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
                cantidadClases += classDeclarationSyntax.Count;

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

            for (int i = 0; i < semanticModels.Count; i++)
            {
                //Obtengo todas las classes del modelo.
                classDeclarationSyntax = roots[i].DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
                CompilationUnitSyntax cu = (CompilationUnitSyntax)roots[i];

                //Recorro las classes para obtener su BaseList
                foreach (ClassDeclarationSyntax claseActual in classDeclarationSyntax)
                {
                    NamespaceDeclarationSyntax namespaceDec = FindNamespace(claseActual);
                    String namespaceName = (namespaceDec == null ? "" : namespaceDec.Name.ToString());
                    // TODO: permitir usar namespace default
                    if (claseActual.BaseList != null && namespaceDec != null)
                    {
                        JsonClass c = JsonClass.FindClass(claseActual.Identifier.ToString(), namespaceName);
                        if (c != null)
                        {
                            foreach (BaseTypeSyntax bt in claseActual.BaseList.Types)
                            {
                                c.Types.Add(bt.Type.ToString());
                            }

                            foreach (UsingDirectiveSyntax ud in ((CompilationUnitSyntax)roots[i]).Usings)
                            {
                                c.Usings.Add(ud.Name.ToString());
                            }
                        }
                    }
                }
            }

            if (debug) output.WriteLine("\n\nLista de Llamadas:\n----------------------------------------------------------------\n\n");

            JsonClass.ResolveHierarchy();

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

                    if (symbol == null)
                    {
                        bool stop = true;
                        if (stop) stop = false;
                        string errortype = "none";
                    }

                    //if (methodDec != null && classDec != null)
                    //{
                    //    if (((ClassDeclarationSyntax)classDec).Identifier.Text.Equals("SIALnAbogados") &&
                    //        ((MethodDeclarationSyntax)methodDec).Identifier.Text.Equals("ConsultarAbogados"))
                    //    {
                    //        bool stop = true;
                    //        if (stop) stop = false;
                    //    }
                    //}

                    if (methodDec != null && classDec != null && namespaceDec != null && symbol != null)
                    {
                        if (symbol is IMethodSymbol && methodDec is MethodDeclarationSyntax)
                        {

                            IMethodSymbol iSymbol = symbol as IMethodSymbol;
                            if (!iSymbol.MethodKind.ToString().Equals("ReducedExtension") &&
                                !iSymbol.MethodKind.ToString().Equals("LocalFunction"))
                            {
                                // 0 search only 
                                JsonMethod caller = JsonMethod.FindMethod(FindMethodName(methodDec) + GetMethodSignature((MethodDeclarationSyntax)methodDec),
                                    FindClassName(classDec), namespaceDec.Name.ToString());

                                //if (caller.Name.Equals("ConsultarAbogados(IAbogados)") && caller.ClassName.Equals("SIAWcfAbogados"))
                                //{
                                //    bool stop = true;
                                //    if (stop) stop = false;
                                //}

                                string mname = iSymbol.Name;
                                string cname = iSymbol.ContainingSymbol.Name;
                                string nname = iSymbol.ContainingNamespace.ToString();
                                // 0 search only
                                JsonMethod callee = JsonMethod.FindMethod(mname + GetMethodSignature(iSymbol), cname, nname);

                               if (caller != null && callee != null)
                                {
                                    if (caller.Id == callee.Id)
                                    {
                                        caller.IsRecursive = true;
                                    }
                                    else
                                    {
                                        JsonCall callerEntry = new JsonCall(caller.Id, caller.Name, caller.ClassId, caller.ClassName, caller.NamespaceId, caller.FullNamespaceName, caller);
                                        JsonCall calleeEntry = new JsonCall(callee.Id, callee.Name, callee.ClassId, callee.ClassName, callee.NamespaceId, callee.FullNamespaceName, callee);
                                        bool isNewCall = !callee.CalledBy.Contains(callerEntry);

                                        if (isNewCall)
                                        {
                                            if (debug) output.WriteLine(String.Format("{0,-150} {1,-150} {2,-150} {3,-150} {4,-150} {5,-150} {6,-15} {7,-15} {8,-15} {9,-15} {10,-15} {11,-15}",
                                                                           caller.Name, callee.Name,
                                                                           caller.ClassName, callee.ClassName,
                                                                           caller.FullNamespaceName, callee.FullNamespaceName,
                                                                           caller.Id, callee.Id,
                                                                           caller.ClassId, callee.ClassId,
                                                                           caller.NamespaceId, callee.NamespaceId));
                                        }

                                        if (isNewCall) callee.CalledBy.Add(callerEntry);
                                        if (!caller.Calls.Contains(calleeEntry)) caller.Calls.Add(calleeEntry);

                                        if (isNewCall && callee.GetClass.Children.Count > 0)
                                        {
                                            JsonClass.PropagateCall(caller, callee, callerEntry, callee.GetClass);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (debug) output.Flush();
        }

        private static string GetSignatureFromParameters(List<ParameterSyntax> l)
        {
            string sig = "(";
            foreach (ParameterSyntax p in l)
            {
                if (sig.Length > 1) sig += ",";
                sig += p.Type.ToString();
            }
            sig += ")";
            return sig;
        }

        private static string GetMethodSignature(MethodDeclarationSyntax md)
        {
            List<ParameterSyntax> l = md.ParameterList.Parameters.ToList<ParameterSyntax>();
            return GetSignatureFromParameters(l);
        }

        private static string GetMethodSignature(IMethodSymbol md)
        { 
            if (md.DeclaringSyntaxReferences != null)
            {
                // retrieve semantic model of method
                SyntaxReference syntaxReference = md.DeclaringSyntaxReferences.FirstOrDefault();
                if (syntaxReference != null)
                {
                    var syntax = syntaxReference.GetSyntax();
                    if (syntax is MethodDeclarationSyntax)
                    {
                        MethodDeclarationSyntax declaration = (MethodDeclarationSyntax)syntaxReference.GetSyntax();
                        return GetSignatureFromParameters(declaration.ParameterList.Parameters.ToList<ParameterSyntax>());
                    }
                    if (syntax is DelegateDeclarationSyntax)
                    {
                        DelegateDeclarationSyntax declaration = (DelegateDeclarationSyntax)syntaxReference.GetSyntax();
                        return GetSignatureFromParameters(declaration.ParameterList.Parameters.ToList<ParameterSyntax>());
                    }
                }
            }

            string sig = "(";
            foreach (IParameterSymbol p in md.Parameters)
            {
                if (sig.Length > 1) sig += ",";
                sig += p.Type.ToString();
            }
            sig += ")";
            return sig;
        }

        private static void CloudUnattended()
        {
            string[] projects = new string[10];
            projects[0] = @"C:\Users\jnavas05\Desktop\repos\sources\sources\industry\DCI";
            projects[1] = @"C:\Users\jnavas05\Desktop\repos\sources\sources\industry\FastWorks";
            projects[2] = @"C:\Users\jnavas05\Desktop\repos\sources\sources\industry\IPSUM";
            projects[3] = @"C:\Users\jnavas05\Desktop\repos\sources\sources\industry\SGIE";
            projects[4] = @"C:\Users\jnavas05\Desktop\repos\sources\sources\industry\TimeReporter";
            projects[5] = @"C:\Users\jnavas05\Desktop\repos\sources\sources\open source\json.net";
            projects[6] = @"C:\Users\jnavas05\Desktop\repos\sources\sources\open source\mongo";
            projects[7] = @"C:\Users\jnavas05\Desktop\repos\sources\sources\open source\neo4j";
            projects[8] = @"C:\Users\jnavas05\Desktop\repos\sources\sources\open source\netmf";
            projects[9] = @"C:\Users\jnavas05\Desktop\repos\sources\sources\open source\nodejs-tools";

            string output_dir = @"C:\Users\jnavas05\Desktop\repos\output";

            foreach (string p in projects)
            {
                LoadProject(p, output_dir);
            }

        }

        private static void LoadProject(string p, string o)
        {
            JsonProject project = new JsonProject();
            JsonNamespace.Project = project;

            bool loadNeo4jOnly = false;

            Compilation myCompilation = null;
            if (loadNeo4jOnly == false)
                myCompilation = CreateTestCompilation(p);//Llama a la clase para crear la lista de archivos

            //System.IO.StreamWriter diag_output = new System.IO.StreamWriter(@"C:\Users\jnavas\source\repos\output\diagnostics.txt");

            //foreach (Diagnostic d in myCompilation.GetDiagnostics())
            //{
            //    diag_output.WriteLine(d.ToString());

            //}
            //diag_output.Flush();
            //diag_output.Close();

            bool run = true;

            string output_path = o;

            if (o == null)
            {
                FolderBrowserDialog salida = new FolderBrowserDialog();
                salida.Description = @"Output folder";
                salida.SelectedPath = @"C:\Users\jnavas\source\repos\output";
                //salida.SelectedPath = @"C:\Users\Administrador\Documents\GitHub\avibanalysis\ExtractIndirectCoupling\output";
                //salida.SelectedPath = @"C:\Users\Steven\Desktop\output";
                if (salida.ShowDialog() == DialogResult.OK)
                    output_path = salida.SelectedPath;
                else
                    run = false;
            }

            if (run)
            {

                Stopwatch timer = new Stopwatch();
                if (loadNeo4jOnly == false)
                {
                    Console.Write("Loading ASTs...");
                    timer.Start();
                    ExtractGraphFromAST(project, myCompilation, output_path);
                    myCompilation = null;
                    timer.Stop();
                    Console.WriteLine(" (ellapsed time: " + (((double)timer.ElapsedMilliseconds) / 60000.0).ToString() + " min)");

                    Console.Write("Collapsing SCCs...");
                    timer.Reset(); timer.Start();
                    CollapseGraphSCC();
                    timer.Stop();
                    Console.WriteLine(" (ellapsed time: " + (((double)timer.ElapsedMilliseconds) / 60000.0).ToString() + " min)");

                SaveNeo4JGraph(project);
                    Console.Write("Collecting SCC Metrics using DFS...");
                    timer.Reset(); timer.Start();
                    JsonMethod.CollectSccMetricsUsingDFS();
                    timer.Stop();
                    Console.WriteLine(" (ellapsed time: " + (((double)timer.ElapsedMilliseconds) / 60000.0).ToString() + " min)");

                    /*
                    // Code to generate HPC Project Input File
                    Console.Write("Writing HPC Project Input File...");
                    timer.Reset(); timer.Start();
                    WriteHPCInputFile(salida.SelectedPath);
                    timer.Stop();
                    Console.WriteLine(" (ellapsed time: " + (((double)timer.ElapsedMilliseconds) / 60000.0).ToString() + " min)");
                    */

                    Console.Write("Counting chains using BFS...");
                    timer.Reset(); timer.Start();
                    JsonMethod.CountChainsUsingBFS();
                    timer.Stop();
                    Console.WriteLine(" (ellapsed time: " + (((double)timer.ElapsedMilliseconds) / 60000.0).ToString() + " min)");

                    Console.WriteLine("\n\n");
                    Console.WriteLine("System: " + JsonNamespace.Project.Name);
                    Console.WriteLine("    LOC: " + JsonMethod.Stats.numLOC);
                    Console.WriteLine("    Classes: " + JsonMethod.Stats.numClases);
                    Console.WriteLine("    Methods: " + JsonMethod.Stats.numMetodos);
                    Console.WriteLine("    Methods per class: " + JsonMethod.Stats.numMetodosPorClase);
                    Console.WriteLine("    Max Calls To Method: " + JsonMethod.Stats.maxCallsToName);
                    Console.WriteLine("    Max Calls To: " + JsonMethod.Stats.maxCalls);
                    Console.WriteLine("    Avg Calls To: " + JsonMethod.Stats.promCalls);
                    Console.WriteLine("    Max Called By Method: " + JsonMethod.Stats.maxCalledByName);
                    Console.WriteLine("    Max Called By: " + JsonMethod.Stats.maxCalledby);
                    Console.WriteLine("    Avg Called By: " + JsonMethod.Stats.promCalledby);
                    Console.WriteLine("    Chains: " + JsonMethod.Stats.numCadenas);
                    Console.WriteLine("    Max ChainLen: " + JsonMethod.Stats.maxLargoCadena);
                    Console.WriteLine("    Avg ChainLen: " + JsonMethod.Stats.promLargoCadena);
                    Console.WriteLine("\n\n");

                    Console.Write("Collecting PDG Metrics using Parallelism...");
                    timer.Reset(); timer.Start();
                    JsonMethod.CollectMetricsInParallel();
                    timer.Stop();
                    Console.WriteLine(" (ellapsed time: " + (((double)timer.ElapsedMilliseconds) / 60000.0).ToString() + " min)");

                    // Comment to count chains
                    /*
                    Console.Write("Collecting PDG Metrics using Dfs...");
                    timer.Reset(); timer.Start();
                    JsonMethod.CollectMetricsUsingDfs();
                    //Thread T = new Thread(JsonMethod.CollectMetricsUsingDfs, 1024*1024*10);
                    //T.Start();
                    //T.Join();
                    timer.Stop();
                    Console.WriteLine(" (ellapsed time: " + (((double)timer.ElapsedMilliseconds) / 60000.0).ToString() + " min)");
                    */

                }


                // Uncomment this
                /**/
                Console.Write("Saving graph with metrics in neo4j...");
                timer.Reset(); timer.Start();
                timer.Stop();
                Console.WriteLine(" (ellapsed time: " + (((double)timer.ElapsedMilliseconds) / 60000.0).ToString() + " min)");
                /**/



                /*
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
                */

                /*
                JsonSerializer serializer = new JsonSerializer();

                using (StreamWriter sw = new StreamWriter(@"" + salida.SelectedPath + @"\" + project.Name + @".json"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Formatting = Formatting.Indented;
                    serializer.Serialize(writer, project);
                }
                */

                /*
                // Serialize City attributes
                JsonSerializer serializer = new JsonSerializer();

                using (StreamWriter sw = new StreamWriter(@"" + salida.SelectedPath + @"\" + project.Name + @".json"))
                {
                    sw.Write(project.JSerialize().ToString());
                    sw.Flush();
                }
                */

                /*MongoDB MapReduce*/
                /*
                Console.Write("Running MapReduce on MongoDB...");
                timer.Reset(); timer.Start();
                JsonMethod.MapReduceMetrics(MetricType.icr, MagnitudeFunctionType.sum, WeightFunctionType.cyc);
                timer.Stop();
                Console.WriteLine(" (ellapsed time: " + (((double)timer.ElapsedMilliseconds) / 60000.0).ToString() + " min)");
                */
            }

            Console.WriteLine("Process finished successfully!");
            //if (o == null) Console.Read();

        }

        private static void WriteHPCInputFile(string path)
        {
            System.IO.StreamWriter output = null;
            output = new System.IO.StreamWriter(@"" + path + @"\graph_info.txt");

            foreach (KeyValuePair<string, JsonMethod> kv in JsonMethod.Methods)
            {
                JsonMethod m = kv.Value;
                Dictionary<int, int> calls = new Dictionary<int, int>();
                Dictionary<int, int> calledby = new Dictionary<int, int>();
                string scalls = "", scalledby = "";

                foreach (JsonCall c in m.Calls)
                {
                    JsonMethod cm = c.Method;
                    if (cm.IsCollapsed) cm = cm.Scc;
                    if (calls.ContainsKey(cm.Id) == false) calls.Add(cm.Id, cm.Id);
                }

                foreach (KeyValuePair<int, int> k in calls)
                {
                    scalls += k.Key.ToString() + " ";
                }

                foreach (JsonCall c in m.CalledBy)
                {
                    JsonMethod cm = c.Method;
                    if (cm.IsCollapsed) cm = cm.Scc;
                    if (calledby.ContainsKey(cm.Id) == false) calledby.Add(cm.Id, cm.Id);
                }

                foreach (KeyValuePair<int, int> k in calledby)
                {
                    scalledby += k.Key.ToString() + " ";
                }

                output.WriteLine(String.Format("{0} {1} {2} {3}{4} {5}",
                                               m.Id, m.Loc, calls.Count, scalls, calledby.Count, scalledby));
            }

            foreach (JsonMethod m in JsonMethod.SccList)
            {
                Dictionary<int, int> calls = new Dictionary<int, int>();
                Dictionary<int, int> calledby = new Dictionary<int, int>();
                string scalls = "", scalledby = "";

                foreach (JsonCall c in m.Calls)
                {
                    JsonMethod cm = c.Method;
                    if (calls.ContainsKey(cm.Id) == false) calls.Add(cm.Id, cm.Id);
                }

                foreach (KeyValuePair<int, int> k in calls)
                {
                    scalls += k.Key.ToString() + " ";
                }

                foreach (JsonCall c in m.CalledBy)
                {
                    JsonMethod cm = c.Method;
                    if (calledby.ContainsKey(cm.Id) == false) calledby.Add(cm.Id, cm.Id);
                }

                foreach (KeyValuePair<int, int> k in calledby)
                {
                    scalledby += k.Key.ToString() + " ";
                }

                output.WriteLine(String.Format("{0,-10} {1,-10} {2,-5} {3}{4,-5} {5}",
                                               m.Id, m.Loc, calls.Count, scalls, calledby.Count, scalledby));
            }

            output.Flush();
        }

        public static void Metrics()
        { 
            int cantidadClases = 0;
            //***********************************************
            //* Aca comienza el paso 1 del pseudocódigo     *
            //*                                             *
            //***********************************************
            List<Metodo> listaMetodos = new System.Collections.Generic.List<Metodo>();
            Compilation myCompilation = CreateTestCompilation(null);//Llama a la clase para crear la lista de archivos

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
            salida.SelectedPath = @"C:\Users\jnavas\source\repos\output";

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

                //Console.Read();
            }
        }

        private static Compilation CreateTestCompilation(string p)//JsonClass para la creacion de los árboles de sintaxis
        {
            String programPath = p;

            bool run = true;

            if (p == null)
            {
                FolderBrowserDialog entrada = new FolderBrowserDialog();
                //entrada.SelectedPath = @"C:\Users\Administrador\Documents\repos\roslyn";
                entrada.SelectedPath = @"C:\Users\jnavas\source\repos";
                //entrada.SelectedPath = @"C:\Users\jnavas\source\repos";
                //entrada.SelectedPath = @"C:\Users\Steven\Desktop\Sources\";
                entrada.Description = @"Input folder";
                if (entrada.ShowDialog() == DialogResult.OK)
                    programPath = entrada.SelectedPath;
                else
                    run = false;
            }

            if (run)
            {
                Console.WriteLine(programPath);
                // creation of the syntax tree for every file    
                //String programPath = @"C:\Users\Steven\Desktop\Código prueba\shadowsocks-windows-master";
                //  String programPath = @"C:\Users\Steven\Desktop\Código prueba\mongo-csharp-driver-master\mongo-csharp-driver-master\src";//         <--------------------------- DIRECTORIO DONDE ESTÁN LOS ARCHIVOS .cs
                //String programPath = @"C:\Users\jnavas\source\repos\AnalizadorDFS\AnalizadorDFS\Ejemplo";//         <--------------------------- DIRECTORIO DONDE ESTÁN LOS ARCHIVOS .cs
                //String programPath = @""+ entrada.SelectedPath;
                // String programPath = @"" + entrada.SelectedPath;
                //String programPath = @"C:\Users\jnavas\source\repos\netmf";
                //String programPath = @"C:\Users\jnavas\source\repos\mongo";

                //String programPath = @"C:\Users\jnavas\source\repos\roslyn";
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

            foreach(KeyValuePair<string, JsonMethod> entry in JsonMethod.Methods)
            {
                JsonMethod m = entry.Value;
                if (m.IsMethod == true && m.IsCollapsed == false)
                {
                    // substitute calls to SCC's methods to a call to the SCC method
                    foreach (JsonCall cm in m.Calls.ToList())
                    {
                        if (cm.Method.IsCollapsed == true)
                        {
                            JsonMethod scc = cm.Method.Scc;
                            JsonCall sccCall = new JsonCall(scc.Id, scc.Name, -1, "noclass", -1, "nonamespace", scc);
                            if (m.Calls.Contains(sccCall) == false) m.Calls.Add(sccCall);
                            m.CallsSCC.Add(cm);
                            m.Calls.Remove(cm);
                        }
                    }
                    // substitute calls from SCC's methods to a call from the SCC method
                    foreach (JsonCall cm in m.CalledBy.ToList())
                    {
                        if (cm.Method.IsCollapsed == true)
                        {
                            JsonMethod scc = cm.Method.Scc;
                            JsonCall sccCalledby = new JsonCall(scc.Id, scc.Name, -1, "noclass", -1, "nonamespace", scc);
                            if (m.CalledBy.Contains(sccCalledby) == false) m.CalledBy.Add(sccCalledby);
                            m.CalledBySCC.Add(cm);
                            m.CalledBy.Remove(cm);
                        }
                    }
                }
            }

            foreach (JsonMethod m in JsonMethod.SccList)
            {
                // substitute calls to SCC's methods to a call to the SCC method
                foreach (JsonCall cm in m.Calls.ToList())
                {
                    if (cm.Method.IsCollapsed == true)
                    {
                        JsonMethod scc = cm.Method.Scc;
                        JsonCall sccCall = new JsonCall(scc.Id, scc.Name, -1, "noclass", -1, "nonamespace", scc);
                        if (m.Calls.Contains(sccCall) == false) m.Calls.Add(sccCall);
                        m.CallsSCC.Add(cm);
                        m.Calls.Remove(cm);
                    }
                }
                // substitute calls from SCC's methods to a call from the SCC method
                foreach (JsonCall cm in m.CalledBy.ToList())
                {
                    if (cm.Method.IsCollapsed == true)
                    {
                        JsonMethod scc = cm.Method.Scc;
                        JsonCall sccCalledby = new JsonCall(scc.Id, scc.Name, -1, "noclass", -1, "nonamespace", scc);
                        if (m.CalledBy.Contains(sccCalledby) == false) m.CalledBy.Add(sccCalledby);
                        m.CalledBySCC.Add(cm);
                        m.CalledBy.Remove(cm);
                    }
                }
            }
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
                                                            icrlmin: toInt(f[ 8]),    icrlmax: toInt(f[9]),  icrlavg: toInt(f[10]), icrlsum: toInt(f[11]), icrlnet: toInt(f[12]),
                                                            icflmin: toInt(f[13]),    icflmax: toInt(f[14]), icflavg: toInt(f[15]), icflsum: toInt(f[16]), icflnet: toInt(f[17]),
                                                            icrcmin: toInt(f[18]),    icrcmax: toInt(f[19]), icrcavg: toInt(f[20]), icrcsum: toInt(f[21]), icrcnet: toInt(f[22]),
                                                            icfcmin: toInt(f[23]),    icfcmax: toInt(f[24]), icfcavg: toInt(f[25]), icfcsum: toInt(f[26]), icfcnet: toInt(f[27]),
                                                            icrkmin: toInt(f[28]),    icrkmax: toInt(f[29]), icrkavg: toInt(f[30]), icrksum: toInt(f[31]), icrknet: toInt(f[32]),
                                                            icfkmin: toInt(f[33]),    icfkmax: toInt(f[34]), icfkavg: toInt(f[35]), icfksum: toInt(f[36]), icfknet: toInt(f[37]),
                                                            ismethod: toInt(f[38]),   iscollapsed: toInt(f[39]), isrecursive: toInt(f[40]),
                                                            isscc: toInt(f[41]),      sccid:   toInt(f[42]), calls:   toInt(f[43]), calledby: toInt(f[44]),
                                                            method: f[45]
                                                         })
                                                        CREATE (p)-[:HAS_METHOD]->(m)
                                                        CREATE (n)-[:CONTAINS_METHOD]->(m)
                                                        CREATE (c)-[:OWNS_METHOD]->(m)"));
                session.WriteTransaction(tx => tx.Run(@"LOAD CSV FROM ""file:///sccs.csv"" as f
                                                        MERGE (p:Project { id: f[4] })
                                                        CREATE (m:Method { id: f[0], name: f[1], 
                                                            lines:   toInt(f[ 5]), cyclomatic: toInt(f[6]),  constant: toInt(f[7]),
                                                            icrlmin: toInt(f[ 8]),    icrlmax: toInt(f[9]),  icrlavg: toInt(f[10]), icrlsum: toInt(f[11]), icrlnet: toInt(f[12]),
                                                            icflmin: toInt(f[13]),    icflmax: toInt(f[14]), icflavg: toInt(f[15]), icflsum: toInt(f[16]), icflnet: toInt(f[17]),
                                                            icrcmin: toInt(f[18]),    icrcmax: toInt(f[19]), icrcavg: toInt(f[20]), icrcsum: toInt(f[21]), icrcnet: toInt(f[22]),
                                                            icfcmin: toInt(f[23]),    icfcmax: toInt(f[24]), icfcavg: toInt(f[25]), icfcsum: toInt(f[26]), icfcnet: toInt(f[27]),
                                                            icrkmin: toInt(f[28]),    icrkmax: toInt(f[29]), icrkavg: toInt(f[30]), icrksum: toInt(f[31]), icrknet: toInt(f[32]),
                                                            icfkmin: toInt(f[33]),    icfkmax: toInt(f[34]), icfkavg: toInt(f[35]), icfksum: toInt(f[36]), icfknet: toInt(f[37]),
                                                            ismethod: toInt(f[38]),   iscollapsed: toInt(f[39]), isrecursive: toInt(f[40]),
                                                            isscc: toInt(f[41]),      sccid:   toInt(f[42]), calls:   toInt(f[43]), calledby: toInt(f[44]),
                                                            method: f[45]
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

                //session.WriteTransaction(tx => tx.Run(@"LOAD CSV FROM ""file:///collapses.csv"" as f
                //                                        MERGE (m1:Method { id: f[0] })
                //                                        MERGE (m2:Method { id: f[1] })
                //                                        CREATE (m1)-[:COLLAPSES]->(m2)"));

                session.WriteTransaction(tx => tx.Run(@"LOAD CSV FROM ""file:///ics.csv"" as f
                                                        MERGE (m1:Method { id: f[0] })
                                                        MERGE (m2:Method { id: f[1] })
                                                        CREATE (ics:ICS  { 
                                                            icslmin: toInt(f[ 2]), icslmax: toInt(f[ 3]), icslavg: toInt(f[ 4]), icslsum: toInt(f[ 5]), icslnet: toInt(f[ 6]),
                                                            icscmin: toInt(f[ 7]), icscmax: toInt(f[ 8]), icscavg: toInt(f[ 9]), icscsum: toInt(f[10]), icscnet: toInt(f[11]),
                                                            icskmin: toInt(f[12]), icskmax: toInt(f[13]), icskavg: toInt(f[14]), icsksum: toInt(f[15]), icsknet: toInt(f[16])
                                                        })
                                                        CREATE (m1)-[:ICS]->(ics)-[:ICS]->(m2)"));

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
                if (m.IsCollapsed == false)
                {
                    methodsSW.WriteLine(String.Format(@"{0}{1},{2},{0}{3},{0}{4},{0},{5},{6},{7}," +
                    "{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22}," +
                    "{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35},{36},{37}," +
                    "{38},{39},{40},{41},{42},{43},{44},{45}",
                    project.Name, m.Id, "\"" + m.Fullname + "\"", m.ClassId, m.NamespaceId, m.Loc, m.Cyc, m.Kon,
                    m.Loc_metrics.Fmin, m.Loc_metrics.Fmax, m.Loc_metrics.Favg, m.Loc_metrics.Fsum, m.Loc_metrics.Fnet,
                    m.Loc_metrics.Bmin, m.Loc_metrics.Bmax, m.Loc_metrics.Bavg, m.Loc_metrics.Bsum, m.Loc_metrics.Bnet,
                    m.Cyc_metrics.Fmin, m.Cyc_metrics.Fmax, m.Cyc_metrics.Favg, m.Cyc_metrics.Fsum, m.Cyc_metrics.Fnet,
                    m.Cyc_metrics.Bmin, m.Cyc_metrics.Bmax, m.Cyc_metrics.Bavg, m.Cyc_metrics.Bsum, m.Cyc_metrics.Bnet,
                    m.Kon_metrics.Fmin, m.Kon_metrics.Fmax, m.Kon_metrics.Favg, m.Kon_metrics.Fsum, m.Kon_metrics.Fnet,
                    m.Kon_metrics.Bmin, m.Kon_metrics.Bmax, m.Kon_metrics.Bavg, m.Kon_metrics.Bsum, m.Kon_metrics.Bnet,
                    m.IsMethod ? "1" : "0", m.IsCollapsed ? "1" : "0", m.IsRecursive ? "1" : "0", m.IsScc ? "1" : "0",
                    m.IsCollapsed ? m.SccId : 0, m.Calls.Count, m.CalledBy.Count, "\"" + m.Name + "\""));

                    if (m.WasProcessed == false)
                    {
                        Console.WriteLine("Method " + m.Fullname + " was not processed!");
                    }

                    foreach (JsonCall c in m.Calls)
                    {
                        callsSW.WriteLine(String.Format(@"{0}{1},{0}{2}", project.Name, m.Id, c.Id));
                    }
                }
            }

            foreach (JsonMethod m in JsonMethod.SccList)
            {
                sccsSW.WriteLine(String.Format(@"{0}{1},{2},{3},{4},{0},{5},{6},{7}," +
                    "{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22}," +
                    "{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35},{36},{37}," +
                    "{38},{39},{40},{41},{42},{43},{44},{45}",
                    project.Name, m.Id, m.Fullname, "", "", m.Loc, m.Cyc, m.Kon,
                    m.Loc_metrics.Fmin, m.Loc_metrics.Fmax, m.Loc_metrics.Favg, m.Loc_metrics.Fsum, m.Loc_metrics.Fnet,
                    m.Loc_metrics.Bmin, m.Loc_metrics.Bmax, m.Loc_metrics.Bavg, m.Loc_metrics.Bsum, m.Loc_metrics.Bnet,
                    m.Cyc_metrics.Fmin, m.Cyc_metrics.Fmax, m.Cyc_metrics.Favg, m.Cyc_metrics.Fsum, m.Cyc_metrics.Fnet,
                    m.Cyc_metrics.Bmin, m.Cyc_metrics.Bmax, m.Cyc_metrics.Bavg, m.Cyc_metrics.Bsum, m.Cyc_metrics.Bnet,
                    m.Kon_metrics.Fmin, m.Kon_metrics.Fmax, m.Kon_metrics.Favg, m.Kon_metrics.Fsum, m.Kon_metrics.Fnet,
                    m.Kon_metrics.Bmin, m.Kon_metrics.Bmax, m.Kon_metrics.Bavg, m.Kon_metrics.Bsum, m.Kon_metrics.Bnet,
                    m.IsMethod?"1":"0", m.IsCollapsed?"1":"0", m.IsRecursive?"1":"0", m.IsScc?"1":"0",
                    m.IsCollapsed?m.SccId:0, m.Calls.Count, m.CalledBy.Count, m.Name));

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

            foreach (Tuple<int, int, PairMetrics> t in JsonMethod.PairMetrics)
            {
                int m1 = t.Item1;
                int m2 = t.Item2;
                PairMetrics pair = t.Item3;

                icsSW.WriteLine(String.Format(@"{0}{1},{0}{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17}",
                    project.Name, m1, m2,
                    pair.L.Fmin, pair.L.Fmax, pair.L.Favg, pair.L.Fsum, pair.L.Fnet,
                    pair.C.Fmin, pair.C.Fmax, pair.C.Favg, pair.C.Fsum, pair.C.Fnet,
                    pair.K.Fmin, pair.K.Fmax, pair.K.Favg, pair.K.Fsum, pair.K.Fnet));
            }

            // TODO: write pairs
            /*
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
                    */
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
            //Console.ReadLine();

        }


        private class CyclomaticComplexityCounter
        {
            public int Calculate(SyntaxNode node, SemanticModel semanticModel)
            {
                var analyzer = new InnerComplexityAnalyzer(semanticModel);
                var result = analyzer.Calculate(node);

                return result;
            }

            private class InnerComplexityAnalyzer : CSharpSyntaxWalker
            {
                private static readonly SyntaxKind[] Contributors = new[]
                                                                    {
                                                                    SyntaxKind.CaseSwitchLabel,
                                                                    SyntaxKind.CoalesceExpression,
                                                                    SyntaxKind.ConditionalExpression,
                                                                    SyntaxKind.LogicalAndExpression,
                                                                    SyntaxKind.LogicalOrExpression,
                                                                    SyntaxKind.LogicalNotExpression
                                                                };

                // private static readonly string[] LazyTypes = new[] { "System.Threading.Tasks.Task" };
                private readonly SemanticModel _semanticModel;
                private int _counter;

                public InnerComplexityAnalyzer(SemanticModel semanticModel)
                    : base(SyntaxWalkerDepth.Node)
                {
                    _semanticModel = semanticModel;
                    _counter = 1;
                }

                public int Calculate(SyntaxNode syntax)
                {
                    if (syntax != null)
                    {
                        Visit(syntax);
                    }

                    return _counter;
                }

                public override void Visit(SyntaxNode node)
                {
                    base.Visit(node);
                    if (Contributors.Contains(node.Kind()))
                    {
                        _counter++;
                    }
                }

                public override void VisitWhileStatement(WhileStatementSyntax node)
                {
                    base.VisitWhileStatement(node);
                    _counter++;
                }

                public override void VisitForStatement(ForStatementSyntax node)
                {
                    base.VisitForStatement(node);
                    _counter++;
                }

                public override void VisitForEachStatement(ForEachStatementSyntax node)
                {
                    base.VisitForEachStatement(node);
                    _counter++;
                }

                //// TODO: Calculate for tasks
                ////public override void VisitInvocationExpression(InvocationExpressionSyntax node)
                ////{
                ////	if (_semanticModel != null)
                ////	{
                ////		var symbol = _semanticModel.GetSymbolInfo(node).Symbol;
                ////		if (symbol != null)
                ////		{
                ////			switch (symbol.Kind)
                ////			{
                ////				case SymbolKind.Method:
                ////					var returnType = ((IMethodSymbol)symbol).ReturnType;
                ////					break;
                ////			}
                ////		}
                ////	}
                ////	base.VisitInvocationExpression(node);
                ////}

                ////	base.VisitInvocationExpression(node);
                ////}
                public override void VisitArgument(ArgumentSyntax node)
                {
                    switch (node.Expression.Kind())
                    {
                        case SyntaxKind.ParenthesizedLambdaExpression:
                            {
                                var lambda = (ParenthesizedLambdaExpressionSyntax)node.Expression;
                                Visit(lambda.Body);
                            }

                            break;
                        case SyntaxKind.SimpleLambdaExpression:
                            {
                                var lambda = (SimpleLambdaExpressionSyntax)node.Expression;
                                Visit(lambda.Body);
                            }

                            break;
                    }

                    base.VisitArgument(node);
                }

                public override void VisitDefaultExpression(DefaultExpressionSyntax node)
                {
                    base.VisitDefaultExpression(node);
                    _counter++;
                }

                public override void VisitContinueStatement(ContinueStatementSyntax node)
                {
                    base.VisitContinueStatement(node);
                    _counter++;
                }

                public override void VisitGotoStatement(GotoStatementSyntax node)
                {
                    base.VisitGotoStatement(node);
                    _counter++;
                }

                public override void VisitIfStatement(IfStatementSyntax node)
                {
                    base.VisitIfStatement(node);
                    _counter++;
                }

                public override void VisitCatchClause(CatchClauseSyntax node)
                {
                    base.VisitCatchClause(node);
                    _counter++;
                }
            }
        }

        private class LinesOfCodeCalculator
        {
            public int Calculate(SyntaxNode node)
            {
                var innerCalculator = new InnerLinesOfCodeCalculator();
                return innerCalculator.Calculate(node);
            }

            private class InnerLinesOfCodeCalculator : CSharpSyntaxWalker
            {
                private int _counter;

                public InnerLinesOfCodeCalculator()
                    : base(SyntaxWalkerDepth.Node)
                {
                }

                public int Calculate(SyntaxNode node)
                {
                    if (node != null)
                    {
                        Visit(node);
                    }

                    return Math.Max(1, _counter);
                }

                public override void VisitCheckedStatement(CheckedStatementSyntax node)
                {
                    base.VisitCheckedStatement(node);
                    _counter++;
                }

                public override void VisitDoStatement(DoStatementSyntax node)
                {
                    base.VisitDoStatement(node);
                    _counter++;
                }

                public override void VisitEmptyStatement(EmptyStatementSyntax node)
                {
                    base.VisitEmptyStatement(node);
                    _counter++;
                }

                public override void VisitExpressionStatement(ExpressionStatementSyntax node)
                {
                    base.VisitExpressionStatement(node);
                    _counter++;
                }

                /// <summary>
                /// Called when the visitor visits a AccessorDeclarationSyntax node.
                /// </summary>
                public override void VisitAccessorDeclaration(AccessorDeclarationSyntax node)
                {
                    if (node.Body == null)
                    {
                        _counter++;
                    }

                    base.VisitAccessorDeclaration(node);
                }

                public override void VisitFixedStatement(FixedStatementSyntax node)
                {
                    base.VisitFixedStatement(node);
                    _counter++;
                }

                public override void VisitForEachStatement(ForEachStatementSyntax node)
                {
                    base.VisitForEachStatement(node);
                    _counter++;
                }

                public override void VisitForStatement(ForStatementSyntax node)
                {
                    base.VisitForStatement(node);
                    _counter++;
                }

                public override void VisitGlobalStatement(GlobalStatementSyntax node)
                {
                    base.VisitGlobalStatement(node);
                    _counter++;
                }

                public override void VisitGotoStatement(GotoStatementSyntax node)
                {
                    base.VisitGotoStatement(node);
                    _counter++;
                }

                public override void VisitIfStatement(IfStatementSyntax node)
                {
                    base.VisitIfStatement(node);
                    _counter++;
                }

                public override void VisitInitializerExpression(InitializerExpressionSyntax node)
                {
                    base.VisitInitializerExpression(node);
                    _counter += node.Expressions.Count;
                }

                public override void VisitLabeledStatement(LabeledStatementSyntax node)
                {
                    base.VisitLabeledStatement(node);
                    _counter++;
                }

                public override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax node)
                {
                    base.VisitLocalDeclarationStatement(node);
                    if (!node.Modifiers.Any(SyntaxKind.ConstKeyword))
                    {
                        _counter++;
                    }
                }

                public override void VisitLockStatement(LockStatementSyntax node)
                {
                    base.VisitLockStatement(node);
                    _counter++;
                }

                public override void VisitReturnStatement(ReturnStatementSyntax node)
                {
                    base.VisitReturnStatement(node);
                    if (node.Expression != null)
                    {
                        _counter++;
                    }
                }

                public override void VisitSwitchStatement(SwitchStatementSyntax node)
                {
                    base.VisitSwitchStatement(node);
                    _counter++;
                }

                public override void VisitThrowStatement(ThrowStatementSyntax node)
                {
                    base.VisitThrowStatement(node);
                    _counter++;
                }

                public override void VisitUnsafeStatement(UnsafeStatementSyntax node)
                {
                    base.VisitUnsafeStatement(node);
                    _counter++;
                }

                public override void VisitUsingDirective(UsingDirectiveSyntax node)
                {
                    base.VisitUsingDirective(node);
                    _counter++;
                }

                public override void VisitUsingStatement(UsingStatementSyntax node)
                {
                    base.VisitUsingStatement(node);
                    _counter++;
                }

                /// <summary>
                /// Called when the visitor visits a ConstructorDeclarationSyntax node.
                /// </summary>
                public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
                {
                    base.VisitConstructorDeclaration(node);
                    _counter++;
                }

                public override void VisitWhileStatement(WhileStatementSyntax node)
                {
                    base.VisitWhileStatement(node);
                    _counter++;
                }

                public override void VisitYieldStatement(YieldStatementSyntax node)
                {
                    base.VisitYieldStatement(node);
                    _counter++;
                }
            }
        }

        private class HalsteadMetrics : IHalsteadMetrics
        {
            public static readonly IHalsteadMetrics GenericInstanceGetPropertyMetrics;
            public static readonly IHalsteadMetrics GenericInstanceSetPropertyMetrics;
            public static readonly IHalsteadMetrics GenericStaticGetPropertyMetrics;
            public static readonly IHalsteadMetrics GenericStaticSetPropertyMetrics;

            static HalsteadMetrics()
            {
                GenericInstanceSetPropertyMetrics = new HalsteadMetrics(5, 3, 4, 3);
                GenericStaticSetPropertyMetrics = new HalsteadMetrics(4, 3, 3, 3);
                GenericInstanceGetPropertyMetrics = new HalsteadMetrics(3, 2, 3, 2);
                GenericStaticGetPropertyMetrics = new HalsteadMetrics(2, 1, 2, 1);
            }

            public HalsteadMetrics(int numOperands, int numOperators, int numUniqueOperands, int numUniqueOperators)
            {
                NumberOfOperands = numOperands;
                NumberOfOperators = numOperators;
                NumberOfUniqueOperands = numUniqueOperands;
                NumberOfUniqueOperators = numUniqueOperators;
            }

            public int NumberOfOperands { get; }

            public int NumberOfOperators { get; }

            public int NumberOfUniqueOperands { get; }

            public int NumberOfUniqueOperators { get; }

            public IHalsteadMetrics Merge(IHalsteadMetrics other)
            {
                if (other == null)
                {
                    return this;
                }

                return new HalsteadMetrics(
                    NumberOfOperands + other.NumberOfOperands,
                    NumberOfOperators + other.NumberOfOperators,
                    NumberOfUniqueOperands + other.NumberOfUniqueOperands,
                    NumberOfUniqueOperators + other.NumberOfUniqueOperators);
            }

            public int GetBugs()
            {
                var volume = GetVolume();

                return (int)(volume / 3000);
            }

            public double GetDifficulty()
            {
                return NumberOfUniqueOperands == 0
                    ? 0
                    : ((NumberOfUniqueOperators / 2.0) * (NumberOfOperands / ((double)NumberOfUniqueOperands)));
            }

            public TimeSpan GetEffort()
            {
                var effort = GetDifficulty() * GetVolume();
                return TimeSpan.FromSeconds(effort / 18.0);
            }

            public int GetLength()
            {
                return NumberOfOperators + NumberOfOperands;
            }

            public int GetVocabulary()
            {
                return NumberOfUniqueOperators + NumberOfUniqueOperands;
            }

            public double GetVolume()
            {
                const double newBase = 2.0;
                double vocabulary = GetVocabulary();
                double length = GetLength();
                if (vocabulary.Equals(0.0))
                {
                    return 0.0;
                }

                return length * Math.Log(vocabulary, newBase);
            }
        }


        private class HalsteadAnalyzer : SyntaxWalker
        {
            private IHalsteadMetrics _metrics = new HalsteadMetrics(0, 0, 0, 0);

            public HalsteadAnalyzer()
                : base(SyntaxWalkerDepth.Node)
            {
            }

            public IHalsteadMetrics Calculate(SyntaxNode syntax)
            {
                if (syntax != null)
                {
                    Visit(syntax);
                    return _metrics;
                }

                return _metrics;
            }

            /// <summary>
            /// Called when the walker visits a node.  This method may be overridden if subclasses want to handle the node.  Overrides should call back into this base method if they want the children of this node to be visited.
            /// </summary>
            /// <param name="node">The current node that the walker is visiting.</param>
            public override void Visit(SyntaxNode node)
            {
                var blockSyntax = node as BlockSyntax;
                if (blockSyntax != null)
                {
                    VisitBlock(blockSyntax);
                }

                base.Visit(node);
            }

            public void VisitBlock(BlockSyntax node)
            {
                var tokens = node.DescendantTokens().ToList();
                var dictionary = ParseTokens(tokens, Operands.All);
                var dictionary2 = ParseTokens(tokens, Operators.All);
                var metrics = new HalsteadMetrics(
                    numOperands: dictionary.Values.Sum(x => x.Count),
                    numUniqueOperands: dictionary.Values.SelectMany(x => x).Distinct().Count(),
                    numOperators: dictionary2.Values.Sum(x => x.Count),
                    numUniqueOperators: dictionary2.Values.SelectMany(x => x).Distinct().Count());
                _metrics = metrics;
            }

            private static IDictionary<SyntaxKind, IList<string>> ParseTokens(IEnumerable<SyntaxToken> tokens, IEnumerable<SyntaxKind> filter)
            {
                IDictionary<SyntaxKind, IList<string>> dictionary = new Dictionary<SyntaxKind, IList<string>>();
                foreach (var token in tokens)
                {
                    var kind = token.Kind();
                    if (filter.Any(x => x == kind))
                    {
                        IList<string> list;
                        var valueText = token.ValueText;
                        if (!dictionary.TryGetValue(kind, out list))
                        {
                            dictionary[kind] = new List<string>();
                            list = dictionary[kind];
                        }

                        list.Add(valueText);
                    }
                }

                return dictionary;
            }
        }


        private class Operands
        {
            public static readonly IEnumerable<SyntaxKind> All = new[]
                                                                     {
                                                                     SyntaxKind.IdentifierToken,
                                                                     SyntaxKind.StringLiteralToken,
                                                                     SyntaxKind.NumericLiteralToken,
                                                                     SyntaxKind.AddKeyword,
                                                                     SyntaxKind.AliasKeyword,
                                                                     SyntaxKind.AscendingKeyword,
                                                                     SyntaxKind.AsKeyword,
                                                                     SyntaxKind.AsyncKeyword,
                                                                     SyntaxKind.AwaitKeyword,
                                                                     SyntaxKind.BaseKeyword,
                                                                     SyntaxKind.BoolKeyword,
                                                                     SyntaxKind.BreakKeyword,
                                                                     SyntaxKind.ByKeyword,
                                                                     SyntaxKind.ByteKeyword,
                                                                     SyntaxKind.CaseKeyword,
                                                                     SyntaxKind.CatchKeyword,
                                                                     SyntaxKind.CharKeyword,
                                                                     SyntaxKind.CheckedKeyword,
                                                                     SyntaxKind.ChecksumKeyword,
                                                                     SyntaxKind.ClassKeyword,
                                                                     SyntaxKind.ConstKeyword,
                                                                     SyntaxKind.ContinueKeyword,
                                                                     SyntaxKind.DecimalKeyword,
                                                                     SyntaxKind.DefaultKeyword,
                                                                     SyntaxKind.DefineKeyword,
                                                                     SyntaxKind.DelegateKeyword,
                                                                     SyntaxKind.DescendingKeyword,
                                                                     SyntaxKind.DisableKeyword,
                                                                     SyntaxKind.DoKeyword,
                                                                     SyntaxKind.DoubleKeyword,
                                                                     SyntaxKind.ElifKeyword,
                                                                     SyntaxKind.ElseKeyword,
                                                                     SyntaxKind.EndIfKeyword,
                                                                     SyntaxKind.EndRegionKeyword,
                                                                     SyntaxKind.EnumKeyword,
                                                                     SyntaxKind.EqualsKeyword,
                                                                     SyntaxKind.ErrorKeyword,
                                                                     SyntaxKind.EventKeyword,
                                                                     SyntaxKind.ExplicitKeyword,
                                                                     SyntaxKind.ExternKeyword,
                                                                     SyntaxKind.FalseKeyword,
                                                                     SyntaxKind.FieldKeyword,
                                                                     SyntaxKind.FinallyKeyword,
                                                                     SyntaxKind.FixedKeyword,
                                                                     SyntaxKind.FloatKeyword,
                                                                     SyntaxKind.ForEachKeyword,
                                                                     SyntaxKind.ForKeyword,
                                                                     SyntaxKind.FromKeyword,
                                                                     SyntaxKind.GetKeyword,
                                                                     SyntaxKind.GlobalKeyword,
                                                                     SyntaxKind.GotoKeyword,
                                                                     SyntaxKind.GroupKeyword,
                                                                     SyntaxKind.HiddenKeyword,
                                                                     SyntaxKind.IfKeyword,
                                                                     SyntaxKind.ImplicitKeyword,
                                                                     SyntaxKind.InKeyword,
                                                                     SyntaxKind.InterfaceKeyword,
                                                                     SyntaxKind.InternalKeyword,
                                                                     SyntaxKind.IntKeyword,
                                                                     SyntaxKind.IntoKeyword,
                                                                     SyntaxKind.IsKeyword,
                                                                     SyntaxKind.JoinKeyword,
                                                                     SyntaxKind.LetKeyword,
                                                                     SyntaxKind.LineKeyword,
                                                                     SyntaxKind.LockKeyword,
                                                                     SyntaxKind.LongKeyword,
                                                                     SyntaxKind.MakeRefKeyword,
                                                                     SyntaxKind.MethodKeyword,
                                                                     SyntaxKind.ModuleKeyword,
                                                                     SyntaxKind.NamespaceKeyword,
                                                                     SyntaxKind.NullKeyword,
                                                                     SyntaxKind.ObjectKeyword,
                                                                     SyntaxKind.OnKeyword,
                                                                     SyntaxKind.OperatorKeyword,
                                                                     SyntaxKind.OrderByKeyword,
                                                                     SyntaxKind.OutKeyword,
                                                                     SyntaxKind.OverrideKeyword,
                                                                     SyntaxKind.ParamKeyword,
                                                                     SyntaxKind.ParamsKeyword,
                                                                     SyntaxKind.PartialKeyword,
                                                                     SyntaxKind.PragmaKeyword,
                                                                     SyntaxKind.PrivateKeyword,
                                                                     SyntaxKind.PropertyKeyword,
                                                                     SyntaxKind.ProtectedKeyword,
                                                                     SyntaxKind.PublicKeyword,
                                                                     SyntaxKind.ReadOnlyKeyword,
                                                                     SyntaxKind.ReferenceKeyword,
                                                                     SyntaxKind.RefKeyword,
                                                                     SyntaxKind.RefTypeKeyword,
                                                                     SyntaxKind.RefValueKeyword,
                                                                     SyntaxKind.RegionKeyword,
                                                                     SyntaxKind.RemoveKeyword,
                                                                     SyntaxKind.RestoreKeyword,
                                                                     SyntaxKind.ReturnKeyword,
                                                                     SyntaxKind.SByteKeyword,
                                                                     SyntaxKind.SealedKeyword,
                                                                     SyntaxKind.SelectKeyword,
                                                                     SyntaxKind.SetKeyword,
                                                                     SyntaxKind.ShortKeyword,
                                                                     SyntaxKind.SizeOfKeyword,
                                                                     SyntaxKind.StackAllocKeyword,
                                                                     SyntaxKind.StaticKeyword,
                                                                     SyntaxKind.StringKeyword,
                                                                     SyntaxKind.StructKeyword,
                                                                     SyntaxKind.SwitchKeyword,
                                                                     SyntaxKind.ThisKeyword,
                                                                     SyntaxKind.TrueKeyword,
                                                                     SyntaxKind.TryKeyword,
                                                                     SyntaxKind.TypeKeyword,
                                                                     SyntaxKind.TypeOfKeyword,
                                                                     SyntaxKind.TypeVarKeyword,
                                                                     SyntaxKind.UIntKeyword,
                                                                     SyntaxKind.ULongKeyword,
                                                                     SyntaxKind.UncheckedKeyword,
                                                                     SyntaxKind.UndefKeyword,
                                                                     SyntaxKind.UnsafeKeyword,
                                                                     SyntaxKind.UShortKeyword,
                                                                     SyntaxKind.UsingKeyword,
                                                                     SyntaxKind.VirtualKeyword,
                                                                     SyntaxKind.VoidKeyword,
                                                                     SyntaxKind.VolatileKeyword,
                                                                     SyntaxKind.WarningKeyword,
                                                                     SyntaxKind.WhereKeyword,
                                                                     SyntaxKind.WhileKeyword,
                                                                     SyntaxKind.YieldKeyword
                                                                 };
        }

        private class Operators
        {
            public static readonly IEnumerable<SyntaxKind> All = new[]
                                                                     {
                                                                     SyntaxKind.DotToken,
                                                                     SyntaxKind.EqualsToken,
                                                                     SyntaxKind.SemicolonToken,
                                                                     SyntaxKind.PlusPlusToken,
                                                                     SyntaxKind.PlusToken,
                                                                     SyntaxKind.PlusEqualsToken,
                                                                     SyntaxKind.MinusMinusToken,
                                                                     SyntaxKind.MinusToken,
                                                                     SyntaxKind.MinusEqualsToken,
                                                                     SyntaxKind.AsteriskToken,
                                                                     SyntaxKind.AsteriskEqualsToken,
                                                                     SyntaxKind.SlashToken,
                                                                     SyntaxKind.SlashEqualsToken,
                                                                     SyntaxKind.PercentToken,
                                                                     SyntaxKind.PercentEqualsToken,
                                                                     SyntaxKind.AmpersandToken,
                                                                     SyntaxKind.BarToken,
                                                                     SyntaxKind.CaretToken,
                                                                     SyntaxKind.TildeToken,
                                                                     SyntaxKind.ExclamationToken,
                                                                     SyntaxKind.ExclamationEqualsToken,
                                                                     SyntaxKind.GreaterThanToken,
                                                                     SyntaxKind.GreaterThanEqualsToken,
                                                                     SyntaxKind.LessThanToken,
                                                                     SyntaxKind.LessThanEqualsToken
                                                                 };
        }
    }



}
