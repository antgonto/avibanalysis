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
public class CloneFinder {

    /**
     * @param args the command line arguments
     */
    
    /*
    HECHO
    --matriz
    --maxima subsecuencia local
    --con varios puntos de partida
    --
    -ver https://www.slideshare.net/avrilcoghlan/the-smith-waterman-algorithm
    TODO
    -limpiar espacios de los tokens
    -visualizar de manera bonita la matriz
    */
    
    
    public static void main(String[] args) {
        // TODO code application logic here
        
        /*
        String an1="if numero==0:\n" +
"        return \"Numero neutro\"\n" +
"    else:\n" +
"        if numero%2==0:\n" +
"            return \"El numero es par\"\n" +
"        else:\n" +
"            return \"El numero es impar\"";
        String an2="if year>=2000:\n" +
"        if year%4==0:\n" +
"            return True\n" +
"        else:\n" +
"            return False\n" +
"    else:\n" +
"        return \"ERROR EN EL ANiO\"";
        String tira1="GGCTCAATCA";
        String tira2="ACCTAAGG";
        String archivo1="C:\\Users\\Juan\\Desktop\\prueba.java";
        String archivo2="C:\\Users\\Juan\\Desktop\\prueba2.java";
        */
        String archivo1;
        String archivo2;
        int match, mismatch, gap;
        
        //los datos de configuracion estan almacenados en un archivo XML
        //el path del archivo se recibe como un parametro
        String infoPath;
        try{
            infoPath = args[0];
        }
        catch(Exception e){
            //si no se paso como parametro se toma el valor default
            infoPath = "C:\\Users\\Juan\\Desktop\\investigaciones\\abiv\\avibanalysis\\CloneFinder\\src\\clonefinder\\info.xml";
        }
        XMLInfo info = new XMLInfo(infoPath);
        archivo1 = info.getArchivo1();
        archivo2 = info.getArchivo2();
        match = info.getMatch();
        mismatch = info.getMismatch();
        gap = info.getGap();
        
        System.out.println("Archvo de configuracion procesado...");
        ListaTokens tokens1 = Parser.parsear(archivo1);
        ListaTokens tokens2 = Parser.parsear(archivo2);
        System.out.println("Archivos parseados...");
        SmithWaterman sm=new SmithWaterman();
        sm.setMatch(match);
        sm.setMismatch(mismatch);
        sm.setGap(gap);
        sm.printear();
        //sm.construirMatriz(tira1, tira2);
        sm.construirMatriz(tokens1, tokens2);
        sm.prettyPrint();
        //sm.obtenerAlineamientos();
    }
    
}
