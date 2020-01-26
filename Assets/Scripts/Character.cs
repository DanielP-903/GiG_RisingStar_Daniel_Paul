using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    // Character movement speed
    [SerializeField] private float SingleNodeMoveTime = 0.5f;

    // Current pos on a tile on the map
    public EnvironmentTile CurrentPosition { get; set; }

    public EnvironmentTile CurrentTarget { get; set; }

    private GameObject theGame;

    // Character health
    public float Health { get; set; }

    // Tile size definitions
    private const float TileSize = 10.0f;
    private const float TileHeight = 2.5f;

    // Enum to easily determine character type and ownership states
    public enum CharacterType
    {
        Forager,
        Warrior
    };

    public enum Ownership
    {
        Player,
        Enemy
    };

    // Character's type
    public CharacterType MyType { get; set; }

    // Who owns this character
    public Ownership OwnedBy { get; set; }

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

    private IEnumerator DoGoTo(List<EnvironmentTile> route)
    {
        GameObject game = GameObject.FindWithTag("GameController");
        if (game.GetComponent<Game>().isGameStarted)
        {
            // Move through each tile in the given route
            if (route != null)
            {
                //Vector3 position = CurrentPosition.Position;
                Vector3 position = transform.position;
                for (int count = 0; count < route.Count; ++count)
                {
                    Vector3 next = route[count].Position;
                    yield return DoMove(position, next);
                    CurrentPosition = route[count];
                    position = next;
                }
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

    // Character helper func to check if a character is near a particular tile
    public bool CheckAround(EnvironmentTile destinationTile)
    {
        // Reference to the game controller
        theGame = GameObject.FindGameObjectWithTag("GameController");

        // Get indices of tiles
        Vector2Int index = theGame.GetComponent<Game>().FindIndex(this.CurrentPosition);
        Vector2Int destIndex = theGame.GetComponent<Game>().FindIndex(destinationTile);

        // Check if character is on any of the 8 surrounding tiles
        if (index.x + 1 == destIndex.x && index.y == destIndex.y) { return true; }
        if (index.x - 1 == destIndex.x && index.y == destIndex.y) { return true; }
        if (index.x == destIndex.x && index.y + 1 == destIndex.y) { return true; }
        if (index.x == destIndex.x && index.y - 1 == destIndex.y) { return true; }
        if (index.x + 1 == destIndex.x && index.y + 1 == destIndex.y) { return true; }
        if (index.x + 1 == destIndex.x && index.y - 1 == destIndex.y) { return true; }
        if (index.x - 1 == destIndex.x && index.y + 1 == destIndex.y) { return true; }
        if (index.x - 1 == destIndex.x && index.y - 1 == destIndex.y) { return true; }

        // Otherwise, character is not near this tile
        return false;
    }
}