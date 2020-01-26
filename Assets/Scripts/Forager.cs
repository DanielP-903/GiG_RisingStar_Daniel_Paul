using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forager : Character
{
    public int capacity = 0;
    [SerializeField] public int maxCapacity = 1;
    private GameObject game;
    private Environment mMap;
    private EnvironmentTile baseTile;
    private int attempts = 0;
    private bool headedBackToBase = false;
    private bool beingAttacked = false;
    private List<EnvironmentTile> exclusions = new List<EnvironmentTile>();
    private EnvironmentTile tile = null;
    private List<EnvironmentTile> route = null;

    // Start is called before the first frame update
    void Start()
    {
        Health = 100;
        game = GameObject.FindGameObjectWithTag("GameController");
        mMap = game.GetComponentInChildren<Environment>();
        baseTile = game.GetComponentInChildren<Environment>().baseTile;
        CurrentTarget = null;
        headedBackToBase = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.OwnedBy == Ownership.Enemy)
        {
            baseTile = game.GetComponentInChildren<Environment>().enemyBaseTile;
        }
        else
        {
            baseTile = game.GetComponentInChildren<Environment>().baseTile;
        }

        if (game.GetComponent<Game>().isGameStarted == true)
        {
            DoForager();
        }
    }

    private void CheckIfAttacked(List<Warrior> warriors)
    {
        beingAttacked = false;

        foreach (Warrior warrior in warriors)
        {
            if (CheckAround(warrior.CurrentPosition, mMap))
            {
                CurrentTarget = null;
                beingAttacked = true;
            }
        }
    }

    private void FindTile()
    {
        float shortestLength = float.MaxValue;
        float temp = 0;
        foreach (EnvironmentTile t in mMap.foragerTilesTBC)
        {
            if (t.InUse == false && exclusions.Contains(t) == false)
            {
                temp = Vector3.Distance(transform.position, t.transform.position);
                if (temp < shortestLength)
                {
                    shortestLength = temp;
                    tile = t;
                }
            }
        }
    }

    public void DoForager()
    {
        Vector3 oldT = transform.position;
        route = null;

        if (OwnedBy == Ownership.Player) { CheckIfAttacked(game.GetComponent<Game>().EnemyWarriorList); }
        else if (OwnedBy == Ownership.Enemy) { CheckIfAttacked(game.GetComponent<Game>().warriorList); }

        if (CurrentTarget == null)
        {
            FindTile();

            if (capacity >= maxCapacity && headedBackToBase == false && beingAttacked == false)
            {
                EnvironmentTile tile2 = game.GetComponent<Game>().CheckAround(baseTile, this.CurrentPosition);
                if (tile2 != null)
                {
                    route = mMap.Solve(this.CurrentPosition, tile2, "forager");
                    if (route != null)
                    {
                        GoTo(route);
                        CurrentTarget = baseTile;
                        headedBackToBase = true;
                    }
                }
            }
            else if (tile != null && headedBackToBase == false && beingAttacked == false)
            {
                bool check = false;
                while (check == false)
                {
                    EnvironmentTile tile2 = game.GetComponent<Game>().CheckAround(tile, this.CurrentPosition);
                    if (tile2 != null)
                    {
                        route = mMap.Solve(this.CurrentPosition, tile2, "forager");
                        tile.InUse = true;
                        check = true;
                    }
                    else
                    {
                        exclusions.Add(tile);
                        FindTile();
                        check = false;
                    }
                }

                if (route != null)
                {
                    GoTo(route);
                    CurrentTarget = tile;
                }
                else
                {
                    Debug.LogWarning("Something bad has happened...");
                }
            }
        }
        else
        {
            if (beingAttacked == false)
            {
                Forage();
            }
            else            
            {
                transform.position = oldT;
            }
        }
    }

    public void Forage()
    {
        if (CurrentTarget != null && capacity < maxCapacity && headedBackToBase == false)
        {
            if (CheckAround(CurrentTarget, mMap))
            {
                float otherHealth = CurrentTarget.Health;

                Vector2Int pos = game.GetComponent<Game>().FindIndex(CurrentTarget);

                Vector3 tilePosition = mMap.mMap[pos.x][pos.y].transform.position;

                if (Time.deltaTime >= 0)
                {
                    CurrentTarget.Health -= 0.5f; 
                    if (CurrentTarget.Health <= 0)
                    {
                        mMap.RevertTile(pos.x, pos.y);
                        CurrentTarget.InUse = false;
                        CurrentTarget = null;
                        capacity++;
                    }
                }

                
            }
        }
        else if (CurrentTarget == baseTile)
        {
            if (CheckAround(CurrentTarget, mMap))
            {
                CurrentTarget = null;
                if (this.OwnedBy == Ownership.Player)
                {
                    game.GetComponent<Game>().cash += capacity * 50;
                }
                else
                {
                    game.GetComponent<Game>().enemyCash += capacity * 50;
                }

                capacity = 0;
                headedBackToBase = false;
                exclusions.Clear();
            }
        }
        else
        {
            if (beingAttacked == false)
            {
                headedBackToBase = true;
            }
            CurrentTarget = null;
        }

    }
}