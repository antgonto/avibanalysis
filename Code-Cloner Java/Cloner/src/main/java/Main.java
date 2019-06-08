import org.eclipse.jdt.core.JavaCore;
import org.eclipse.jdt.core.JavaModelException;
import org.eclipse.jdt.core.dom.AST;
import org.eclipse.jdt.core.dom.ASTParser;
import org.eclipse.jdt.core.dom.ASTVisitor;
import org.eclipse.jdt.core.dom.Block;
import org.eclipse.jdt.core.dom.CompilationUnit;
import org.eclipse.jdt.core.dom.MethodDeclaration;
import org.eclipse.jdt.core.dom.MethodInvocation;
import org.eclipse.jdt.core.dom.NumberLiteral;
import org.eclipse.jdt.core.dom.SimpleName;
import org.eclipse.jdt.core.dom.Statement;
import org.eclipse.jdt.core.dom.StringLiteral;
import org.eclipse.jdt.core.dom.TypeDeclaration;
import org.eclipse.jdt.core.dom.VariableDeclarationStatement;
import org.eclipse.jdt.core.dom.rewrite.ASTRewrite;
import org.eclipse.jdt.core.dom.rewrite.ListRewrite;
import org.eclipse.jface.text.BadLocationException;
import org.eclipse.jface.text.Document;
import org.eclipse.text.edits.MalformedTreeException;
import org.eclipse.text.edits.TextEdit;
import org.json.*;

import java.io.BufferedWriter;
import java.io.FileOutputStream;
import java.io.FileWriter;
import java.io.IOException;
import java.io.OutputStreamWriter;
import java.io.Writer;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.Dictionary;
import java.util.HashSet;
import java.util.Hashtable;
import java.util.List;
import java.util.Map;
import java.util.Random;
import java.util.Set;


import com.google.gson.Gson;

