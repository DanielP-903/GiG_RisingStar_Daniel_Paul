using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private float SingleNodeMoveTime = 0.5f;

    public EnvironmentTile CurrentPosition { get; set; }

    public EnvironmentTile CurrentTarget { get; set; }

    public float Health { get; set; }

    private const float TileSize = 10.0f;
    private const float TileHeight = 2.5f;



    public enum CharacterType
    {
        Forager,
        Warrior
    };

    public bool playerOwned { get; set; }

    public CharacterType MyType { get; set; }

    private IEnumerator DoMove(Vector3 position, Vector3 destination)
    {
        // Move between the two specified positions over the specified amount of time
        if (position != destination)
        {
            transform.rotation = Quaternion.LookRotation(destination - position, Vector3.up);

            Vector3 p = transform.position;
            float t = 0.0f;

            while (t < SingleNodeMoveTime)
            {
                t += Time.deltaTime;
                p = Vector3.Lerp(position, destination, t / SingleNodeMoveTime);
                transform.position = p;
                yield return null;
            }
        }
    }

    public Vector2Int FindIndex(EnvironmentTile tile, Environment mMap)
    {
        Vector2Int rVal = new Vector2Int(-1, -1);

        for (int i = 0; i < mMap.Size.x; i++)
        {
            for (int j = 0; j < mMap.Size.y; j++)
            {
                if (mMap.mMap[i][j] == tile)
                {
                    rVal = new Vector2Int(i, j);
                }
            }
        }

        return rVal;
    }

    private IEnumerator DoGoTo(List<EnvironmentTile> route)
    {
        // Move through each tile in the given route
        if (route != null)
        {
            Vector3 position = CurrentPosition.Position;
            for (int count = 0; count < route.Count; ++count)
            {
                Vector3 next = route[count].Position;
                yield return DoMove(position, next);
                CurrentPosition = route[count];
                position = next;
            }
        }
    }

    public void GoTo(List<EnvironmentTile> route)
    {
        // Clear all coroutines before starting the new route so 
        // that clicks can interupt any current route animation
        StopAllCoroutines();
        StartCoroutine(DoGoTo(route));
    }

    public bool CheckAround(EnvironmentTile destinationTile, Environment mMap)
    {
        Vector2Int index = FindIndex(this.CurrentPosition, mMap);
        Vector2Int destIndex = FindIndex(destinationTile, mMap);

        if (index.x + 1 == destIndex.x && index.y == destIndex.y) { return true; }
        if (index.x - 1 == destIndex.x && index.y == destIndex.y) { return true; }
        if (index.x == destIndex.x && index.y + 1 == destIndex.y) { return true; }
        if (index.x == destIndex.x && index.y - 1 == destIndex.y) { return true; }
        if (index.x + 1 == destIndex.x && index.y + 1 == destIndex.y) { return true; }
        if (index.x + 1 == destIndex.x && index.y - 1 == destIndex.y) { return true; }
        if (index.x - 1 == destIndex.x && index.y + 1 == destIndex.y) { return true; }
        if (index.x - 1 == destIndex.x && index.y - 1 == destIndex.y) { return true; }

        return false;
        //return true;
    }
}