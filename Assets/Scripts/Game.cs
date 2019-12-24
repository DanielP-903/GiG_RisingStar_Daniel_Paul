﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Game : MonoBehaviour
{
    [SerializeField] private Camera MainCamera;
    [SerializeField] private Character Character;
    [SerializeField] private Character Enemy;
    [SerializeField] private Canvas Menu;
    [SerializeField] private Canvas Hud;
    [SerializeField] private Transform CharacterStart;
    [SerializeField] private Transform EnemyStart;

    private RaycastHit[] mRaycastHits;
    private Character mCharacter;
    private Character mEnemy;
    private Environment mMap;
    private Vector3 posLastFrame;

    public Material texMaterial;

    public bool isGameStarted;

    private readonly int NumberOfRaycastHits = 1;
    
    void Start()
    {
        mRaycastHits = new RaycastHit[NumberOfRaycastHits];
        mMap = GetComponentInChildren<Environment>();
        mCharacter = Instantiate(Character, transform);
        mCharacter.tag = "Player";
        mEnemy = Instantiate(Enemy, transform);
        //mEnemy.gameObject.transform.position += new Vector3(10,0,10);
        ShowMenu(true);
    }


    private Vector2Int findIndex(EnvironmentTile tile)
    {
        for (int i = 0; i < mMap.Size.x; i++)
        {
            for (int j = 0; j < mMap.Size.y; j++)
            {
                if (mMap.mMap[i][j] == tile)
                {
                    return new Vector2Int(i, j);
                }
            }
        }

        return new Vector2Int(-1, -1);
    }

    private void Update()
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
                    List<EnvironmentTile> route = mMap.Solve(mCharacter.CurrentPosition, tile, "player");
                    mCharacter.GoTo(route);
                }
            }
        }

        Ray screenLook = MainCamera.ScreenPointToRay(Input.mousePosition);
        int hits2 = Physics.RaycastNonAlloc(screenLook, mRaycastHits);
        if (hits2 > 0)
        {
            EnvironmentTile tile = mRaycastHits[0].transform.GetComponent<EnvironmentTile>();
            //tile.GetComponent<MeshRenderer>().material = texMaterial;
            Vector2Int index = findIndex(tile);
            mMap.mMap[index.x][index.y] = mMap.AccessibleTiles[1];
            //tile = mMap.AccessibleTiles[1];
        }

        if (posLastFrame != mCharacter.transform.position)
        {
            EnvironmentTile tile2 = CheckAround(mCharacter.CurrentPosition);
            if (tile2 != null)
            {
                List<EnvironmentTile> route2 = mMap.Solve(mEnemy.CurrentPosition, tile2, "enemy");
                mEnemy.GoTo(route2);
            }
        }

        posLastFrame = mCharacter.transform.position;
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

    public void ShowMenu(bool show)
    {
        if (Menu != null && Hud != null)
        {
            Menu.enabled = show;
            Hud.enabled = !show;

            if( show )
            {
                mCharacter.transform.position = CharacterStart.position;
                mCharacter.transform.rotation = CharacterStart.rotation;
                mEnemy.transform.position = EnemyStart.position;
                mEnemy.transform.rotation = EnemyStart.rotation;
                mMap.CleanUpWorld();

                isGameStarted = false;
            }
            else
            {
                mCharacter.transform.position = mMap.Start.Position;
                mCharacter.transform.rotation = Quaternion.identity;
                mCharacter.CurrentPosition = mMap.Start;

                mEnemy.transform.position = mMap.Start.Position;
                mEnemy.transform.rotation = Quaternion.identity;
                mEnemy.CurrentPosition = mMap.Start;

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
