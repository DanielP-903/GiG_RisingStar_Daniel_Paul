using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentTile : MonoBehaviour
{
    // Tile class representing each grid cell that makes up the world

    // Connections list used in path-finding
    public List<EnvironmentTile> Connections { get; set; }

    // Reference to tile parent
    public EnvironmentTile Parent { get; set; }

    // Tile path-finding helpers
    public Vector3 Position { get; set; }
    public float Global { get; set; }
    public float Local { get; set; }
    public bool Visited { get; set; }

    // Accessibility of tile for the characters (rocks)
    public bool IsAccessible { get; set; }

    // Type of tile (room for future expansion)
    public string Type { get; set; }

    // Tile health (used for foraging rock etc.)
    public float Health { get; set; }

    // In-use checker so that no two foragers can forage the same tile
    public bool InUse { get; set; }
}
