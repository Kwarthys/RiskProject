import java.util.ArrayList;
import java.util.Date;

public class Terrain {
	
	int map2[][], map1[][], width, height;

	private ArrayList<State> allStates = new ArrayList<>();
	public ArrayList<State> getAllStates(){return allStates;}
	
	private ArrayList<State> noVoisins = new ArrayList<>();
	public ArrayList<State> getNoVoisins(){return noVoisins;}
	
	public Terrain(int pwidth, int pheight)
	{
		width = pwidth; height = pheight;
		
		map1 = new int[height][width];
		map2 = new int[height][width];
		
		for(int j = 0; j < height;j++)
		{
			for(int i = 0; i < width;i++)
			{
				map1[j][i] = 0;
				map2[j][i] = -1;
			}
		}
	}
	
	public int[][] generateMap()
	{
		Date startTime = new Date();
		System.out.println("Generating...");
		for(int i = 0; i < 400; i++)
			generate();
		System.out.println("Generated in " + (new Date().getTime() - startTime.getTime()) + "ms\nCreating States...");
		
		Date startStates = new Date();
		//drawStates();

		System.out.println("States Created in " + (new Date().getTime() - startStates.getTime()) + "ms\n Merging States...");

		Date startMerge = new Date();
		//while(allStates.size() > 45)
		//	mergeStates();

		System.out.println("States merged in " + (new Date().getTime() - startMerge.getTime()) + "ms.");
		System.out.println("OverAll generation in " + (new Date().getTime() - startTime.getTime()) + "ms.");
		
		return map1;
	}

	private void generate()
	{
		int bord = 10;
		int proba = 3000000;
		
		for(int j = 0; j < height;j++)
		{
			for(int i = 0; i < width;i++)
			{
				if( j<bord || i<bord || j>height-bord || i>width-bord)
				{
					//System.out.println("Avant " + map1[j][i]);
					if(map2[j][i] != 1)map2[j][i] = map1[j][i];
					//System.out.println("Apres " + map1[j][i]);
				}
				else if(map1[j][i] == 0) //Detection du zéro
				{
					if((int)(proba*Math.random()) == 42)
					{
						//System.out.println("True ? : " + (i<250 || j<250 || i>750 || j >750));
						if( i<150 || j<150 || i>850 || j >850) //Bord de map
						{
							//System.out.println("here : " + i + " " + j);
							if((int)(100*Math.random()) == 42)
							{
								//System.out.println("Dans le bord : " + i + "|" + j);
								map2[j][i] = 1;
							}
							else if(map2[j][i] != 1)map2[j][i] = 0;
						}
						else
						{
							//System.out.println("Hors Bord : " + i + "|" + j);
							map2[j][i] = 1;
						}
						//System.out.println("Must be one " + map2[j][i]);
					}
					else if(map2[j][i] != 1)map2[j][i] = 0;
					//System.out.println("Must be zero " + map1[j][i]);
				}
				else //Conversion des voisins
				{
					map2[j][i] = 1;
					//System.out.println("computing " + j + " " + i + " " + map1[j][i]);
					ArrayList<int[]> voisins = getVoisins(j,i,0);
					int laChance = voisins.size()*10;
					//System.out.println("Voisins size : " + voisins.size() + " chance : " + laChance);
					
					if(voisins.size()!=0)
					{
						int leVoisin = (int)(Math.random()*voisins.size());
						int leJ = voisins.get(leVoisin)[0];
						int leI = voisins.get(leVoisin)[1];
						//System.out.println("leJ leI : " + leJ + " " + leI);
						//System.out.println("le random : " + (int)(Math.random()*100));
						if((int)(Math.random()*100) < laChance)
						{
							//System.out.println((leVoisin+1) + " " + voisins.size() + " " + " : " + (leJ-j) + " " + (leI-i));
							map2[leJ][leI] = 1;
						}
					}
				}
				
			}
		}

		applyMap();
	}
	
	private void applyMap()
	{
		for(int j = 0; j < height;j++)
		{
			for(int i = 0; i < width;i++)
			{
				map1[j][i] = map2[j][i];
				map2[j][i] = -1;
			}
		}
	}
	
	private ArrayList<int[]> getVoisins(int j, int i, int comparator)
	{
		ArrayList<int[]> leReturn = new ArrayList<int[]>();
		
		if(map1[j-1][i] == comparator)//DESSUS
		{
			int[] tmp = {j-1,i};
			leReturn.add(tmp);
		}
		
		if(map1[j+1][i] == comparator)//DESSOUS
		{
			int[] tmp = {j+1,i};
			leReturn.add(tmp);
		}
		
		if(map1[j][i+1] == comparator)//DROITE
		{
			int[] tmp = {j,i+1};
			leReturn.add(tmp);
		}
		
		if(map1[j][i-1] == comparator)//GAUCHE
		{
			int[] tmp = {j,i-1};
			leReturn.add(tmp);
		}
		
		return leReturn;
	}
	
	private void drawStates()
	{
		int stateID = 1;
		for(int j = 0; j < height ; j++)
		{
			for (int i = 0; i < width; i++)
			{
				if(map1[j][i] == 1 && map2[j][i] == -1)
				{
					allStates.add(new State(stateID++));
					//System.out.println("new State " + stateID);
					stateBuilding(allStates.get(allStates.size()-1), j, i);
				}
				else if(map1[j][i] == 0)map2[j][i] = 0;
			}
		}
		applyMap();
	}
	
