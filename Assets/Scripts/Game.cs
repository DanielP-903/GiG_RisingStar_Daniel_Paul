using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using Random = System.Random;

public class Game : MonoBehaviour
{
    // Unity editor controlled vars
    [SerializeField] private int startingCash = 100;
    [SerializeField] private int enemyStartingCash = 100;
    [SerializeField] private int foragerCost = 100;
    [SerializeField] private int warriorCost = 300;
    [SerializeField] public int maxUnits = 10;

    [SerializeField] private Camera MainCamera;
    [SerializeField] private Camera OverviewCamera;
    private Camera currentCam;

    // Holders for corresponding enemy and forager types
    [SerializeField] private Forager forager;
    [SerializeField] private Warrior warrior;
    [SerializeField] private Forager enemyForager;
    [SerializeField] private Warrior enemyWarrior;
    
    // Other controlled vars
    [SerializeField] public float plrBaseHealth = 100.0f;
    [SerializeField] public float enmBaseHealth = 100.0f;

    [SerializeField] private Canvas Menu;
    [SerializeField] private Canvas Hud;
    [SerializeField] private Canvas H2p;
    [SerializeField] public int resStone;

    // Define lists for player and enemy for each char type
    public List<Forager> foragerList = new List<Forager>();
    public List<Warrior> warriorList = new List<Warrior>();
    public List<Forager> EnemyForagerList = new List<Forager>();
    public List<Warrior> EnemyWarriorList = new List<Warrior>();

    // Initialise unchanging game vars
    public int unitCount = 0;
    public int enemyUnitCount = 0;
    public int cash = 0;
    public int enemyCash = 0;
    private float timer = 5.0f;
    private float timer_Cash = 10.0f;
    private float timerEnd = 10.0f;
    private bool finishGame = false;
    public bool isGameStarted;
    public Character.Ownership winner = Character.Ownership.Player;

    // Define environment
    public Environment mMap;
    public EnvironmentTile baseTile;
    public EnvironmentTile enemyBaseTile;

    void Start()
    {
        // Get environment information
        mMap = GetComponentInChildren<Environment>();
        baseTile = GetComponentInChildren<Environment>().baseTile;
        enemyBaseTile = GetComponentInChildren<Environment>().enemyBaseTile;

        // Set cash values for both sides to pre-defined starting amounts
        cash = startingCash;
        enemyCash = enemyStartingCash;

        // On start, show menu first
        ShowMenu(true);
    }

    // Player forager creator
    public void CreateForager()
    {
        // Creates forager if unit count below max and cash is enough
        if (unitCount < maxUnits && cash - foragerCost >= 0)
        {
            // Create and instantiate a new forager for the player
            Forager newForager;
            newForager = Instantiate(forager, transform);
            newForager.transform.position = new Vector3(-75, 2.5f, -75);
            newForager.transform.rotation = Quaternion.identity;
            newForager.CurrentPosition = mMap.mMap[0][0];
            newForager.tag = "Player";
            newForager.MyType = Character.CharacterType.Forager;
            newForager.CurrentTarget = null;
            newForager.OwnedBy = Character.Ownership.Player;
            foragerList.Add(newForager);

            // Increase no units and take cash from player
            unitCount++;
            cash -= foragerCost;
        }
        else { Debug.Log("Failed to spawn forager"); }
    }

    // Player warrior creator
    public void CreateWarrior()
    {
        // Creates warrior if unit count below max and cash is enough
        if (unitCount < maxUnits && cash - warriorCost >= 0)
        {
            // Create and instantiate a new warrior for the player
            Warrior newWarrior;
            newWarrior = Instantiate(warrior, transform);
            newWarrior.transform.position = new Vector3(-75, 2.5f, -75);
            newWarrior.transform.rotation = Quaternion.identity;
            newWarrior.CurrentPosition = mMap.mMap[0][0];
            newWarrior.tag = "Player";
            newWarrior.MyType = Character.CharacterType.Warrior;
            newWarrior.CurrentTarget = null;
            newWarrior.OwnedBy = Character.Ownership.Player;
            warriorList.Add(newWarrior);

            // Increase no units and take cash from player
            unitCount++;
            cash -= warriorCost;
        }
        else { Debug.Log("Failed to spawn warrior"); }
    }

