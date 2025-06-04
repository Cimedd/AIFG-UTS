using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public float moveSpeed = 3f;
    public LayerMask obstacleLayer;
    public Transform movePoint;
    public bool isProtected = false;
    float protectionTime= 4f;
    float protectionCooldown = 15f;
    public Grid gridRef;
    //public Image skill;
    public GameObject shieldVFX;
    public float faceDir = 0;

    public FacingDirection.Direction facingDirection;

    // Start is called before the first frame update
    void Start()
    {
        Node start = gridRef.CellFromWorld(transform.position);
        transform.position = start.WorldPosition; 
        movePoint.parent = null;
        protectionCooldown= 5f;
        //skill.color = new Color(152f / 255f, 152f / 255f, 152f / 255f, 128f / 255f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);
        if(Vector3.Distance(transform.position, movePoint.position) <= .05f)
        {
            float x = Input.GetAxisRaw("Horizontal");
            float y = Input.GetAxisRaw("Vertical");
            if (Mathf.Abs(x) == 1f)
            {
                facingDirection = (x > 0f) ? FacingDirection.Direction.Right : FacingDirection.Direction.Left;
                if (!Physics2D.OverlapCircle((movePoint.position + new Vector3(x, 0f, 0f)), 0.2f, obstacleLayer))
                {
                    movePoint.position += new Vector3(x, 0f, 0f);
                }
            }
            else if(Mathf.Abs(y) == 1f)
            {
                facingDirection = (y > 0f) ? FacingDirection.Direction.Up : FacingDirection.Direction.Down;
                if (!Physics2D.OverlapCircle((movePoint.position + new Vector3(0f, y, 0f)), 0.2f, obstacleLayer))
                {
                    movePoint.position += new Vector3(0, y, 0f);
                }
            }
        }

      /*  if(!isProtected && protectionCooldown >= 0f)
        {
            protectionCooldown -= Time.deltaTime;
            if(protectionCooldown <= 0f)
            {
                skill.color = new Color(255f / 255f, 230f / 255f, 230f / 255f, 255f / 255f);
            }
        }*/

        /*if(Input.GetMouseButtonDown(0) && protectionCooldown <= 0f) {
            StartCoroutine(Protection());
        }*/

      
    }

    /*private IEnumerator Protection()
    {
        isProtected= true;
        shieldVFX.SetActive(true);
        skill.color = new Color(152f / 255f, 152f / 255f, 152f / 255f, 128f/255f);
        yield return new WaitForSeconds(protectionTime);
        isProtected= false;
        shieldVFX.SetActive(false);
        protectionCooldown = 15f;
    }*/



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.name == "TreasureChest")
        {
            GameManagers.Instance.updateSoul();
            Destroy(collision.gameObject);
        }
    }

}
