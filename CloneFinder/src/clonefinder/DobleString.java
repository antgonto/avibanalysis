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
public class DobleString {
    private String strArriba;
    private String strAbajo;
    
    public DobleString(){
        strArriba="";
        strAbajo="";
    }
    public DobleString(String str1, String str2){
        this.strArriba=str1;
        this.strAbajo=str2;
    }
    /**
     * Obtiene el string que va en la parte superior en la visualizacion de alineamientos de smith-waterman
     * @return el string
     */
    public String getStringArriba(){
        return strArriba;
    }
    /**
     * Obtiene el string que va en la parte inferior en la visualizacion de alineamientos de smith-waterman
     * @return el string
     */
    public String getStringAbajo(){
        return strAbajo;
    }
    /**
     * agrega un caracter al string superior
     * @param s lo que se quiere agregar al string
     */
    public void appendStringArriba(String s){
        strArriba=s+strArriba;
    }
    /**
     * agrega un caracter al string inferior
     * @param s lo que se quiere agregar al string
     */
    public void appendStringAbajo(String s){
        strAbajo=s+strAbajo;
    }
    
    /**
     * imprime de forma bonita la visualizacion de alineamientos locales de smith-waterman
     */
    public void prettyPrint(){
        System.out.println(strArriba);
        String a="";
        for(int i=0;i<strArriba.length();i++){
            a+="|";
        }
        System.out.println(a);
        System.out.println(strAbajo);
    }
    
}
