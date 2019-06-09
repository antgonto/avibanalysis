using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectParser
{
    class Options
    {
        [Option('n', "name", Required = true, HelpText = "Name of system to be processed.")]
        public string Name { get; set; }

        [Option('o', "outdir", Required = true, HelpText = "Output directory.")]
        public string Outdir { get; set; }

        [Option('p', "pause", HelpText = "Make a pause at the end.")]
        public bool Pause { get; set; }

        [Option('m', "month", HelpText = "Month offset from now.")]
        public int Month { get; set; }

        [Option('s', "solutions", Required = true, HelpText = "Solutions of the system to process.")]
        public IEnumerable<string> Solutions { get; set; }
    }
}
