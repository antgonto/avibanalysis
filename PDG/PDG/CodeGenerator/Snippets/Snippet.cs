using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CodeGenerator.Snippets
{
    abstract class Snippet
    {
        public int RequiredMethods { get; set; }
        public string Body { get; set; }
        public List<int> IdentationLevels { get; set; }

        public Snippet(int requiredMethods, string body) {
            RequiredMethods = requiredMethods;
            Body = body;
            IdentationLevels = new List<int>(RequiredMethods);
        }

        public void WriteSnippet(StreamWriter writer, string[] invokedMethods) {
            writer.WriteLine(Body, invokedMethods);
        }
    }
}
