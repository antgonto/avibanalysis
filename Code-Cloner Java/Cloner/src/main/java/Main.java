import org.eclipse.jdt.core.JavaCore;
import org.eclipse.jdt.core.JavaModelException;
import org.eclipse.jdt.core.dom.AST;
import org.eclipse.jdt.core.dom.ASTNode;
import org.eclipse.jdt.core.dom.ASTParser;
import org.eclipse.jdt.core.dom.ASTVisitor;
import org.eclipse.jdt.core.dom.Block;
import org.eclipse.jdt.core.dom.BlockComment;
import org.eclipse.jdt.core.dom.Comment;
import org.eclipse.jdt.core.dom.CompilationUnit;
import org.eclipse.jdt.core.dom.LineComment;
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
import java.io.File;
import java.io.FileOutputStream;
import java.io.FileWriter;
import java.io.IOException;
import java.io.OutputStreamWriter;
import java.io.Writer;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Date;
import java.util.Dictionary;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Hashtable;
import java.util.List;
import java.util.Map;
import java.util.Random;
import java.util.Set;


import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import com.google.gson.JsonElement;
import com.google.gson.JsonParser;

public class Main {
	static int clonesID = 0;
	static int identifierConsecutive = 0;
	static int numberLiterals = 0;
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
	
	static String[] keys;
	final static Map<String, ArrayList<MethodDeclaration>> methodMap = new HashMap<String, ArrayList<MethodDeclaration>>();
	final static Map<String, String> javaRootMap = new HashMap<String, String>(); // Root - Content of file.
	final static ArrayList<String> javaRootMapOutput = new ArrayList(); // Root - Content of file.
	final static Map<String, AST> astMap = new HashMap<String, AST>(); // Root - Content of file.
	final static Map<String, ASTRewrite> astRewriteMap = new HashMap<String, ASTRewrite>(); // Root - Content of file.

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
	static int countClonesByType[] = new int[4];
	static String sourceSnippets = "";
	static String sourceProject = "";
	static String outputJson = "";
	static BufferedWriter writerDescription;
	static CompilationUnit cuProject;
	static ArrayList<GroupClone> groupByClone = new ArrayList();
	public static void main(String[] args) {
		try{
			/*
			 * LOAD ALL PARAMETERS FROM JSON
			 */
			String text = new String(Files.readAllBytes(Paths.get("C:\\Users\\Steven\\OneDrive - Estudiantes ITCR\\Github\\Clone-Java\\Code-Cloner Java\\Files\\Config.js")), StandardCharsets.UTF_8);
			JSONObject obj = new JSONObject(text);
			snippets = obj.getString("snippets");
			pathSalida = obj.getString("salida");
			outputJson = obj.getString("outputJson");
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
			//
			getJavaFilesInput(project);
			/*
			 * LOAD AND CREATE COMPILATION UNIT FROM SNIPPETS AND PROJECT
			 */
			Document documentSnippets = new org.eclipse.jface.text.Document(sourceSnippets);
			CompilationUnit cuSnippets = parse(documentSnippets);
			getSnippets(cuSnippets);
		

			int copy = 1;
			for(int i = 0; i<QuantityOfClone;i++)
			{
				
				Random rand = new Random();
				int typeClone = rand.nextInt(3)+1;
				int numberOfCopy = rand.nextInt((maxCopy - minCopy))+minCopy;
				
				createNewClons(minLimitSentences,maxLimitSentences,numberOfCopy,typeClone,i);
				countClonesByType[typeClone]++;
			}
			
			writeOutput(project,pathSalida);
        	
			//Etapa de lectura de la salida para la obtención de las nuevas lineas de código.
			for(int i = 0; i<javaRootMapOutput.size();i++)
			{
				String sourceOutput = new String(Files.readAllBytes(Paths.get(javaRootMapOutput.get(i))), StandardCharsets.UTF_8);
				Document documentOutput = new org.eclipse.jface.text.Document(sourceOutput);
				CompilationUnit cuOutput = parse(documentOutput);
				AST astOutput = cuOutput.getAST();
				getLines(cuOutput,documentOutput);
			}
			writeJsonLog();
			
		}
		catch(Exception ex){
			System.out.println(ex.toString());
		}
	}
	public static void writeJsonLog()
	{
		JSONObject json = new JSONObject();
		Date date = new Date();
		DateFormat dateFormat = new SimpleDateFormat("yyyy/MM/dd HH:mm:ss");
		json.put("Date",dateFormat.format(date));
		json.put("Quantity clones type 1",countClonesByType[1]);
		json.put("Quantity clones type 2",countClonesByType[2]);
		json.put("Quantity clones type 3",countClonesByType[3]);
		JSONArray cloneList = new JSONArray();
		for(int k = 0; k<groupByClone.size();k++)
		{
			//json.put("Clone : "+k,);
			JSONArray copiasList = new JSONArray();
			ArrayList<CloneCopy> clones= groupByClone.get(k).arrayClones;
			
			JSONObject copia  = new JSONObject();
			int i = 0;
			copia.put("ID:",clones.get(i).ID);
			copia.put("StartLine:",clones.get(i).startLine);
			copia.put("EndLine:",clones.get(i).endLine);
			copia.put("Name Method Ubication",clones.get(i).name);
			copia.put("Path:",clones.get(i).absolutePath);
			JSONArray cloneListPair = new JSONArray();
			for(int j = 1;j<clones.size();j++)
			{
				JSONObject pair = new JSONObject();
				pair.put("ID:",clones.get(j).ID);
				pair.put("StartLine:",clones.get(j).startLine);
				pair.put("EndLine:",clones.get(j).endLine);
				pair.put("Name Method Ubication",clones.get(j).name);
				pair.put("Path:",clones.get(j).absolutePath);
				cloneListPair.put(pair);
			}
			copia.put("Pairs", cloneListPair);
			copiasList.put(copia);
			
			JSONObject copiasJSON = new JSONObject();
			copiasJSON.put("Copias", copiasList);
			copiasJSON.put("Tipo de clon",groupByClone.get(k).typeClone);
			cloneList.put(copiasJSON);
		}
		json.put("Clones", cloneList);
		
		try (FileWriter file = new FileWriter(outputJson)) {
			JsonParser jp = new JsonParser();
			JsonElement je = jp.parse(json.toString());
			Gson gson = new GsonBuilder().setPrettyPrinting().create();
			String prettyJsonString = gson.toJson(je);
            file.write(prettyJsonString);
            file.flush();
 
        } catch (IOException e) {
            e.printStackTrace();
        }
		
	}
	public static void getJavaFilesInput(String root) throws IOException, JavaModelException, MalformedTreeException, BadLocationException
	{
	    final File folder = new File(root);
        search(".*\\.java", folder, javaRootMap);
        keys = javaRootMap.keySet().toArray(new String[javaRootMap.size()]);
        for (String key : keys)
        {
        	Document documentProject = new org.eclipse.jface.text.Document(javaRootMap.get(key));
			cuProject = parse(documentProject);
			
			AST astProject = cuProject.getAST();
			ASTRewrite rewriteProject = ASTRewrite.create(astProject);
			astMap.put(key, astProject);
			astRewriteMap.put(key, rewriteProject);
			getMethodsFromProject(key, cuProject);

        }
	}
	public static void writeOutput(String root, String rootOutput) throws IOException, JavaModelException, MalformedTreeException, BadLocationException
	{
	    final File folder = new File(root);
	    searchFolderOutput(".*\\.java", folder,rootOutput);
	}
	
