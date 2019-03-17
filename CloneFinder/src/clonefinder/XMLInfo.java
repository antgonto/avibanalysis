/*
 * To change this license header, choose License Headers in Project Properties.
 * To change this template file, choose Tools | Templates
 * and open the template in the editor.
 */
package clonefinder;

import java.io.File;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import org.w3c.dom.Document;

/**
 * Clase que se encarga de retornar informacion del archivo de configuracion
 * Esta informacion se usa para modificar el comportamiento del algoritmo
 * La informacion se saca de un archivo .xml
 * @author Juan
 */
public class XMLInfo {
    
    private String archivo;
    private File file;
    private DocumentBuilderFactory documentBuilderFactory;
    DocumentBuilder documentBuilder;
    Document document;
    
    
    public XMLInfo(String archivo){
        try{
            this.archivo=archivo;
            file = new File(archivo);
            documentBuilderFactory = DocumentBuilderFactory
            .newInstance();
            documentBuilder = documentBuilderFactory.newDocumentBuilder();
            document = documentBuilder.parse(file);
        }
        catch(Exception e){
            System.out.println("Error leyendo el archivo");
        }
    }
    
    public String getArchivo1(){
        String dato = document.getElementsByTagName("archivo1").item(0).getTextContent();
        return dato;
    }
    
    public String getArchivo2(){
        String dato = document.getElementsByTagName("archivo2").item(0).getTextContent();
        return dato;
    }
    
    public int getMatch(){
        String dato = document.getElementsByTagName("match").item(0).getTextContent();
        return Integer.parseInt(dato);
    }
    
    public int getMismatch(){
        String dato = document.getElementsByTagName("mismatch").item(0).getTextContent();
        return Integer.parseInt(dato);
    }
    
    public int getGap(){
        String dato = document.getElementsByTagName("gap").item(0).getTextContent();
        return Integer.parseInt(dato);
    }
    
   
    
}