public class Main {
	static int clones = 0;
	static int identifierConsecutive = 0;
	static int numberLiterals = 0;
	final static ArrayList<MethodDeclaration> methodOfProject = new ArrayList();
	//Snippets de 1 sentencia.
	final static ArrayList listSnippetsSizeOne = new ArrayList();
	//Snippets de 2 sentencias.
	final static ArrayList listSnippetsSizeTwo = new ArrayList();
	//Snippets de 3 sentencias.
	final static ArrayList listSnippetsSizeThree = new ArrayList();
	//Snippets de 5 sentencias.
	final static ArrayList listSnippetsSizeFive = new ArrayList();
	public static void main(String[] args) {
		try{
			/*
			 * LOAD ALL PARAMETERS FROM JSON
			 */
            String text = new String(Files.readAllBytes(Paths.get("C:\\Users\\Steven\\Documents\\GitHub\\avibanalysis\\Code-Cloner Java\\Files\\Config.js")), StandardCharsets.UTF_8);
            JSONObject obj = new JSONObject(text);
            final String snippets = obj.getString("snippets");
            final String pathSalida = obj.getString("salida");
            final String project = obj.getString("project");
            int minLimit = obj.getInt("minLimit");
            int maxLimit = obj.getInt("maxLimit");
            int minCopy = obj.getInt("minOfCopy");
            int maxCopy = obj.getInt("maxOfCopy");
            int minLimitSentences = obj.getInt("minLimitSentences");
            int maxLimitSentences = obj.getInt("maxLimitSentences");
            int QuantityOfClone = obj.getInt("quantityOfClone");
            final String sourceSnippets = new String(Files.readAllBytes(Paths.get(snippets)), StandardCharsets.UTF_8);
            final String sourceProject = new String(Files.readAllBytes(Paths.get(project)), StandardCharsets.UTF_8);
            
            /*
			 * LOAD AND CREATE COMPILATION UNIT FROM SNIPPETS AND PROJECT
			 */
            Document documentSnippets = new org.eclipse.jface.text.Document(sourceSnippets);
            CompilationUnit cu = parse(documentSnippets);
            
            Document documentProject = new org.eclipse.jface.text.Document(sourceProject);
            CompilationUnit cuProject = parse(documentProject);
            
            AST astProject = cuProject.getAST();
            ASTRewrite rewriteProject = ASTRewrite.create(astProject);
            
            getSnippets(cu);
            getMethodsFromProject(cuProject);
            
            int copy = 1;
        	for(int i = 0; i<QuantityOfClone;i++)
            {
        		Random rand = new Random();
            	int numberOfCopy = rand.nextInt((maxCopy - minCopy))+minCopy;	
        		createNewClons(astProject,rewriteProject,minLimitSentences,maxLimitSentences,numberOfCopy,2);
            }
         
            TextEdit edits2 = rewriteProject.rewriteAST(documentProject,null);
            //TextEdit edits = rewriteSnippets.rewriteAST(documentSnippets,null);
            edits2.apply(documentProject);
			
			BufferedWriter writer = new BufferedWriter(new FileWriter(pathSalida));
		    writer.write(documentProject.get());
		    writer.close();
            
        }
        catch(Exception ex){
            System.out.println(ex.toString());
        }
	}
	public static void createNewClons(AST ast, ASTRewrite rewriter,int minBound, int maxBound,int cantidadClones, int tipoClon) throws JavaModelException, IllegalArgumentException, MalformedTreeException, BadLocationException, IOException 
	{
		
		Random rand = new Random();
		int numberOfSentences = rand.nextInt((maxBound - minBound) + 1);
		numberOfSentences += minBound;
		System.out.println("NumberOfSentences: "+numberOfSentences);
		
		
		ArrayList<ArrayList<Statement>> clones = new ArrayList();
		for(int i =0;i<cantidadClones;i++)
		{
			ArrayList<Statement> r = new ArrayList();
			clones.add(r);
		}
		
			//GET THE SIZE OF THE NEW CLON
			//numberOfSentences = 5;
			while(numberOfSentences>=1)
			{

				int max = 1;
				int min = 1;
				if(numberOfSentences>=5)
				{
					max = 4;
				}else
				{
					if(numberOfSentences>=3)
					{
						max = 3;
					}else
					{
						if(numberOfSentences>=2)
						{
							max = 2;
						}
						else
						{
							max = 1;
						}
					}
					
				}
				//SELECT SNIPPET
				int sentence = rand.nextInt(max)+1;
				//int sentence = 4;	
				switch(sentence)
				{
				case 4:
					int element = rand.nextInt(listSnippetsSizeFive.size());
					MethodDeclaration methods = (MethodDeclaration)listSnippetsSizeFive.get(element);
					for (int i = 0; i < methods.getBody().statements().size(); i++ )
					{
						Statement statements = (Statement)methods.getBody().statements().get(i);
						if (tipoClon ==1)
						{
							clones.get(0).add(statements);
						}
						else
						{
							if(tipoClon == 2)
							{
								for (int j=0;j<cantidadClones;j++)
								{
									Statement newStatement =(Statement)statements.copySubtree(ast, statements);
									newStatement = convertStatmentToTypeTwo(newStatement);
									clones.get(j).add(newStatement);
								}
								
							}
						}
					}break;
					
				case 3:
					element = rand.nextInt(listSnippetsSizeThree.size());
					methods = (MethodDeclaration)listSnippetsSizeThree.get(element);
					for (int i = 0; i < methods.getBody().statements().size(); i++ )
					{
						Statement statements = (Statement)methods.getBody().statements().get(i);
						if (tipoClon ==1)
						{
							clones.get(0).add(statements);
						}
						else
						{
							if(tipoClon == 2)
							{
								for (int j=0;j<cantidadClones;j++)
								{

									Statement newStatement =(Statement)statements.copySubtree(ast, statements);
									newStatement = convertStatmentToTypeTwo(newStatement);
									clones.get(j).add(newStatement);
								}
								
							}
						}
					}break;
				case 2:
					element = rand.nextInt(listSnippetsSizeTwo.size());
					methods = (MethodDeclaration)listSnippetsSizeTwo.get(element);
					for (int i = 0; i < methods.getBody().statements().size(); i++ )
					{
						Statement statements = (Statement)methods.getBody().statements().get(i);
						if (tipoClon ==1)
						{
							clones.get(0).add(statements);
						}
						else
						{
							if(tipoClon == 2)
							{
								for (int j=0;j<cantidadClones;j++)
								{

									Statement newStatement =(Statement)statements.copySubtree(ast, statements);
									newStatement = convertStatmentToTypeTwo(newStatement);
									clones.get(j).add(newStatement);
								}
								
							}
						}
					}break;
				case 1:
					element = rand.nextInt(listSnippetsSizeOne.size());
					methods = (MethodDeclaration)listSnippetsSizeOne.get(element);
					for (int i = 0; i < methods.getBody().statements().size(); i++ )
					{
						Statement statements = (Statement)methods.getBody().statements().get(i);
						if (tipoClon ==1)
						{
							clones.get(0).add(statements);
						}
						else
						{
							if(tipoClon == 2)
							{
								for (int j=0;j<cantidadClones;j++)
								{

									Statement newStatement =(Statement)statements.copySubtree(ast, statements);
									newStatement = convertStatmentToTypeTwo(newStatement);
									clones.get(j).add(newStatement);
								}
								
							}
						}
					}break;
				}
				numberOfSentences -= sentence;
			}
		//Agrego los clones a sus métodos correspondientes		
		if (tipoClon ==1)
		{
			for (int i =0;i<cantidadClones;i++)
			{
				MethodDeclaration methodSelected = methodOfProject.get(rand.nextInt(methodOfProject.size()));
				ListRewrite listRewrite = rewriter.getListRewrite(methodSelected.getBody(), Block.STATEMENTS_PROPERTY);
				
				//Selecciono el clon 0 por que es el que contiene los de tipo uno.
				for(int j =0;j<clones.get(0).size();j++)
				{
					//Inserto todos los Statements a un metodo.
					listRewrite.insertFirst(clones.get(0).get(j), null);
				}
				
		    	
			}
			return;
		}
		else
		{
			for (int i =0;i<cantidadClones;i++)
			{
				MethodDeclaration methodSelected = methodOfProject.get(rand.nextInt(methodOfProject.size()));
				ListRewrite listRewrite = rewriter.getListRewrite(methodSelected.getBody(), Block.STATEMENTS_PROPERTY);
				
				
				for(int j =0;j<clones.get(i).size();j++)
				{
					//Inserto todos los Statements a un metodo.
					listRewrite.insertFirst(clones.get(i).get(j), null);
				}
			}
		}
		
	}
	
