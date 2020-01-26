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

    [SerializeField] public int maxUnits = 10;
    public int unitCount = 0;
    public int enemyUnitCount = 0;
    public float timer = 5.0f;
    public float timer_Cash = 10.0f;

    [SerializeField] private Canvas Menu;
    [SerializeField] private Canvas Hud;
    [SerializeField] private Canvas H2p;
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
    //public Character.CharacterType selectionType = Character.CharacterType.Forager;
    public Character.Ownership winner = Character.Ownership.Player;

    public Material texMaterial;
    public bool isGameStarted;

    public EnvironmentTile baseTile;
    public EnvironmentTile enemyBaseTile;




    private readonly int NumberOfRaycastHits = 10;
    private float timerEnd = 10.0f;
    private bool finishGame = false;
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
            winner = Character.Ownership.Enemy;
            finishGame = true;
        }
    }

    public void AttackEnemyBase()
    {
        enmBaseHealth -= 0.2f;

        if (enmBaseHealth <= 0.0f)
        {
            Debug.Log("You win! GAME OVER");
            winner = Character.Ownership.Player;
            finishGame = true;
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

    private void UpdateGame()
    {
        Hud.transform.GetChild(4).GetComponent<Text>().text = "Cash \n" + cash;
        Hud.transform.GetChild(5).GetComponent<Text>().text = "Units \n" + unitCount + " / 10";


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

    public void splash()
    {
        if (timerEnd > 0)
        {
            if (winner == Character.Ownership.Enemy)
            {
                plrBaseHealth = 0.0f;
                Hud.transform.GetChild(6).gameObject.SetActive(true);
            }
            else
            {
                enmBaseHealth = 0.0f;
                Hud.transform.GetChild(7).gameObject.SetActive(true);
            }

            timerEnd -= Time.deltaTime;
        }
        else
        {
            finishGame = false;
            timerEnd = 10.0f;
            isGameStarted = false;
            Hud.transform.GetChild(6).gameObject.SetActive(false);
            Hud.transform.GetChild(7).gameObject.SetActive(false);
            ShowMenu(true);
        }
    }

    private void Update()
    {
        if (isGameStarted)
        {
            if (finishGame == false)
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
                    else if (EnemyForagerList.Count == 0)
                    {
                        EnemyGenerator(Character.CharacterType.Forager);
                    }
                }
            }
            else
            {
                splash();
            }
        }
    }

    public void ShowH2P(bool show)
    {
        if (H2p != null && Menu!= null)
        {
            if (show)
            {
                Menu.enabled = false;
                Hud.enabled = false;
                H2p.enabled = true;

                MainCamera.enabled = true;
                OverviewCamera.enabled = false;

                currentCam = MainCamera;
            }
            else
            {
                Menu.enabled = true;
                Hud.enabled = false;
                H2p.enabled = false;
                ShowMenu(true);
            }
        }
    }

    public void ShowMenu(bool show)
    {
        if (Menu != null && Hud != null)
        {
            Menu.enabled = show;
            Hud.enabled = !show;
            H2p.enabled = false;
            if (show)
            {
                finishGame = false;
                timer = 5;
                timer_Cash = 10;
                timerEnd = 10;
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
