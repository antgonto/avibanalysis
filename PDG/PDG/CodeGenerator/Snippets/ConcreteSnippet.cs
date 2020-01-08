using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace CodeGenerator.Snippets
{
    class ConcreteSnippet : Snippet
    {

        public ConcreteSnippet(int requiredMethods, string body) : base(requiredMethods, body) { }

    }
}
