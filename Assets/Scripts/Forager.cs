using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forager : Character
{
    // Start is called before the first frame update
    void Start()
    { }
    

    // Update is called once per frame
    void Update()
    { }

    public void Forage(ref Environment mMap, ref List<EnvironmentTile> mAll)
    {
        if (CurrentTarget != null)
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
            }
        }
    }
}
