using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnmHealthBar : MonoBehaviour
{
    private GameObject theGame;

    // Start is called before the first frame update
    void Start()
    {
        theGame = GameObject.FindGameObjectWithTag("GameController");
    }

    // Update is called once per frame
    void Update()
    {
        if (theGame.GetComponent<Game>().isGameStarted == false)
        {
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }
        transform.localScale = new Vector3(theGame.GetComponent<Game>().plrBaseHealth / 100.0f, 1.0f, 1.0f);
    }
}
