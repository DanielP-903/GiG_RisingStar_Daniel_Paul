using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Tilemaps;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

public class Environment : MonoBehaviour
{
    [SerializeField] public List<EnvironmentTile> AccessibleTiles;
    [SerializeField] public List<EnvironmentTile> InaccessibleTiles;
    [SerializeField] public Vector2Int Size;
    [SerializeField] private float AccessiblePercentage;

    public EnvironmentTile[][] mMap;
    public List<EnvironmentTile> mAll;
    private List<EnvironmentTile> mToBeTested;
    private List<EnvironmentTile> mLastSolution;

    public EnvironmentTile baseTile;
    public EnvironmentTile enemyBaseTile;

    private readonly Vector3 NodeSize = Vector3.one * 9.0f; 
    private const float TileSize = 10.0f;
    private const float TileHeight = 2.5f;

    private float[] cornerValues = new float[4]{0,0,0,0};

    public EnvironmentTile Start { get; private set; }
    public Vector3 finalPosition;

    private bool initial = false;

    private void Awake()
    {
        mAll = new List<EnvironmentTile>();
        mToBeTested = new List<EnvironmentTile>();
    }

    private void OnDrawGizmos()
    {
        // Draw the environment nodes and connections if we have them
        if (mMap != null)
        {
            for (int x = 0; x < Size.x; ++x)
            {
                for (int y = 0; y < Size.y; ++y)
                {
                    if (mMap[x][y].Connections != null)
                    {
                        for (int n = 0; n < mMap[x][y].Connections.Count; ++n)
                        {
                            Gizmos.color = Color.blue;
                            Gizmos.DrawLine(mMap[x][y].Position, mMap[x][y].Connections[n].Position);
                        }
                    }

                    // Use different colours to represent the state of the nodes
                    Color c = Color.white;
                    if ( !mMap[x][y].IsAccessible )
                    {
                        c = Color.red;
                    }
                    else
                    {
                        if(mLastSolution != null && mLastSolution.Contains( mMap[x][y] ))
                        {
                            c = Color.green;
                        }
                        else if (mMap[x][y].Visited)
                        {
                            c = Color.yellow;
                        }
                    }

                    Gizmos.color = c;
                    Gizmos.DrawWireCube(mMap[x][y].Position, NodeSize);
                }
            }
        }
    }

    private float CheckPercentage()
    {
        float accessible = 0, nonAccessible = 0, gridSize = 0;

        for (int x = 0; x < Size.x; ++x)
        {  
            for (int y = 0; y < Size.y; ++y)
            {
                if (mMap[x][y].IsAccessible == true)
                {
                    accessible++;
                }
                else
                {
                    nonAccessible++;
                }
            }
        }

        gridSize = Size.x * Size.y;

        return (nonAccessible*100 / gridSize);

    }

