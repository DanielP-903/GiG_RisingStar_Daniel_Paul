using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class Warrior : Character
{
    // Game and environment reference
    private GameObject game;
    private Environment mMap;
    private EnvironmentTile baseTile;

    // Attack targets for both a warrior and forager
    private Warrior AttackTarget_W = null;
    private Forager AttackTarget_F = null;

    // Warrior states, attacker (if any) and tile ref
    private bool beingAttacked = false;
    private Warrior attacker = null;
    private EnvironmentTile tile = null;

    // Start is called before the first frame update
    void Start()
    {
        // Define health and references to game/environment
        Health = 100;
        game = GameObject.FindGameObjectWithTag("GameController");
        mMap = game.GetComponentInChildren<Environment>();
        baseTile = game.GetComponentInChildren<Environment>().baseTile;
    }

    // Update is called once per frame
    void Update()
    {
        // Ensure that the base tile for this warrior is correct based on ownership
        if (this.OwnedBy == Ownership.Enemy)
        {
            baseTile = game.GetComponentInChildren<Environment>().baseTile;
        }
        else
        {
            baseTile = game.GetComponentInChildren<Environment>().enemyBaseTile;
        }

        // Call warrior functionality
        DoWarrior();
    }

    // Target finder func to find closest target for warrior to attack
    private void FindTarget(List<Warrior> warriors, List<Forager> foragers)
    {
        // Define target and assoc vars
        float shortestLength = float.MaxValue;
        float temp;
        AttackTarget_F = null;
        AttackTarget_W = null;

        // Target warriors by default so check if there is any
        if (warriors.Count == 0)
        {
            // No warriors on map so find a forager to attack
            foreach (Forager forager in foragers)
            {
                // Get distance and compare to shortest one
                temp = Vector3.Distance(this.transform.position, forager.transform.position);
                if (temp < shortestLength)
                {
                    // Found a shorter dist target so this is the target
                    shortestLength = temp;
                    tile = forager.CurrentPosition;
                    AttackTarget_F = forager;
                }
            }
        }
        else // Opposing warrior(s) exist so target them
        {
            foreach (Warrior warrior in warriors)
            {
                // Get distance and compare to shortest one
                temp = Vector3.Distance(this.transform.position, warrior.transform.position);

                if (temp < shortestLength)
                {
                    // Found a shorter dist target so this is the target
                    shortestLength = temp;
                    tile = warrior.CurrentPosition;
                    AttackTarget_W = warrior;
                }
            }
        }

        // Check if no attack target was found (e.g. no opposing warriors/foragers on map)
        // so attack the base
        if (AttackTarget_F == null && AttackTarget_W == null)
        {
            // Holder for route to the shortest point surrounding the base tile
            List<EnvironmentTile> route = new List<EnvironmentTile>();
            EnvironmentTile tile2 = game.GetComponent<Game>().CheckAround(baseTile, this.CurrentPosition);

            // Ensure a surrounding tile is found
            if (tile2 != null)
            {
                // Calc route to the base tile
                route = mMap.Solve(this.CurrentPosition, tile2, "warrior");

                // Check if route is possible
                if (route != null)
                {
                    // Follow route and set target
                    GoTo(route);
                    CurrentTarget = tile2;
                }
                else
                {
                    // Impossible to go to base tile so do nothing
                    CurrentTarget = null;
                }
            }
        }
        else
        {
            // We have found a target so set it to the tile
            CurrentTarget = tile;
        }
    }

    // Func to check if this warrior is being attacked
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
                beingAttacked = true;
                attacker = warrior;
                return;
            }
        }
    }

    // Warrior main functionality
    public void DoWarrior()
    {
        // Check if I'm being attacked by something
        if (OwnedBy == Ownership.Player) { CheckIfAttacked(game.GetComponent<Game>().EnemyWarriorList); }
        else if (OwnedBy == Ownership.Enemy) { CheckIfAttacked(game.GetComponent<Game>().warriorList); }

        // Check if I have a target
        if (CurrentTarget == null)
        {
            // Find something to attack
            if (OwnedBy == Ownership.Player) { FindTarget(game.GetComponent<Game>().EnemyWarriorList, game.GetComponent<Game>().EnemyForagerList); }
            else if (OwnedBy == Ownership.Enemy) { FindTarget(game.GetComponent<Game>().warriorList, game.GetComponent<Game>().foragerList); }

            // If nothing to attack, attack base
            if (tile == null && AttackTarget_F == null && AttackTarget_W == null && beingAttacked == false)
            {
                tile = baseTile;
            }
            else if (beingAttacked == false) // Otherwise go to the target to attack
            {
                // Route and tile surrounding the target holders
                List<EnvironmentTile> route = new List<EnvironmentTile>();
                EnvironmentTile tile2 = game.GetComponent<Game>().CheckAround(tile, this.CurrentPosition);

                // Check if there is a surrounding tile
                if (tile2 != null)
                {
                    // Calculate route and if valid, go to it
                    route = mMap.Solve(this.CurrentPosition, tile2, "warrior");
                    if (route != null)
                    {
                        GoTo(route);
                        CurrentTarget = tile;
                    }
                }
            }
            else // We're being attacked so attack the 'attacker'
            {
                CurrentTarget = attacker.CurrentPosition;
            }
        }
        else // I have a target
        {
            // If near target, attack
            if (CheckAround(this.CurrentTarget) == true)
            {       
                CurrentTarget = null;
                Attack();
            }
        }
    }

    // Attacking functionality to damage characters/base
    public void Attack()
    {
        // Check if a forager target is identified
        if (AttackTarget_F != null)
        {
            // Check if we are near the target
            if (CheckAround(AttackTarget_F.CurrentPosition))
            {
                // Reduce target health
                AttackTarget_F.GetComponent<Forager>().Health -= 10.0f;

                // Check if target is dead
                if (AttackTarget_F.GetComponent<Forager>().Health <= 0)
                {
                    // No longer being attacked 
                    beingAttacked = false;

                    // Destroy and clean-up target 
                    Destroy(AttackTarget_F.gameObject);

                    // Remove target from appropriate list and unit count
                    if (OwnedBy == Ownership.Player)
                    {
                        game.GetComponent<Game>().EnemyForagerList.Remove(AttackTarget_F.GetComponent<Forager>());
                        game.GetComponent<Game>().enemyUnitCount--;
                    }
                    else
                    {
                        game.GetComponent<Game>().foragerList.Remove(AttackTarget_F.GetComponent<Forager>());
                        game.GetComponent<Game>().unitCount--;
                    }

                    // Reset target and tile
                    AttackTarget_F = null;
                    tile = null;
                }
            }
        }
        else if (AttackTarget_W != null) // Otherwise check if a warrior target is identified
        {
            // Check if we are near the target
            if (CheckAround(AttackTarget_W.CurrentPosition))
            {
                // Reduce target health
                AttackTarget_W.GetComponent<Warrior>().Health -= 10.0f;

                // Check if target is dead
                if (AttackTarget_W.GetComponent<Warrior>().Health <= 0)
                {
                    // No longer being attacked 
                    beingAttacked = false;

                    // Destroy and clean-up target 
                    Destroy(AttackTarget_W.gameObject);

                    // Remove target from appropriate list and unit count
                    if (OwnedBy == Ownership.Player)
                    {
                        game.GetComponent<Game>().EnemyWarriorList.Remove(AttackTarget_W.GetComponent<Warrior>());
                        game.GetComponent<Game>().enemyUnitCount--;
                    }
                    else
                    {
                        game.GetComponent<Game>().warriorList.Remove(AttackTarget_W.GetComponent<Warrior>());
                        game.GetComponent<Game>().unitCount--;
                    }

                    // Reset target and tile
                    AttackTarget_W = null;
                    tile = null;
                }
            }
        }
        else if (CheckAround(baseTile)) // Otherwise, check if we are near the opposing base tile
        {
            // Attack base dependent on if I'm a player or enemy
            if (this.OwnedBy == Ownership.Enemy)
            {
                game.GetComponent<Game>().AttackPlayerBase();
            }
            else
            {
                game.GetComponent<Game>().AttackEnemyBase();
            }
        }
    }
}