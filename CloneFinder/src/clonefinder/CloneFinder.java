/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package clonefinder;

/**
 *
 * @author Juan
 */
public class CloneFinder {

    /**
     * @param args the command line arguments
     */
    
    /*
    HECHO
    -matriz
    -maxima subsecuencia local
    --con varios puntos de partida
    -ver https://www.slideshare.net/avrilcoghlan/the-smith-waterman-algorithm
    TODO
    -sacar funciones de archivos
    -maxima subsecuencia local cuando hay dos posibilidades de divergencia
    -documentacion
    -interpretacion de resultados
    */
    public static void main(String[] args) {
        // TODO code application logic here
        String an1="if numero==0:\n" +
"        return \"Numero neutro\"\n" +
"    else:\n" +
"        if numero%2==0:\n" +
"            return \"El numero es par\"\n" +
"        else:\n" +
"            return \"El numero es impar\"";
        String an2="if year>=2000:\n" +
"        if year%4==0:\n" +
"            return True\n" +
"        else:\n" +
"            return False\n" +
"    else:\n" +
"        return \"ERROR EN EL ANO\"";
        String tira1="GGCTCAATCA";
        String tira2="ACCTAAGG";
        SmithWaterman sm=new SmithWaterman();
        sm.setMatch(2);
        sm.setMismatch(-1);
        sm.setGap(-2);
        sm.printear();
        sm.construirMatriz(tira1, tira2);
        sm.prettyPrint();
        sm.obtenerAlineamientos();
    }
    
}