    private void Generate()
    {
        // Setup the map of the environment tiles according to the specified width and height
        // Generate tiles from the list of accessible and inaccessible prefabs using a random
        // and the specified accessible percentage
        mMap = new EnvironmentTile[Size.x][];

        int halfWidth = Size.x / 2;
        int halfHeight = Size.y / 2;
        Vector3 position = new Vector3( -(halfWidth * TileSize), 0.0f, -(halfHeight * TileSize) );
        bool start = true;

        for ( int x = 0; x < Size.x; ++x)
        {
            mMap[x] = new EnvironmentTile[Size.y];
            for (int y = 0; y < Size.y; ++y)
            {
                bool isAccessible = true;
                List<EnvironmentTile> tiles = isAccessible ? AccessibleTiles : InaccessibleTiles;
                EnvironmentTile prefab = tiles[0];
                EnvironmentTile tile = Instantiate(prefab, position, Quaternion.identity, transform);
                tile.Position = new Vector3(position.x + (TileSize / 2), TileHeight, position.z + (TileSize / 2));
                tile.IsAccessible = isAccessible;
                tile.gameObject.name = string.Format("Tile({0},{1})", x, y);
                tile.Type = string.Format("ground");
                mMap[x][y] = tile;
                mAll.Add(tile);
                if (start) { Start = tile; }
                position.z += TileSize;
                start = false;
            }
            position.x += TileSize;
            position.z = -(halfHeight * TileSize);
        }

        MakeTileInaccessible(1, 1, 5);
        baseTile = mMap[1][1];
        MakeTileInaccessible(Size.x - 2, Size.y - 2, 6);
        enemyBaseTile = mMap[Size.x - 2][Size.y - 2];

        while (CheckPercentage() < 20  || initial == false)
        {
            float[,] result = Midpoint_Displacement(5.0f, 20.0f, Size.x, 0);

            for (int i = 0; i < Size.x; i++)
            {
                for (int j = 0; j < Size.y; j++)
                {
                    if (result[i, j] < 20 && i != 0 && j != 0)
                    {
                        if (mMap[i][j].IsAccessible == true && Random.Range(0, 100) > 50)
                        {
                            MakeTileInaccessible(i, j, 0);
                        }
                    }
                }
            }

            initial = true;
        }

        int randomNumber = Random.Range(0, 100); // Get random number

        if (randomNumber < 50) {
            RiverMaker(true);
        }
        else {
            RiverMaker(false);
        }

        RiverMaker(true);

        finalPosition = position;
        
        Vector2Int BasePosition = new Vector2Int(1, 1);
        MakeSurroundingTilesAccessible(BasePosition);
        BasePosition = new Vector2Int(Size.x - 2, Size.y - 2);
        MakeSurroundingTilesAccessible(BasePosition);

        //MakeTileInaccessible(4, 5, 0);
        //RevertTile(4, 5);
    }



    public void MakeSurroundingTilesAccessible(Vector2Int Pos)
    {
        RevertTile(Pos.x - 1, Pos.y);
        RevertTile(Pos.x + 1, Pos.y);
        RevertTile(Pos.x, Pos.y + 1);
        RevertTile(Pos.x, Pos.y - 1);
        RevertTile(Pos.x - 1, Pos.y - 1);
        RevertTile(Pos.x - 1, Pos.y + 1);
        RevertTile(Pos.x + 1, Pos.y - 1);
        RevertTile(Pos.x + 1, Pos.y + 1);
    }

    public void RiverMaker(bool direction)
    {
        float riverStartTile = 0.0f;
        float riverEndTile = float.MaxValue;

        if (direction == true) // pos
        {
            riverStartTile = Random.Range(2, Size.x - 1);

            while (riverEndTile > mMap.Length)
            {
                riverEndTile = Random.Range(riverStartTile + 3, Size.x - 1);
            }
            for (int i = (int)Random.Range(0, riverStartTile - 1); i < riverEndTile; i++)
            {
                if (mMap[i][(int)riverStartTile].IsAccessible == true)
                {
                    MakeTileInaccessible(i, (int) riverStartTile, 4);
                }
            }
        }
        else
        {
            riverStartTile = Random.Range(2, Size.y - 1);
            while (riverEndTile > mMap.Length)
            {
                riverEndTile = Random.Range(riverStartTile + 3, Size.y - 1);
            }

            for (int i = (int)Random.Range(0, riverStartTile - 1); i < riverEndTile; i++)
            {
                if (mMap[(int) riverStartTile][i].IsAccessible == true)
                {
                    MakeTileInaccessible((int) riverStartTile, i, 4);
                } 
            }
        }
    }

