using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class terrainGeneration : MonoBehaviour {

    public Transform city;

    public Material sand, grass;


    /************* CHUNKS ***************/
    public Vector2 chunkNumber = new Vector2(100,100);
    public Vector2 chunkSize = new Vector2(16,16);

    public GameObject chunk;

    private GameObject[,] allChunks;
    /************************************/

    private float[,] map1, map2, _HeightMap;
    private int _width, _height;

    private int MAX_HEIGHT = 200;
    private int NBMAXSTATE = 50;

    private float[,,] _alphaMap;

    private ArrayList noVoisins = new ArrayList();

    private ArrayList allStates = new ArrayList();
	// Use this for initialization
	void Start ()
    {
        _width = (int)(chunkNumber.x * chunkSize.x); _height = (int)(chunkNumber.y * chunkSize.y);
        _HeightMap = new float[_width, _height];
        map1 = new float[_width, _height];
        map2 = new float[_width, _height];

        generateTerrain();

        /*
        drawStates();

        //testMap(map1);

        while (allStates.Count > NBMAXSTATE)
            mergeStates();

        manageSeaRoads();

        buildFacilities();
        */
        /*

        SplatPrototype hardcodedSplat1 = t.terrainData.splatPrototypes[0];
        SplatPrototype hardcodedSplat2 = t.terrainData.splatPrototypes[1];

        Texture2D texture = getTextureFrom(map1);
        SplatPrototype sp = new SplatPrototype();
        sp.texture = texture;
        sp.tileSize = new Vector2(t.terrainData.size.x, t.terrainData.size.z); //here 3000 3000
        sp.tileOffset = new Vector2(0, 0);
        SplatPrototype[] sps = new SplatPrototype[3];
        sps[0] = hardcodedSplat1;
        sps[1] = hardcodedSplat2;
        sps[2] = sp;
        t.terrainData.splatPrototypes = sps;
        t.terrainData.SetAlphamaps(0,0,_alphaMap);
        */
    }

    private void generateTerrain()
    {
        for (int j = 0; j < _height; j++)
        {
            for (int i = 0; i < _width; i++)
            {
                float noise = computeHeight(i,j, new Vector2(_width, _height));

                if (noise > 0.5)
                    map1[j, i] = 1;
                else
                    map1[j, i] = 0;

                map2[j, i] = -1;

                _HeightMap[j, i] = noise * MAX_HEIGHT;
            }
        }
        //HeightMap is constructed

        allChunks = new GameObject[(int)chunkNumber.x, (int)chunkNumber.y];
        for(int yi = 0; yi < chunkNumber.y; yi++)
        {
            for (int xi = 0; xi < chunkNumber.x; xi++)
            {
                Mesh mesh = new Mesh();
                Vector2 chunkStart = new Vector2(xi * (chunkSize.x-1), yi * (chunkSize.y - 1));

                Vector3[] vertices = new Vector3[(int)chunkSize.x * (int)chunkSize.y];

                List<int> grassTriangles = new List<int>();
                List<int> sandTriangles = new List<int>();

                for (int j = 0; j < chunkSize.y; j++)
                {
                    for (int i = 0; i < chunkSize.x; i++)
                    {
                        vertices[j * (int)chunkSize.x + i] = new Vector3(i + (int)chunkStart.x, _HeightMap[j + (int)chunkStart.y, i + (int)chunkStart.x], j + (int)chunkStart.y);

                        if(j != 0 && i !=0)
                        {
                            if(vertices[(j - 1) * (int)chunkSize.x + i - 1].y + vertices[j* (int)chunkSize.x + i - 1].y + vertices[(j - 1) * (int)chunkSize.x + i].y + vertices[j* (int)chunkSize.x + i].y > 4.2*MAX_HEIGHT/2)
                            {
                                grassTriangles.Add((j - 1) * (int)chunkSize.x + i - 1);
                                grassTriangles.Add(j * (int)chunkSize.x + i - 1);
                                grassTriangles.Add((j - 1) * (int)chunkSize.x + i);

                                grassTriangles.Add((j - 1) * (int)chunkSize.x + i);
                                grassTriangles.Add(j * (int)chunkSize.x + i - 1);
                                grassTriangles.Add(j * (int)chunkSize.x + i);
                            }
                            else
                            {
                                sandTriangles.Add((j - 1) * (int)chunkSize.x + i - 1);
                                sandTriangles.Add(j * (int)chunkSize.x + i - 1);
                                sandTriangles.Add((j - 1) * (int)chunkSize.x + i);

                                sandTriangles.Add((j - 1) * (int)chunkSize.x + i);
                                sandTriangles.Add(j * (int)chunkSize.x + i - 1);
                                sandTriangles.Add(j * (int)chunkSize.x + i);
                            }
                        }
                    }
                }

                mesh.vertices = vertices;

                Material[] ms;
                if (grassTriangles.ToArray().Length !=  0 && sandTriangles.ToArray().Length != 0)
                {
                    ms = new Material[] { grass, sand };
                    mesh.subMeshCount = 2;
                    mesh.SetTriangles(grassTriangles.ToArray(), 0);
                    mesh.SetTriangles(sandTriangles.ToArray(), 1);
                }
                else if(grassTriangles.ToArray().Length != 0)
                {
                    ms = new Material[] { grass };
                    mesh.subMeshCount = 1;
                    mesh.triangles = grassTriangles.ToArray();
                }
                else
                {
                    ms = new Material[] { sand };
                    mesh.subMeshCount = 1;
                    mesh.triangles = sandTriangles.ToArray();
                }

                //print(vertices[(1 - 1) * (int)chunkSize.x + 1 - 1].y + " " + vertices[1 * (int)chunkSize.x + 1 - 1].y + " " + vertices[(1 - 1) * (int)chunkSize.x + 1].y + " " + vertices[1 * (int)chunkSize.x + 1].y + " > " + 4*MAX_HEIGHT/2);

                allChunks[xi, yi] = Instantiate(chunk, new Vector3(0,0,0), Quaternion.identity, this.transform) as GameObject;

                allChunks[xi, yi].GetComponent<MeshRenderer>().materials = ms;
                allChunks[xi, yi].GetComponent<MeshFilter>().mesh = mesh;
                allChunks[xi, yi].GetComponent<MeshCollider>().sharedMesh = mesh;
            }
        }


    }

    private float computeHeight(float i, float j, Vector2 max_size)
    {

        float[] heights = { 130, 100, 90, 80 };

        float MAX = 0;
        foreach (int h in heights)
        {
            MAX += h;
        }

        float noise = 0;

        float dwidth = (float)max_size.x;
        float dheight = (float)max_size.y;

        float limitInf = 0.2f, limitSup = 1 - limitInf;

        if (i > limitSup * dwidth) noise -= MAX * (1 - MyMaths.decreasingScale((i - limitSup * dwidth) / (limitInf * dwidth)));
        else if (i < limitInf * dwidth) noise -= MAX * (1 - MyMaths.increasingScale(i / (limitInf * dwidth)));


        if (j > limitSup * dheight) noise -= MAX * (1 - MyMaths.decreasingScale((j - limitSup * dheight) / (limitInf * dheight)));
        else if (j < 0.15 * dheight) noise -= MAX * (1 - MyMaths.increasingScale(j / (limitInf * dheight)));


        float x = (float)i; x /= 200;
        float y = (float)j; y /= 200; //200 is experimental


        noise += (heights[0] * Mathf.PerlinNoise(x, y));
        noise += (heights[1] * Mathf.PerlinNoise(x + 10, y + 10));
        noise += (heights[2] * Mathf.PerlinNoise(x * 2, y * 2));
        noise += (heights[3] * Mathf.PerlinNoise(x * 2 + 20, y * 2 + 20));

        return noise / MAX;
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
        print("InterState Road Created");
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


    private void buildFacilities()
    {
        ArrayList drawnRoads = new ArrayList(); //preventing to draw each road twice

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        foreach (State s in allStates)
        {
            int[] center = s.computeCenter();

            float localHeight = _HeightMap[center[0], center[1]];

            Instantiate(city, new Vector3(center[1]*2f, 210, center[0]*2f), Quaternion.identity, this.transform); //Floating cities issue seems linked to the steepness of the terrain

            //Building sea roads form
            ArrayList roads = s.getRoads();
            foreach(SeaRoad sr in roads)
            {
                if(!drawnRoads.Contains(sr))
                {
                    drawnRoads.Add(sr);
                    int[][] points = sr.getLimits();
                    //print(points[0][0] + " " + points[0][1] + " " + points[1][0] + " " + points[1][1]);

                    Instantiate(cube, new Vector3(points[0][1] * 2f, 110, points[0][0] * 2f), Quaternion.identity, this.transform);
                    Instantiate(cube, new Vector3(points[1][1] * 2f, 120, points[1][0] * 2f), Quaternion.identity, this.transform);

                    float angle = MyMaths.getAngle(points[0][1], points[0][0], points[1][1], points[1][0]);

                    GameObject bridge = Instantiate(cube, new Vector3(points[1][1] + points[0][1], 100, points[1][0] + points[0][0]), Quaternion.identity, this.transform) as GameObject;
                    bridge.transform.localScale += new Vector3(2*(float)sr.size(), 0, 0);
                    bridge.transform.Rotate(new Vector3(0, angle, 0));
                }
            }
        }

        print("Drawn Roads : " + drawnRoads.Count);
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
            print("Error : " + (n - allStates.Count));
            if (exit) break;
        }
        
        /****************************************************/

        ArrayList finalRoads = new ArrayList();

        print("Number of continents : " + entireList.Count);

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
            print("Road Created");
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
        _alphaMap = new float[_height,_width,3];
        Texture2D texture = new Texture2D(_height,_width);

        bool b = true;

        for (int j = 0; j < _height; j++)
        {
            for (int i = 0; i < _width; i++)
            {
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

        foreach(State s in allStates)
        {
            ArrayList bounds = s.getBoundaries();
            foreach(int[] p in bounds)
            {
                texture.SetPixel(p[1], p[0], Color.black);
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
