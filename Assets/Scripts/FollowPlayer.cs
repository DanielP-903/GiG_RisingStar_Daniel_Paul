using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
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
        if (GameObject.FindGameObjectWithTag("Player"))
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            GameObject game = GameObject.FindGameObjectWithTag("GameController");
            if (game.GetComponent<Game>().isGameStarted == true)
            {
                //this.transform.position = player.transform.position - new Vector3(140.0f, -140.0f, 140.0f);
                Vector3 pVec = new Vector3();
                pVec.x = Mathf.SmoothStep(this.transform.position.x, player.transform.position.x - 190.0f, 0.5f);// - 140.0f;
                pVec.y = this.transform.position.y;
                pVec.z = Mathf.SmoothStep(this.transform.position.z, player.transform.position.z - 190.0f, 0.5f);// - 140.0f;
                this.transform.position = pVec;
                //this.transform.position.y = 
                //this.transform.position.z = 
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
