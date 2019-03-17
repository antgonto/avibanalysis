using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace edu.tec.avib
{
    class A
    {
        int attribute = 0;

        public int metodoA1() {

            this.attribute = 1000;

            Console.WriteLine("Clase A Metodo 1");
            var claseB = new B();
            if(0==0)
            {

            	claseB.metodoB1();
            }
            else
            {
            	claseB.metodoB2();
            }
            int b3 = claseB.metodoB3() + B.metodoB4();
            return 1;
        }
        public void metodoA2()
        {
            Console.WriteLine("Clase A Metodo 2");
            B claseB = new B();
            claseB.metodoB2();
        }
        public void metodoA3()
        {
            Console.WriteLine("Clase A Metodo 3");
            C clasec = new C();
            clasec.metodoC1();

             int caseSwitch = 1;

		      switch (caseSwitch)
		      {
		          case 1:
		              Console.WriteLine("Case 1");
		              break;
		          case 2:
		              Console.WriteLine("Case 2");
		              break;
		          default:
		              Console.WriteLine("Default case");
		              break;
		      }
        }
        public void metodoA4()
        {
        	int n = 0;
            do
            {
                Console.WriteLine(n);
                n++;
            } while (n < 5);

        }
        public void metodoA5()
        {
            SuperClase v1 = new SubClase();
            v1.inicializar();
            v1.inicializarSuper();
            v1.ejecutar();
            v1.ejecutarSuper();
        }
        public void metodoA6()
        {
            IEjecutable v1 = new SubClase();
            v1.inicializar();
            v1.ejecutar();
            v1 = new SuperClase();
            v1.inicializar();
            v1.ejecutar();
        }
        public void metodoA7()
        {
            SubClase v1 = new SubClase();
            v1.inicializar();
            v1.inicializarSub();
            v1.inicializarSuper();
            v1.ejecutar();
            v1.ejecutarSub();
            v1.ejecutarSuper();
        }
    }
}
