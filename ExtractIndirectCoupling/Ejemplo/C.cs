﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ejemplo1
{
    class C
    {
        public int num;

        public void metodoC1()
        {
            Console.WriteLine("Clase C");
            E claseE = new E();
            claseE.metodoE1();
            A claseA = new A();
            claseA.metodoA1();

        }

    }
}
