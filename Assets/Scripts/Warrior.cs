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

    public void DoWarrior()
    {

        if (CurrentTarget == null)
        {
            EnvironmentTile tile = null;
            float shortestLength = Vector3.Distance(this.transform.position, baseTile.transform.position);
            float temp;
            
            if (this.OwnedBy == Ownership.Player)
            {
                foreach (Warrior warrior in theGame.GetComponent<Game>().EnemyWarriorList)
                {
                    temp = Vector3.Distance(this.transform.position, warrior.transform.position);
                    if (temp < shortestLength)
                    {
                        shortestLength = temp;
                        tile = warrior.CurrentPosition;
                        AttackTarget_W = warrior;
                    }
                }
                foreach (Forager forager in theGame.GetComponent<Game>().EnemyForagerList)
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
            else if (this.OwnedBy == Ownership.Enemy)
            {
                foreach (Warrior warrior in theGame.GetComponent<Game>().warriorList)
                {
                    temp = Vector3.Distance(this.transform.position, warrior.transform.position);
                    if (temp < shortestLength)
                    {
                        shortestLength = temp;
                        tile = warrior.CurrentPosition;
                        AttackTarget_W = warrior;
                    }
                }
                foreach (Forager forager in theGame.GetComponent<Game>().foragerList)
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

            if (tile == null)
            {
                tile = baseTile;
            }

            EnvironmentTile tile2 = theGame.GetComponent<Game>().CheckAround(tile, this.CurrentPosition);
            List<EnvironmentTile> route = mMap.Solve(this.CurrentPosition, tile2, "warrior");
            GoTo(route);
            CurrentTarget = tile;

        }
        else
        {
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
                    Destroy(AttackTarget_F.gameObject);
                    theGame.GetComponent<Game>().EnemyForagerList.Remove(AttackTarget_F.GetComponent<Forager>());
                    theGame.GetComponent<Game>().enemyUnitCount--;
                    AttackTarget_F = null;
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
                    Destroy(AttackTarget_W.gameObject);
                    theGame.GetComponent<Game>().EnemyWarriorList.Remove(AttackTarget_W.GetComponent<Warrior>());
                    theGame.GetComponent<Game>().enemyUnitCount--;
                    AttackTarget_W = null;
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
