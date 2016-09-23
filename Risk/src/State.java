import java.util.ArrayList;

public class State {
	
	private ArrayList<int[]> lands = new ArrayList<>();
	private ArrayList<int[]> boundaries = new ArrayList<>();
	private ArrayList<State> neighbours = new ArrayList<>();
	
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

	public int size() {
		return lands.size();
	}

	public ArrayList<int[]> getBoundaries() {
		return boundaries;
	}

	public ArrayList<State> getNeighbours() {
		return neighbours;
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
	
	public int addToBoundaries(int j, int i)//Returns the state ID 
	{
		int[] tmp = {j,i};
		boundaries.add(tmp);
		return stateID;
	}
	
	public int addToBoundaries(int[] c)//Returns the state ID 
	{
		boundaries.add(c);
		return stateID;
	}
	
	public void resetBoundaries() 
	{
		boundaries = new ArrayList<>();
	}
	
	public void addNeighbour(State s)
	{
		neighbours.add(s);
	}

}
