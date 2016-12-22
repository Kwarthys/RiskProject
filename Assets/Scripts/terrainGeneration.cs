using UnityEngine;
using System.Collections;

public class terrainGeneration : MonoBehaviour {

    public Transform city;


    private Terrain t;
    private float[,] map1, map2, _HeightMap;
    private int _width, _height;

    private int NBMAXSTATE = 40;

    private float[,,] _alphaMap;

    private ArrayList noVoisins = new ArrayList();

    private ArrayList allStates = new ArrayList();
	// Use this for initialization
	void Start () {

        generateTerrain();

        drawStates();

        //testMap(map1);

        while (allStates.Count > NBMAXSTATE)
            mergeStates();

        buildCities();

        manageSeaRoads();

        SplatPrototype hardcodedSplat1 = t.terrainData.splatPrototypes[0];
        SplatPrototype hardcodedSplat2 = t.terrainData.splatPrototypes[1];

        Texture2D texture = getTextureFrom(map1);
        SplatPrototype sp = new SplatPrototype();
        sp.texture = texture;
        sp.tileSize = new Vector2(1000,1000);
        sp.tileOffset = new Vector2(0, 0);
        SplatPrototype[] sps = new SplatPrototype[3];
        sps[0] = hardcodedSplat1;
        sps[1] = hardcodedSplat2;
        sps[2] = sp;
        t.terrainData.splatPrototypes = sps;
        t.terrainData.SetAlphamaps(0,0,_alphaMap);

    }

    private void generateTerrain()
    {

        t = GetComponent<Terrain>();
        TerrainData TD = t.terrainData;

        _HeightMap = new float[TD.heightmapHeight, TD.heightmapWidth]; 

         _width = TD.heightmapWidth; _height = TD.heightmapHeight;
        map1 = new float[_height, _width];
        map2 = new float[_height, _width];

        float[] heights = { 130, 100, 90, 80 };

        float MAX_HEIGHT = 0;
        for (int i = 0; i < heights.Length; i++)
        {
            MAX_HEIGHT += heights[i];
        }

        for (int j = 0; j < TD.heightmapHeight; j++)
        {
            for (int i = 0; i < TD.heightmapWidth; i++)
            {
                float noise = 0;

                float dwidth = (float)TD.heightmapWidth;
                float dheight = (float)TD.heightmapHeight;

                float limitInf = 0.2f, limitSup = 1 - limitInf;

                if (i > limitSup * dwidth) noise -= MAX_HEIGHT * (1 - MyMaths.decreasingScale((i - limitSup * dwidth) / (limitInf * dwidth)));
                //(MAX_HEIGHT - MAX_HEIGHT*(-i/(0.15*dwidth)+(1/0.15)));
                else if (i < limitInf * dwidth) noise -= MAX_HEIGHT * (1 - MyMaths.increasingScale(i / (limitInf * dwidth)));


                if (j > limitSup * dheight) noise -= MAX_HEIGHT * (1 - MyMaths.decreasingScale((j - limitSup * dheight) / (limitInf * dheight)));
                else if (j < 0.15 * dheight) noise -= MAX_HEIGHT * (1 - MyMaths.increasingScale(j / (limitInf * dheight)));





                float x = (float)i; x /= 200;
                float y = (float)j; y /= 200;


                noise += (heights[0] * Mathf.PerlinNoise(x, y));
                noise += (heights[1] * Mathf.PerlinNoise(x + 10, y + 10));
                noise += (heights[2] * Mathf.PerlinNoise(x * 2, y * 2));
                noise += (heights[3] * Mathf.PerlinNoise(x * 2 + 20, y * 2 + 20));

                _HeightMap[j, i] = noise / MAX_HEIGHT;

                if (noise / MAX_HEIGHT > 0.5)
                    map1[j, i] = 1;
                else
                    map1[j, i] = 0;

                map2[j, i] = -1;

            }
        }
        TD.SetHeights(0, 0, _HeightMap);

    }



