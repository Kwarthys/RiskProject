import java.awt.Color;
import java.awt.Graphics;

import javax.swing.JFrame;
import javax.swing.JPanel;

@SuppressWarnings("serial")
public class WarZone extends JPanel{

	private int map[][];
	
	private Terrain terrainGenerator;
	
	public WarZone(JFrame pparent)
	{
		super();
		
		terrainGenerator = new Terrain(1000,1000);
		
		map = terrainGenerator.generateMap();
		
		int moy = 0;
		int i = 0;
		for(State s : terrainGenerator.getAllStates())
		{
			moy+=s.size();i++;
			System.out.println(s.size());
		}
		moy/=i;
		System.out.println(moy);
		
	}
	/*
	public void start(JFrame parent)
	{
		parent.repaint();
		while(terrainGenerator.nextMerge())
		{
			parent.repaint();
			try{
				Thread.sleep(300);
			}catch(InterruptedException e){
				e.printStackTrace();
			}
		}
	}
	*/
	
	
	public void paintComponent(Graphics g)
	{
		//System.out.println("Painting");
		int stateMaxID = 0;
		for(int i = 0; i < terrainGenerator.getAllStates().size();i++)
			stateMaxID = Math.max(stateMaxID, terrainGenerator.getAllStates().get(i).getID());
		
		for(int j = 0; j < 1000;j++)
		{
			for(int i = 0; i < 1000;i++)
			{
				if(map[j][i] == -1)g.setColor(Color.WHITE);
				else if(map[j][i] == 0)g.setColor(Color.BLUE);
				else 
				{
					g.setColor(new Color(100+(int)(Math.pow(map[j][i],4)%156), 100+(int)(Math.pow(map[j][i],6)%156) ,255-(int)(Math.pow(map[j][i],3)%200)));
				}
				
				g.fillRect(i, j, 1,1);
				
				//g.setColor(Color.WHITE);
				//g.drawRect(150, 150, 700, 700);
			}
		}
		
		g.setColor(Color.BLACK);
		for(State s : terrainGenerator.getAllStates())
		{
			for(int[] c : s.getBoundaries())
			{
				g.fillRect(c[1], c[0], 1,1);
			}
		}		
	}
}