    // Enemy forager/warrior generator
    private void EnemyGenerator(Character.CharacterType type)
    {
        if (type == Character.CharacterType.Forager)
        {
            if (enemyUnitCount < maxUnits && enemyCash - foragerCost >= 0)
            {
                Forager newForager;
                newForager = Instantiate(enemyForager, transform);
                newForager.transform.position = new Vector3(75, 2.5f, 75);
                newForager.transform.rotation = Quaternion.identity;
                newForager.CurrentPosition = mMap.mMap[mMap.Size.x - 1][mMap.Size.y - 1];
                newForager.tag = "Enemy";
                newForager.MyType = Character.CharacterType.Forager;
                newForager.CurrentTarget = null;
                newForager.OwnedBy = Character.Ownership.Enemy;
                EnemyForagerList.Add(newForager);
                enemyUnitCount++;
                enemyCash -= foragerCost;
            }
            else
            {
                Debug.Log("Failed to spawn enemy");
            }
        }
        else if (type == Character.CharacterType.Warrior)
        {
            if (enemyUnitCount < maxUnits && enemyCash - warriorCost >= 0)
            {
                Warrior newWarrior;
                newWarrior = Instantiate(enemyWarrior, transform);
                newWarrior.transform.position = new Vector3(75, 2.5f, 75);
                newWarrior.transform.rotation = Quaternion.identity;
                newWarrior.CurrentPosition = mMap.mMap[mMap.Size.x - 1][mMap.Size.y - 1];
                newWarrior.tag = "Enemy";
                newWarrior.MyType = Character.CharacterType.Warrior;
                newWarrior.CurrentTarget = null;
                newWarrior.OwnedBy = Character.Ownership.Enemy;
                EnemyWarriorList.Add(newWarrior);
                enemyUnitCount++;
                enemyCash -= warriorCost;
            }
            else
            {
                Debug.Log("Failed to spawn enemy warrior");
            }
        }
    }

    // Helper func to find the index of a tile in the map array
    public Vector2Int FindIndex(EnvironmentTile tile)
    {
        Vector2Int rVal = new Vector2Int(-1, -1);

        // Loop and find tile in map
        for (int i = 0; i < mMap.Size.x; i++)
        {
            for (int j = 0; j < mMap.Size.y; j++)
            {
                if (mMap.mMap[i][j] == tile)
                {
                    // Found tile index in map, set to return value
                    rVal = new Vector2Int(i, j);
                }
            }
        }

        return rVal;
    }

    // Func for enemies attacking player base
    public void AttackPlayerBase()
    {
        // Reduce base health
        plrBaseHealth -= 0.2f;

        // Check if player base is destroyed
        if (plrBaseHealth <= 0.0f)
        {
            // Enemy has won, set to winner and flag to show lose text
            Debug.Log("You lose! GAME OVER");
            winner = Character.Ownership.Enemy;
            finishGame = true;
        }
    }

    // Func for player attacking enemy base
    public void AttackEnemyBase()
    {
        // Reduce base health
        enmBaseHealth -= 0.2f;

        // Check if enemy base is destroyed
        if (enmBaseHealth <= 0.0f)
        {
            // Player has won, set to winner and flag to show win text
            Debug.Log("You win! GAME OVER");
            winner = Character.Ownership.Player;
            finishGame = true;
        }
    }

