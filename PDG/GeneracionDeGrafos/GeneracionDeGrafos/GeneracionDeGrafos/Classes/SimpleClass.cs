using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using GeneracionDeGrafos.Methods;
using GeneracionDeGrafos;

namespace GeneracionDeGrafos.Classes
{
    class SimpleClass : Class
    {

        public SimpleClass() : base() {
        }

        protected override void writeClassHead(StreamWriter writer) {
            writer.WriteLine(Templates.simpleClassHead, name);
            writer.WriteLine();
        }

    }

}
