﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class Game : MonoBehaviour
{
    [SerializeField] private Camera MainCamera;
    [SerializeField] private Camera OverviewCamera;
    [SerializeField] private Character[] Character = new Character[2];
    [SerializeField] private Canvas Menu;
    [SerializeField] private Canvas Hud;
    [SerializeField] private Transform CharacterStart;
    [SerializeField] private Transform EnemyStart;
    [SerializeField] public int resStone;
    
    private RaycastHit[] mRaycastHits;
    private Character[] mCharacter = new Character[2];
    private Environment mMap;
    private EnvironmentTile posLastFrame;

    private Camera currentCam;

    public int characterSelection = -1;

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

        mCharacter[0].MyType = global::Character.CharacterType.Forager;
        mCharacter[1].MyType = global::Character.CharacterType.Forager;

        mCharacter[0].CurrentTarget = null;
        mCharacter[1].CurrentTarget = null;

        characterSelection = -1;



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

    private void UpdatePlayer()
    {
        Debug.Log(mCharacter[characterSelection].CurrentTarget);

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
                        route = mMap.Solve(mCharacter[characterSelection].CurrentPosition, tile, "player");
                        mCharacter[characterSelection].GoTo(route);
                        mCharacter[characterSelection].CurrentTarget = null;
                    }
                    else if (tile.Type == "rock")
                    {
                        EnvironmentTile tile2 = CheckAround(tile);
                        route = mMap.Solve(mCharacter[characterSelection].CurrentPosition, tile2, "player");
                        mCharacter[characterSelection].GoTo(route);
                        mCharacter[characterSelection].CurrentTarget = tile;
                    }
                }
            }
        }

        if (mCharacter[characterSelection].CurrentTarget != null)
        {
            Vector2Int pos = FindIndex(mCharacter[characterSelection].CurrentTarget);

            if (mMap.mMap[pos.x][pos.y + 1] != null)
            {
                if (mCharacter[characterSelection].CurrentPosition == mMap.mMap[pos.x][pos.y + 1])
                {
                    mCharacter[characterSelection].Forage(ref mMap, ref mMap.mAll);
                }
            } 
            
            if (mMap.mMap[pos.x][pos.y - 1] != null)
            {
                if (mCharacter[characterSelection].CurrentPosition == mMap.mMap[pos.x][pos.y - 1])
                {
                    mCharacter[characterSelection].Forage(ref mMap, ref mMap.mAll);
                }
            }
            
            if (mMap.mMap[pos.x + 1][pos.y] != null)
            {
                if (mCharacter[characterSelection].CurrentPosition == mMap.mMap[pos.x + 1][pos.y])
                {
                    mCharacter[characterSelection].Forage(ref mMap, ref mMap.mAll);
                }
            }
            
            if (mMap.mMap[pos.x - 1][pos.y] != null)
            {
                if (mCharacter[characterSelection].CurrentPosition == mMap.mMap[pos.x - 1][pos.y])
                {
                    mCharacter[characterSelection].Forage(ref mMap, ref mMap.mAll);
                }
            }
        }
    }

    private void UpdateGame()
    {
        if (Input.GetMouseButtonDown(0) && characterSelection == -1)
        {
            Ray screenClick = currentCam.ScreenPointToRay(Input.mousePosition);
            int hits = Physics.RaycastNonAlloc(screenClick, mRaycastHits);
            if (hits > 0)
            {
                EnvironmentTile tile = mRaycastHits[0].transform.GetComponent<EnvironmentTile>();

                for (int i = 0; i < Character.Length; i++)
                {
                    if (mCharacter[i].CurrentPosition == tile)
                    { 
                        for (int j = 0; j < Character.Length; j++)
                        {
                            mCharacter[j].gameObject.tag = "default";
                        }
                        MainCamera.enabled = true;
                        OverviewCamera.enabled = false;
                        currentCam = MainCamera;
                        characterSelection = i;
                        mCharacter[i].gameObject.tag = "Player";
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            for (int j = 0; j < Character.Length; j++)
            {
                mCharacter[j].gameObject.tag = "default";
            }

            MainCamera.enabled = false;
            OverviewCamera.enabled = true;

            currentCam = OverviewCamera;

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

            /*
            if (Input.GetKey(KeyCode.A))
            {
                addVec += (new Vector3(-1, 0, 0));
            }
            else if (Input.GetKey(KeyCode.D))
            {
                addVec += (new Vector3(1, 0, 0));
            }
            if (Input.GetKey(KeyCode.W))
            {
                addVec += (new Vector3(0, 0, 1));
            }
            else if (Input.GetKey(KeyCode.S))
            {
                addVec += (new Vector3(0, 0, -1));
            }
            */

            currentCam.transform.position = addVec;
            OverviewCamera.transform.position = addVec;
        }

    }

    //private void UpdateEnemy()
    //{
    //    if (posLastFrame != mCharacter[0].CurrentPosition)//transform.position)
    //    {
    //        EnvironmentTile tile2 = CheckAround(mCharacter[0].CurrentPosition);
    //        if (tile2 != null)
    //        {
    //            //List<EnvironmentTile> route2 = mMap.Solve(mEnemy.CurrentPosition, tile2, "enemy");
    //            //mEnemy.GoTo(route2);
    //        }
    //    }
    //    posLastFrame = mCharacter[0].CurrentPosition;
    //}

    private void Update()
    {
        if (isGameStarted)
        {
            UpdateGame();

            if (characterSelection != -1)
            {
                UpdatePlayer();

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
                mCharacter[0].transform.position = CharacterStart.position;
                mCharacter[0].transform.rotation = CharacterStart.rotation;
                mCharacter[1].transform.position = new Vector3(-80,0,-60);
                mCharacter[1].transform.rotation = CharacterStart.rotation;

                mMap.CleanUpWorld();

                isGameStarted = false;

                MainCamera.enabled = true;
                OverviewCamera.enabled = false;

                currentCam = MainCamera;
            }
            else
            {
                mCharacter[0].transform.position = mMap.Start.Position;
                mCharacter[0].transform.rotation = Quaternion.identity;
                mCharacter[0].CurrentPosition = mMap.Start;
                
                mCharacter[1].transform.position = mMap.Start.Position;
                mCharacter[1].transform.rotation = Quaternion.identity;
                mCharacter[1].CurrentPosition = mMap.Start;

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