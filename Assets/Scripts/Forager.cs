using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forager : Character
{
    public int capacity = 0;
    [SerializeField] public int maxCapacity = 1;
    private GameObject theGame;
    private Environment mMap;
    private EnvironmentTile baseTile;

    private bool headedBackToBase = false;
    // Start is called before the first frame update
    void Start()
    {
        theGame = GameObject.FindGameObjectWithTag("GameController");
        mMap = theGame.GetComponentInChildren<Environment>();
        baseTile = theGame.GetComponentInChildren<Environment>().baseTile;
        CurrentTarget = null;
        headedBackToBase = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.OwnedBy == Ownership.Enemy)
        {
            baseTile = theGame.GetComponentInChildren<Environment>().enemyBaseTile;
        }
        else
        {
            baseTile = theGame.GetComponentInChildren<Environment>().baseTile;
        }
        DoForager();
    }

    public void DoForager()
    {
        if (CurrentTarget == null)
        {
            EnvironmentTile tile = null;
            float shortestLength = float.MaxValue;
            float temp;
            for (int i = 0; i < mMap.Size.x; i++)
            {
                for (int j = 0; j < mMap.Size.y; j++)
                {
                    if (mMap.mMap[i][j].IsAccessible == false && mMap.mMap[i][j].Type == "rock")
                    {
                        temp = Vector3.Distance(this.transform.position, mMap.mMap[i][j].transform.position);
                        if (temp < shortestLength)
                        {
                            shortestLength = temp;
                            tile = mMap.mMap[i][j];
                        }
                    }
                }
            }


            
            if (capacity >= maxCapacity && headedBackToBase == false)
            {
                EnvironmentTile tile2 = theGame.GetComponent<Game>().CheckAround(baseTile);
                List<EnvironmentTile> route = mMap.Solve(this.CurrentPosition, tile2, "forager");
                GoTo(route);
                CurrentTarget = baseTile;
                headedBackToBase = true;
            }
            else if (tile != null && tile.Type == "rock" && headedBackToBase == false)
            {
                EnvironmentTile tile2 = theGame.GetComponent<Game>().CheckAround(tile);
                List<EnvironmentTile> route = mMap.Solve(this.CurrentPosition, tile2, "forager");
                GoTo(route);
                CurrentTarget = tile;          
            }
        }
        else
        {
            Forage();
        }
    }

    public void Forage()
    {
        if (CurrentTarget != null && capacity < maxCapacity && headedBackToBase == false)
        {
            if (CheckAround(CurrentTarget, mMap))
            {
                Debug.Log("Foraging...");
                float otherHealth = CurrentTarget.Health;

                Vector2Int pos = FindIndex(CurrentTarget, mMap);

                Vector3 tilePosition = mMap.mMap[pos.x][pos.y].transform.position;

                CurrentTarget.Health = 0;

                if (CurrentTarget.Health <= 0)
                {
                    mMap.RevertTile(pos.x, pos.y);
                    CurrentTarget = null;
                    capacity++;
                }
            }
        }
        else if (CurrentTarget == baseTile)
        {
            if (CheckAround(CurrentTarget, mMap))
            {
                CurrentTarget = null;
                theGame.GetComponent<Game>().cash += capacity * 100;
                capacity = 0;
                headedBackToBase = false;
            }
        }
        else
        {
            CurrentTarget = null;
            headedBackToBase = true;
        }

    }
}
