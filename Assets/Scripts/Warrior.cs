using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class Warrior : Character
{
    private GameObject theGame;
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
        theGame = GameObject.FindGameObjectWithTag("GameController");
        mMap = theGame.GetComponentInChildren<Environment>();
        baseTile = theGame.GetComponentInChildren<Environment>().baseTile;
    }

    // Update is called once per frame
    void Update()
    {
        if (this.OwnedBy == Ownership.Enemy)
        {
            baseTile = theGame.GetComponentInChildren<Environment>().baseTile;
        }
        else
        {
            baseTile = theGame.GetComponentInChildren<Environment>().enemyBaseTile;
        }
        DoWarrior();
    }

    private void FindTarget(List<Warrior> warriors, List<Forager> foragers)
    {
        float shortestLength = float.MaxValue;// Vector3.Distance(this.transform.position, baseTile.transform.position);
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
            EnvironmentTile tile2 = theGame.GetComponent<Game>().CheckAround(baseTile, this.CurrentPosition);
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

        //if (beingAttacked == false)
        //{
        //    CurrentTarget = null;
        //}
    }

    public void DoWarrior()
    {
        // Check if I'm being attacked by something
        if (OwnedBy == Ownership.Player) { CheckIfAttacked(theGame.GetComponent<Game>().EnemyWarriorList); }
        else if (OwnedBy == Ownership.Enemy) { CheckIfAttacked(theGame.GetComponent<Game>().warriorList); }

        if (CurrentTarget == null)
        {
            // Find something to attack
            if (OwnedBy == Ownership.Player) { FindTarget(theGame.GetComponent<Game>().EnemyWarriorList, theGame.GetComponent<Game>().EnemyForagerList); }
            else if (OwnedBy == Ownership.Enemy) { FindTarget(theGame.GetComponent<Game>().warriorList, theGame.GetComponent<Game>().foragerList); }

            if (tile == null && AttackTarget_F == null && AttackTarget_W == null && beingAttacked == false)
            {
                tile = baseTile;
            }
            else if (beingAttacked == false)
            {
                List<EnvironmentTile> route = new List<EnvironmentTile>();
                EnvironmentTile tile2 = theGame.GetComponent<Game>().CheckAround(tile, this.CurrentPosition);
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
                Debug.Log("ATTACK IN  PROGRESS...");

                if (AttackTarget_F.GetComponent<Forager>().Health <= 0)
                {
                    beingAttacked = false;
                    Destroy(AttackTarget_F.gameObject);
                    if (OwnedBy == Ownership.Player)
                    {
                        theGame.GetComponent<Game>().EnemyForagerList.Remove(AttackTarget_F.GetComponent<Forager>());
                        theGame.GetComponent<Game>().enemyUnitCount--;
                    }
                    else
                    {
                        theGame.GetComponent<Game>().foragerList.Remove(AttackTarget_F.GetComponent<Forager>());
                        theGame.GetComponent<Game>().unitCount--;
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
                Debug.Log("ATTACK IN  PROGRESS...");

                if (AttackTarget_W.GetComponent<Warrior>().Health <= 0)
                {
                    beingAttacked = false;
                    Destroy(AttackTarget_W.gameObject);
                    if (OwnedBy == Ownership.Player)
                    {
                        theGame.GetComponent<Game>().EnemyWarriorList.Remove(AttackTarget_W.GetComponent<Warrior>());
                        theGame.GetComponent<Game>().enemyUnitCount--;
                    }
                    else
                    {
                        theGame.GetComponent<Game>().warriorList.Remove(AttackTarget_W.GetComponent<Warrior>());
                        theGame.GetComponent<Game>().unitCount--;
                    }
                    AttackTarget_W = null;
                    tile = null;
                }
            }
        }
        else if (CheckAround(baseTile, mMap))
        {
            //Debug.Log("attacking...");
            if (this.OwnedBy == Ownership.Enemy)
            {
                theGame.GetComponent<Game>().AttackPlayerBase();
            }
            else
            {
                theGame.GetComponent<Game>().AttackEnemyBase();
            }
        }
    }
}
