/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package clonefinder;
import java.util.*;

/**
 *
 * @author Juan
 */
public class FilaAlineamiento {
    
    private ArrayList<Integer> fila=new ArrayList<Integer>();
    
    /**
     * Establece el numero de filas en la matriz de alineamiento
     * @param tam 
     */
    public FilaAlineamiento(int tam){
        for(int i=0;i<tam+1;i++)fila.add(0);
    }
    
    /**
     * llena todas las filas con 0
     */
    public void setZeros(){
        for(int i=0;i<fila.size();i++)fila.set(i, 0);
    }
    
    /**
     * obtiene el valor en la posicion j en la fila
     * @param j
     * @return 
     */
    public int getJ(int j){
        return fila.get(j);
    }
    
    /**
     * agrega padding a un numero de la matriz
     * se usa para imprimir
     * @param s la tira a la que agregarle padding
     * @param n cuanto padding se quiere
     * @return la tira con padding
     */
    private static String padLeft(String s, int n) {
        return String.format("%1$" + n + "s", s);  
    }
    
    /**
     * imprime de manera bonita una fila
     * @param padding cuanto padding se quiere
     */
    public void prettyPrint(int padding){
        String tira="";
        for(int i=0;i<fila.size();i++){
            tira+=padLeft(Integer.toString(fila.get(i)),padding);
        }
        System.out.println(tira);
    }
    
    /**
     * @return obtiene el numero de elementos en la fila
     */
    public int getColumnas(){
        return fila.size();
    }
    
    /**
     * establece el valor de la posicion j en la fila
     * @param j
     * @param valor 
     */
    public void setJ(int j, int valor){
        fila.set(j, valor);
    }
}
