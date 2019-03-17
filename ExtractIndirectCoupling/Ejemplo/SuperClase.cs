using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace edu.tec.avib
{
    class SuperClase: IEjecutable
    {
    	public void inicializarSuper()
    	{
    		// nothing here
    	}
    	public int ejecutarSuper()
    	{
    		return 0; // success
    	}
    	public void inicializar()
    	{
    		this.inicializarSuper();
    	}
    	public int ejecutar()
    	{
    		return this.ejecutarSuper();
    	}
    }
}