import java.util.ArrayList;
import java.util.Date;

public class Terrain {
	
	int map2[][], map1[][], width, height;
	
	private double seed;

	private ArrayList<State> allStates = new ArrayList<>();
	public ArrayList<State> getAllStates(){return allStates;}
	
	private ArrayList<State> noVoisins = new ArrayList<>();
	public ArrayList<State> getNoVoisins(){return noVoisins;}
	
	public Terrain(int pwidth, int pheight, double pSeed)
	{
		width = pwidth; height = pheight;
		
		seed = pSeed;
		
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
		generate();
		System.out.println("Generated in " + (new Date().getTime() - startTime.getTime()) + "ms\nCreating States...");
		
		Date startStates = new Date();
		drawStates();

		System.out.println("States Created in " + (new Date().getTime() - startStates.getTime()) + "ms\n Merging States...");

		Date startMerge = new Date();
		while(allStates.size() > 45)
			mergeStates();

		System.out.println("States merged in " + (new Date().getTime() - startMerge.getTime()) + "ms.");
		System.out.println("OverAll generation in " + (new Date().getTime() - startTime.getTime()) + "ms.");
		
		return map1;
	}

	private void generate()
	{
		for(int j = 0; j < height;j++)
		{
			for(int i = 0; i < width;i++)
			{
				double noise = 0;
				
				if(i > 850 || i < 150)
				{
					if(i>850)noise-= (150 - (1000-i));
					else noise-=(150-i);
				}
				if( j > 850 || j<150)
				{
					if(j>850)noise-= (150 - (1000-j));
					else noise-=(150-j);
				}
				
				double x = (double)i;x/=200;
				double y = (double)j;y/=200;
				noise += (int)(127*ImprovedNoise.noise(x,y,seed));
				noise += (int)(100*ImprovedNoise.noise(x+10,y+10,seed));
				noise += (int)(80*ImprovedNoise.noise(x*2,y*2,seed));
				noise += (int)(5*ImprovedNoise.noise(x*50,y*50,seed));
				noise += (int)(5*ImprovedNoise.noise(x*100,y*100,seed));
				if(noise>0)
					map1[j][i] = 1;
				else
					map1[j][i] = 0;
				
			}
		}
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
