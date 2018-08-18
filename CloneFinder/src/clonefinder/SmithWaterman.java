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
public class SmithWaterman {
    private static int MATCH=2;
    private static int MISMATCH=-1;
    private static int GAP=-2; 
    
    private static int maximo(int a, int b, int c){
        int d=0;
        if(a>=b && a>=c && a>=d)return a;
        if(b>=a && b>=c && b>=d)return b;
        if(c>=a && c>=b && c>=d)return c;
        return d;
    }
    
    public static MatrizAlineamiento construirMatriz(String tira1,String tira2){
        MatrizAlineamiento m=new MatrizAlineamiento(tira1,tira2);
        for(int i=1;i<m.getFilas();i++){
            for(int j=1;j<m.getColumnas();j++){
                int a=m.getIJ(i-1, j-1);
                if(m.getSigma(i, j))a+=MATCH;
                    else a+=MISMATCH;
                int b=m.getIJ(i-1,j)+GAP;
                int c=m.getIJ(i, j-1)+GAP;
                a=maximo(a,b,c);
                m.setIJ(a, i, j);
            }
        }
        return m;
    }
    
    //establece los coeficientes usados en el matching
    public static void setMatch(int i){
        MATCH=i;
    }
    public static void setMismatch(int i){
        MISMATCH=i;
    }
    public static void setGap(int i){
        GAP=i;
    }
    
    public static void printear(){
        System.out.println("Match: "+Integer.toString(MATCH));
        System.out.println("Mismatch: "+Integer.toString(MISMATCH));
        System.out.println("GAP: "+Integer.toString(GAP));
    }
}
