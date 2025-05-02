using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


public class Ghost : MonoBehaviour
{

    public Transform targetPos;
    [SerializeField]
    Transform  currentPos, chosenPoint, latestPoint;
    SpriteRenderer ghostSprite;

    public float moveSpeed = 3f;
    public Grid gridRef;
    public Transform[] wanderSpots;
    public List<Node> openList = new List<Node>();
    public HashSet<Node> closedList = new HashSet<Node>();
    public List<Node> finalPath = new List<Node>();
    [SerializeField] bool isMoving = false;
    private Coroutine moveCoroutine;



    [SerializeField] bool isPhasing = false;
    [SerializeField] float phaseDuration = 5f;
    [SerializeField] float phaseCooldown = 20f;
    public enum GhostState
    {
        Wandering,Seeking,Fleeing
    }
    public GhostState state;

    private void Awake()
    {
        state = GhostState.Wandering;
        currentPos = transform;
        ghostSprite= GetComponent<SpriteRenderer>();
    }
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (!isMoving)
        {
            switch (state)
            {
                case GhostState.Wandering:
                    if (chosenPoint != null)
                        latestPoint = chosenPoint;
                    chosenPoint = wanderSpots[Random.Range(0, wanderSpots.Length)];
                    GetPath(currentPos.position, chosenPoint.position);
                    StartMoving(finalPath);
                    break;
                case GhostState.Seeking:
                    GetPath(currentPos.position, targetPos.position);
                    StartMoving(finalPath);
                    break;
                case GhostState.Fleeing:
                    GetPath(currentPos.position, latestPoint.position);
                    StartMoving(finalPath);
                    break;
            }
        }

        if (!isPhasing && phaseCooldown > 0f)
        {
            phaseCooldown -= Time.deltaTime;
            if (phaseCooldown <= 0f)
            {
                StopMoving();
                StartCoroutine(Phase()); 
            }
        }


    }

    public void StartMoving(List<Node> path)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(FollowPath(path));
    }

    public void StopMoving()
    {
        isMoving = false;
        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }
    }


    private IEnumerator FollowPath(List<Node> path)
    {
/*        Debug.Log(state);*/
        isMoving = true;

/*        Debug.Log(path.Count);*/
        foreach (Node node in path)
        {
            Vector3 targetPos = node.WorldPosition;

            while ((transform.position - targetPos).sqrMagnitude > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }
        
        isMoving = false;
        moveCoroutine = null;
        
    }

    private IEnumerator Phase()
    {
        isPhasing= true;
        phaseCooldown = 20f;
        ghostSprite.color = new Color(1f, 1f, 1f, 0.3f);
        Debug.Log("Phasing");
        yield return new WaitForSeconds(phaseDuration); 
        isPhasing= false;
        ghostSprite.color = new Color(1f, 1f, 1f, 1f);
        Debug.Log("Phasing FInished");
        StopMoving();
    }

    void GetPath(Vector3 start, Vector3 target)
    {
        openList.Clear();
        closedList.Clear();
        finalPath.Clear();
        Node startingNode = gridRef.CellFromWorld(start);
        Node targetNode = gridRef.CellFromWorld(target);

        if (startingNode == null || targetNode == null)
        {  
            return;
        }

        startingNode.MoveCost= 0;
        startingNode.HeuristicCost = GetManhattanDistance(startingNode,targetNode);
        startingNode.ParentNode = null;

        openList.Add(startingNode);


        while (openList.Count > 0)
        {
            Node currentNode = openList[0];

            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].TotalCost < currentNode.TotalCost || openList[i].TotalCost == currentNode.TotalCost)
                {
                    if (openList[i].HeuristicCost < currentNode.HeuristicCost)
                    {
                        currentNode = openList[i];
                    }
                }
            }
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if (currentNode.GridX == targetNode.GridX && currentNode.GridY == targetNode.GridY)
            {
                GetFinalPath(startingNode, targetNode);
                return;
            }

            foreach (Node neighborNode in gridRef.GetNeightboringNodes(currentNode))
            {
                if (closedList.Contains(neighborNode))
                {
                    continue; 
                }

                if (!isPhasing && neighborNode.IsWall)
                {
                    continue; 
                }

                int costToNeighbor = currentNode.MoveCost + GetManhattanDistance(currentNode, neighborNode);

                if (costToNeighbor < neighborNode.MoveCost || !openList.Contains(neighborNode))
                {
                    neighborNode.MoveCost = costToNeighbor;
                    neighborNode.HeuristicCost = GetManhattanDistance(neighborNode, targetNode);
                    neighborNode.ParentNode = currentNode;

                    if (!openList.Contains(neighborNode))
                    {
                        openList.Add(neighborNode);
                    }

                }
            }

        }
    }

    void GetFinalPath(Node start, Node end)
    {
      
        Node currentnode = end;

        while (currentnode != start)
        {
            finalPath.Add(currentnode);
            currentnode = currentnode.ParentNode;
        }

        finalPath.Reverse();

        gridRef.finalPath = finalPath;
     
    }

    int GetManhattanDistance(Node nodeA, Node nodeB)
    {
        int distanceX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
        int distanceY = Mathf.Abs(nodeA.GridY - nodeB.GridY);

        return distanceX + distanceY;
    }

    public void DetectPlayer(Transform player, bool isProtected)
    {
        targetPos = player.transform;
        state = isProtected ? GhostState.Fleeing : GhostState.Seeking;
        StopMoving();
    }

    public void PlayerOutOfRange()
    {
        StartCoroutine(DelayReturnToWandering());
    }

    private IEnumerator DelayReturnToWandering()
    {
        yield return new WaitForSeconds(3f);

        state = GhostState.Wandering;
        StopMoving();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManagers.Instance.GameOver("You Died");
        }
    }
}