    // Type:
    // 0 = rocks
    // 1 = stone
    // 2 = trees
    // 3 = logs
    // 4 = water
    // 5 = player base
    public void MakeTileInaccessible(int x, int y, int type) 
    {
        finalPosition = mMap[x][y].transform.position;

        EnvironmentTile prefabNA = InaccessibleTiles[0];

        switch (type)
        {
            case 0: prefabNA = InaccessibleTiles[Random.Range(4, 10)]; break;
            case 1: prefabNA = InaccessibleTiles[Random.Range(10, 16)]; break;
            case 2: prefabNA = InaccessibleTiles[Random.Range(2, 4)]; break;
            case 3: prefabNA = InaccessibleTiles[Random.Range(0, 2)]; break;
            case 4: prefabNA = InaccessibleTiles[16]; break;
            case 5: prefabNA = InaccessibleTiles[17]; break;
            case 6: prefabNA = InaccessibleTiles[18]; break;
        }

        var t = mMap[x][y];

        t.GetComponent<MeshFilter>().sharedMesh = prefabNA.GetComponent<MeshFilter>().sharedMesh;
        t.GetComponent<MeshRenderer>().materials = prefabNA.GetComponent<MeshRenderer>().sharedMaterials;
        if (type == 0 || type == 5 || type == 6) {
            Instantiate(prefabNA.GetComponentInChildren<MeshFilter>(), finalPosition, mMap[x][y].transform.rotation, t.transform);
        }
        t.IsAccessible = false;

        switch (type)
        {
            case 0: t.Type = "rock";
                t.Health = 100; break;
            case 1: t.Type = "stone";
                t.Health = 100; break;
            case 2: t.Type = "tree";
                t.Health = 100; break;
            case 3: t.Type = "logs";
                t.Health = 100; break;
            case 4: t.Type = "water"; break;
            case 5: t.Type = "player base"; break;
            case 6: t.Type = "enemy base"; break;
            default: t.Type = "error"; break;
        }

        if (mAll.FindIndex(ind => ind.Equals(mMap[x][y])) != -1)
        {
            mAll[mAll.FindIndex(ind => ind.Equals(mMap[x][y]))] = t;
        }

        mMap[x][y] = t;
    }

    public void RevertTile(int x, int y)
    {
        finalPosition = mMap[x][y].transform.position;

        EnvironmentTile prefabA = AccessibleTiles[0];

        var t = mMap[x][y];

        t.GetComponent<MeshFilter>().sharedMesh = prefabA.GetComponent<MeshFilter>().sharedMesh;
        t.GetComponent<MeshRenderer>().materials = prefabA.GetComponent<MeshRenderer>().sharedMaterials;
        t.IsAccessible = true;
        t.Type = string.Format("ground");

        //Destroy(t.GetComponentInChildren<MeshFilter>().sharedMesh);

        foreach (Transform child in t.transform)
        {
            GameObject.Destroy(child.gameObject);
        }// ???

        if (mAll.FindIndex(ind => ind.Equals(mMap[x][y])) != -1)
        {
            mAll[mAll.FindIndex(ind => ind.Equals(mMap[x][y]))] = t;
        }

        mMap[x][y] = t;
    }

    private void SetupConnections()
    {
        // Currently we are only setting up connections between adjacent nodes
        for (int x = 0; x < Size.x; ++x)
        {
            for (int y = 0; y < Size.y; ++y)
            {
                EnvironmentTile tile = mMap[x][y];
                tile.Connections = new List<EnvironmentTile>();
                if (x > 0)
                {
                    tile.Connections.Add(mMap[x - 1][y]);
                }

                if (x < Size.x - 1)
                {
                    tile.Connections.Add(mMap[x + 1][y]);
                }

                if (y > 0)
                {
                    tile.Connections.Add(mMap[x][y - 1]);
                }

                if (y < Size.y - 1)
                {
                    tile.Connections.Add(mMap[x][y + 1]);
                }
            }
        }
    }

    private float Distance(EnvironmentTile a, EnvironmentTile b)
    {
        // Use the length of the connection between these two nodes to find the distance, this 
        // is used to calculate the local goal during the search for a path to a location
        float result = float.MaxValue;
        EnvironmentTile directConnection = a.Connections.Find(c => c == b);
        if (directConnection != null)
        {
            result = TileSize;
        }
        return result;
    }

