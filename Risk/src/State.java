import java.util.ArrayList;

public class State {
	
	private ArrayList<int[]> lands = new ArrayList<>();
	private int stateID;
	
	public State(int id)
	{
		stateID = id;
	}
	
	public int getID()
	{
		return stateID;
	}

	public ArrayList<int[]> getLands() {
		return lands;
	}

	public void setLands(ArrayList<int[]> lands) {
		this.lands = lands;
	}
	
	public int addToState(int j, int i)
	{
		int[] tmp = {j,i};
		lands.add(tmp);
		return stateID;
	}

}
