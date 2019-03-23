using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace edu.tec.avib
{
    class SuperClase: IEjecutable
    {
    	public virtual void inicializarSuper()
    	{
    		// nothing here
    	}
    	public virtual int ejecutarSuper()
    	{
    		return 0; // success
    	}
    	public virtual void inicializar()
    	{
    		this.inicializarSuper();
    	}
    	public virtual int ejecutar()
    	{
    		return this.ejecutarSuper();
    	}
    }
}