    private void smallIslandMerge(State s)
    {
        if (s.size() < 30)
        {
            atlantide(s);
            return;
        }

        float smallerDistance = Mathf.Infinity;
        State closestState = s;
        int[][] ps = new int[2][];
        for (int i = 0; i < allStates.Count; i++)
        {
            State n = (State)allStates[i];
            if (s != n)
            {
                for (int lep1 = 0; lep1 < s.getBoundaries().Count; lep1++)
                {
                    int[] p1 = (int[])s.getBoundaries()[lep1];
                    for (int lep2 = 0; lep2 < n.getBoundaries().Count; lep2++)
                    {
                        int[] p2 = (int[])n.getBoundaries()[lep2];
                        if (MyMaths.getDistance(p1, p2) < smallerDistance)
                        {
                            if (n != closestState) closestState = n;
                            smallerDistance = MyMaths.getDistance(p1, p2);
                            ps[0] = p1; ps[1] = p2;
                        }
                    }
                }
            }
        }
        s = stateFusion(s, closestState);
        s.addRoad(new SeaRoad(ps[0], ps[1], s, s));
    }


    /***************************************** STATE MANAGEMENT *******************************************/

    private void drawStates()
    {
        int stateID = 1;
        for (int j = 0; j < _height; j++)
        {
            for (int i = 0; i < _width; i++)
            {
                if (map1[j,i] == 1 && map2[j,i] == -1)
                {
                    allStates.Add(new State(stateID++));
                    //print("new State " + stateID);
                    stateBuilding((State)allStates[allStates.Count - 1], j, i);
                }
                else if (map1[j,i] == 0) map2[j,i] = 0;
            }
        }
        applyMap();
    }



    private void stateBuilding(State s, int j, int i)
    {
        map2[j,i] = s.addToState(j, i);
        //System.out.println("State ID : " + s.getID());
        if (s.getLands().Count > 200) return; //-> SETTING SIZE OF STATES HERE

        bool isFrontier = false;

        ArrayList voisins = getVoisins(j, i, 1);
        if (voisins.Count < 4) isFrontier = true;
        while (voisins.Count > 0)
        {
            int[] c = (int[])voisins[(int)(Random.Range(0, voisins.Count-1))];
            voisins.RemoveAt((int)(Random.Range(0, voisins.Count - 1)));
            //System.out.println(c[0] + " " + c[1]);
            if (map1[c[0],c[1]] == 1) // if terrain is land
            {
                if (map2[c[0],c[1]] == -1) // if land has not yet been stated
                {
                    //System.out.println("Coming from : " + j + "|" + i + " to " + c[0] + "|" + c[1]);
                    stateBuilding(s, c[0], c[1]);
                }
                else if (map2[c[0],c[1]] != s.getID()) // if land is another state
                {
                    isFrontier = true;
                }
            }

        }

        if (isFrontier)
            s.addToBoundaries(j, i);
    }


    private void mergeStates()
    {
        int smallerIndex = -1;
        float smallerSize = Mathf.Infinity;
        for (int i = 0; i < allStates.Count; i++)
        {
            State s = (State)allStates[i];
            if (smallerSize > s.size() && !noVoisins.Contains(allStates[i]))
            {
                smallerSize = s.size();
                smallerIndex = i;
            }
        }

        State smallerState = (State)allStates[smallerIndex];

        computeNeighbours(smallerState);

        if (smallerState.getNeighbours().Count == 0)
        {
            smallIslandMerge(smallerState);
            return;
        }
        State otherState;

        int[] count = new int[smallerState.getNeighbours().Count];
        for (int i = 0; i < smallerState.getNeighbours().Count; i++)
        {
            count[i] = 0;
            foreach (int[] c in smallerState.getBoundaries())
            {
                State neighbour = (State)smallerState.getNeighbours()[i];
                //if (getVoisins(c[0], c[1], smallerState.getNeighbours().get(i).getID()).size() != 0)
                if (getVoisins(c[0], c[1], neighbour.getID()).Count != 0)
                {
                    count[i] = count[i] + 1;
                }
            }
        }
        smallerIndex = 0;
        int biggerBoundaries = count[0];
        for (int i = 0; i < count.Length; i++)
        {
            if (biggerBoundaries < count[i])
            {
                smallerIndex = i;
                biggerBoundaries = count[i];
            }
        }

        otherState = (State)smallerState.getNeighbours()[smallerIndex];

        stateFusion(smallerState, otherState);

    }


