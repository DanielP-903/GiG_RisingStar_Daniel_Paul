using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlrHealthBar : MonoBehaviour
{
    // Player health bar logic

    // Reference to the game controller
    private GameObject theGame;

    // Start is called before the first frame update
    void Start()
    {
        // Get game controller
        theGame = GameObject.FindGameObjectWithTag("GameController");
    }

    // Update is called once per frame
    void Update()
    {
        // If the game hasn't started, reset the health bar
        if (theGame.GetComponent<Game>().isGameStarted == false)
        {
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }

        // Update health bar using percentage of player base health value
        transform.localScale = new Vector3(theGame.GetComponent<Game>().plrBaseHealth / 100.0f, 1.0f, 1.0f);
    }
}
