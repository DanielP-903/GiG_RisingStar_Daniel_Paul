using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class follow_player : MonoBehaviour
{
    private Vector3 initialPos;

    // Start is called before the first frame update
    void Start()
    {
        initialPos = this.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameObject.FindGameObjectWithTag("Player"))// == true)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            GameObject game = GameObject.FindGameObjectWithTag("GameController");
            if (game.GetComponent<Game>().isGameStarted == true)
            {
                this.transform.position = player.transform.position - new Vector3(140.0f, -140.0f, 140.0f);
            }
            else
            {
                this.transform.position = initialPos;
            }
        }
        else
        {
            this.transform.position = initialPos;
        }
    }
}
