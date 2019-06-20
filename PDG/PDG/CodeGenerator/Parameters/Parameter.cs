using System.Collections.Generic;

namespace CodeGenerator.Parameters
{
    class Parameter
    {
        public string type { get; set; }
        public string name { get; set; }
        public int amount { get; set; }

        public Parameter(string type) {
            this.type = type;
            name = "parameter" + amount.ToString();
            amount++;
        }

        static public string formatParameters(List<Parameter> parameters) {
            string formatedParameters = "";

            // Si existe más de un parámetro
            if (parameters.Count > 0) {

                // Escribir el primer parámetro manualmente
                Parameter firstParameter = parameters[0];
                formatedParameters += (firstParameter.type + " " + firstParameter.name);

                // Escribir el resto de parámetros
                for (int i = 1; i < parameters.Count; i++) {
                    // Separador de parámetros
                    formatedParameters += ", ";

                    Parameter parameter = parameters[i];
                    formatedParameters += (parameter.type + " " + parameter.name);
                }
            }

            return formatedParameters;
        } 
    }
}