	public static void search(final String pattern, final File folder, Map<String,String> result) throws IOException {
        for (final File f : folder.listFiles()) {

            if (f.isDirectory()) {
                search(pattern, f, result);
            }

            if (f.isFile()) {
                if (f.getName().matches(pattern)) {
                	String sourceProject = new String(Files.readAllBytes(Paths.get(f.getAbsolutePath())), StandardCharsets.UTF_8);
                    result.put(f.getAbsolutePath(), sourceProject);
                }
            }

        }
    }
	public static void searchFolderOutput(final String pattern, final File folder, String outputPath) throws IOException, MalformedTreeException, BadLocationException {
        for (final File f : folder.listFiles()) {

            if (f.isDirectory()) {
            	String directory = outputPath +"\\"+ f.getName();
	    	    File dir = new File(directory);
	    	    if (!dir.exists()) dir.mkdirs();
	    	    new File(directory);
            
            	searchFolderOutput(pattern, f, outputPath + "\\"+ f.getName());
            }

            if (f.isFile()) {
                if (f.getName().matches(pattern)) {
                	ASTRewrite rewriteProject = astRewriteMap.get(f.getAbsolutePath());
    				Document documentProject = new org.eclipse.jface.text.Document(javaRootMap.get(f.getAbsolutePath()));
    				TextEdit edits2 = rewriteProject.rewriteAST(documentProject,null);
    				//TextEdit edits = rewriteSnippets.rewriteAST(documentSnippets,null);
    				edits2.apply(documentProject);
    				//MODIFICAR PATH DE SALIDA
    				BufferedWriter writer = new BufferedWriter(new FileWriter(outputPath+"\\"+f.getName()));
    				writer.write(documentProject.get());
    				writer.close();
    				javaRootMapOutput.add(outputPath+"\\"+f.getName());
                }
                else
                {
                	String sourceProject = new String(Files.readAllBytes(Paths.get(f.getAbsolutePath())), StandardCharsets.UTF_8);
                	Document documentProject = new org.eclipse.jface.text.Document(javaRootMap.get(f.getAbsolutePath()));
                	documentProject.set(sourceProject);
                	BufferedWriter writer = new BufferedWriter(new FileWriter(outputPath+"\\"+f.getName()));
    				writer.write(documentProject.get());
    				writer.close();
                }
            }

        }
    }
	public static void createNewClons(int minBound, int maxBound,int cantidadClones, int tipoClon, int ID_CLONE) throws JavaModelException, IllegalArgumentException, MalformedTreeException, BadLocationException, IOException 
	{
		Random rand = new Random();
		int numberOfSentences = rand.nextInt((maxBound - minBound) + 1);
		numberOfSentences += minBound;
		System.out.println("NumberOfSentences: "+numberOfSentences);

		//Get random AST and ASTRewrite
		int totalClass = javaRootMap.size();
		
		
		ArrayList<CloneCopy> clones = new ArrayList();
		ArrayList<Integer> indexKey = new ArrayList();
		for(int i =0;i<cantidadClones;i++)
		{
			CloneCopy clonCopy = new CloneCopy(++clonesID);
			clones.add(clonCopy);
			int indexAST = (rand.nextInt(totalClass));// Index for get random AST
			
			String comment = "//START @..CLON:"+ID_CLONE+"@..COPY:"+i+"@..ID_GENERAL:"+clonesID+"@..TYPE:"+tipoClon;
			ASTRewrite rewriter = astRewriteMap.get(keys[indexAST]);
			indexKey.add(indexAST);
			Statement placeHolderStart = (Statement) rewriter.createStringPlaceholder(comment, ASTNode.EMPTY_STATEMENT);
			clones.get(i).statements.add(placeHolderStart);
			comment = "//END @..CLON:"+ID_CLONE+"@..COPY:"+i+"@..ID_GENERAL:"+clonesID;
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
					String key = keys[indexKey.get(j)];
					AST ast = astMap.get(key);
					
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
			String key = keys[indexKey.get(i)];
			AST ast = astMap.get(key);
			ASTRewrite rewriter = astRewriteMap.get(key);
			//Seleccionar metodo de la clase donde voy a insertar el clon
			ArrayList<MethodDeclaration> methodOfProject = methodMap.get(key);
			MethodDeclaration methodSelected = methodOfProject.get(rand.nextInt(methodOfProject.size()));
			methodSelected.getParent();
			//clones.get(i).ubication = methodSelected.getName().getIdentifier();
			ListRewrite listRewrite = rewriter.getListRewrite(methodSelected.getBody(), Block.STATEMENTS_PROPERTY);
			clones.get(i).name = methodSelected.getName().getIdentifier();
			clones.get(i).absolutePath = key;
			switch(tipoClon) {
			case 1:
				for(int j =0;j<clones.get(i).statements.size();j++)
				{
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
		
		GroupClone gr = new GroupClone();
		gr.typeClone = tipoClon;
		for(int i = 0;i<clones.size();i++)
		{
			gr.arrayClones.add(clones.get(i));
		}
		groupByClone.add(gr);
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
	
	public static void getLines(final CompilationUnit cu,final Document documentOutput)
	{
		AST ast = cu.getAST();
		for(Comment comment: (List<Comment>) cu.getCommentList())
		{
			comment.accept(new ASTVisitor() {
				//by add more visit method like the following below, then all king of statement can be visited.
				public boolean visit(LineComment node) {
					int lineNumber = cu.getLineNumber(node.getStartPosition());
					System.out.println(lineNumber);
					int start = node.getStartPosition();
					int end = start + node.getLength();
					String comment = null;
					try {
						comment = documentOutput.get(start, node.getLength());
						if(comment.contains("START @..CLON")) {
							String[] parts = comment.split("@");
							String[] idCloneSplit = parts[1].split(":");
							String[] idCopySplit = parts[2].split(":");
							//groupByClone
							int idClone = Integer.parseInt(idCloneSplit[1]);
							int idCopy = Integer.parseInt(idCopySplit[1]);
							CloneCopy clone = groupByClone.get(idClone).arrayClones.get(idCopy);
							clone.startLine = lineNumber +1;
							System.out.println("Linecita "+clone.startLine);
						}
						else
						{
							if(comment.contains("END @..CLON")) {
								String[] parts = comment.split("@");
								String[] idCloneSplit = parts[1].split(":");
								String[] idCopySplit = parts[2].split(":");
								//groupByClone
								int idClone = Integer.parseInt(idCloneSplit[1]);
								int idCopy = Integer.parseInt(idCopySplit[1]);
								CloneCopy clone = groupByClone.get(idClone).arrayClones.get(idCopy);
								clone.endLine = lineNumber -1;
								System.out.println("Linecita Final "+clone.startLine);
							}
						}
						
					} catch (BadLocationException e) {
						// TODO Auto-generated catch block
						e.printStackTrace();
					}
					//System.out.println(comment);
					return true;
					}});
		}
		ArrayList<Integer> lineasNuevas = new ArrayList();
		System.out.println(lineasNuevas.size());
		for(int i = 0; i<lineasNuevas.size();i++)
		{
			System.out.println("Linea: "+lineasNuevas.get(i));
		}
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
	
	public static void getMethodsFromProject(String key, CompilationUnit cu)
	{
		AST ast = cu.getAST();
		ASTRewrite  rewriter = ASTRewrite.create(ast);
		cu.recordModifications();		
		cu.accept(new ASTVisitor() {
			//by add more visit method like the following below, then all king of statement can be visited.
			public boolean visit(MethodDeclaration node) {
				ArrayList<MethodDeclaration>  methodOfProject = methodMap.get(key);
				if(methodOfProject == null)
				{
					ArrayList<MethodDeclaration> newArray = new ArrayList();
					newArray.add(node);
					methodMap.put(key,newArray);
				}
				else
				{
					methodOfProject.add(node);
				}
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