    private State stateFusion(State s1, State s2)
    {
        State biggerIDState, lowerIDState;
        if (s1.getID() > s2.getID())
        {
            biggerIDState = s1;
            lowerIDState = s2;
        }
        else
        {
            biggerIDState = s2;
            lowerIDState = s1;
        }

        allStates.Remove(biggerIDState);

        for (int i = 0; i < biggerIDState.getLands().Count; i++)
        {
            int[] c = (int[])biggerIDState.getLands()[i];
            map1[c[0],c[1]] = lowerIDState.getID();
            lowerIDState.addToState(c[0], c[1]);
        }

        computeBoundaries(lowerIDState);

        lowerIDState.addRoadsOf(biggerIDState);

        return lowerIDState;

    }


    private void buildCities()
    {
        foreach(State s in allStates)
        {
            int[] center = s.computeCenter();

            float localHeight = _HeightMap[center[0], center[1]];

            Instantiate(city, new Vector3(center[1]*2, localHeight * 200f , center[0]*2), Quaternion.identity); //Floating cities linked to the steepness of the terrain
        }
    }

    /****************************** SEA ROADS MANAGEMENT *************************************************/


    private void manageSeaRoads()
    {
        //MANAGE ALL THE ONE-STATE ISLAND

        foreach (State s in allStates)
        {
            s.computeCenter();
            computeNeighbours(s);
            if (s.getNeighbours().Count == 0)
                roadToClosestState(s);
        }

        ArrayList entireList = new ArrayList();                 //ArrayList<ArrayList<State>> entireList = new ArrayList<>();
        int n = 0;
        while (n < allStates.Count)
        {
            entireList.Add(new ArrayList());                    //ArrayList<State>());

            State toCompute = null;
            bool stop = false;
            for (int i = 0; i < allStates.Count && !stop; i++)
            {
                bool free = true;
                foreach (ArrayList list in entireList)
                {
                    if (list.Contains(allStates[i]))
                    {
                        free = false;
                    }
                }

                if (free)
                {
                    toCompute = (State)allStates[i];
                    stop = true;
                }
            }

            bool exit = false;
            if (toCompute != null)
            {
                getAllNeighbours(toCompute, (ArrayList)entireList[entireList.Count - 1]);
            }
            else
            {
                exit = true;
            }

            n = 0;
            foreach (ArrayList list in entireList) n += list.Count;
            if (exit) break;
        }
        
        /****************************************************/

        ArrayList finalRoads = new ArrayList();

        foreach (ArrayList continent in entireList)
        {
            ArrayList roads = getShortestRoads(continent, entireList);
            for (int i = 0, added = 0; added < 2 && i < roads.Count; i++)
            {
                bool ok = true;
                foreach (SeaRoad s in finalRoads)
                {
                    SeaRoad road = (SeaRoad)roads[i];
                    if (s.size() == road.size()) ok = false;
                }
                if (ok)
                {
                    finalRoads.Add(roads[i]);
                    added++;
                }
            }
        }

        foreach (SeaRoad s in finalRoads)
        {
            if (roadIsValid(s)) applyRoad(s);
        }

    }

    private void applyRoad(SeaRoad sr)
    {
        State[] ss = sr.getLinkedStates();
        foreach (State s in ss)
        {
            s.addRoad(sr);
        }
    }

    private ArrayList getShortestRoads(ArrayList continent, ArrayList continentList)
    {
        Organizer org = new Organizer(); //SeaRoad special array that sort the roads by size

        foreach (ArrayList otherContinent in continentList)
        {
            if (otherContinent != continent)
            {
                foreach (State s1 in continent)
                {
                    foreach (State s2 in otherContinent)
                    {
                        org.Add(createRoad(s1, s2));
                    }
                }
            }
        }
        return org;
    }


