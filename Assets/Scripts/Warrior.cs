using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class Warrior : Character
{
    private GameObject game;
    private Environment mMap;
    private EnvironmentTile baseTile;

    private Warrior AttackTarget_W = null;
    private Forager AttackTarget_F = null;

    private bool beingAttacked = false;
    private Warrior attacker = null;
    private EnvironmentTile tile = null;

    // Start is called before the first frame update
    void Start()
    {
        Health = 100;
        game = GameObject.FindGameObjectWithTag("GameController");
        mMap = game.GetComponentInChildren<Environment>();
        baseTile = game.GetComponentInChildren<Environment>().baseTile;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.OwnedBy == Ownership.Enemy)
        {
            baseTile = game.GetComponentInChildren<Environment>().baseTile;
        }
        else
        {
            baseTile = game.GetComponentInChildren<Environment>().enemyBaseTile;
        }
        DoWarrior();
    }

    private void FindTarget(List<Warrior> warriors, List<Forager> foragers)
    {
        float shortestLength = float.MaxValue;
        float temp;
        AttackTarget_F = null;
        AttackTarget_W = null;

        // Target warriors by default
        if (warriors.Count == 0)
        {
            foreach (Forager forager in foragers)
            {
                temp = Vector3.Distance(this.transform.position, forager.transform.position);
                if (temp < shortestLength)
                {
                    shortestLength = temp;
                    tile = forager.CurrentPosition;
                    AttackTarget_F = forager;
                }
            }
        }
        else
        {
            foreach (Warrior warrior in warriors)
            {
                temp = Vector3.Distance(this.transform.position, warrior.transform.position);
                if (temp < shortestLength)
                {
                    shortestLength = temp;
                    tile = warrior.CurrentPosition;
                    AttackTarget_W = warrior;
                }
            }
        }

        if (AttackTarget_F == null && AttackTarget_W == null)
        {
            List<EnvironmentTile> route = new List<EnvironmentTile>();
            EnvironmentTile tile2 = game.GetComponent<Game>().CheckAround(baseTile, this.CurrentPosition);
            if (tile2 != null)
            {
                route = mMap.Solve(this.CurrentPosition, tile2, "warrior");
                if (route != null)
                {
                    GoTo(route);
                    CurrentTarget = tile2;
                }
                else
                {
                    CurrentTarget = null;
                }
            }
        }
        else
        {
            CurrentTarget = tile;
        }
    }

    private void CheckIfAttacked(List<Warrior> warriors)
    {
        beingAttacked = false;

        foreach (Warrior warrior in warriors)
        {
            if (CheckAround(warrior.CurrentPosition, mMap))
            {
                beingAttacked = true;
                attacker = warrior;
                return;
            }
        }
    }

    public void DoWarrior()
    {
        // Check if I'm being attacked by something
        if (OwnedBy == Ownership.Player) { CheckIfAttacked(game.GetComponent<Game>().EnemyWarriorList); }
        else if (OwnedBy == Ownership.Enemy) { CheckIfAttacked(game.GetComponent<Game>().warriorList); }

        if (CurrentTarget == null)
        {
            // Find something to attack
            if (OwnedBy == Ownership.Player) { FindTarget(game.GetComponent<Game>().EnemyWarriorList, game.GetComponent<Game>().EnemyForagerList); }
            else if (OwnedBy == Ownership.Enemy) { FindTarget(game.GetComponent<Game>().warriorList, game.GetComponent<Game>().foragerList); }

            if (tile == null && AttackTarget_F == null && AttackTarget_W == null && beingAttacked == false)
            {
                tile = baseTile;
            }
            else if (beingAttacked == false)
            {
                List<EnvironmentTile> route = new List<EnvironmentTile>();
                EnvironmentTile tile2 = game.GetComponent<Game>().CheckAround(tile, this.CurrentPosition);
                if (tile2 != null)
                {
                    route = mMap.Solve(this.CurrentPosition, tile2, "warrior");
                    if (route != null)
                    {
                        GoTo(route);
                        CurrentTarget = tile;
                    }
                }
            }
            else
            {
                CurrentTarget = attacker.CurrentPosition;
            }
        }
        else // I have a target
        {
            // If near target, attack
            if (CheckAround(this.CurrentTarget, mMap) == true)
            {       
                CurrentTarget = null;
                Attack();
            }
        }
    }

    public void Attack()
    {
        if (AttackTarget_F != null)
        {
            if (CheckAround(AttackTarget_F.CurrentPosition, mMap))
            {
                AttackTarget_F.GetComponent<Forager>().Health -= 10.0f;
                if (AttackTarget_F.GetComponent<Forager>().Health <= 0)
                {
                    beingAttacked = false;
                    Destroy(AttackTarget_F.gameObject);
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
                    AttackTarget_F = null;
                    tile = null;
                }
            }
        }
        else if (AttackTarget_W != null)
        {
            if (CheckAround(AttackTarget_W.CurrentPosition, mMap))
            {
                AttackTarget_W.GetComponent<Warrior>().Health -= 10.0f;
                if (AttackTarget_W.GetComponent<Warrior>().Health <= 0)
                {
                    beingAttacked = false;
                    Destroy(AttackTarget_W.gameObject);
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
                    AttackTarget_W = null;
                    tile = null;
                }
            }
        }
        else if (CheckAround(baseTile, mMap))
        {
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