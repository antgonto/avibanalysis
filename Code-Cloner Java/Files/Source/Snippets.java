package example;

public class Snippets {

   /*
   *  Todos los ejemplos de snippets de 1 sentencia.
   *  
   */
   public static void snippetOneSentence1()
   {
         System.out.println("Hello World!");
   }

   public static void snippetOneSentence2()
   {
         for(int i = 2; i <= 100; i++)
         {
            resultado *= i;
         }
   }

   public static void snippetOneSentence3()
   {
         int resultad = (base10Num % 2) + resultad;

   }

   public static void snippetOneSentence4()
   {
      
        base10Num = Math.abs(base10); 
   }

   public static void snippetOneSentence5()
   {
      
      System.out.println(new BigInteger(1024, 10, new Random()));
   }

   public static void snippetOneSentence6()
   {
       for(int i = 2; i < result.length; i++)
         result[i] = true;
   }

   public static void snippetOneSentence7()
   {
      final double SQRT = Math.sqrt(num);
   }
   public static void snippetOneSentence8()
   {
      
        base10Num = Math.abs(base10); 
   }
   public static void snippetOneSentence9()
   {
       if(num % 58 == 0)
         result++;
   }

   public static void snippetOneSentence10()
   {
      try
      {   
         Scanner s = new Scanner( new File("scores.dat") );
          while( s.hasNextInt() )
          { 
            System.out.println( s.nextInt() );
          }
     }
      catch(IOException e)
      {  System.out.println( e );
      }
   }

 

   /*
   *  Todos los ejemplos de snippets de 2 sentencias.
   *  
   */

   public static void snippetTwoSentence1()
   {
      int numPrimes;
        for(int i = 2; i < 10000000; i++) {
         if(isPrime(i)) {
            numPrimes++;
         }
      }
   }
   public static void snippetTwoSentence2()
   {
      int total = 0;
      for(int val : list)
      {  total += val;
      }
   }


   public static void snippetTwoSentence3()
   {
      PrintStream writer = new PrintStream( new File("randInts.txt"));
      Random r = new Random();
   }


   public static void snippetTwoSentence4()
   {
       //search for location to insert value
        int pos = 0;
        while( pos < size() && value > get(pos) ){
            pos++;
        }
   }


   public static void snippetTwoSentence5()
   {
      boolean result;
        if(other == null)
            // we know this is not null so can't be equal
            result = false;
        else if(this == other)
            // quick check if this and other refer to same IntList object
            result = true;
   }


   public static void snippetTwoSentence6()
   {
      DemoClass dc = new DemoClass();
        dc.overloadTester();
   }


   public static void snippetTwoSentence7()
   {
      int result = 0;
      if(n == 0)
         result = 1;
      else
         result = n * fact(n-1);
   }

   public static void snippetTwoSentence8()
   {
      char[][] board = blankBoard(n);
      boolean solved = canSolve(board, 0);
   }
   public static void snippetTwoSentence9()
   {
      printOne();
      printTwo();
   }
   public static void snippetTwoSentence10()
   {
      startTime = System.nanoTime();
      stopTime = System.nanoTime(); 
   }



   /*
   *  Todos los ejemplos de snippets de 3 sentencias.
   *  
   */
   public static void snippetThreeSentence1()
   {
     String s1 = "Computer Science";
     int x = 307;
     String s2 = s1 + " " + x;

   }
   public static void snippetThreeSentence2()
   {
     PrintStream writer = new PrintStream( new File("randInts.txt"));
         Random r = new Random();
         final int LIMIT = 100;
   }
   public static void snippetThreeSentence3()
   {
     URL mySite = new URL("http://www.cs.utexas.edu/~scottm");
            URLConnection yc = mySite.openConnection();
            Scanner in = new Scanner(new InputStreamReader(yc.getInputStream()));
   }
   public static void snippetThreeSentence4()
   {
     Rectangle r1 = new Rectangle(0,0,5,5);
     System.out.println("In method go. r1 " + r1 + "\n");
     r1.setSize(10, 15);
   }
   public static void snippetThreeSentence5()
   {
     int[] temp = new int[newSize];
      int limit = Math.min(list.length, newSize);

      for(int i = 0; i < limit; i++)
      {  temp[i] = list[i];
      }
   }
   public static void snippetThreeSentence6()
   {
     int temp;
      boolean changed = true;
      for(int i = 0; i < list.length && changed; i++)
      {  changed = false;
         for(int j = 0; j < list.length - i - 1; j++)
         {  assert (j > 0) && (j + 1 < list.length) : "loop counter j " + j +
               "is out of bounds.";
            if(list[j] > list[j+1])
            {  changed = true;
               temp = list[j + 1];
               list[j + 1] = list[j];
               list[j] = temp;
            }
         }
      }
   }
   public static void snippetThreeSentence7()
   {
     boolean ascending = true;
      int index = 1;
      while( ascending && index < list.length )
      {  assert index >= 0 && index < list.length;

         ascending = (list[index - 1] <= list[index]);
         index++;
      }
   }
   public static void snippetThreeSentence8()
   {
     int count = 0;
            while (in.hasNext()) {
                System.out.println(in.next());
                count++;
            }
            System.out.println("Number of tokens: " + count);
   }
   public static void snippetThreeSentence9()
   {
      String result = "size: " + iSize + ", elements: [";
        for(int i = 0; i < iSize - 1; i++)
            result += iValues[i] + ", ";
        if(iSize > 0 )
            result += iValues[iSize - 1];
   }
   public static void snippetThreeSentence10()
   {
     IntListVer2 list1 = new IntListVer2();
     IntListVer2 list2 = new IntListVer2(100);
     for(int i = 0; i < 100; i += 5){
            list1.add(i);
            list2.add(i);
        }
   }

