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
    class Program
    {
        static void Main(string[] args)
        {
            Generator g = new Generator();
            g.Clone(@"C:\Users\hp\Desktop\Class1.cs", 2, 2, 1); //path, # metodos clonados, # de clones, # de sentencias clonadas
        }

    }
}