    // Helper func to find the closest tile around another tile (mainly for forager)
    public EnvironmentTile CheckAround(EnvironmentTile tile, EnvironmentTile objective)
    {
        EnvironmentTile temp = null;
        int dist = int.MaxValue;

        // Ensure parameters are valid
        if (tile != null && objective != null)
        {
            // Check through all tiles connected to initial tile
            foreach (EnvironmentTile e in tile.Connections)
            {
                // Ensure tile is accessible
                if (e.IsAccessible == true)
                {
                    // Calculate length of route from the objective (forager tile pos) to the tile 
                    List<EnvironmentTile> route = mMap.Solve(objective, e, "Game");

                    // Ensure route calculated is possible
                    if (route != null)
                    {
                        if (route.Count < dist)
                        {
                            // Found a shortest route to a surrounding tile
                            dist = route.Count;
                            temp = e;
                        }
                    }                                       
                }
            }
        }
        return temp;
    }

    // Update overall game controls and hud
    private void UpdateGame()
    {
        // Update player cash and unit count
        Hud.transform.GetChild(4).GetComponent<Text>().text = "Cash \n" + cash;
        Hud.transform.GetChild(5).GetComponent<Text>().text = "Units \n" + unitCount + " / 10";

        // Ensure game camera is the same as the overview cam
        if (currentCam == OverviewCamera)
        {
            // Limits for how left, right, up and down you can move the camera
            if (currentCam.transform.position.x < -250)
            {
                currentCam.transform.position = new Vector3(-250f, currentCam.transform.position.y, currentCam.transform.position.z);
            }
            if (currentCam.transform.position.x > -50)
            {
                currentCam.transform.position = new Vector3(-50f, currentCam.transform.position.y, currentCam.transform.position.z);
            }
            if (currentCam.transform.position.z > -50)
            {
                currentCam.transform.position = new Vector3(currentCam.transform.position.x, currentCam.transform.position.y, -50f);
            }
            if (currentCam.transform.position.z < -250)
            {
                currentCam.transform.position = new Vector3(currentCam.transform.position.x, currentCam.transform.position.y, -250f);
            }

            // Camera movement and zoom vars
            Vector3 addVec = currentCam.transform.position;
            float zoom = 1.0f;

            // Camera movement controls (WASD)
            if (Input.GetKey(KeyCode.A))    
            {
                // Move left if allowed
                if ((currentCam.transform.position + (new Vector3(-1, 0, 1))).x > - 250)
                {
                    addVec += (new Vector3(-1, 0, 1));
                }
            }
            else if (Input.GetKey(KeyCode.D))
            {
                // Move right if allowed
                if ((currentCam.transform.position + (new Vector3(1, 0, -1))).x < -50)
                {
                    addVec += (new Vector3(1, 0, -1));
                }
            } 
            if (Input.GetKey(KeyCode.W))
            {
                // Move up if allowed
                if ((currentCam.transform.position + (new Vector3(1.5f, 0, 1.5f))).z < -50)
                {
                    addVec += (new Vector3(1.5f, 0, 1.5f));
                }
            }
            else if (Input.GetKey(KeyCode.S))
            {
                // Move down if allowed
                if ((currentCam.transform.position + (new Vector3(-1.5f, 0, -1.5f))).z > -250)
                {
                    addVec += (new Vector3(-1.5f, 0, -1.5f));
                }
            }

            
            // Camera zoom in/out
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                // Zoom in by decreasing orthographic size (due to iso layout)
                if (currentCam.orthographicSize - zoom > 20)
                {
                    currentCam.orthographicSize -= zoom;
                    OverviewCamera.orthographicSize -= zoom;
                }
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                // Zoom out by increasing orthographic size (due to iso layout)
                if (currentCam.orthographicSize + zoom < 80)
                {
                    currentCam.orthographicSize += zoom;
                    OverviewCamera.orthographicSize += zoom;
                }
            }

            // Update all cam pos
            currentCam.transform.position = addVec;
            OverviewCamera.transform.position = addVec;
        }

    }

    // Splash screen to display losing/winning text
    public void splash()
    {
        // Loop timer for 10 seconds (how long the splash screen lasts
        if (timerEnd > 0) 
        {
            // Check who winner is
            if (winner == Character.Ownership.Enemy)
            {
                // Enemy wins, keep player base health at 0, activate lose text
                plrBaseHealth = 0.0f;
                Hud.transform.GetChild(6).gameObject.SetActive(true);
            }
            else
            {
                // Player wins, keep enemy base health at 0, activate win text
                enmBaseHealth = 0.0f;
                Hud.transform.GetChild(7).gameObject.SetActive(true);
            }

            timerEnd -= Time.deltaTime;
        }
        else // Timer expired
        {
            // Reset vars and disable lose/win text
            finishGame = false;
            timerEnd = 10.0f;
            isGameStarted = false;
            Hud.transform.GetChild(6).gameObject.SetActive(false);
            Hud.transform.GetChild(7).gameObject.SetActive(false);

            // Go back to menu
            ShowMenu(true);
        }
    }

    // Main game loop
    private void Update()
    {
        if (isGameStarted)
        {
            // Check if game is over and winner/loser screen is to be displayed
            if (finishGame == false)
            {
                // Update main game
                UpdateGame();

                // Every 10 seconds, give 10 cash to the player
                timer_Cash -= Time.deltaTime;
                if (timer_Cash < 0)
                {
                    timer_Cash = 10.0f;
                    cash += 10;
                }

                // Enemy unit creation delay
                if (enemyUnitCount < 10)
                {
                    timer -= Time.deltaTime;
                }

                // Enemy unit generation after delay expires
                if (timer <= 0)
                {
                    // Reset delay
                    timer = 5;

                    // Calculate a random value to determine which unit to spawn
                    float random = UnityEngine.Random.Range(0, 1000);
                    if (random < 500)   // 50% of the time, try to spawn a warrior
                    {
                        EnemyGenerator(Character.CharacterType.Warrior);
                    }
                    else if (EnemyForagerList.Count < 4 && random >= 750)   // 25% chance of spawning another forager after game start (up to 4 for the enemy)
                    {
                        EnemyGenerator(Character.CharacterType.Forager);
                    }
                    else if (EnemyForagerList.Count == 0)   // Otherwise, spawn a forager for the first unit
                    {
                        EnemyGenerator(Character.CharacterType.Forager);
                    }
                }
            }
            else // Game is over so display splash screen for loser/winner
            {
                splash();
            }
        }
    }

    // Func for switching to the how to play screen
    public void ShowH2P(bool show)
    {
        // Ensures canvas' exist in context
        if (H2p != null && Menu!= null)
        {
            // Show how to play screen
            if (show)
            {
                // Enable the how to play screen
                Menu.enabled = false;
                Hud.enabled = false;
                H2p.enabled = true;

                // Enable cam
                MainCamera.enabled = true;
                OverviewCamera.enabled = false;

                // Set cam
                currentCam = MainCamera;
            }
            else // Otherwise, go back to menu
            {
                // Enable menu and show
                Menu.enabled = true;
                Hud.enabled = false;
                H2p.enabled = false;
                ShowMenu(true);
            }
        }
    }

    // Clean up func to reset vars and delete leftover objects
    private void CleanUpGame()
    {
        // Reset timers
        timer = 5;
        timer_Cash = 10;
        timerEnd = 10;

        // Destroy all leftover character objects
        foreach (var f in foragerList)
        {
            Destroy(f.gameObject);
        }

        foreach (var w in warriorList)
        {
            Destroy(w.gameObject);
        }

        foreach (var f in EnemyForagerList)
        {
            Destroy(f.gameObject);
        }

        foreach (var w in EnemyWarriorList)
        {
            Destroy(w.gameObject);
        }

        // Clear all character lists
        foragerList.Clear();
        warriorList.Clear();
        EnemyForagerList.Clear();
        EnemyWarriorList.Clear();

        // If backing out of game, clean up world
        if (H2p.enabled == false)
        {
            mMap.CleanUpWorld();
        }
    }

    // Func for switching between menu and game view
    public void ShowMenu(bool show)
    {
        if (Menu != null && Hud != null)
        {
            // Change state to menu screen or main game screen
            Menu.enabled = show;
            Hud.enabled = !show;


            // Show menu
            if (show)
            {
                // Set game variables for returning to menu
                finishGame = false;
                isGameStarted = false;

                // Clean up leftover game vars/objects
                CleanUpGame();

                // Only menu or hud screens should be toggles, how to play screen always off
                H2p.enabled = false;

                // Enable menu camera
                MainCamera.enabled = true;
                OverviewCamera.enabled = false;

                // Change camera to menu camera
                currentCam = MainCamera;
            }
            else // Show main game w/ hud
            {
                // Reset game variables for new game 
                cash = startingCash;
                enemyCash = enemyStartingCash;
                unitCount = 0;
                enemyUnitCount = 0;
                timer = 5.0f;
                enmBaseHealth = 100.0f;
                plrBaseHealth = 100.0f;

                // Flag game is starting
                isGameStarted = true;

                // Enable game camera
                MainCamera.enabled = false;
                OverviewCamera.enabled = true;

                // Change camera to game cam
                currentCam = OverviewCamera;

            }
        }
    }

    public void Generate()
    {
        mMap.GenerateWorld();

    }

    public void Exit()
    {
#if !UNITY_EDITOR
            Application.Quit();
#endif
    }
}

