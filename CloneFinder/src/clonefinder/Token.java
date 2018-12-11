/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package clonefinder;

/**
 * Clase que almacena un token
 * Almacena tanto el codigo del token como el texto
 * @author Juan
 */
public class Token {
    String texto;
    int numero;
    
    /**
     * Se crea
     * @param numero el codigo del token
     * @param texto el texto asociado al token
     */
    public Token(int numero, String texto){
        this.texto=texto;
        this.numero=numero;
    }
    
    /**
     * Retorna el codigo del token
     * @return 
     */
    public int getNumero(){
        return numero;
    }
    
    /**
     * Retorna el texto asociado al token
     * @return 
     */
    public String getTexto(){
        return texto;
    }
    
}
