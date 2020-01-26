using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forager : Character
{
    // Forager capacity (amount to forage before returning to base)
    [SerializeField] public int maxCapacity = 1;
    public int capacity = 0;

    // References to game and environment objects
    private GameObject game;
    private Environment mMap;
    private EnvironmentTile baseTile;

    // Forager states
    private bool headedBackToBase = false;
    private bool beingAttacked = false;

    // Access to tile exclusion list, a tile to check and route to use
    private List<EnvironmentTile> exclusions = new List<EnvironmentTile>();
    private EnvironmentTile tile = null;
    private List<EnvironmentTile> route = null;

    // Start is called before the first frame update
    void Start()
    {
        // Initialise health and get game/environment references
        Health = 100;
        game = GameObject.FindGameObjectWithTag("GameController");
        mMap = game.GetComponentInChildren<Environment>();
        baseTile = game.GetComponentInChildren<Environment>().baseTile;

        // Initialise forager states
        CurrentTarget = null;
        headedBackToBase = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Set base tile to go to dependent on ownership
        if (this.OwnedBy == Ownership.Enemy)
        {
            baseTile = game.GetComponentInChildren<Environment>().enemyBaseTile;
        }
        else
        {
            baseTile = game.GetComponentInChildren<Environment>().baseTile;
        }

        // Do main forager functionality if the game is running
        if (game.GetComponent<Game>().isGameStarted == true)
        {
            DoForager();
        }
    }

    // Func to check if this forager is being attacked
    private void CheckIfAttacked(List<Warrior> warriors)
    {
        beingAttacked = false;

        // Go through each opposing warrior to check if near
        foreach (Warrior warrior in warriors)
        {
            // Check if this warrior is near the other warrior position
            if (CheckAround(warrior.CurrentPosition))
            {
                // We are near an opposing warrior so we are being attacked
                CurrentTarget = null;
                beingAttacked = true;
            }
        }
    }

    // Helper func to find closest rock to go to
    private void FindTile()
    {
        float shortestLength = float.MaxValue;
        float temp = 0;

        // Loop through all inaccessible tiles to find closest
        foreach (EnvironmentTile t in mMap.foragerTilesTBC)
        {
            // Ensure tile is not in use and is not in the excluded tile list
            if (t.InUse == false && exclusions.Contains(t) == false)
            {
                temp = Vector3.Distance(transform.position, t.transform.position);

                // Check then set if the dist from forager to rock is the closest so far
                if (temp < shortestLength)
                {
                    shortestLength = temp;
                    tile = t;
                }
            }
        }
    }

    // Main forager functionality
    public void DoForager()
    {
        // Initialise prev frame pos and route
        Vector3 oldT = transform.position;
        route = null;

        // Check if being attacked
        if (OwnedBy == Ownership.Player) { CheckIfAttacked(game.GetComponent<Game>().EnemyWarriorList); }
        else if (OwnedBy == Ownership.Enemy) { CheckIfAttacked(game.GetComponent<Game>().warriorList); }

        // If no target, try and get one
        if (CurrentTarget == null)
        {
            // Find a target tile
            FindTile();

            // Check if forager is over encumbered, so head back to base to deposit cash
            if (capacity >= maxCapacity && headedBackToBase == false && beingAttacked == false)
            {
                // Get tile surrounding base
                EnvironmentTile tile2 = game.GetComponent<Game>().CheckAround(baseTile, this.CurrentPosition);

                // Check if surrounding tile exists
                if (tile2 != null)
                {
                    // Get route to surrounding tile and check if valid
                    route = mMap.Solve(this.CurrentPosition, tile2, "forager");
                    if (route != null)
                    {
                        // Go to the base tile
                        GoTo(route);
                        CurrentTarget = baseTile;
                        headedBackToBase = true;
                    }
                }
            }
            else if (tile != null && headedBackToBase == false && beingAttacked == false) // Otherwise find a way to the rock target
            {
                bool check = false;

                // Whilst a path to the rock isn't available, try and find one
                while (check == false)
                {
                    // Get tile surrounding rock and check if valid
                    EnvironmentTile tile2 = game.GetComponent<Game>().CheckAround(tile, this.CurrentPosition);
                    if (tile2 != null)
                    {
                        // Found a traverse-able tile, find the route then exit loop
                        route = mMap.Solve(this.CurrentPosition, tile2, "forager");
                        tile.InUse = true;
                        check = true;
                    }
                    else // Otherwise, this tile can't be accessed so exclude it from tile searches
                    {
                        // Add to exclusions list and find a new tile to check
                        exclusions.Add(tile);
                        FindTile();
                        check = false;
                    }
                }

                // Check if route calculated is valid then go to
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
        else // If all else fails and it doesn't know what's happened
        {
            // Check if we're not being attacked
            if (beingAttacked == false) 
            {
                // Not being attacked so try to forage
                Forage();
            }
            else // Otherwise, reset position to last frame
            {
                transform.position = oldT;
            }
        }
    }

    // Forager main forage rock func
    public void Forage()
    {
        // Check if we have a target, we can carry the resource and we're not going back to base
        if (CurrentTarget != null && capacity < maxCapacity && headedBackToBase == false)
        {
            // Check if we're in the vicinity of the target
            if (CheckAround(CurrentTarget))
            {
                // Get position index and transform pos
                Vector2Int pos = game.GetComponent<Game>().FindIndex(CurrentTarget);
                Vector3 tilePosition = mMap.mMap[pos.x][pos.y].transform.position;

                if (Time.deltaTime >= 0)
                {
                    // Reduce health of rock
                    CurrentTarget.Health -= 0.5f; 

                    // When rock health is 0, revert the tile to a grass plane
                    if (CurrentTarget.Health <= 0)
                    {
                        mMap.RevertTile(pos.x, pos.y);

                        // Reset target values and in use var
                        CurrentTarget.InUse = false;
                        CurrentTarget = null;

                        // Increase held capacity
                        capacity++;
                    }
                }

                
            }
        }
        else if (CurrentTarget == baseTile) // Heading back to base tile
        {
            // Check if in vicinity of base tile
            if (CheckAround(CurrentTarget))
            {
                // Reset current target
                CurrentTarget = null;

                // Check who to give the cash to (which base we're at)
                if (this.OwnedBy == Ownership.Player)
                {
                    game.GetComponent<Game>().cash += capacity * 50;
                }
                else
                {
                    game.GetComponent<Game>().enemyCash += capacity * 50;
                }

                // Reset capacity and other vars including all exclusion tiles (as new paths will have opened)
                capacity = 0;
                headedBackToBase = false;
                exclusions.Clear();
            }
        }
        else // Otherwise we might be getting attacked
        {
            // Check if we are not being attacked
            if (beingAttacked == false)
            {
                // Then head back to base
                headedBackToBase = true;
            }

            // Set target to null;
            CurrentTarget = null;
        }
    }
}