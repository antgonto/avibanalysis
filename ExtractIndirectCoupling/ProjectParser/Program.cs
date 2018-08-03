﻿using System;
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
                    SyntaxNode classDec = FindClass(declaracionDeMetodoActual);
                    NamespaceDeclarationSyntax namespaceDec = FindNamespace(classDec);
                    JsonMetodo m = JsonMetodo.GetMetodo(
                        declaracionDeMetodoActual.Identifier.ToString(),
                        FindClassName(classDec),
                        namespaceDec == null ? "" : namespaceDec.Name.ToString());

                    output.WriteLine(String.Format("{0,-150} {1,-150} {2,-150} {3,-15} {4,-15} {5,-15}",
                                                   m.Name, m.ClaseName, m.PaqueteName, m.Id, m.ClaseId, m.PaqueteId));
                }

                //Obtengo todas las clases del modelo.
                classDeclarationSyntax = roots[i].DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();

                //Recorro las clases para obtener sus atributos
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
                            JsonAtributo.GetAtributo(
                                variable.Identifier.ToString(),
                                claseActual.Identifier.ToString(),
                                namespaceDec == null ? "" : namespaceDec.Name.ToString());
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
                            if (!iSymbol.MethodKind.ToString().Equals("ReducedExtension"))
                            {
                                JsonMetodo caller = JsonMetodo.GetMetodo(FindMethodName(methodDec),
                                    FindClassName(classDec), namespaceDec == null ? "" : namespaceDec.Name.ToString());

                                string mname = iSymbol.Name;
                                string cname = iSymbol.ContainingSymbol.Name;
                                string nname = iSymbol.ContainingNamespace.ToString();

                                JsonMetodo callee = JsonMetodo.GetMetodo(mname, cname, nname);

                                JsonCall callerEntry = new JsonCall(caller.Id, caller.Name, caller.ClaseId, caller.ClaseName, caller.PaqueteId, caller.PaqueteName, caller);
                                JsonCall calleeEntry = new JsonCall(callee.Id, callee.Name, callee.ClaseId, callee.ClaseName, callee.PaqueteId, callee.PaqueteName, callee);

                                if (!callee.CalledBy.Contains(callerEntry))
                                {
                                    output.WriteLine(String.Format("{0,-150} {1,-150} {2,-150} {3,-150} {4,-150} {5,-150} {6,-15} {7,-15} {8,-15} {9,-15} {10,-15} {11,-15}",
                                                                   caller.Name, callee.Name,
                                                                   caller.ClaseName, callee.ClaseName,
                                                                   caller.PaqueteName, callee.PaqueteName,
                                                                   caller.Id, callee.Id,
                                                                   caller.ClaseId, callee.ClaseId,
                                                                   caller.PaqueteId, callee.PaqueteId));
                                }

                                if (!callee.CalledBy.Contains(callerEntry)) callee.CalledBy.Add(callerEntry);
                                if (!caller.Calls.Contains(calleeEntry)) caller.Calls.Add(calleeEntry);
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
            JsonPaquete.Project = project;

            Compilation myCompilation = CreateTestCompilation();//Llama a la clase para crear la lista de archivos

            FolderBrowserDialog salida = new FolderBrowserDialog();
            salida.Description = @"Output folder";
            salida.SelectedPath = @"C:\Users\jnavas\source\repos\avibanalysis\ExtractIndirectCoupling\output";
            if (salida.ShowDialog() == DialogResult.OK)
            {
                ExtractGraphFromAST(project, myCompilation, salida.SelectedPath);

                JsonMetodo.CountChainsUsingDFS(project);
                //JsonMetodo.CollectChainsUsingDFS(project);

                JsonSerializer serializer = new JsonSerializer();

                using (StreamWriter sw = new StreamWriter(@"" + salida.SelectedPath + @"\" + project.Name + @".json"))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, project);
                }
            }

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
                //Obtengo todas las clases del modelo.
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
                //Recorro las clases para obtener sus atributos
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

                                        /*Console.WriteLine("JsonClase: " + classSyntax.Identifier);
                                        Console.WriteLine("Metodo que invoca: " + declarationSyntax.ElementAt(i).Identifier);
                                        Console.WriteLine("JsonClase invocada: " + currentSemanticModel.GetSymbolInfo(expression).Symbol.ContainingType.Name);
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
                     Console.WriteLine("JsonClase: "+invocacion.JsonClase);
                    Console.WriteLine("Tipo: " + invocacion.Tipo);
                    Console.WriteLine("Método/Atributo: "+invocacion.Nombre);
                     Console.WriteLine("Cantidad de metodos/atributos que llama: "+invocacion.ListaLlamadas.Count());
                    Console.WriteLine("\t----------------------------------------------------------");
                    for (int i = 0; i < invocacion.ListaLlamadas.Count(); i++)
                     {
                        Console.WriteLine("\t JsonClase: " + invocacion.ListaLlamadas.ElementAt(i).JsonClase);
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
            System.IO.StreamWriter output = new System.IO.StreamWriter(@"C:\Users\Steven\Documents\GitHub\AnalizadorDFSMaster\AnalizadorDFS\output.txt");
            if (salida.ShowDialog() == DialogResult.OK)
            {
                output = new System.IO.StreamWriter(@"" + salida.SelectedPath + "\\output.txt");

            }

            // Send output to a file
            //System.IO.StreamWriter output = new System.IO.StreamWriter(@"C:\Users\jnavas\source\repos\ExtractIndirectCoupling\output.txt");
            //System.IO.StreamWriter output = new System.IO.StreamWriter(@"C:\Users\jnavas\source\repos\AnalizadorDFS\AnalizadorDFS\output.txt");

            //grafo.imprimirCadenas(output);

            /*foreach (Metodo invocacion in listaMetodos)
            {
                if (invocacion.EsLlamador || invocacion.EsLlamado)
                {
                    output.WriteLine("JsonClase: " + invocacion.JsonClase);
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
                                    i + 2, //invocacion.JsonClase + "."+ invocacion.Nombre, 
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
                                                   i + 1, //invocacion.JsonClase + "." + invocacion.Nombre, 
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
            output.WriteLine("Cantidad de clases: " + cantidadClases);
            output.WriteLine("Cadena más larga: " + grafo.CantidadMayorMetodosEnCadena);
            output.WriteLine("Cantidad de cadenas: " + grafo.ListaDeCadenas.Count);

            output.Flush();

            Console.Read();
        }

        [STAThread]
        private static Compilation CreateTestCompilation()//JsonClase para la creacion de los árboles de sintaxis
        {
            FolderBrowserDialog entrada = new FolderBrowserDialog();
            entrada.SelectedPath = @"C:\Users\jnavas\source\repos";
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
    }
}
