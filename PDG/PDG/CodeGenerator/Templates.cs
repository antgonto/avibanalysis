using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeGenerator
{
    static class Templates
    {
        static public string namespaceHead =
            "namespace namespace1\n{";

        static public string namespaceTail =
            "}";
        
        /* class className
         * {
         * parameter, name of class
         */
        static public string simpleClassHead =
            "\tclass {0}\n\t{{";

        static public string simpleClassTail =
            "\t}";

        /* public void nameMethod(parameters...) {
         * Parameters:
         *   first, access modifier
         *   second, return type
         *   third, method name
         *   fourth, parameters
         */
        static public string simpleMethodHead =
            "\t\t{0} {1} {2}({3}) {{";

        static public string simpleMethodTail =
            "\t\t}";

        /* Clase1 instanceClass1 = new Clase1();
         * Parameters:
         *   first name of class
         *   second name of instace
         */
        static public string instanceSimpleClass =
            "\t\t\t{0} {1} = new {0}();";

        /* classInstance1.method1(paramters...);
         * Parameters:
         *   first, name of instance
         *   second, name of method
         *   third, parameters of method
         */
        static public string invokeSimpleMethodFromClass =
            "\t\t\t{0}.{1}({2});";

        /* First parameter number of instance
         */
        static public string nameInstaceClass =
            "classInstance{0:D}";

        /* static public void nameMethod(parameters...) {
         * Parameters:
         *   first, access modifier
         *   second, return type
         *   third, method name
         *   fourth, parameters
         */
        static public string staticMethodHead =
            "\t\tstatic {0} {1} {2}({3}) {{";

    }
}
