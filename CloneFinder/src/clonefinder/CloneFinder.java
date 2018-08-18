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
    TODO
    ya se tiene la matriz, que funciona adecuadamente y se pueden establecer los coeficientes
    de match, mismatch y gap. ahora se debe proceder a encotnrar la subsecuencia local mas grnde, para 
    esto hay que almacenar el punto de la matriz donde esta el maximo valor (ver https://www.slideshare.net/avrilcoghlan/the-smith-waterman-algorithm)
    despues de esto se debe crear una funcion para sacar el texto de las funciones de java, podria hacerse con una pila de {}, y todo
    el texto que esta entre {} de segundo nivel es texto de una funcion. 
    conretamente
    -sacar texto de funciones
    -obtener la subsecuencia local matcheante mas grande
    
    */
    public static void main(String[] args) {
        // TODO code application logic here
        String tira1="TCAGTTGCC";
        String tira2="AGGTTG";
        SmithWaterman.setMatch(1);
        SmithWaterman.setMismatch(-2);
        SmithWaterman.setGap(-2);
        SmithWaterman.printear();
        MatrizAlineamiento m=SmithWaterman.construirMatriz(tira1, tira2);
        m.prettyPrint();
    }
    
}