    private float Heuristic(EnvironmentTile a, EnvironmentTile b)
    {
        // Use the locations of the node to estimate how close they are by line of sight
        // experiment here with better ways of estimating the distance. This is used  to
        // calculate the global goal and work out the best order to prossess nodes in
        return Vector3.Distance(a.Position, b.Position);
    }

    public void GenerateWorld()
    {
        Generate();
        SetupConnections();
    }

    public void CleanUpWorld()
    {
        if (mMap != null)
        {
            for (int x = 0; x < Size.x; ++x)
            {
                for (int y = 0; y < Size.y; ++y)
                {
                    Destroy(mMap[x][y].gameObject);
                }
            }
        }
    }

    public List<EnvironmentTile> Solve(EnvironmentTile begin, EnvironmentTile destination, string plrEnm)
    {
        List<EnvironmentTile> result = null;
        if (begin != null && destination != null)
        {
            // Nothing to solve if there is a direct connection between these two locations
            EnvironmentTile directConnection = begin.Connections.Find(c => c == destination);
            if (directConnection == null)
            {
                // Set all the state to its starting values
                mToBeTested.Clear();

                for( int count = 0; count < mAll.Count; ++count )
                {
                    mAll[count].Parent = null;
                    mAll[count].Global = float.MaxValue;
                    mAll[count].Local = float.MaxValue;
                    mAll[count].Visited = false;
                }

                // Setup the start node to be zero away from start and estimate distance to target
                EnvironmentTile currentNode = begin;
                currentNode.Local = 0.0f;
                currentNode.Global = Heuristic(begin, destination);

                // Maintain a list of nodes to be tested and begin with the start node, keep going
                // as long as we still have nodes to test and we haven't reached the destination
                mToBeTested.Add(currentNode);

                while (mToBeTested.Count > 0 && currentNode != destination)
                {
                    // Begin by sorting the list each time by the heuristic
                    mToBeTested.Sort((a, b) => (int)(a.Global - b.Global));

                    // Remove any tiles that have already been visited
                    mToBeTested.RemoveAll(n => n.Visited);

                    // Check that we still have locations to visit
                    if (mToBeTested.Count > 0)
                    {
                        // Mark this note visited and then process it
                        currentNode = mToBeTested[0];
                        currentNode.Visited = true;

                        // Check each neighbour, if it is accessible and hasn't already been 
                        // processed then add it to the list to be tested 
                        for (int count = 0; count < currentNode.Connections.Count; ++count)
                        {
                            EnvironmentTile neighbour = currentNode.Connections[count];

                            if (!neighbour.Visited && neighbour.IsAccessible)
                            {
                                mToBeTested.Add(neighbour);
                            }

                            // Calculate the local goal of this location from our current location and 
                            // test if it is lower than the local goal it currently holds, if so then
                            // we can update it to be owned by the current node instead 
                            float possibleLocalGoal = currentNode.Local + Distance(currentNode, neighbour);
                            if (possibleLocalGoal < neighbour.Local)
                            {
                                neighbour.Parent = currentNode;
                                neighbour.Local = possibleLocalGoal;
                                neighbour.Global = neighbour.Local + Heuristic(neighbour, destination);
                            }
                        }
                    }
                }

                // Build path if we found one, by checking if the destination was visited, if so then 
                // we have a solution, trace it back through the parents and return the reverse route
                if (destination.Visited)
                {
                    result = new List<EnvironmentTile>();
                    EnvironmentTile routeNode = destination;

                    while (routeNode.Parent != null)
                    {
                        result.Add(routeNode);
                        routeNode = routeNode.Parent;
                    }
                    result.Add(routeNode);
                    result.Reverse();

                    Debug.LogFormat("Path Found: {0} steps {1} long", result.Count, destination.Local);
                }
                else
                {
                    Debug.LogWarning("Path Not Found: " + plrEnm);
                }
            }
            else
            {
                result = new List<EnvironmentTile>();
                result.Add(begin);
                result.Add(destination);
                Debug.LogFormat("Direct Connection: {0} <-> {1} {2} long", begin, destination, TileSize);
            }
        }
        else
        {
            Debug.LogWarning("Cannot find path for invalid nodes: " + plrEnm);
        }

        mLastSolution = result;

        return result;
    }

