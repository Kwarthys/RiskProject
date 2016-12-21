using UnityEngine;
using System.Collections;

public class State {

    private ArrayList lands = new ArrayList();
    private ArrayList boundaries = new ArrayList();
    private ArrayList neighbours = new ArrayList();

    private ArrayList roads = new ArrayList();

    private int stateID;

    private int[] _center;

    public State(int id)
    {
        stateID = id;
    }

    public int getID()
    {
        return stateID;
    }

    public void setID(int newid)
    {
        stateID = newid;
    }

    public ArrayList getLands()
    {
        return lands;
    }

    public int size()
    {
        return lands.Count;
    }

    public ArrayList getBoundaries()
    {
        return boundaries;
    }

    public ArrayList getNeighbours()
    {
        return neighbours;
    }

    public void setLands(ArrayList lands)
    {
        this.lands = lands;
    }

    public int addToState(int j, int i)
    {
        int[] tmp = { j, i };
        lands.Add(tmp);
        return stateID;
    }

    public int addToBoundaries(int j, int i)//Returns the state ID 
    {
        int[] tmp = { j, i };
        boundaries.Add(tmp);
        return stateID;
    }

    public int addToBoundaries(int[] c)//Returns the state ID 
    {
        boundaries.Add(c);
        return stateID;
    }

    public void resetBoundaries()
    {
        boundaries = new ArrayList();
    }

    public void resetNeighbours()
    {
        neighbours = new ArrayList();
    }

    public void addNeighbour(State s)
    {
        if (!neighbours.Contains(s)) neighbours.Add(s);
    }

    public int[] computeCenter()
    {
        int x = 0, y = 0;
        for (int i = 0; i < lands.Count; i++)
        {
            int[] p = (int[])lands[i];
            x += p[1];
            y += p[0];
        }
        x /= lands.Count;
        y /= lands.Count;
        int[] tmp = { y, x };
        if (pointInState(tmp))
        {
            setCenter(tmp);
            return tmp;
        }

        //print("State is not convex");
        int rand = (int)(Random.Range(0,(lands.Count - 1)));
        setCenter((int[])lands[rand]);

        return getCenter();
    }

    private bool pointInState(int[] point)
    {
        //for (int[] p : lands)
        for(int i = 0; i < lands.Count; i++) 
        {
            int[] p = (int[])lands[i];
            if (p[0] == point[0] && p[1] == point[1]) return true;
        }

        return false;
    }

    public int[] getCenter()
    {
        return _center;
    }

    public void setCenter(int[] _center)
    {
        this._center = _center;
    }

    public void addRoadsOf(State exState)
    {
        for (int i = 0; i < exState.getRoads().Count; i++)
        {
            SeaRoad road = (SeaRoad)exState.getRoads()[i];
            road.rebase(exState, this);
            this.addRoad(road);
        }
    }

    public void addRoad(SeaRoad r)
    {
        roads.Add(r);
        for (int i = 0;i <  r.getLinkedStates().Length; i ++)
        {
            State s = r.getLinkedStates()[i];
            if (this.getID() != s.getID())
            {
                this.addNeighbour(s);
            }
        }
    }

    public void getNeighboursByRoads()
    {
        for (int i = 0; i <  roads.Count; i++)
        {
            SeaRoad road = (SeaRoad)roads[i];
            for (int j = 0; j < road.getLinkedStates().Length; j++)
            {
                State s = road.getLinkedStates()[j];
                if (s != this) addNeighbour(s);
            }
        }
    }

    public void addRoad(ArrayList rs)
    {
        for (int i = 0; i < rs.Count; i++)
            roads.Add(rs[i]);
    }

    public ArrayList getRoads()
    {
        return roads;
    }

}
