using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CloneGenerator
{
    class Generator
    {
        List<MethodDeclarationSyntax> methodList;
        SourceText sourceText;
        List<TextLine> newLines;
        Random random = new Random();
        int totalMethods;
        int minRIdx;
        int offset;
        int carry;

        public void Clone(string path, int methodsToCloneNumber, int cloneNumber, int sentencesToCloneNumber)
        {
            string sourceCode = loadFile(path);
            this.sourceText = SourceText.From(sourceCode);
            this.newLines = sourceText.Lines.ToList();

            
            //obtengo el AST
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(sourceText);
            var syntaxRoot = syntaxTree.GetRoot();
            methodList = syntaxRoot.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();

            totalMethods = methodList.Count();
            minRIdx = totalMethods;

            Console.WriteLine(totalMethods);


            //para cada metodo por clonar
            for (int i = 0; i < methodsToCloneNumber; i++)
            {
                //obtengo el metodo que voy a clonar
                int methodToCloneIdx = random.Next(0, totalMethods);
                int receivingMethodIdx = methodToCloneIdx;
                
                var methodToClone = methodList.ElementAt(methodToCloneIdx);

                Console.WriteLine("Del metodo " + methodToClone.Identifier + "\n");

                //obtengo la cantidad de lineas del metodo
                var methodBodySpan = methodToClone.Body.GetLocation().GetLineSpan();

                var methodStart = methodBodySpan.StartLinePosition.Line + 1;
                var methodEnd = methodBodySpan.EndLinePosition.Line;

                var methodBodyLength = methodEnd - methodStart;

                //para cada clon del metodo
                for(int j = 0; j < cloneNumber; j++) {
                    //obtengo la cantidad de lineas que voy a clonar;
                    int linesToCloneNumber = random.Next(1, sentencesToCloneNumber);
                    carry += linesToCloneNumber;

                    //obtengo a partir de cual linea del método voy a clonar

                    int cloneStart = random.Next(methodStart, methodEnd - linesToCloneNumber + 1);

                    Console.WriteLine("\t" + linesToCloneNumber + " lineas a partir de la linea " + (cloneStart + 1) + "\n");

                    //obtengo el metodo donde se va a clonar
                    int temp;
                    do
                    {
                        temp = random.Next(0, totalMethods);
                    } while (temp == methodToCloneIdx || temp == receivingMethodIdx);

                    receivingMethodIdx = temp;

                    if(receivingMethodIdx <= minRIdx)
                    {
                        minRIdx = receivingMethodIdx;
                        offset = 0;
                    }
                    else
                    {
                        offset = carry;
                    }
         
                    var receivingMethod = methodList.ElementAt(receivingMethodIdx);

                    Console.WriteLine("\t En el metodo " + receivingMethod.Identifier + "\n");

                    //obtengo la linea a partir de la cual voy a insertar el clon
                    var receivingMethodBodySpan = receivingMethod.Body.GetLocation().GetLineSpan();

                    var receivingMethodStart = receivingMethodBodySpan.StartLinePosition.Line + 1;
                    var receivingMethodEnd = receivingMethodBodySpan.EndLinePosition.Line;

                    int cloneLocation = random.Next(receivingMethodStart, receivingMethodEnd);

                    Console.WriteLine("\t A partir de la linea " + (cloneLocation + 1) + "\n");


                    cloneLocation += offset;

                    //inserto las lineas
                    int cont = 0;
                    for (int k = 0; k < linesToCloneNumber; k++)
                    {
                        newLines.Insert(cloneLocation + cont, sourceText.Lines[cloneStart + k]);
                        cont++;
                    }  

                }

                //elimino el metodo de la lista para no elegirlo de nuevo
                methodList.RemoveAt(methodToCloneIdx);

                totalMethods = methodList.Count();

            }

            //escribo el archivo
            using (StreamWriter writer = new StreamWriter("clonedCode.txt"))
            {
                foreach (var m in newLines)
                {
                    writer.WriteLine(m.ToString());
                }
            }

            Console.ReadKey();


        }


        public void setSourceText(string code)
        {
            this.sourceText = SourceText.From(code);
        }

        public SourceText getSourceText()
        {
            return this.sourceText;
        }

        public void setNewLines(SourceText st)
        {
            this.newLines = st.Lines.ToList();
        }

        public List<TextLine> getNewLines()
        {
            return this.newLines;
        }

        public void setMethodList(SourceText st)
        {
            //var sf = CSharpSyntaxTree.ParseText(sourceText);
            //var newLines = sourceText.Lines.ToList();

            var syntaxTree = SyntaxFactory.ParseSyntaxTree(sourceText);
            var syntaxRoot = syntaxTree.GetRoot();
            this.methodList = syntaxRoot.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
        }

        public static String loadFile(string path)
        {
            string readContents;
            using (StreamReader streamReader = new StreamReader(path, Encoding.UTF8))
            {
                readContents = streamReader.ReadToEnd();
            }
            return readContents;
        }
    }
}
