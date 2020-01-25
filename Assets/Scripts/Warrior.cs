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
        beingAttacked = false;

        if (OwnedBy == Ownership.Player)
        {
            foreach (Warrior warrior in theGame.GetComponent<Game>().EnemyWarriorList)
            {
                if (CheckAround(warrior.CurrentPosition, mMap))
                {
                    //CurrentTarget = null;
                    beingAttacked = true;
                    Debug.Log("binch u lie");
                }
            }
        }
        else if (OwnedBy == Ownership.Enemy)
        {
            foreach (Warrior warrior in theGame.GetComponent<Game>().warriorList)
            {
                if (CheckAround(warrior.CurrentPosition, mMap))
                {
                    //CurrentTarget = null;
                    beingAttacked = true;
                    Debug.Log("binch u lie");
                }
            }
        }

        if (CurrentTarget == null)
        {
            EnvironmentTile tile = null;
            float shortestLength = float.MaxValue;// Vector3.Distance(this.transform.position, baseTile.transform.position);
            float temp;
            
            if (this.OwnedBy == Ownership.Player)
            {
                // Target warriors by default
                if (theGame.GetComponent<Game>().EnemyWarriorList.Count == 0)
                {
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
                else
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
                }
            }
            else if (this.OwnedBy == Ownership.Enemy)
            {
                if (theGame.GetComponent<Game>().warriorList.Count == 0)
                {
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
                else
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
                }
            }

            if (tile == null && AttackTarget_F == null && AttackTarget_W == null)
            {
                tile = baseTile;
            }

            List<EnvironmentTile> route = null;

            if (beingAttacked == false)
            {
                EnvironmentTile tile2 = theGame.GetComponent<Game>().CheckAround(tile, this.CurrentPosition);
                route = mMap.Solve(this.CurrentPosition, tile2, "warrior");
            }

            if (route != null)
            {
                GoTo(route);
                CurrentTarget = tile;
            }
            else
            {
                CurrentTarget = null;
            }
        }
        else
        {
            if (CheckAround(this.CurrentTarget, mMap) == true)
            {       
                CurrentTarget = null;
                Attack();
            }

            //if (beingAttacked == false)
            //{
            //    if (AttackTarget_F != null)
            //    {
            //        //if (this.CurrentTarget.transform.position.x > AttackTarget_F.transform.position.x - 5 &&
            //        //    this.CurrentTarget.transform.position.x < AttackTarget_F.transform.position.x + 5 &&
            //        //    this.CurrentTarget.transform.position.z > AttackTarget_F.transform.position.z - 5 &&
            //        //    this.CurrentTarget.transform.position.z < AttackTarget_F.transform.position.z + 5)
            //        //{
            //            AttackTarget_F = null;
            //            CurrentTarget = null;
            //        //}
            //    }
            //    else if (AttackTarget_W != null)
            //    {
            //        //if (this.CurrentTarget.transform.position.x > AttackTarget_W.transform.position.x - 5 &&
            //        //    this.CurrentTarget.transform.position.x < AttackTarget_W.transform.position.x + 5 &&
            //        //    this.CurrentTarget.transform.position.z > AttackTarget_W.transform.position.z - 5 &&
            //        //    this.CurrentTarget.transform.position.z < AttackTarget_W.transform.position.z + 5)
            //        //{
            //            AttackTarget_W = null;
            //            CurrentTarget = null;
            //        //}
            //    }
            //}
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
