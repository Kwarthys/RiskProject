import java.awt.Color;
import java.awt.Graphics;
import java.util.ArrayList;

import javax.swing.JPanel;

@SuppressWarnings("serial")
public class WarZone extends JPanel{

	private int map1[][] = new int[1000][1000];
	private int map2[][] = new int[1000][1000];

	private ArrayList<State> allStates = new ArrayList<>();
	
	private ArrayList<State> noVoisins = new ArrayList<>();
	
	public WarZone()
	{
		super();
		setSize(1000,1000);
		for(int j = 0; j < 1000;j++)
		{
			for(int i = 0; i < 1000;i++)
			{
				map1[j][i] = 0;
				map2[j][i] = -1;
			}
		}
		
		generateMap();
	}
	
	private void generateMap()
	{
		System.out.println("Generating...");
		for(int i = 0; i < 400; i++)
			generate();
		System.out.println("Generated.\nCreating States...");
		
		drawStates();

		System.out.println("States Created.\n Merging States...");
		
		while(allStates.size() > 30)
			mergeStates();

		System.out.println("States merged.");
	}
	
	private void applyMap()
	{
		for(int j = 0; j < 1000;j++)
		{
			for(int i = 0; i < 1000;i++)
			{
				map1[j][i] = map2[j][i];
				map2[j][i] = -1;
			}
		}
	}
	
	private void generate()
	{
		int bord = 10;
		int proba = 3000000;
		
		for(int j = 0; j < 1000;j++)
		{
			for(int i = 0; i < 1000;i++)
			{
				if( j<bord || i<bord || j>1000-bord || i>1000-bord)
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
		for(int j = 0; j < 1000 ; j++)
		{
			for (int i = 0; i < 1000; i++)
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
			noVoisins.add(smallerState);
			return;
		}
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
		
		stateFusion(smallerState,smallerState.getNeighbours().get(smallerIndex));
		
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
	
	public void paintComponent(Graphics g)
	{

		System.out.println("Painting");
		System.out.println("nb of states : " + allStates.size());
		
		int stateMaxID = 0;
		for(int i = 0; i < allStates.size();i++)
			stateMaxID = Math.max(stateMaxID, allStates.get(i).getID());
		
		for(int j = 0; j < 1000;j++)
		{
			for(int i = 0; i < 1000;i++)
			{
				if(map1[j][i] == -1)g.setColor(Color.WHITE);
				else if(map1[j][i] == 0)g.setColor(Color.BLUE);
				else 
				{
					g.setColor(new Color((int)(Math.pow(map1[j][i],4)%256), 100+(int)(Math.pow(map1[j][i],6)%156) ,255-(int)(Math.pow(map1[j][i],3)%256)));
				}
				
				g.fillRect(i, j, 1,1);
				
				//g.setColor(Color.WHITE);
				//g.drawRect(150, 150, 700, 700);
			}
		}
		
		g.setColor(Color.BLACK);
		for(State s : allStates)
		{
			for(int[] c : s.getBoundaries())
			{
				g.fillRect(c[1], c[0], 1,1);
			}
		}
		
		
	}
}
