using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace edu.tec.avib
{
    class Program
    {
        static void Main(string[] args)
        {
            IContract sh = new SubClass();
            //sh.SuperMethod1();
            //sh.SuperMethod2();
            sh.Contract();
        }
    }
}