    public float[,] Midpoint_Displacement(float minRange, float maxRange, int mapSize, int start)
    {
        float[,] map = new float[mapSize,mapSize];

        // Get the size of terrain iterations required
        int size = (int)Math.Abs(Math.Sqrt(mapSize - 1));

        // Ensure that resolution is correct size (2^n + 1)
        for (int i = 0; i < 12; i++)
        {
            // Check resolution
            if (Math.Pow(2, i) == (mapSize - 1))
            {
                // Increase size based on resolution
                size = i + 1;
                break;
            }
        }

        // Define containers for square end point, initially terrain resolution
        float WIDTH = mapSize - 1;
        float HEIGHT = mapSize - 1;

        // Define containers for square starting point x, y
        float topHORIZONTAL = 0;
        float topVERTICAL = 0;

        // Initialise grid (default height)
        for (int j = start; j < (mapSize); j++)
        {
            for (int i = start; i < (mapSize); i++)
            {
                map[j,i] = 1.0f;
            }
        }

        // Seed corners initially
        map[0,0] = Random.Range(minRange,maxRange);
        map[0,mapSize - 1] = Random.Range(minRange, maxRange);
        map[mapSize - 1,0] = Random.Range(minRange, maxRange);
        map[mapSize - 1,mapSize - 1] = Random.Range(minRange, maxRange);

        // Define width and height of terrain
        WIDTH = mapSize - 1;
        HEIGHT = mapSize - 1;

        // Get number of squares within the terrain
        int squares = (int)((mapSize - 1) / WIDTH);

        // Define horizontal and vertical square containers
        int squaresH = 1;
        int squaresV = 1;

        // Define iteration number
        int itr = 0;

        // Define current width and height for current square
        int CURRENT_WIDTH = 0;
        int CURRENT_HEIGHT = 0;

        // Do midpoint displacement until the number of squares reaches maximum
        while (itr < size)
        {
            // Get no of squares at this iteration
            squares = (int) Math.Pow(4, itr);

            // Increment iteration no
            itr++;

            // Reset Start/End points for square
            topHORIZONTAL = 0;
            topVERTICAL = 0;
            WIDTH = mapSize - 1;
            HEIGHT = mapSize - 1;

            // Get width and height of squares
            WIDTH /= (float) Math.Sqrt(squares);
            HEIGHT /= (float) Math.Sqrt(squares);

            // Get no of squares horizontally and vertically
            squaresH = (int) Math.Sqrt(squares);
            squaresV = (int) Math.Sqrt(squares);

            // Reset current width and height of square
            CURRENT_WIDTH = 0;
            CURRENT_HEIGHT = 0;

            // Loop through all horizontal squares
            for (int i = 0; i < squaresH; i++)
            {
                // Add the current width
                CURRENT_WIDTH += (int) WIDTH;

                // Reset current height and vertical start pos
                CURRENT_HEIGHT = 0;
                topVERTICAL = 0;

                // Check if width or horizontal start pos has gone beyond terrain limits
                if (CURRENT_WIDTH > (mapSize) || topHORIZONTAL > (mapSize))
                {
                    break;
                }

                // Loop through all vertical squares
                for (int j = 0; j < squaresV; j++)
                {
                    // Add the current height
                    CURRENT_HEIGHT += (int) HEIGHT;

                    // Check if height or vertical start pos has gone beyond terrain limits
                    if (CURRENT_HEIGHT > (mapSize) || topVERTICAL > (mapSize))
                    {
                        break;
                    }

                    // Do square step to find corners and centre of this square at the start x,y and end x,y
                    SquareStep(map, (int) topHORIZONTAL, (int) topVERTICAL, CURRENT_WIDTH, CURRENT_HEIGHT);

                    // Do diamond step to find sides of this square at the start x,y and end x,y
                    diamond_step(map, (int) topHORIZONTAL, (int) topVERTICAL, CURRENT_WIDTH, CURRENT_HEIGHT, minRange, maxRange);

                    // Add the height to the vertical start pos
                    topVERTICAL += HEIGHT;
                }

                // Add the width to the horizontal start pos
                topHORIZONTAL += WIDTH;
            }

            // Half max and min range 
            minRange /= 2.0f;
            maxRange /= 2.0f;
        }

        return map;
    }

