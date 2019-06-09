import java.util.ArrayList;
import java.util.List;

import org.eclipse.jdt.core.IMethod;
import org.eclipse.jdt.core.dom.ASTVisitor;
import org.eclipse.jdt.core.dom.MethodDeclaration;

public class MainMethodVisitor extends ASTVisitor {
	List<MethodDeclaration> methods = new ArrayList<MethodDeclaration>();

    @Override
    public boolean visit(MethodDeclaration node) {
        System.out.println("No me llamaron");
    	IMethod iMethod = (IMethod) node.resolveBinding().getJavaElement();
    	System.out.println(iMethod.getElementName());
    	return true;
    }
    
}
