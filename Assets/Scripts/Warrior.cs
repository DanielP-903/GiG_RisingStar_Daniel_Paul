using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Warrior : Character
{
    private GameObject theGame;
    private Environment mMap;
    private EnvironmentTile baseTile;
    // Start is called before the first frame update
    void Start()
    {
        theGame = GameObject.FindGameObjectWithTag("GameController");
        mMap = theGame.GetComponentInChildren<Environment>();
        baseTile = theGame.GetComponentInChildren<Environment>().baseTile;
    }

    // Update is called once per frame
    void Update()
    {
        DoWarrior();
    }

    public void DoWarrior()
    {
        if (CurrentTarget == null)
        {
            EnvironmentTile tile = null;
            float shortestLength = float.MaxValue;
            float temp;
            foreach (Warrior warrior in theGame.GetComponent<Game>().warriorList)
            {
                if (warrior.OwnedBy == Ownership.Enemy)
                {
                    temp = Vector3.Distance(this.transform.position, warrior.transform.position);
                    if (temp < shortestLength)
                    {
                        shortestLength = temp;
                        tile = warrior.CurrentPosition;
                    }
                }

            }
            foreach (Forager forager in theGame.GetComponent<Game>().foragerList)
            {
                if (forager.OwnedBy == Ownership.Enemy)
                {
                    temp = Vector3.Distance(this.transform.position, forager.transform.position);
                    if (temp < shortestLength)
                    {
                        shortestLength = temp;
                        tile = forager.CurrentPosition;
                    }
                }

            }

            if (tile == null)
            {
                tile = baseTile;
            }

            EnvironmentTile tile2 = theGame.GetComponent<Game>().CheckAround(tile);
            List<EnvironmentTile> route = mMap.Solve(this.CurrentPosition, tile2, "warrior");
            GoTo(route);
            CurrentTarget = tile;
        }
        else
        {
            Attack();
        }
    }

    public void Attack()
    {
        Debug.Log("attacking...");
    }
}
