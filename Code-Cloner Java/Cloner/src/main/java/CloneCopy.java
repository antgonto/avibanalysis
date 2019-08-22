import java.util.ArrayList;

import org.eclipse.jdt.core.dom.Statement;

public class CloneCopy {
	ArrayList<Statement> statements = new ArrayList();
	int ID = -1;
	int startLine = -1;
	int endLine = -1;
	String name = "";
	String absolutePath = "";
	
	public CloneCopy(int _ID)
	{
		this.ID = _ID;
	}
}
