using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using CodeGenerator.Methods;
using CodeGenerator;

namespace CodeGenerator.Classes
{
    class SimpleClass : Class
    {

        public SimpleClass() : base() {
        }

        public SimpleClass(string name) : base(name) {
        }

        protected override void WriteClassHead(StreamWriter writer) {
            writer.WriteLine(Templates.simpleClassHead, Name);
            writer.WriteLine();
        }

    }

}
