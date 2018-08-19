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
            B claseB = new B();
            if(0==0)
            {

            	claseB.metodoB1();
            }
            else
            {
            	claseB.metodoB2();
            }
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
    }
}
