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
    
    //retorna el elemento i,j en la matriz de alineamiento
    public int getIJ(int i, int j){
        return matriz.get(i).getJ(j);
    }
    
    public void prettyPrint(){
        for(int i=0;i<matriz.size();i++){
            matriz.get(i).prettyPrint(PADDING);
        }
    }
    
    public int getFilas(){
        return matriz.size();
    }
    public int getColumnas(){
        return matriz.get(0).getColumnas();
    }
    
    public void setIJ(int valor, int i, int j){
        matriz.get(i).setJ(j, valor);
    }
    
    public boolean getSigma(int i, int j){
        if(tira1.charAt(j-1)==tira2.charAt(i-1))return true;
        return false;
        
    }
    
    
}
