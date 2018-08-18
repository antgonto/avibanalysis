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
    
    public FilaAlineamiento(int tam){
        for(int i=0;i<tam+1;i++)fila.add(0);
    }
    public void setZeros(){
        for(int i=0;i<fila.size();i++)fila.set(i, 0);
    }
    
    public int getJ(int j){
        return fila.get(j);
    }
    
    //agrega padding a una tira de caracteres
    private static String padLeft(String s, int n) {
        return String.format("%1$" + n + "s", s);  
    }
    
    public void prettyPrint(int padding){
        String tira="";
        for(int i=0;i<fila.size();i++){
            tira+=padLeft(Integer.toString(fila.get(i)),padding);
        }
        System.out.println(tira);
    }
    
    public int getColumnas(){
        return fila.size();
    }
    
    public void setJ(int j, int valor){
        fila.set(j, valor);
    }
}
