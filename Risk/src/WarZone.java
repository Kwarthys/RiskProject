import java.awt.Color;
import java.awt.Graphics;
import java.util.ArrayList;

import javax.swing.JPanel;

public class WarZone extends JPanel{

	private int map1[][] = new int[1000][1000];
	private int map2[][] = new int[1000][1000];
	
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
		System.out.println("Generating");
		for(int i = 0; i < 400; i++)
			generate();
		System.out.println("Generated");
		
		//detectContinent();
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
					ArrayList<int[]> voisins = getVoisin(j,i,0);
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
		
		for(int j = 0; j < 1000;j++)
		{
			for(int i = 0; i < 1000;i++)
			{
				map1[j][i] = map2[j][i];
				//System.out.print(map1[j][i]);
				map2[j][i] = -1;
			}
		}
	}
	
	private ArrayList<int[]> getVoisin(int j, int i, int comparator)
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
	
	public void paintComponent(Graphics g)
	{

		System.out.println("Painting");
		
		for(int j = 0; j < 1000;j++)
		{
			for(int i = 0; i < 1000;i++)
			{
				//map1[j][i];
				//System.out.println(map1[j][i]);
				
				if(map1[j][i] == 1)g.setColor(Color.GREEN);
				else if(map1[j][i] == -1)g.setColor(Color.BLACK);
				else g.setColor(Color.BLUE);
				g.fillRect(i, j, 1,1);
				
				//g.setColor(Color.WHITE);
				//g.drawRect(200, 200, 600, 600);
			}
		}
		
	}
	
	private void detectContinent()
	{
		int continentID = 1;
		for(int j = 0; j < 1000 ; j++)
		{
			for (int i = 0; i < 1000; i++)
			{
				if(map1[j][i] == 1 && map2[j][i] == -1)
				{
					startRecursiveContinent(j,i,continentID);
				}
				else
					map2[j][i] = 0;
			}
		}
	}
	
	private void startRecursiveContinent(int j, int i, int id)
	{
		map2[j][i] = id;
		ArrayList<int[]> voisins = getVoisin(j,i,1);
	}
}
