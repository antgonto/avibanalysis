using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ejemplo1
{
    class B
    {
        public int numb;
        public int metodoB1()
        {

            Console.WriteLine("Clase B metodo b1");
            C claseC = new C();
            claseC.metodoC1();
           //A claseA = new A();
           // claseA.metodoA2();
           numb = 3;
            return 1;
        }
        public void metodoB2()
        {

            Console.WriteLine("Clase B metodo b2");
            C claseC = new C();
            claseC.metodoC1();
            E claseE = new E();
            claseE.metodoE1();
        }
    }
}
