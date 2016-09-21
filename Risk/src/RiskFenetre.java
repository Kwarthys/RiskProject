import java.awt.BorderLayout;

import javax.swing.JFrame;
import javax.swing.JPanel;

public class RiskFenetre extends JFrame
{
	private JPanel container = new JPanel();
	private WarZone sim;
	
	public RiskFenetre()
	{	
		setTitle("Risk Project");		
		
		container.setLayout(new BorderLayout());
		
		setSize(1050,1050);
		sim = new WarZone();
		container.add(sim, BorderLayout.CENTER);
			
		this.setContentPane(container);
		setVisible(true);	
			
		System.out.println("lancement !");
		
		setLocationRelativeTo(null);		
		setDefaultCloseOperation(JFrame.EXIT_ON_CLOSE);
		go();
	}
		
	private void go()
	{
		boolean go = true;
		while(go)
		{
			System.out.println("ça tourne");
			repaint();
			try{
				Thread.sleep(20);
			}catch(InterruptedException e){
				e.printStackTrace();
			}
			
			go = false;
		}
	}
}
