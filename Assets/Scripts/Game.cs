using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class Game : MonoBehaviour
{
    public static Game game;

    [SerializeField] private int startingCash = 500;
    [SerializeField] private int enemyStartingCash = 500;
    [SerializeField] private int foragerCost = 100;
    [SerializeField] private int warriorCost = 200;

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

    [SerializeField] private Canvas Menu;
    [SerializeField] private Canvas Hud;
    [SerializeField] private Transform CharacterStart;
    [SerializeField] private Transform EnemyStart;
    [SerializeField] public int resStone;

    [SerializeField] private float plrBaseHealth = 100.0f;
    [SerializeField] private float enmBaseHealth = 100.0f;

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
            Debug.Log("Spawned warrior");
            unitCount++;
            cash -= warriorCost;
        }
        else { Debug.Log("Failed to spawn warrior"); }
    }

    private void EnemyGenerator()
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
        else { Debug.Log("Failed to spawn enemy"); }
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
        plrBaseHealth -= 0.5f;

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
        enmBaseHealth -= 0.5f;

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

            Debug.Log(string.Format(tile.Type));

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
        float dist = float.MaxValue;
        if (tile != null && objective != null)
        {
            foreach (EnvironmentTile e in tile.Connections)
            {
                if (e.IsAccessible == true)
                {
                    if (Vector3.Distance(objective.transform.position, e.transform.position) < dist)
                    {
                        if (mMap.Solve(objective, e, "forager") != null)
                        {
                            dist = Vector3.Distance(objective.transform.position, e.transform.position);
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
        Debug.Log(foragerList[characterSelection].CurrentTarget);

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
        Debug.Log(warriorList[characterSelection].CurrentTarget);

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
        Hud.transform.GetChild(1).GetComponent<Text>().text = "Base Health: " + plrBaseHealth;
        Hud.transform.GetChild(2).GetComponent<Text>().text = "Enemy Base Health: " + enmBaseHealth;
        Hud.transform.GetChild(3).GetComponent<Text>().text = "Cash: " + cash;

        bool check = false;

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
        }

        if (currentCam == OverviewCamera)
        {
            Vector3 addVec = currentCam.transform.position;

            if (Input.GetKey(KeyCode.A))
            {
                addVec += (new Vector3(-1, 0, 1));
            }
            else if (Input.GetKey(KeyCode.D))
            {
                addVec += (new Vector3(1, 0, -1));
            } 
            if (Input.GetKey(KeyCode.W))
            {
                addVec += (new Vector3(1.5f, 0, 1.5f));
            }
            else if (Input.GetKey(KeyCode.S))
            {
                addVec += (new Vector3(-1.5f, 0, -1.5f));
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

            if (enemyUnitCount < 1)
            {
                EnemyGenerator();
            }

            if (characterSelection != -1)
            {
                if (selectionType == Character.CharacterType.Forager)
                {
                    UpdateForagers();
                }
                else
                {
                    UpdateWarriors();
                }
                
                CheckSelectedTile();
            }
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

                /*int index = 0;
                float positionInc = -75.0f;
                foreach (var f in foragerList)
                {
                    f.transform.position = new Vector3(positionInc, 2.5f, -75);
                    f.transform.rotation = Quaternion.identity;
                    f.CurrentPosition = mMap.mMap[index][0];
                    index++;
                    positionInc += 10;
                }

                index = 0;
                positionInc = -75.0f;
                foreach (var w in warriorList)
                {
                    w.transform.position = new Vector3(positionInc, 2.5f, -75);
                    w.transform.rotation = Quaternion.identity;
                    w.CurrentPosition = mMap.mMap[index][0];
                    index++;
                    positionInc += 10;
                }

                index = 1;
                positionInc = -75.0f;
                foreach (var f in EnemyForagerList)
                {
                    f.transform.position = new Vector3(positionInc, 2.5f, 75);
                    f.transform.rotation = Quaternion.identity;
                    f.CurrentPosition = mMap.mMap[mMap.Size.x - index][mMap.Size.y];
                    index++;
                    positionInc -= 10;
                }

                index = 1;
                positionInc = 75.0f;
                foreach (var w in EnemyWarriorList)
                {
                    w.transform.position = new Vector3(positionInc, 2.5f, 75);
                    w.transform.rotation = Quaternion.identity;
                    w.CurrentPosition = mMap.mMap[mMap.Size.x - index][mMap.Size.y];
                    index++;
                    positionInc -= 10;
                }*/

                unitCount = 0;
                enemyUnitCount = 0;

                isGameStarted = true;

                MainCamera.enabled = false;
                OverviewCamera.enabled = true;

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