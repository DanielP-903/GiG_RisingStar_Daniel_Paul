﻿using System;
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
    public static Game game;

    [SerializeField] private int startingCash = 100;
    [SerializeField] private int enemyStartingCash = 100;
    [SerializeField] private int foragerCost = 100;
    [SerializeField] private int warriorCost = 300;

    public int cash = 0;
    public int enemyCash = 0;

    [SerializeField] private Camera MainCamera;
    [SerializeField] private Camera OverviewCamera;

    [SerializeField] private Forager forager;
    [SerializeField] private Warrior warrior;
    [SerializeField] private Forager enemyForager;
    [SerializeField] private Warrior enemyWarrior;

    public List<Forager> foragerList = new List<Forager>();
    public List<Warrior> warriorList = new List<Warrior>();
    public List<Forager> EnemyForagerList = new List<Forager>();
    public List<Warrior> EnemyWarriorList = new List<Warrior>();
    //[SerializeField] private Forager[] foragers = new Forager[2];
    //[SerializeField] private Warrior[] warriors = new Warrior[1];
    //private Forager[] mForagers = new Forager[2];
    //private Warrior[] mWarriors = new Warrior[1];

    [SerializeField] public int maxUnits = 10;
    public int unitCount = 0;
    public int enemyUnitCount = 0;
    public float timer = 5.0f;
    public float timer_Cash = 10.0f;

    [SerializeField] private Canvas Menu;
    [SerializeField] private Canvas Hud;
    [SerializeField] private Transform CharacterStart;
    [SerializeField] private Transform EnemyStart;
    [SerializeField] public int resStone;

    [SerializeField] public float plrBaseHealth = 100.0f;
    [SerializeField] public float enmBaseHealth = 100.0f;

    private RaycastHit[] mRaycastHits;
    public Environment mMap;
    private EnvironmentTile posLastFrame;

    private Camera currentCam;

    public int characterSelection = -1;
    public Character.CharacterType selectionType = Character.CharacterType.Forager;

    public Material texMaterial;
    public bool isGameStarted;

    public EnvironmentTile baseTile;
    public EnvironmentTile enemyBaseTile;

    private readonly int NumberOfRaycastHits = 10;

    void Start()
    {
        mRaycastHits = new RaycastHit[NumberOfRaycastHits];

        mMap = GetComponentInChildren<Environment>();
        baseTile = GetComponentInChildren<Environment>().baseTile;
        enemyBaseTile = GetComponentInChildren<Environment>().enemyBaseTile;


        characterSelection = -1;

        cash = startingCash;
        enemyCash = enemyStartingCash;

        ShowMenu(true);
    }

    public void CreateForager()
    {
        if (unitCount < maxUnits && cash - foragerCost >= 0)
        {
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
            //Debug.Log("Spawned forager");
            unitCount++;
            cash -= foragerCost;
        }
        else { Debug.Log("Failed to spawn forager"); }
    }

    public void CreateWarrior()
    {
        if (unitCount < maxUnits && cash - warriorCost >= 0)
        {
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
            //Debug.Log("Spawned warrior");
            unitCount++;
            cash -= warriorCost;
        }
        else { Debug.Log("Failed to spawn warrior"); }
    }

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

    public Vector2Int FindIndex(EnvironmentTile tile)
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

    public void AttackPlayerBase()
    {
        plrBaseHealth -= 0.2f;

        if (plrBaseHealth <= 0.0f)
        {
            Debug.Log("You lose! GAME OVER");
            isGameStarted = false;
            Application.Quit();
            Exit();
            ShowMenu(true);
        }
    }

    public void AttackEnemyBase()
    {
        enmBaseHealth -= 0.2f;

        if (enmBaseHealth <= 0.0f)
        {
            Debug.Log("You win! GAME OVER");
            isGameStarted = false;
            Application.Quit();
            ShowMenu(true);
        }
    }

    private void CheckSelectedTile()
    {
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

        Ray screenLook = currentCam.ScreenPointToRay(Input.mousePosition);
        int hits2 = Physics.RaycastNonAlloc(screenLook, mRaycastHits);
        if (hits2 > 0)
        {
            EnvironmentTile tile = mRaycastHits[0].transform.GetComponent<EnvironmentTile>();

            //Debug.Log(string.Format(tile.Type));

            Vector2Int index = FindIndex(tile);
            if (index.x != -1 && index.y != -1)
            {
                if (tile.Type == "ground")
                {
                    tile.GetComponent<MeshRenderer>().materials =
                        mMap.AccessibleTiles[1].GetComponent<MeshRenderer>().sharedMaterials;
                }
            }
        }
    }

    public EnvironmentTile CheckAround(EnvironmentTile tile, EnvironmentTile objective)
    {
        EnvironmentTile temp = null;
        int dist = int.MaxValue;
        if (tile != null && objective != null)
        {
            foreach (EnvironmentTile e in tile.Connections)
            {
                if (e.IsAccessible == true)
                {
                    List<EnvironmentTile> route = mMap.Solve(objective, e, "Game");
                    if (route != null)
                    {
                        if (route.Count < dist)
                        {
                            dist = route.Count;
                            temp = e;
                        }
                    }                                       
                }
            }
        }
        return temp;
    }

    private void UpdateForagers()
    {
        //Debug.Log(foragerList[characterSelection].CurrentTarget);

        // Check to see if the player has clicked a tile and if they have, try to find a path to that 
        // tile. If we find a path then the character will move along it to the clicked tile. 
        if (Input.GetMouseButtonDown(0))
        {
            Ray screenClick = currentCam.ScreenPointToRay(Input.mousePosition);
            int hits = Physics.RaycastNonAlloc(screenClick, mRaycastHits);
            if (hits > 0)
            {
                EnvironmentTile tile = mRaycastHits[0].transform.GetComponent<EnvironmentTile>();

                if (tile != null)
                {
                    List<EnvironmentTile> route;

                    if (tile.Type == "ground")
                    {
                        route = mMap.Solve(foragerList[characterSelection].CurrentPosition, tile, "player");
                        foragerList[characterSelection].GoTo(route);
                        foragerList[characterSelection].CurrentTarget = null;
                    }
                    else if (tile.Type == "rock")
                    {
                        EnvironmentTile tile2 = CheckAround(tile, foragerList[characterSelection].CurrentPosition);
                        route = mMap.Solve(foragerList[characterSelection].CurrentPosition, tile2, "player");
                        foragerList[characterSelection].GoTo(route);
                        foragerList[characterSelection].CurrentTarget = tile;
                    }
                }
            }
        }

        if (foragerList[characterSelection].CurrentTarget != null)
        {
            Vector2Int pos = FindIndex(foragerList[characterSelection].CurrentTarget);

            if (mMap.mMap[pos.x][pos.y + 1] != null)
            {
                if (foragerList[characterSelection].CurrentPosition == mMap.mMap[pos.x][pos.y + 1])
                {
                    foragerList[characterSelection].Forage();
                }
            } 
            
            if (mMap.mMap[pos.x][pos.y - 1] != null)
            {
                if (foragerList[characterSelection].CurrentPosition == mMap.mMap[pos.x][pos.y - 1])
                {
                    foragerList[characterSelection].Forage();
                }
            }
            
            if (mMap.mMap[pos.x + 1][pos.y] != null)
            {
                if (foragerList[characterSelection].CurrentPosition == mMap.mMap[pos.x + 1][pos.y])
                {
                    foragerList[characterSelection].Forage();
                }
            }
            
            if (mMap.mMap[pos.x - 1][pos.y] != null)
            {
                if (foragerList[characterSelection].CurrentPosition == mMap.mMap[pos.x - 1][pos.y])
                {
                    foragerList[characterSelection].Forage();
                }
            }
        }
    }
    
    private void UpdateWarriors()
    {
        //Debug.Log(warriorList[characterSelection].CurrentTarget);

        // Check to see if the player has clicked a tile and if they have, try to find a path to that 
        // tile. If we find a path then the character will move along it to the clicked tile. 
        if (Input.GetMouseButtonDown(0))
        {
            Ray screenClick = currentCam.ScreenPointToRay(Input.mousePosition);
            int hits = Physics.RaycastNonAlloc(screenClick, mRaycastHits);
            if (hits > 0)
            {
                EnvironmentTile tile = mRaycastHits[0].transform.GetComponent<EnvironmentTile>();

                if (tile != null)
                {
                    List<EnvironmentTile> route;

                    if (tile.Type == "ground")
                    {
                        route = mMap.Solve(warriorList[characterSelection].CurrentPosition, tile, "player");
                        warriorList[characterSelection].GoTo(route);
                        warriorList[characterSelection].CurrentTarget = null;
                    }
                    else if (tile.Type == "enemy base")
                    {
                        EnvironmentTile tile2 = CheckAround(tile, warriorList[characterSelection].CurrentPosition);
                        route = mMap.Solve(warriorList[characterSelection].CurrentPosition, tile2, "player");
                        warriorList[characterSelection].GoTo(route);
                        warriorList[characterSelection].CurrentTarget = tile;
                    }
                }
            }
        }

        if (warriorList[characterSelection].CurrentTarget != null)
        {
            Vector2Int pos = FindIndex(warriorList[characterSelection].CurrentTarget);
            if (mMap.mMap[pos.x][pos.y + 1] != null)
            {
                if (warriorList[characterSelection].CurrentPosition == mMap.mMap[pos.x][pos.y + 1])
                {
                    AttackEnemyBase();
                }
            }
            if (mMap.mMap[pos.x][pos.y - 1] != null)
            {
                if (warriorList[characterSelection].CurrentPosition == mMap.mMap[pos.x][pos.y - 1])
                {
                    AttackEnemyBase();
                }
            }
            if (mMap.mMap[pos.x + 1][pos.y] != null)
            {
                if (warriorList[characterSelection].CurrentPosition == mMap.mMap[pos.x + 1][pos.y])
                {
                    AttackEnemyBase();
                }
            }
            if (mMap.mMap[pos.x - 1][pos.y] != null)
            {
                if (warriorList[characterSelection].CurrentPosition == mMap.mMap[pos.x - 1][pos.y])
                {
                    AttackEnemyBase();
                }
            }
        }
    }

    private void UpdateGame()
    {
        Hud.transform.GetChild(4).GetComponent<Text>().text = "Cash \n" + cash;
        Hud.transform.GetChild(5).GetComponent<Text>().text = "Units \n" + unitCount + " / 10";

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

        if (currentCam == OverviewCamera)
        {
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
            Vector3 addVec = currentCam.transform.position;
            float zoom = 1.0f;

            // Camera movement controls (WASD)
            if (Input.GetKey(KeyCode.A))
            {
                if ((currentCam.transform.position + (new Vector3(-1, 0, 1))).x > - 250)
                {
                    addVec += (new Vector3(-1, 0, 1));
                }
            }
            else if (Input.GetKey(KeyCode.D))
            {
                if ((currentCam.transform.position + (new Vector3(1, 0, -1))).x < -50)
                {
                    addVec += (new Vector3(1, 0, -1));
                }
            } 
            if (Input.GetKey(KeyCode.W))
            {
                if ((currentCam.transform.position + (new Vector3(1.5f, 0, 1.5f))).z < -50)
                {
                    addVec += (new Vector3(1.5f, 0, 1.5f));
                }
            }
            else if (Input.GetKey(KeyCode.S))
            {
                if ((currentCam.transform.position + (new Vector3(-1.5f, 0, -1.5f))).z > -250)
                {
                    addVec += (new Vector3(-1.5f, 0, -1.5f));
                }
            }

            

            // Camera zoom in/out
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                if (currentCam.orthographicSize - zoom > 20)
                {
                    currentCam.orthographicSize -= zoom;
                    OverviewCamera.orthographicSize -= zoom;
                }
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                if (currentCam.orthographicSize + zoom < 80)
                {
                    currentCam.orthographicSize += zoom;
                    OverviewCamera.orthographicSize += zoom;
                }
            }

            currentCam.transform.position = addVec;
            OverviewCamera.transform.position = addVec;
        }

    }

    private void Update()
    {
        if (isGameStarted)
        {
            UpdateGame();

            timer_Cash -= Time.deltaTime;
            if (timer_Cash < 0)
            {
                timer_Cash = 10.0f;
                cash += 10;
            }

            if (enemyUnitCount < 10)
            {
                timer -= Time.deltaTime;
            }

            if (timer <= 0)
            {
                timer = 5;
                float random = UnityEngine.Random.Range(0, 1000);
                if (random < 500)
                {
                    EnemyGenerator(Character.CharacterType.Warrior);
                }
                else if (EnemyForagerList.Count < 4 && random >= 750)
                {
                    EnemyGenerator(Character.CharacterType.Forager);
                } 
                else if (EnemyForagerList.Count==0)
                {
                    EnemyGenerator(Character.CharacterType.Forager);
                }
            }

            //if (characterSelection != -1)
            //{
            //    if (selectionType == Character.CharacterType.Forager)
            //    {
            //        UpdateForagers();
            //    }
            //    else
            //    {
            //        UpdateWarriors();
            //    }
            //    CheckSelectedTile();
            //}
        }
    }

    public void ShowMenu(bool show)
    {
        if (Menu != null && Hud != null)
        {
            Menu.enabled = show;
            Hud.enabled = !show;

            if (show)
            {

                foreach (var f in foragerList)
                {
                    Destroy(f.gameObject);
                    f.transform.position = CharacterStart.position;
                    f.transform.rotation = CharacterStart.rotation;
                }

                foreach (var w in warriorList)
                {
                    Destroy(w.gameObject);
                    w.transform.position = CharacterStart.position;
                    w.transform.rotation = CharacterStart.rotation;
                }

                foreach (var f in EnemyForagerList)
                {
                    Destroy(f.gameObject);
                    f.transform.position = CharacterStart.position;
                    f.transform.rotation = CharacterStart.rotation;
                }

                foreach (var w in EnemyWarriorList)
                {
                    Destroy(w.gameObject);
                    w.transform.position = CharacterStart.position;
                    w.transform.rotation = CharacterStart.rotation;
                }

                foragerList.Clear();
                warriorList.Clear();
                EnemyForagerList.Clear();
                EnemyWarriorList.Clear();


                mMap.CleanUpWorld();

                isGameStarted = false;

                MainCamera.enabled = true;
                OverviewCamera.enabled = false;

                currentCam = MainCamera;
            }
            else
            {
                cash = startingCash;
                enemyCash = enemyStartingCash;

                unitCount = 0;
                enemyUnitCount = 0;

                isGameStarted = true;

                timer = 5.0f;

                MainCamera.enabled = false;
                OverviewCamera.enabled = true;

                currentCam = OverviewCamera;
                enmBaseHealth = 100.0f;
                plrBaseHealth = 100.0f;
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