   /*
   *  Todos los ejemplos de snippets de 5 sentencias.
   *  
   */


   public static void snippetFiveSentence1()
   {
     list1 = new IntListVer2();
        for(int i = 0; i < 10000; i++)
            list1.add(i);
        s.start();
        list1.toString();
        s.stop();
   }
   public static void snippetFiveSentence2()
   {
     Map options = JavaCore.getOptions();
      JavaCore.setComplianceOptions(JavaCore.VERSION_1_7, options);
      ASTParser parser = ASTParser.newParser(AST.JLS3);
      parser.setCompilerOptions(options);
      parser.setSource(doc.get().toCharArray());
   }
   public static void snippetFiveSentence3()
   {
      String text = new String(Files.readAllBytes(Paths.get("C:\\Users\\Steven\\Documents\\GitHub\\avibanalysis\\Code-Cloner Java\\Cloner\\src\\Config.js")), StandardCharsets.UTF_8);
      JSONObject obj = new JSONObject(text);
      final String project = obj.getString("path");
      final String pathSalida = obj.getString("salida");
      int minLimit = obj.getInt("minLimit");
   }

   public static void snippetFiveSentence4()
   {
    assert data != null : "Failed precondition makeSet. parameter cannot be null";
        boolean good = true;
        int i = 0;
        while( good && i < data.length ){
            good = data[i] != null;
            i++;
        }
        return good;        
   }

   public static void snippetFiveSentence5()
   {
     assert hasNext();
            
            Object result = nextNode.getData();
            nextNode = nextNode.getNext();
            
            removeOK = true;
            posToRemove++;
   }

   public static void snippetFiveSentence6()
   {
     Node newNode = new Node(obj, null);
        if( size == 0 )
            head = newNode;
        else
            tail.setNext(newNode);
        tail = newNode;
        size++;
        size++;
   }

   public static void snippetFiveSentence7()
   {
    String result = "";
        Node temp = head;
        for(int i = 0; i < size; i++){
            result += temp.getData() + " ";
            temp = temp.getNext();
        }
        i++;
        return result;
   }

   public static void snippetFiveSentence8()
   {
      lineFromFile = scannerToReadAirlines.nextLine();
      airlineNames = lineFromFile.split(",");
      newAirline = new Airline(airlineNames);
      String goal = keyboard.nextLine();
      airlinesPartnersNetwork.add( newAirline );

   }

   public static void snippetFiveSentence9()
   {
    assert data != null && data.length > 0 : "Failed precondition";
            name = data[0];
            partners = new ArrayList<String>();
            for(int i = 1; i < data.length; i++)
                partners.add( data[i] );
      return true;
   }

   public static void snippetFiveSentence10()
   {
      //Wordcount
      File f = new File("ciaFactBook2008.txt");
      Scanner sc;
      sc = new Scanner(f);
      Map<String, Integer> wordCount = new TreeMap<String, Integer>();
      while(sc.hasNext()) 
      {
         String word = sc.next();
         if(!wordCount.containsKey(word))
           wordCount.put(word, 1);
         else
           wordCount.put(word, wordCount.get(word) + 1);
      }
   }

}