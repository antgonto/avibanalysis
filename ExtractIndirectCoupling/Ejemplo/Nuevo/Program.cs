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
            A claseA = new A();
            //Console.Write(claseA.metodoA2());
            //claseA.metodoA1();
            claseA.metodoA1(7, "hola");
            claseA.metodoA2();

            // claseA.metodoA4();
            Console.Read();

            Class sh = new SubClass();
            sh.SuperMethod1();
            sh.SuperMethod2();
            sh.Contract();
        }
    }
}
