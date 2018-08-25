/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package clonefinder;

import java.util.*;
/**
 * Clase usada para almacenar la matriz de alineamiento
 * @author Juan
 */
public class MatrizAlineamiento {
    private int PADDING=3;
    String tira1;
    String tira2;
    ArrayList<FilaAlineamiento>matriz=new ArrayList<FilaAlineamiento>();
    public MatrizAlineamiento(String str1,String str2){
        tira1=str1;
        tira2=str2;
        //se crea la matriz de tamano str1.length()+1xstr2.length()+1
        for(int i=0;i<str2.length()+1;i++){
            matriz.add(new FilaAlineamiento(str1.length()));
        }
    }
    
    /**
     * 
     * @param i
     * @param j
     * @return el elemento en la posicion i,j de la matriz
     */
    public int getIJ(int i, int j){
        return matriz.get(i).getJ(j);
    }
    
    /**
     * Imprime la matriz de forma bonita
     * Invoca el metodo prettyPrint de las filas
     */
    public void prettyPrint(){
        for(int i=0;i<matriz.size();i++){
            matriz.get(i).prettyPrint(PADDING);
        }
    }
    
    /**
     * 
     * @return retorna el numero de filas
     */
    public int getFilas(){
        return matriz.size();
    }
    
    /**
     * 
     * @return el numero de columnas
     */
    public int getColumnas(){
        return matriz.get(0).getColumnas();
    }
    
    /**
     * establece el valor de la posicion i,j en la matriz
     * @param valor
     * @param i
     * @param j 
     */
    public void setIJ(int valor, int i, int j){
        matriz.get(i).setJ(j, valor);
    }
    
    /**
     * Obtiene el valor de sigma usado en el algoritmo smith-waterman
     * @param i
     * @param j
     * @return el valor de sigma
     */
    public boolean getSigma(int i, int j){
        if(tira1.charAt(j-1)==tira2.charAt(i-1))return true;
        return false;
        
    }
    
    
}
