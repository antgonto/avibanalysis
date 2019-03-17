using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace edu.tec.avib
{
    class SubClase: SuperClase
    {
    	public override void inicializarSuper()
    	{
    		// nothing here
    	}
    	public override int ejecutarSuper()
    	{
    		return 0; // success
    	}
    	public void inicializarSub()
    	{
    		//inicializarSuper();
    		base.inicializarSuper();
    	}

    	public int ejecutarSub()
    	{
//    		return ejecutarSuper() + base.ejecutarSuper();
    		return base.ejecutarSuper();
    	}
    }
}