    private void roadToClosestState(State s)
    {
        double smallerDistance = Mathf.Infinity;
        State closestState = s;
        int[][] ps = new int[2][];
        foreach (State n in allStates)
        {
            if (s != n)
            {
                foreach (int[] p1 in s.getBoundaries())
                {
                    foreach (int[] p2 in n.getBoundaries())
                    {
                        if (MyMaths.getDistance(p1, p2) < smallerDistance)
                        {
                            if (n != closestState) closestState = n;
                            smallerDistance = MyMaths.getDistance(p1, p2);
                            ps[0] = p1; ps[1] = p2;
                        }
                    }
                }
            }
        }
        //System.out.println(ps[0][1] + " " + ps[0][0] + " " + ps[1][1] + " " + ps[1][0]);
        if (smallerDistance > 0 && s != closestState)
        {
            SeaRoad sr = new SeaRoad(ps[0], ps[1], s, closestState);
            s.addRoad(sr);
            closestState.addRoad(sr);
        }
    }


    private bool roadIsValid(SeaRoad s)
    {
        /****Road is valid if its starting and ending point are next to water*****/
        bool valid = true;
        int[][] points = s.getLimits();
        foreach (int[] p in points)
        {
            if (getVoisins(p[0], p[1], 0).Count == 0) valid = false;
        }

        return valid;
    }

    private SeaRoad createRoad(State s1, State s2)
    {
        double smallerDistance = Mathf.Infinity;
        int[][] ps = new int[2][];
        foreach (int[] p1 in s1.getBoundaries())
        {
            foreach (int[] p2 in s2.getBoundaries())
            {
                if (MyMaths.getDistance(p1, p2) < smallerDistance)
                {
                    smallerDistance = MyMaths.getDistance(p1, p2);
                    ps[0] = p1; ps[1] = p2;
                }
            }
        }

        return new SeaRoad(ps[0], ps[1], s1, s2);
    }

    /************************************** Texture Management *********************************************/

    private Texture2D getTextureFrom(float[,]map)
    {
        _alphaMap = new float[t.terrainData.alphamapHeight, t.terrainData.alphamapWidth,3];
        Texture2D texture = new Texture2D(t.terrainData.alphamapHeight, t.terrainData.alphamapWidth);

        bool b = true;

        for (int j = 0; j < t.terrainData.alphamapHeight; j++)
        {
            for (int i = 0; i < t.terrainData.alphamapWidth; i++)
            {
                /***************************OLD FASHION PAINTING*****
                if (map[j, i] == 0)
                {
                    texture.SetPixel(i, j, Color.cyan);
                    if (b)
                    {
                        _alphaMap[j, i, 0] = 0;         //Grass
                        _alphaMap[j, i, 1] = 0.5f;      //Sand
                        _alphaMap[j, i, 2] = 0.5f;      //Color
                    }
                }
                else
                {
                    if (b)
                    {
                        _alphaMap[j, i, 0] = 0.5f;
                        _alphaMap[j, i, 1] = 0;
                        _alphaMap[j, i, 2] = 0.5f;
                    }
                    texture.SetPixel(i, j, new Color((100 + Mathf.Pow(map[j, i], 8) % 155) / 255, (Mathf.Pow(map[j, i], 10) % 255) / 255, (Mathf.Pow(map[j, i], 6) % 255) / 255));
                }
                */
                //Painting terrain colors on state ID
                _alphaMap[j, i, 2] = 0.5f;
                if (map[j, i] != 0)
                {
                    texture.SetPixel(i, j, new Color((100 + Mathf.Pow(map[j, i], 8) % 155) / 255, (Mathf.Pow(map[j, i], 10) % 255) / 255, (Mathf.Pow(map[j, i], 6) % 255) / 255));
                }
                else
                {
                    texture.SetPixel(i, j, Color.cyan);
                }

                //Applying textures depending on heights
                float SEUIL_INF = 0.51f, SEUIL_SUP = 0.54f;
                if (_HeightMap[j, i] < SEUIL_INF) //Lower Sea
                {
                    _alphaMap[j, i, 0] = 0;         //Grass
                    _alphaMap[j, i, 1] = 0.5f;      //Sand                    
                }
                else if (_HeightMap[j, i] < SEUIL_SUP) //shore
                {
                    _alphaMap[j, i, 0] = (1 / (SEUIL_SUP - SEUIL_INF) * _HeightMap[j, i] - SEUIL_INF / (SEUIL_SUP - SEUIL_INF)) / 2;
                    _alphaMap[j, i, 1] = (1 / (SEUIL_INF - SEUIL_SUP) * _HeightMap[j, i] - SEUIL_SUP / (SEUIL_INF - SEUIL_SUP)) / 2;

                    if (b)
                    {
                        print(_alphaMap[j, i, 0] + " and " + _alphaMap[j, i, 1] + " --> sum = " + (_alphaMap[j, i, 0] + _alphaMap[j, i, 1]));
                        b = false;
                    }
                }
                else //High land
                {
                    _alphaMap[j, i, 0] = 0.5f;
                    _alphaMap[j, i, 1] = 0;
                }
            }
        }

        texture.Apply();
        return texture;
    }


