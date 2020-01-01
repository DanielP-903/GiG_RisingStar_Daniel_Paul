using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class Game : MonoBehaviour
{
    [SerializeField] private Camera MainCamera;
    [SerializeField] private Character[] Character = new Character[2];
    //[SerializeField] private Character Enemy;
    [SerializeField] private Canvas Menu;
    [SerializeField] private Canvas Hud;
    [SerializeField] private Transform CharacterStart;
    [SerializeField] private Transform EnemyStart;

    private RaycastHit[] mRaycastHits;
    private Character[] mCharacter = new Character[2];
    private Character mEnemy;
    private Environment mMap;
    private EnvironmentTile posLastFrame;

    public int characterSelection;

    public Material texMaterial;
    public bool isGameStarted;

    private readonly int NumberOfRaycastHits = 10;

    void Start()
    {
        mRaycastHits = new RaycastHit[NumberOfRaycastHits];
        mMap = GetComponentInChildren<Environment>();
        mCharacter[0] = Instantiate(Character[0], transform);
        mCharacter[1] = Instantiate(Character[1], transform);
        mCharacter[0].tag = "Player";
        mCharacter[1].tag = "Player";
        mCharacter[0].CurrentTarget = null;
        //mEnemy = Instantiate(Enemy, transform);
        ShowMenu(true);
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

    private void CheckSelectedTile()
    {
        for (int i = 0; i < mMap.Size.x; i++)
        {
            for (int j = 0; j < mMap.Size.y; j++)
            {
                if (mMap.mMap[i][j].IsAccessible == true)
                {
                    mMap.mMap[i][j].GetComponent<MeshRenderer>().materials = mMap.AccessibleTiles[0].GetComponent<MeshRenderer>().sharedMaterials;
                }
                else
                {
                    //mMap.mMap[i][j].GetComponent<MeshRenderer>().materials = mMap.InaccessibleTiles[4].GetComponent<MeshRenderer>().sharedMaterials;
                }
            }
        }

        Ray screenLook = MainCamera.ScreenPointToRay(Input.mousePosition);
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
                    tile.GetComponent<MeshRenderer>().materials = mMap.AccessibleTiles[1].GetComponent<MeshRenderer>().sharedMaterials;
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

    private void UpdatePlayer()
    {
        // Check to see if the player has clicked a tile and if they have, try to find a path to that 
        // tile. If we find a path then the character will move along it to the clicked tile. 
        if (Input.GetMouseButtonDown(0))
        {
            Ray screenClick = MainCamera.ScreenPointToRay(Input.mousePosition);
            int hits = Physics.RaycastNonAlloc(screenClick, mRaycastHits);
            if (hits > 0)
            {
                EnvironmentTile tile = mRaycastHits[0].transform.GetComponent<EnvironmentTile>();

                if (tile != null)
                {
                    List<EnvironmentTile> route;

                    //Debug.Log(string.Format(tile.Type));

                    if (tile.Type == "ground")
                    {
                        route = mMap.Solve(mCharacter[0].CurrentPosition, tile, "player");
                        mCharacter[0].GoTo(route);
                    }
                    else
                    {
                        //for (int i = 4; i < 10; i++)
                        //{
                            //string objType = "Object: " + i;
                            //Debug.Log("objType: " + i);
                            //if (tile.Type == objType)
                            //{
                                EnvironmentTile tile2 = CheckAround(tile);
                                route = mMap.Solve(mCharacter[0].CurrentPosition, tile2, "player");
                                mCharacter[0].GoTo(route);
                                mCharacter[0].CurrentTarget = tile;
                                mCharacter[0].Forage(ref mMap, ref mMap.mAll);
                                //break;
                            //}
                        //}
                    }
                }
            }
        }
    }

    private void UpdateEnemy()
    {
        if (posLastFrame != mCharacter[0].CurrentPosition)//transform.position)
        {
            EnvironmentTile tile2 = CheckAround(mCharacter[0].CurrentPosition);
            if (tile2 != null)
            {
                //List<EnvironmentTile> route2 = mMap.Solve(mEnemy.CurrentPosition, tile2, "enemy");
                //mEnemy.GoTo(route2);
            }
        }

        posLastFrame = mCharacter[0].CurrentPosition;
    }

    private void Update()
    {
        if (isGameStarted)
        {
            UpdatePlayer();

            CheckSelectedTile();

            //UpdateEnemy();
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
                mCharacter[0].transform.position = CharacterStart.position;
                mCharacter[0].transform.rotation = CharacterStart.rotation;
                //mEnemy.transform.position = EnemyStart.position;
                //mEnemy.transform.rotation = EnemyStart.rotation;
                mMap.CleanUpWorld();

                isGameStarted = false;
            }
            else
            {
                mCharacter[0].transform.position = mMap.Start.Position;
                mCharacter[0].transform.rotation = Quaternion.identity;
                mCharacter[0].CurrentPosition = mMap.Start;

                //mEnemy.transform.position = mMap.Start.Position;
                //mEnemy.transform.rotation = Quaternion.identity;
                //mEnemy.CurrentPosition = mMap.Start;

                isGameStarted = true;
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