/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package clonefinder;

import com.github.javaparser.JavaParser;
import com.github.javaparser.JavaToken;
import com.github.javaparser.TokenRange;
import com.github.javaparser.ast.CompilationUnit;
import java.io.File;
import java.util.ArrayList;


/**
 * Clase que se encarga de parsear los archivos
 * @author Juan
 */
public class Parser {
    
    public static void main(String[] args){
        String archivo="C:\\Users\\Juan\\Desktop\\prueba.java";
        try{
            parsear(archivo);
        }
        catch(Exception e){
            System.out.println("File not found exception");
        }
    }
    /*TODO: 
    arreglar smith-waterman para que lo haga con numeros
    determinar que tokens no son importantes
    */
    /**
     * Se encarga de retornar una lista con los tokens del archivo
     * @param archivo el path del archivo a tokenizar
     * @return un arraylist con los tokens del archivo
     * @throws Exception si no puede abrir el archivo
     */
    public static ListaTokens parsear(String archivo){
        ListaTokens tokens = new ListaTokens();
        try{
            //ArrayList<Token> tokens = new ArrayList<Token>();
            File file = new File(archivo);
            CompilationUnit cu = JavaParser.parse(file);
            Token tok;
            for (JavaToken t:cu.getTokenRange().get()){
                tok = new Token(t.getKind(), t.getText());
                tokens.insertarToken(tok);
                //System.out.println(t.getKind()+" "+t.getText());
            }
            return tokens;
        }
        catch (Exception e){
            return tokens;
        }
    }
    
}
