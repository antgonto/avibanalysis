/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package clonefinder;

import java.util.ArrayList;

/**
 *
 * @author Juan
 */
public class ListaTokens {
    ArrayList<Token> tokens;
    public ListaTokens(){
        tokens = new ArrayList<Token>();
    }
    
    public int getTamano(){
        return tokens.size();
    }
    
    /**
     * Retorna el codigo del token en la posicion i
     * @param i
     * @return 
     */
    public int getToken(int i){
        return tokens.get(i).getNumero();
    }
    
    /**
     * Retorna el texto asociado al token en la posicion i
     * @param i el numero de token que se quiere consultar
     * @return 
     */
    public String getTexto(int i){
        return tokens.get(i).getTexto();
    }
    
    public void insertarToken(Token t){
        tokens.add(t);
    }
}
