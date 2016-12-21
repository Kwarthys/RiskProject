using UnityEngine;
using System.Collections;

public class Organizer : ArrayList{


    public bool Add(SeaRoad a)
    {
        bool search = false;
        for (int i = 0; i < this.Count && !search; i++)
        {
            SeaRoad road = (SeaRoad)this[i];
            if (a.size() < road.size())
            {
                this.Insert(i, a);
                search = true;
            }
        }
        if (!search)
        {
            this.Insert(this.Count, a);
        }
        search = false;
        return true;
    }

    public double moyenne()
    {
        double moyenne = 0;
        for (int i = 0; i < this.Count; i++)
        {
            SeaRoad road = (SeaRoad)this[i];
            moyenne += road.size();
        }
        moyenne /= this.Count;
        return moyenne;
    }
    /**** Still in java, as i don't need it for now, it will stay like that
    public void show()
    {
        System.out.print(size() + " : ");
        for (int i = 0; i < size(); i++)
        {
            System.out.print(" : " + this.get(i).size());
        }
        System.out.println();
    }*/
}
