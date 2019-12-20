using System.Collections;
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

    private RaycastHit[] mRaycastHits;
    private Character mCharacter;
    private Character mEnemy;
    private Environment mMap;

    private readonly int NumberOfRaycastHits = 1;
    
    void Start()
    {
        mRaycastHits = new RaycastHit[NumberOfRaycastHits];
        mMap = GetComponentInChildren<Environment>();
        mCharacter = Instantiate(Character, transform);
        mEnemy = Instantiate(Enemy, transform);
        mEnemy.gameObject.transform.position += new Vector3(10,0,10);
        ShowMenu(true);
    }

    private void Update()
    {
        // Check to see if the player has clicked a tile and if they have, try to find a path to that 
        // tile. If we find a path then the character will move along it to the clicked tile. 
        if(Input.GetMouseButtonDown(0))
        {
            Ray screenClick = MainCamera.ScreenPointToRay(Input.mousePosition);
            int hits = Physics.RaycastNonAlloc(screenClick, mRaycastHits);
            if( hits > 0)
            {
                EnvironmentTile tile = mRaycastHits[0].transform.GetComponent<EnvironmentTile>();
                if (tile != null)
                {
                    List<EnvironmentTile> route = mMap.Solve(mCharacter.CurrentPosition, tile);
                    mCharacter.GoTo(route);
                }
            }
        }


        mCharacter.getCurrentPosition();

        //mCharacter.gameObject.
        int hits2 = Physics.RaycastNonAlloc(screenClick, mRaycastHits);
        EnvironmentTile tile = mRaycastHits[0].transform.GetComponent<EnvironmentTile>();
        if (tile2 != null)
        {
            List<EnvironmentTile> route = mMap.Solve(mEnemy.CurrentPosition, tile);
            mEnemy.GoTo(route);
        }
        // Get character pos
        //wmCharacter.gameObject.transform.position;
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
                mMap.CleanUpWorld();
            }
            else
            {
                mCharacter.transform.position = mMap.Start.Position;
                mCharacter.transform.rotation = Quaternion.identity;
                mCharacter.CurrentPosition = mMap.Start;
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