    /**************************************** MAP MANAGEMENT *****************************************/

    private int defragID(float[,] map)
    {
        int newID = 1;
        foreach(State s in allStates)
        {
            s.setID(newID);
            foreach(int[] p in s.getLands())
            {
                map[p[0], p[1]] = newID;
            }
            newID++;
        }

        return newID;
    }


    private void atlantide(State s)
    {
        allStates.Remove(s);
        for (int i = 0; i < s.getLands().Count; i++)
        {
            int[] c = (int[])s.getLands()[i];
            map1[c[0], c[1]] = 0;
        }

    }



    private void getAllNeighbours(State s, ArrayList list)
    {
        if (list.Contains(s)) return;
        list.Add(s);
        computeNeighbours(s);

        foreach (State leS in s.getNeighbours())
        {
            getAllNeighbours(leS, list);
        }
    }

    private ArrayList getVoisins(int j, int i, int comparator)
    {
        ArrayList leReturn = new ArrayList();

        if (map1[j - 1,i] == comparator)//DESSUS
        {
            int[] tmp = { j - 1, i };
            leReturn.Add(tmp);
        }

        if (map1[j + 1,i] == comparator)//DESSOUS
        {
            int[] tmp = { j + 1, i };
            leReturn.Add(tmp);
        }

        if (map1[j,i + 1] == comparator)//DROITE
        {
            int[] tmp = { j, i + 1 };
            leReturn.Add(tmp);
        }

        if (map1[j,i - 1] == comparator)//GAUCHE
        {
            int[] tmp = { j, i - 1 };
            leReturn.Add(tmp);
        }

        return leReturn;
    }


    private void computeNeighbours(State s)
    {
        s.resetNeighbours();
        for (int i = 0; i < allStates.Count; i++)
        {
            State otherState = (State)allStates[i];
            if (s != otherState) //state not checking boundaries with himself
            {
                for (int j = 0; j < s.getBoundaries().Count; j++)
                {
                    int[] b = (int[])s.getBoundaries()[j];
                    if (getVoisins(b[0], b[1], otherState.getID()).Count != 0)
                    {
                        s.addNeighbour(otherState);
                    }
                }
            }
        }
        s.getNeighboursByRoads();
    }



    private void computeBoundaries(State s)
    {
        s.resetBoundaries();
        for (int i = 0; i < s.getLands().Count; i++)
        {
            int[] c = (int[])s.getLands()[i];
            if (getVoisins(c[0], c[1], s.getID()).Count < 4)
            {
                s.addToBoundaries(c);
            }
        }
    }



    private void testMap(float[,] map)
    {
        int detected = 0;
        while (detected != 10)
        {
            int i = Random.Range(0, 512);
            int j = Random.Range(0, 512);

            if(map[j,i] != 0)
            {
                print(j + " " + i + " " + map[j, i]);
                detected++;
            }
        }
    }

    private void applyMap()
    {
        for (int j = 0; j < _height; j++)
        {
            for (int i = 0; i < _width; i++)
            {
                map1[j, i] = map2[j, i];
                map2[j, i] = -1;
            }
        }
    }


    private void normalizeMap(float[,] map, int maxId)
    {
        for (int j = 0; j < _height; j++)
        {
            for (int i = 0; i < _width; i++)
            {
                if(map[j, i] != 0) map[j, i] = 0.5f*map[j, i]/maxId + 0.5f;
            }
        }
    }

    private void copyMap(float[,] mapFrom, float[,] mapTo)
    {
        for (int j = 0; j < _height; j++)
        {
            for (int i = 0; i < _width; i++)
            {
                mapTo[j,i] = mapFrom[j,i];
            }
        }
    }
}
