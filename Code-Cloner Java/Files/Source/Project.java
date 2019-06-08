package example;

public class Methods {
   
  // Constructor method
 
  Methods() {
    System.out.println("Constructor method is called when an object of it's class is created");
  }
 
  // Main method where program execution begins
 
  public static void main(String[] args) {
    staticMethod();
    Methods object = new Methods();
    object.nonStaticMethod();
  }

  static void staticMethod1() {
    System.out.println("Static method can be called without creating object");
  }
   static void staticMethod2() {
    System.out.println("Static method can be called without creating object");
  } 

  static void staticMethod3() {
    System.out.println("Static method can be called without creating object");
  } 

  static void staticMethod4() {
    System.out.println("Static method can be called without creating object");
  } 

  static void staticMethod5() {
    System.out.println("Static method can be called without creating object");
  } 

  static void staticMethod6() {
    System.out.println("Static method can be called without creating object");
  } 

  static void staticMethod7() {
    System.out.println("Static method can be called without creating object");
  }
 
  // Non static method
 
  void nonStaticMethod() {
    System.out.println("Non static method must be called by creating an object");
  }
}