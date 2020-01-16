using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class Game : MonoBehaviour
{
    [SerializeField] private Camera MainCamera;
    [SerializeField] private Camera OverviewCamera;

    [SerializeField] private Forager forager;
    [SerializeField] private Warrior warrior;

    private List<Forager> foragerList;
    private List<Warrior> warriorList;
    [SerializeField] private Forager[] foragers = new Forager[2];
    [SerializeField] private Warrior[] warriors = new Warrior[1];
    private Forager[] mForagers = new Forager[2];
    private Warrior[] mWarriors = new Warrior[1];

    [SerializeField] public int maxUnits = 10;
    public int unitCount = 0;

    [SerializeField] private Canvas Menu;
    [SerializeField] private Canvas Hud;
    [SerializeField] private Transform CharacterStart;
    [SerializeField] private Transform EnemyStart;
    [SerializeField] public int resStone;

    [SerializeField] private float plrBaseHealth = 100.0f;
    [SerializeField] private float enmBaseHealth = 100.0f;

    private RaycastHit[] mRaycastHits;
    private Environment mMap;
    private EnvironmentTile posLastFrame;

    private Camera currentCam;

    public int characterSelection = -1;
    public Character.CharacterType selectionType = Character.CharacterType.Forager;

    public Material texMaterial;
    public bool isGameStarted;

    private readonly int NumberOfRaycastHits = 10;

    void Start()
    {
        mRaycastHits = new RaycastHit[NumberOfRaycastHits];

        mMap = GetComponentInChildren<Environment>();

        for (int i = 0; i < 2; i++)
        {
            mForagers[i] = Instantiate(foragers[i], transform);
            mForagers[i].tag = "Player";
            mForagers[i].MyType = global::Character.CharacterType.Forager;
            mForagers[i].CurrentTarget = null;
        }

        mWarriors[0] = Instantiate(warriors[0], transform);
        mWarriors[0].tag = "Player";
        mWarriors[0].MyType = global::Character.CharacterType.Forager;
        mWarriors[0].CurrentTarget = null;

        characterSelection = -1;

        ShowMenu(true);
    }

    public void CreateForager()
    {
        if (unitCount < maxUnits)
        {
            Forager newForager = new Forager();
            newForager = Instantiate(forager, transform);
            newForager.tag = "Player";
            newForager.MyType = Character.CharacterType.Forager;
            newForager.CurrentTarget = null;
            foragerList.Add(newForager);
            Debug.Log("Spawned forager");
            unitCount++;
        }
        else { Debug.Log("Failed to spawn forager"); }
    }

    public void CreateWarrior()
    {
        if (unitCount < maxUnits)
        {
            Warrior newWarrior = new Warrior();
            newWarrior = Instantiate(warrior, transform);
            newWarrior.tag = "Player";
            newWarrior.MyType = Character.CharacterType.Warrior;
            newWarrior.CurrentTarget = null;
            warriorList.Add(newWarrior);
            Debug.Log("Spawned warrior");
            unitCount++;
        }
        else { Debug.Log("Failed to spawn warrior"); }
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

    private void AttackPlayerBase()
    {
        plrBaseHealth -= 0.5f;

        if (plrBaseHealth <= 0.0f)
        {
            Debug.Log("You lose! GAME OVER");
            Application.Quit();
            Exit();
            ShowMenu(true);
        }
    }

    private void AttackEnemyBase()
    {
        enmBaseHealth -= 0.5f;

        if (enmBaseHealth <= 0.0f)
        {
            Debug.Log("You win! GAME OVER");
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

    public EnvironmentTile CheckAround(EnvironmentTile tile)
    {
        if (tile != null)
        {
            foreach (EnvironmentTile e in tile.Connections)
            {
                if (e.IsAccessible == true)
                {
                    return e;
                }
            }
        }
        return null;
    }

    private void UpdateForagers()
    {
        //Debug.Log(mForagers[characterSelection].CurrentTarget);

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
                        route = mMap.Solve(mForagers[characterSelection].CurrentPosition, tile, "player");
                        mForagers[characterSelection].GoTo(route);
                        mForagers[characterSelection].CurrentTarget = null;
                    }
                    else if (tile.Type == "rock")
                    {
                        EnvironmentTile tile2 = CheckAround(tile);
                        route = mMap.Solve(mForagers[characterSelection].CurrentPosition, tile2, "player");
                        mForagers[characterSelection].GoTo(route);
                        mForagers[characterSelection].CurrentTarget = tile;
                    }
                }
            }
        }

        if (mForagers[characterSelection].CurrentTarget != null)
        {
            Vector2Int pos = FindIndex(mForagers[characterSelection].CurrentTarget);

            if (mMap.mMap[pos.x][pos.y + 1] != null)
            {
                if (mForagers[characterSelection].CurrentPosition == mMap.mMap[pos.x][pos.y + 1])
                {
                    mForagers[characterSelection].Forage(ref mMap, ref mMap.mAll);
                }
            } 
            
            if (mMap.mMap[pos.x][pos.y - 1] != null)
            {
                if (mForagers[characterSelection].CurrentPosition == mMap.mMap[pos.x][pos.y - 1])
                {
                    mForagers[characterSelection].Forage(ref mMap, ref mMap.mAll);
                }
            }
            
            if (mMap.mMap[pos.x + 1][pos.y] != null)
            {
                if (mForagers[characterSelection].CurrentPosition == mMap.mMap[pos.x + 1][pos.y])
                {
                    mForagers[characterSelection].Forage(ref mMap, ref mMap.mAll);
                }
            }
            
            if (mMap.mMap[pos.x - 1][pos.y] != null)
            {
                if (mForagers[characterSelection].CurrentPosition == mMap.mMap[pos.x - 1][pos.y])
                {
                    mForagers[characterSelection].Forage(ref mMap, ref mMap.mAll);
                }
            }
        }
    }
    
    private void UpdateWarriors()
    {
        Debug.Log(mWarriors[characterSelection].CurrentTarget);

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
                        route = mMap.Solve(mWarriors[characterSelection].CurrentPosition, tile, "player");
                        mWarriors[characterSelection].GoTo(route);
                        mWarriors[characterSelection].CurrentTarget = null;
                    }
                    else if (tile.Type == "enemy base")
                    {
                        EnvironmentTile tile2 = CheckAround(tile);
                        route = mMap.Solve(mWarriors[characterSelection].CurrentPosition, tile2, "player");
                        mWarriors[characterSelection].GoTo(route);
                        mWarriors[characterSelection].CurrentTarget = tile;
                    }
                }
            }
        }

        if (mWarriors[characterSelection].CurrentTarget != null)
        {
            Vector2Int pos = FindIndex(mWarriors[characterSelection].CurrentTarget);
            if (mMap.mMap[pos.x][pos.y + 1] != null)
            {
                if (mWarriors[characterSelection].CurrentPosition == mMap.mMap[pos.x][pos.y + 1])
                {
                    AttackEnemyBase();
                }
            }
            if (mMap.mMap[pos.x][pos.y - 1] != null)
            {
                if (mWarriors[characterSelection].CurrentPosition == mMap.mMap[pos.x][pos.y - 1])
                {
                    AttackEnemyBase();
                }
            }
            if (mMap.mMap[pos.x + 1][pos.y] != null)
            {
                if (mWarriors[characterSelection].CurrentPosition == mMap.mMap[pos.x + 1][pos.y])
                {
                    AttackEnemyBase();
                }
            }
            if (mMap.mMap[pos.x - 1][pos.y] != null)
            {
                if (mWarriors[characterSelection].CurrentPosition == mMap.mMap[pos.x - 1][pos.y])
                {
                    AttackEnemyBase();
                }
            }
        }
    }

    private void UpdateGame()
    {
        Hud.transform.GetChild(1).GetComponent<Text>().text = "Base Health " + plrBaseHealth;
        Hud.transform.GetChild(2).GetComponent<Text>().text = "Enemy Base Health " + enmBaseHealth;

        bool check = false;

        if (Input.GetMouseButtonDown(0) && characterSelection == -1)
        {
            Ray screenClick = currentCam.ScreenPointToRay(Input.mousePosition);
            int hits = Physics.RaycastNonAlloc(screenClick, mRaycastHits);
            if (hits > 0)
            {
                EnvironmentTile tile = mRaycastHits[0].transform.GetComponent<EnvironmentTile>();

                for (int i = 0; i < foragers.Length; i++)
                {
                    if (mForagers[i].CurrentPosition == tile)
                    { 
                        for (int j = 0; j < foragers.Length; j++)
                        {
                            mForagers[j].gameObject.tag = "default";
                        }
                        for (int j = 0; j < warriors.Length; j++)
                        {
                            mWarriors[j].gameObject.tag = "default";
                        }
                        MainCamera.enabled = true;
                        OverviewCamera.enabled = false;
                        currentCam = MainCamera;
                        characterSelection = i;
                        selectionType = Character.CharacterType.Forager; 
                        mForagers[i].gameObject.tag = "Player";
                        check = true;
                    }
                }

                if (!check)
                {
                    for (int i = 0; i < warriors.Length; i++)
                    {
                        if (mWarriors[i].CurrentPosition == tile)
                        {
                            for (int j = 0; j < warriors.Length; j++)
                            {
                                mWarriors[j].gameObject.tag = "default";
                            }
                            for (int j = 0; j < foragers.Length; j++)
                            {
                                mForagers[j].gameObject.tag = "default";
                            }
                            MainCamera.enabled = true;
                            OverviewCamera.enabled = false;
                            currentCam = MainCamera;
                            characterSelection = i;
                            selectionType = Character.CharacterType.Warrior;
                            mWarriors[i].gameObject.tag = "Player";
                        }
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            for (int j = 0; j < foragers.Length; j++)
            {
                mForagers[j].gameObject.tag = "default";
            }
            
            for (int j = 0; j < warriors.Length; j++)
            {
                mWarriors[j].gameObject.tag = "default";
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

    //private void UpdateEnemy()
    //{
    //    if (posLastFrame != mForagers[0].CurrentPosition)//transform.position)
    //    {
    //        EnvironmentTile tile2 = CheckAround(mForagers[0].CurrentPosition);
    //        if (tile2 != null)
    //        {
    //            //List<EnvironmentTile> route2 = mMap.Solve(mEnemy.CurrentPosition, tile2, "enemy");
    //            //mEnemy.GoTo(route2);
    //        }
    //    }
    //    posLastFrame = mForagers[0].CurrentPosition;
    //}

    private void Update()
    {
        if (isGameStarted)
        {
            UpdateGame();

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
                mForagers[0].transform.position = CharacterStart.position;
                mForagers[1].transform.position = new Vector3(-70,0,-80);
                mWarriors[0].transform.position = new Vector3(-80,0,-60);

                mForagers[0].transform.rotation = CharacterStart.rotation;
                mForagers[1].transform.rotation = CharacterStart.rotation;
                mWarriors[0].transform.rotation = CharacterStart.rotation;

                mMap.CleanUpWorld();

                isGameStarted = false;

                MainCamera.enabled = true;
                OverviewCamera.enabled = false;

                currentCam = MainCamera;
            }
            else
            {
                mForagers[0].transform.position = mMap.Start.Position;
                mForagers[0].transform.rotation = Quaternion.identity;
                mForagers[0].CurrentPosition = mMap.Start;

                mForagers[1].transform.position = new Vector3(-65, 2.5f, -75);
                mForagers[1].transform.rotation = Quaternion.identity;
                mForagers[1].CurrentPosition = mMap.mMap[1][0];

                mWarriors[0].transform.position = new Vector3(-55, 2.5f, -75);
                mWarriors[0].transform.rotation = Quaternion.identity;
                mWarriors[0].CurrentPosition = mMap.mMap[2][0];

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