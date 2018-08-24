/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package clonefinder;

import java.util.ArrayList;



/**
 * Clase usada para obtener la subsecuencia local mas grande
 * Implementa la generacion de la matriz de alineamiento
 * Tambien implementa el traceback
 * @author Juan
 */
public class SmithWaterman {
    String tira1;
    String tira2;
    private int MATCH=2;
    private int MISMATCH=-1;
    private int GAP=-2; 
    private ArrayList<Integer>maxIs=new ArrayList<Integer>();
    private ArrayList<Integer>maxJs=new ArrayList<Integer>();
    private MatrizAlineamiento m;
  
    /**
     * Funcion de ayuda que retorna el maximo de 3 valores
     * @param a
     * @param b
     * @param c
     * @return 
     */
    private int maximo(int a, int b, int c){
        int d=0;
        if(a>=b && a>=c && a>=d)return a;
        if(b>=a && b>=c && b>=d)return b;
        if(c>=a && c>=b && c>=d)return c;
        return d;
    }
    /**
     * Genera la matriz de alineamiento
     * @param tira1 una de las dos tiras que se quieren analizar
     * @param tira2 la otra tira que se quiere analizar
     */
    public void construirMatriz(String tira1,String tira2){
        this.tira1=tira1;
        this.tira2=tira2;
        int valmax=-1;
        maxIs.clear();
        maxJs.clear();
        m=new MatrizAlineamiento(tira1,tira2);
        for(int i=1;i<m.getFilas();i++){
            for(int j=1;j<m.getColumnas();j++){
                int a=m.getIJ(i-1, j-1);
                if(m.getSigma(i, j))a+=MATCH;
                    else a+=MISMATCH;
                int b=m.getIJ(i-1,j)+GAP;
                int c=m.getIJ(i, j-1)+GAP;
                a=maximo(a,b,c);
                if(a>valmax){   //almacenar el valor maximo en la matriz
                    maxIs.clear();
                    maxJs.clear();
                    valmax=a;
                    maxIs.add(i);
                    maxJs.add(j);
                }
                else if (a==valmax){    //por si hay mas de un valor maximo en la matriz
                    maxIs.add(i);
                    maxJs.add(j);
                }
                m.setIJ(a, i, j);
            }
        }
    }
    
    /**
     * Establece el coeficiente de match
     * @param i 
     */
    public void setMatch(int i){
        MATCH=i;
    }
    /**
     * Establece el coeficiente de mismatch
     * @param i 
     */
    public void setMismatch(int i){
        MISMATCH=i;
    }
    /**
     * Establece el coeficiente de gap
     * @param i 
     */
    public void setGap(int i){
        GAP=i;
    }
    /**
     * Imprime parametros de la clase, como el coeficiente de match, mismatch y gap
     */
    public void printear(){
        System.out.println("Match: "+Integer.toString(MATCH));
        System.out.println("Mismatch: "+Integer.toString(MISMATCH));
        System.out.println("GAP: "+Integer.toString(GAP));
    }
    
    /**
     * Imprime la matriz de una forma bonita
     */
    public void prettyPrint(){
        m.prettyPrint();
    }
    
    /**
     * Obtiene los alineamientos locales mas grandes a partir de los indices 
     * donde estan los mayores valores de la matriz
     * Puede haber mas de una posicion en la matriz con valores maximos
     * @return 
     */
    public ArrayList<DobleString> obtenerAlineamientos(){
        ArrayList<DobleString>alineamientos=new ArrayList<DobleString>();
        for(int i=0;i<maxIs.size();i++){
            DobleString ds=new DobleString();
            alineamientoEn(maxIs.get(i),maxJs.get(i),  ds);
            alineamientos.add(ds);
        }
        for(int i=0;i<alineamientos.size();i++){
            alineamientos.get(i).prettyPrint();
            System.out.println("********");
        }
        return alineamientos;
    }
    
    /**
     * Retorna la matriz de alineamiento
     * @return 
     */
    public MatrizAlineamiento getMatriz(){
        return m;
    }
    
    /**
     * Se usa cuando se hace el traceback
     * Retorna el valor de la casiila de la diagonal
     * @param i la posicion en i
     * @param j la posicion en j
     * @return el valor de la diagonal
     */
    private int getDiagonal(int i, int j){
        if(m.getSigma(i,j)){
            
            return m.getIJ(i-1,j-1)+MATCH;
        }
        else {
            return m.getIJ(i-1, j-1)+MISMATCH;
        }
        
    }
    /**
     * Retorna el valor de la casilla de arriba
     * Se usa cuando se quiere hacer el traceback del algoritmo
     * @param i la posicion en i
     * @param j la posicion en j
     * @return el valor de la casilla de arriba
     */
    private int getArriba(int i, int j){
        return m.getIJ(i-1, j)+GAP;
    }
    /**
     * Se usa en el traceback de smith-waterman
     * Retorna el valor de la casilla a la izquierda de la que se analiza
     * @param i la posicion en x de la casilla que se esta analizando
     * @param j la posicion en y de la casilla que se esta analizando
     * @return 
     */
    private int getIzquierda(int i, int j){
        return m.getIJ(i, j-1)+GAP;
    }
    /**
     * Retorna la subsecuencia local mas grande a partir de una posicion
     * Usa recursividad de cola
     * @param i
     * @param j
     * @param ds donde se almacena la subsecuencia local que se esta construyendo
     */
    private void alineamientoEn(int i, int j, DobleString ds){
        int arriba=getArriba(i,j);
        int diagonal=getDiagonal(i,j);
        int izquierda=getIzquierda(i,j);
        if(arriba==m.getIJ(i, j)){
            ds.appendStringArriba("-");
            ds.appendStringAbajo(""+tira2.charAt(i-1));
            if(m.getIJ(i-1, j)==0)return;
            alineamientoEn(i-1,j,ds);
        }
        else if(izquierda==m.getIJ(i, j)){
            ds.appendStringArriba(""+tira1.charAt(j-1));
            ds.appendStringAbajo("-");
            if(m.getIJ(i, j-1)==0)return;
            alineamientoEn(i,j-1,ds);
        }
        else if(diagonal==m.getIJ(i, j)){
            ds.appendStringArriba(""+tira1.charAt(j-1));
            ds.appendStringAbajo(""+tira2.charAt(i-1));
            if(m.getIJ(i-1, j-1)==0)return;
            alineamientoEn(i-1,j-1,ds);
        }
        
    } 
}
