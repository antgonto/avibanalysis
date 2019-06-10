import org.eclipse.jdt.core.JavaCore;
import org.eclipse.jdt.core.JavaModelException;
import org.eclipse.jdt.core.dom.AST;
import org.eclipse.jdt.core.dom.ASTNode;
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
	static int clonesID = 0;
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
	
	static String snippets = "";
	static String pathSalida = "" ;
	static String project = "";
	static int minLimit;
	static int maxLimit;
	static int minCopy;
	static int maxCopy;
	static int minLimitSentences;
	static int maxLimitSentences;
	static int QuantityOfClone;
	static int minSentencesDeleteType3;
	static int maxSentencesDeleteType3;
	static int minSentencesAddType3;
	static int maxSentencesAddType3;
	static String sourceSnippets = "";
	static String sourceProject = "";
	static String pathDescripcion = "";
	static BufferedWriter writerDescription;
	public static void main(String[] args) {
		try{
			/*
			 * LOAD ALL PARAMETERS FROM JSON
			 */
			String text = new String(Files.readAllBytes(Paths.get("C:\\Users\\Steven\\Documents\\GitHub\\avibanalysis\\Code-Cloner Java\\Files\\Config.js")), StandardCharsets.UTF_8);
			JSONObject obj = new JSONObject(text);
			snippets = obj.getString("snippets");
			pathSalida = obj.getString("salida");
			pathDescripcion = obj.getString("description");
			writerDescription  = new BufferedWriter(new FileWriter(pathDescripcion));
			project = obj.getString("project");
			minLimit = obj.getInt("minLimit");
			maxLimit = obj.getInt("maxLimit");
			minCopy = obj.getInt("minOfCopy");
			maxCopy = obj.getInt("maxOfCopy");
			minLimitSentences = obj.getInt("minLimitSentences");
			maxLimitSentences = obj.getInt("maxLimitSentences");
			QuantityOfClone = obj.getInt("quantityOfClone");
			minSentencesDeleteType3 = obj.getInt("minSentencesDeleteType3");
			maxSentencesDeleteType3 = obj.getInt("maxSentencesDeleteType3");
			minSentencesAddType3 = obj.getInt("minSentencesAddType3");
			maxSentencesAddType3 = obj.getInt("maxSentencesAddType3");
			String sourceSnippets = new String(Files.readAllBytes(Paths.get(snippets)), StandardCharsets.UTF_8);
			String sourceProject = new String(Files.readAllBytes(Paths.get(project)), StandardCharsets.UTF_8);

			/*
			 * LOAD AND CREATE COMPILATION UNIT FROM SNIPPETS AND PROJECT
			 */
			int typeClone = 3;
			Document documentSnippets = new org.eclipse.jface.text.Document(sourceSnippets);
			CompilationUnit cu = parse(documentSnippets);

			Document documentProject = new org.eclipse.jface.text.Document(sourceProject);
			CompilationUnit cuProject = parse(documentProject);

			AST astProject = cuProject.getAST();
			ASTRewrite rewriteProject = ASTRewrite.create(astProject);

			getSnippets(cu);
			getMethodsFromProject(cuProject);

			int copy = 1;
			writerDescription.write("Cantidad de clones generados de forma individual = "+String.valueOf(QuantityOfClone)+"\n");
			for(int i = 0; i<QuantityOfClone;i++)
			{
				Random rand = new Random();
				int numberOfCopy = rand.nextInt((maxCopy - minCopy))+minCopy;
				writerDescription.write("*****************************************************\n");
				writerDescription.write("Clon tipo = "+String.valueOf(typeClone)+" : \n");
				writerDescription.write("Cantidad de copias = "+String.valueOf(numberOfCopy)+" : \n");
				createNewClons(astProject,rewriteProject,minLimitSentences,maxLimitSentences,numberOfCopy,typeClone);
			}

			TextEdit edits2 = rewriteProject.rewriteAST(documentProject,null);
			//TextEdit edits = rewriteSnippets.rewriteAST(documentSnippets,null);
			edits2.apply(documentProject);

			BufferedWriter writer = new BufferedWriter(new FileWriter(pathSalida));
			writer.write(documentProject.get());
			writer.close();
			writerDescription.close();

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

		ArrayList<CloneCopy> clones = new ArrayList();
		for(int i =0;i<cantidadClones;i++)
		{
			CloneCopy clonCopy = new CloneCopy(++clonesID);
			clones.add(clonCopy);
			String comment = "//Start Clon Type "+String.valueOf(tipoClon)+" ID: "+String.valueOf(clonesID);
			Statement placeHolderStart = (Statement) rewriter.createStringPlaceholder(comment, ASTNode.EMPTY_STATEMENT);
			clones.get(i).statements.add(placeHolderStart);
			comment = "//End Clon Type "+String.valueOf(tipoClon)+" ID: "+String.valueOf(clonesID);
			Statement placeHolderEnd = (Statement) rewriter.createStringPlaceholder(comment, ASTNode.EMPTY_STATEMENT);
			clones.get(i).statements.add(placeHolderEnd);
		}


		//GET THE SIZE OF THE NEW CLON
		//numberOfSentences = 5;
		while(numberOfSentences>=1){

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
			MethodDeclaration methods = getRandomMethodFromSnippet(sentence);

			for (int i = 0; i < methods.getBody().statements().size(); i++ )
			{
				Statement statements = (Statement)methods.getBody().statements().get(i);
				for (int j=0;j<cantidadClones;j++)
				{
					Statement newStatement =(Statement)statements.copySubtree(ast, statements);
					if(tipoClon != 1)
					{
						newStatement = convertStatment(newStatement,"ide_Type_2_");
					}
					clones.get(j).statements.add(clones.get(j).statements.size()-1, newStatement);
				}
			}
			numberOfSentences -= sentence;
		}

		//Agrego los clones a sus métodos correspondientes
		for (int i =0;i<cantidadClones;i++)
		{
			MethodDeclaration methodSelected = methodOfProject.get(rand.nextInt(methodOfProject.size()));
			clones.get(i).ubication = methodSelected.getName().getIdentifier();
			ListRewrite listRewrite = rewriter.getListRewrite(methodSelected.getBody(), Block.STATEMENTS_PROPERTY);
			switch(tipoClon) {
			case 1:
				for(int j =0;j<clones.get(i).statements.size();j++)
				{
					//Inserto todos los Statements a un metodo.
					listRewrite.insertLast(clones.get(i).statements.get(j), null);
					
				}
				break;
			case 2:
				for(int j =0;j<clones.get(i).statements.size();j++)
				{
					//Inserto todos los Statements a un metodo.
					listRewrite.insertLast(clones.get(i).statements.get(j), null);
				}
				break;
			case 3:
					deleteSentencesType3(clones.get(i).statements);
					addSentencesType3(ast,clones.get(i).statements);
					
					for(int j =0;j<clones.get(i).statements.size();j++)
					{
						//Inserto todos los Statements a un metodo.
						listRewrite.insertLast(clones.get(i).statements.get(j), null);
					}
				
				break;
			}
		}
		
		for(int i = 0;i<clones.size();i++)
		{
			writerDescription.write("-------------------------------------------------------------\n");
			writerDescription.write("\tCopia clon ID = "+String.valueOf(clones.get(i).ID)+"\n");
			writerDescription.write("\tCantidad de sentencias = "+String.valueOf(clones.get(i).statements.size())+"\n\n");
			writerDescription.write("\tUbicación de la copia = "+clones.get(i).ubication+"\n\n");
			
			for(int j = i+1;j<clones.size();j++)
			{
				writerDescription.write("\t\tPareja: Copia clon ID: "+String.valueOf(clones.get(i).ID)+" y Copia clon ID:" +String.valueOf(clones.get(j).ID)+ "\n");

			}
			writerDescription.write("\n");
			
		}
		return;

	}

	
	public static void deleteSentencesType3(ArrayList<Statement> sentences)
	{
		Random rand = new Random();
		int delete = rand.nextInt((maxSentencesDeleteType3-minSentencesDeleteType3)+1) + minSentencesDeleteType3;
		while(delete>0)
		{
			int i = rand.nextInt(sentences.size());

			if(i != 0 || i != sentences.size())
			sentences.remove(i);
			delete--;
		}
		
	}
	public static void addSentencesType3(AST ast, ArrayList<Statement> sentences)
	{
		Random rand = new Random();
		int add = rand.nextInt((maxSentencesAddType3-minSentencesAddType3)+1) + minSentencesAddType3;
		while(add>=1){
			int max = 1;
			if(add>=5)
			{
				max = 4;
			}else
			{
				if(add>=3)
				{
					max = 3;
				}else
				{
					if(add>=2)
					{
						max = 2;
					}
					else
					{
						max = 1;
					}
				}

			}
		add = add-max;
		MethodDeclaration methods = getRandomMethodFromSnippet(max);
		for (int i = 0; i < methods.getBody().statements().size(); i++ )
		{	
			Statement statements = (Statement)methods.getBody().statements().get(i);
			Statement newStatement =(Statement)statements.copySubtree(ast, statements);
			newStatement = convertStatment(newStatement,"addForType3_");
			int index = rand.nextInt(sentences.size());
			sentences.add(index, newStatement);
		}
	}
	}
	public static MethodDeclaration getRandomMethodFromSnippet(int sentence)
	{
		MethodDeclaration methods = null;
		Random rand = new Random();
		switch(sentence){
		case 4:
			int element = rand.nextInt(listSnippetsSizeFive.size());
			methods = (MethodDeclaration)listSnippetsSizeFive.get(element);
			break;

		case 3:
			element = rand.nextInt(listSnippetsSizeThree.size());
			methods = (MethodDeclaration)listSnippetsSizeThree.get(element);
			break;
		case 2:
			element = rand.nextInt(listSnippetsSizeTwo.size());
			methods = (MethodDeclaration)listSnippetsSizeTwo.get(element);
			break;
		case 1:
			element = rand.nextInt(listSnippetsSizeOne.size());
			methods = (MethodDeclaration)listSnippetsSizeOne.get(element);
			break;
		}

		return methods;

	}
	public static Statement convertStatment(Statement statement, final String identifier)
	{


		statement.accept(new ASTVisitor()
		{
			Hashtable<String, String> identificadoresNuevos = new Hashtable();
			public boolean visit(SimpleName node) 
			{

				if(!identificadoresNuevos.containsKey(node.getIdentifier()))
				{
					identificadoresNuevos.put(node.getIdentifier(), identifier+identifierConsecutive);
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
