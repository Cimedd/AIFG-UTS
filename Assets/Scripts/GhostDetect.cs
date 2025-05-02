using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostDetect : MonoBehaviour
{
    public Ghost ghost;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            ghost.DetectPlayer(collision.transform, player.isProtected);
     
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Player player = collision.GetComponent<Player>();
            bool shouldFlee = player.isProtected;

            if ((shouldFlee && ghost.state != Ghost.GhostState.Fleeing) ||
             (!shouldFlee && ghost.state != Ghost.GhostState.Seeking))
            {
                ghost.DetectPlayer(collision.transform, shouldFlee);
            }
        }
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        ghost.PlayerOutOfRange();
        
    }
}
