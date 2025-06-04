using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WeepingAngel : MonoBehaviour
{
    public Transform targetPos;
    [SerializeField]
    Transform currentPos, chosenPoint, latestPoint;
    SpriteRenderer angelSprite;
    public bool isFrozen = false;
    public bool seePlayer = false;
    public bool targetInRange = false;
    public LayerMask targetMask;


    public float moveSpeed = 3f;
    public Grid gridRef;
    public Transform[] wanderSpots;
    public List<Node> openList = new List<Node>();
    public HashSet<Node> closedList = new HashSet<Node>();
    public List<Node> finalPath = new List<Node>();
    [SerializeField] bool isMoving = false;
    private Coroutine moveCoroutine;
    public FacingDirection.Direction facingDirection;

    public float radius;
    [Range(0, 360)] public float angle;

    private void Awake()
    {
        currentPos = transform;
        angelSprite = GetComponent<SpriteRenderer>();
    }
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(SensoryRoutine());
    }

    // Update is called once per frame
    void Update()
    {
       
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if(isFrozen)
            {
                Debug.Log("Angel Dead");
            }
            else
            {
                Debug.Log("Player Killed");
            }
            //GameManagers.Instance.GameOver("You Died");
        }
    }

    public IEnumerator SensoryRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.3f);
        while (true)
        {
            yield return wait;
            SensorCheck();
        }
    }

    private void SensorCheck()
    {
        Collider2D player = Physics2D.OverlapCircle(transform.position,radius,targetMask);
        if (player != null)
        {
            targetPos = player.transform;
            Vector2 dirToTarget = (targetPos.position - transform.position).normalized;
            if (Vector2.Angle(transform.right, dirToTarget) < angle/2)
            {
                float distanceToTarget = Vector2.Distance(transform.position, targetPos.position);
                RaycastHit2D ray = Physics2D.Raycast(transform.position, dirToTarget, distanceToTarget);
                if (ray.collider != null)
                {
                    seePlayer = ray.collider.CompareTag("Player");
                    targetInRange = true;
                    Debug.DrawRay(transform.position, targetPos.position - transform.position, Color.red);
                }
                else
                {
                    targetInRange= false;
                }
            }
            else
            {
                targetInRange= false;
            }
        }
    }

    private void CheckFacing()
    {

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
                Vector3 delta = (targetPos - transform.position).normalized;

                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                {
                    facingDirection = (delta.x > 0f) ? FacingDirection.Direction.Right : FacingDirection.Direction.Left;
                }
                else
                {
                    facingDirection = (delta.y > 0f) ? FacingDirection.Direction.Up : FacingDirection.Direction.Down;
                }
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }

        isMoving = false;
        moveCoroutine = null;

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

        startingNode.MoveCost = 0;
        startingNode.HeuristicCost = GetManhattanDistance(startingNode, targetNode);
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
                if (closedList.Contains(neighborNode) || neighborNode.IsWall)
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position,radius);

            
        Vector3 viewAngle1 = DirectionToAngle(transform.position.y, -angle / 2);
        Vector3 viewAngle2 = DirectionToAngle(transform.position.y, angle / 2);

        Handles.color = Color.blue;
        Handles.DrawLine(transform.position, transform.position + viewAngle1 * radius);
        Handles.DrawLine(transform.position, transform.position + viewAngle2 * radius);

    }

    private Vector3 DirectionToAngle(float eulerY, float angleInDegrees)
    {
        float rad = angleInDegrees * Mathf.Deg2Rad;

        angleInDegrees += eulerY;
        return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);

    }

}