	public static Statement convertStatmentToTypeTwo(Statement statement)
	{
		
		
		statement.accept(new ASTVisitor()
				{
				Hashtable<String, String> identificadoresNuevos = new Hashtable();
				public boolean visit(SimpleName node) 
				{

					if(!identificadoresNuevos.containsKey(node.getIdentifier()))
					{
						identificadoresNuevos.put(node.getIdentifier(), "_ide_Cne_Tpe_2_"+identifierConsecutive);
						identifierConsecutive++;
					}
					node.setIdentifier(identificadoresNuevos.get(node.getIdentifier()));
					return true;
				
				}
				public boolean visit(NumberLiteral node) 
				{
					node.setToken(String.valueOf(numberLiterals++));
					return true;
				}
				public boolean visit(StringLiteral node) 
				{
					node.setLiteralValue("LiteString"+String.valueOf(numberLiterals++));
					return true;
				}
				
				});
		return statement;
	}

	
	public static void getSnippets(CompilationUnit cu)
	{
		AST ast = cu.getAST();
		ASTRewrite  rewriter = ASTRewrite.create(ast);
		cu.recordModifications();		
		cu.accept(new ASTVisitor() {
	        //by add more visit method like the following below, then all king of statement can be visited.
			public boolean visit(MethodDeclaration node) {
				List listStatements = node.getBody().statements();
				//Snippets de 1 sentencia.
				switch(listStatements.size())
				{
				case 1:
					listSnippetsSizeOne.add(node);
					break;
				case 2:
					listSnippetsSizeTwo.add(node);
					break;
				case 3:
					listSnippetsSizeThree.add(node);
					break;
				case 5:
					listSnippetsSizeFive.add(node);
					break;
					
				}
				
			
			
			return false;
		}});
		

		System.out.println("Metodos 1 : "+listSnippetsSizeOne.size());
		System.out.println("Metodos 2 : "+listSnippetsSizeTwo.size());
		System.out.println("Metodos 3 : "+listSnippetsSizeThree.size());
		System.out.println("Metodos 5 : "+listSnippetsSizeFive.size());
	}
	public static void getMethodsFromProject(CompilationUnit cu)
	{
		AST ast = cu.getAST();
		ASTRewrite  rewriter = ASTRewrite.create(ast);
		cu.recordModifications();		
		cu.accept(new ASTVisitor() {
	        //by add more visit method like the following below, then all king of statement can be visited.
			public boolean visit(MethodDeclaration node) {
					methodOfProject.add(node);
					return true;
				}	
				});
	}
	public static CompilationUnit parse(Document doc) throws JavaModelException, MalformedTreeException, BadLocationException, IOException
	{
		Map options = JavaCore.getOptions();
		JavaCore.setComplianceOptions(JavaCore.VERSION_1_7, options);
		
		ASTParser parser = ASTParser.newParser(AST.JLS3);
		parser.setCompilerOptions(options);
		parser.setSource(doc.get().toCharArray());
		parser.setResolveBindings(true);
		parser.setKind(ASTParser.K_COMPILATION_UNIT);
		final CompilationUnit cu = (CompilationUnit) parser.createAST(null);
		
		return cu;
		
	}
	
}