	private void stateBuilding(State s,int j, int i)
	{
		map2[j][i] = s.addToState(j,i);
		//System.out.println("State ID : " + s.getID());
		if(s.getLands().size() > 1000)return;
		
		boolean isFrontier = false;
		
		ArrayList<int[]> voisins = getVoisins(j,i,1);
		if(voisins.size() < 4)isFrontier = true;
		while(voisins.size()>0)
		{
			int[] c = voisins.remove((int)(Math.random()*voisins.size()));
			//System.out.println(c[0] + " " + c[1]);
			if(map1[c[0]][c[1]] == 1) // if terrain is land
			{
				if(map2[c[0]][c[1]] == -1) // if land has not yet been stated
				{
					//System.out.println("Coming from : " + j + "|" + i + " to " + c[0] + "|" + c[1]);
					stateBuilding(s, c[0],c[1]);
				}
				else if(map2[c[0]][c[1]] != s.getID()) // if land is another state
				{
					isFrontier = true;
				}
			}

		}
		
		if(isFrontier)
			s.addToBoundaries(j, i);
	}
	
	private void mergeStates()
	{
		int smallerIndex = -1;
		int smallerSize = (int)Float.POSITIVE_INFINITY;
		for(int i = 0; i < allStates.size(); i++)
		{
			if(smallerSize > allStates.get(i).size() && !noVoisins.contains(allStates.get(i)))
			{
				smallerSize = allStates.get(i).size();
				smallerIndex = i;
			}
		}
		
		State smallerState = allStates.get(smallerIndex);
		
		for(State s : allStates)
		{
			if(s != smallerState) //state not checking boundaries with himslef
			{
				for(int[] bSmaller : smallerState.getBoundaries())
				{
					if(getVoisins(bSmaller[0],bSmaller[1], s.getID()).size() != 0)
					{
						smallerState.addNeighbour(s);
					}
				}
			}
		}
		if(smallerState.getNeighbours().size() == 0)
		{
			smallIslandMerge(smallerState);
			return;
		}
		State otherState;
		if(allStates.size()>2000)
		{
			smallerIndex = 0;
			smallerSize = smallerState.getNeighbours().get(0).size();
			for(int i = 0; i < smallerState.getNeighbours().size(); i++)
			{
				if(smallerSize > smallerState.getNeighbours().get(i).size());
				{
					smallerSize = smallerState.getNeighbours().get(i).size();
					smallerIndex = i;
				}
			}
			
			otherState = smallerState.getNeighbours().get(smallerIndex);
		}
		else
		{
			ArrayList<Integer> count = new ArrayList<>(); 
			for(int i = 0; i < smallerState.getNeighbours().size(); i++)
			{
				count.add(i,0);
				for(int[] c : smallerState.getBoundaries())
				{
					if(getVoisins(c[0], c[1], smallerState.getNeighbours().get(i).getID()).size() != 0)
					{
						count.set(i,count.get(i)+1);
					}
				}
			}
			smallerIndex = 0;
			int biggerBoundaries = count.get(0);
			for(int i = 0; i < count.size(); i++)
			{
				if(biggerBoundaries < count.get(i))
				{
					smallerIndex = i;
					biggerBoundaries = count.get(i);
				}
			}
			
			otherState = smallerState.getNeighbours().get(smallerIndex);
		}
		
		stateFusion(smallerState,otherState);
		
	}
	
	private void smallIslandMerge(State s)
	{
		double smallerDistance = Float.POSITIVE_INFINITY;
		State closestState = s;
		for(State n : allStates)
		{
			if(s!=n)
			{
				for(int[] p1 : s.getBoundaries())
				{
					for(int[] p2 : n.getBoundaries())
					{
						if(getDistance(p1,p2) < smallerDistance)
						{
							if(n!=closestState)closestState = n;
							smallerDistance = getDistance(p1,p2);
						}
					}
				}
			}
		}
		
		stateFusion(s, closestState);
	}
	
	private void stateFusion(State s1, State s2)
	{
		State biggerIDState, lowerIDState;
		if(s1.getID()>s2.getID())
		{
			biggerIDState = s1;
			lowerIDState = s2;
		}
		else
		{
			biggerIDState = s2;
			lowerIDState = s1;
		}
	
		allStates.remove(biggerIDState);

		for(int[] c : biggerIDState.getLands())
		{
			map1[c[0]][c[1]] = lowerIDState.getID();
			lowerIDState.addToState(c[0],c[1]);
		}
		
		computeBoundaries(lowerIDState);
		
	}
	
	private void computeBoundaries(State s)
	{
		s.resetBoundaries();
		for(int[] c : s.getLands())
		{
			if(getVoisins(c[0], c[1], s.getID()).size() < 4 )
			{
				s.addToBoundaries(c);
			}
		}
	}
	
	static public double getDistance(int x1, int y1, int x2, int y2)
	{
		return Math.sqrt((x1-x2)*(x1-x2) + (y1-y2)*(y1-y2));
	}
	
	static public double getDistance(int[] p1, int[] p2)
	{
		return getDistance(p1[0], p1[1], p2[0], p2[1]);
	}
}