    public void diamond_step(float[,] map, int startX, int startY, int chX, int chY, float minRange, float maxRange)
    {
        // Midpoint container
        Vector2 mid;

        // Find midpoint between top left point and bottom left point
        mid = FindMidpoint(startX, startY, startX, chY);

        // Calculate height of midpoint by getting the averages of the corner values
        map[(int)mid.x,(int)mid.y] = CalcAverage(cornerValues[0], cornerValues[1], minRange, maxRange);


        // Find midpoint between top left point and top right point
        mid = FindMidpoint(startX, startY, chX, startY);

        // Calculate height of midpoint by getting the averages of the corner values
        map[(int)mid.x, (int)mid.y] = CalcAverage(cornerValues[0], cornerValues[2], minRange, maxRange);


        // Find midpoint between bottom left point and bottom right point
        mid = FindMidpoint(startX, chY, chX, chY);

        // Calculate height of midpoint by getting the averages of the corner values
        map[(int)mid.x, (int)mid.y] = CalcAverage(cornerValues[1], cornerValues[3], minRange, maxRange);


        // Find midpoint between top right point and bottom right point
        mid = FindMidpoint(chX, startY, chX, chY);

        // Calculate height of midpoint by getting the averages of the corner values
        map[(int)mid.x, (int)mid.y] = CalcAverage(cornerValues[2], cornerValues[3], minRange, maxRange);

    }

    // Square step to work out the corner heights and centre of each square
    // Subsidiary of the midpoint displacement algorithm
    public float[,] SquareStep(float[,] map, int startX, int startY, int resX, int resY)
    {
        // Define average height container
        float avg = 0.0f;

        // Define midpoint container
        Vector2 mid;

        // Get corner values of the square
        cornerValues[0] = map[startX, startY];
        cornerValues[1] = map[startX, resY];
        cornerValues[2] = map[resX, startY];
        cornerValues[3] = map[resX, resY];

        // Find the midpoint from the diagonal of top left to bottom right
        // This is the centre point
        mid = FindMidpoint(startX, startY, resX, resY);

        // Calculate the average height of the 4 corners
        avg = cornerValues[0] + cornerValues[1] + cornerValues[2] + cornerValues[3];
        avg /= 4;

        // Set the centre point to the average height
        map[(int)mid.x,(int)mid.y] = avg;

        return map;
    }

    // Calculate average helper function creates random value and adds to average of two points
    public float CalcAverage(float a, float b, float minRange, float maxRange)
    {
        // Calculate random value based on max and min terrain height range
        float random = (float)Random.Range(minRange, maxRange); ;

        // Return the average of the two floats combined with the calculated random value
        return ((a + b) / 2) + random;
    }

    public Vector2 FindMidpoint(float x1, float y1, float x2, float y2)
    {
        // Declare midpoint container
        Vector2 mid;

        // Get midpoint between x values
        mid.x = (x1 + x2) / 2;

        // Get midpoint between y values
        mid.y = (y1 + y2) / 2;

        return mid;
    }
}