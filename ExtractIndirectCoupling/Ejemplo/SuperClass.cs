using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace edu.tec.avib
{

    public interface IContract
    {
        int Contract();
    }

    public class SuperClass: IContract
    {
        public virtual void SuperMethod1() {; }
        public virtual int SuperMethod2() { return 0; }

        public virtual int Contract() { return 25; }
    }

    public class Class: SuperClass
    {
        //public override int SuperMethod2() { return 0; }

        //public override int Contract() { return 10; }
    }

    public class SubClass: Class
    {
        public override void SuperMethod1()
        {
            return;
        }

        public new int SuperMethod2()
        {
            return 3;
        }
    }
}