// UNUSED AND BROKEN FEATURES

/*bool check = false;

        if (Input.GetMouseButtonDown(0) && characterSelection == -1)
        {
            Ray screenClick = currentCam.ScreenPointToRay(Input.mousePosition);
            int hits = Physics.RaycastNonAlloc(screenClick, mRaycastHits);
            if (hits > 0)
            {
                EnvironmentTile tile = mRaycastHits[0].transform.GetComponent<EnvironmentTile>();

                for (int i = 0; i < foragerList.Count; i++)
                {
                    if (foragerList[i].CurrentPosition == tile)
                    { 
                        for (int j = 0; j < foragerList.Count; j++)
                        {
                            foragerList[j].gameObject.tag = "default";
                        }
                        for (int j = 0; j < warriorList.Count; j++)
                        {
                            warriorList[j].gameObject.tag = "default";
                        }
                        MainCamera.enabled = true;
                        OverviewCamera.enabled = false;
                        currentCam = MainCamera;
                        characterSelection = i;
                        selectionType = Character.CharacterType.Forager; 
                        foragerList[i].gameObject.tag = "Player";
                        check = true;
                    }
                }

                if (!check)
                {
                    for (int i = 0; i < warriorList.Count; i++)
                    {
                        if (warriorList[i].CurrentPosition == tile)
                        {
                            for (int j = 0; j < warriorList.Count; j++)
                            {
                                warriorList[j].gameObject.tag = "default";
                            }
                            for (int j = 0; j < foragerList.Count; j++)
                            {
                                foragerList[j].gameObject.tag = "default";
                            }
                            MainCamera.enabled = true;
                            OverviewCamera.enabled = false;
                            currentCam = MainCamera;
                            characterSelection = i;
                            selectionType = Character.CharacterType.Warrior;
                            warriorList[i].gameObject.tag = "Player";
                        }
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            for (int j = 0; j < foragerList.Count; j++)
            {
                foragerList[j].gameObject.tag = "default";
            }
            
            for (int j = 0; j < warriorList.Count; j++)
            {
                warriorList[j].gameObject.tag = "default";
            }

            MainCamera.enabled = false;
            OverviewCamera.enabled = true;

            currentCam = OverviewCamera;

            for (int i = 0; i < mMap.Size.x; i++)
            {
                for (int j = 0; j < mMap.Size.y; j++)
                {
                    if (mMap.mMap[i][j].IsAccessible == true)
                    {
                        mMap.mMap[i][j].GetComponent<MeshRenderer>().materials =
                            mMap.AccessibleTiles[0].GetComponent<MeshRenderer>().sharedMaterials;
                    }
                }
            }

            characterSelection = -1;
        }*/
