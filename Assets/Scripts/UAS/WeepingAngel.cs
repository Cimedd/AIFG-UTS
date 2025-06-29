using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WeepingAngel : MonoBehaviour
{
    public Transform targetPos;

    public FieldOfView fov;
    [SerializeField]
    public Transform currentPos, chosenPoint, latestPoint;
    SpriteRenderer sprite;
    public bool isFrozen = false;
    public bool targetInRange = false;
    public bool isDormant = false;
    public LayerMask targetMask;
    public float targetCooldownDuration = 1.5f; 
    public float targetCooldownTimer = 0f;
    public bool isTest = false;


    public float moveSpeed = 3f;
    public Grid gridRef;
    public Transform[] wanderSpots;
    public List<Node> openList = new List<Node>();
    public HashSet<Node> closedList = new HashSet<Node>();
    public List<Node> finalPath = new List<Node>();
    [SerializeField] public bool isMoving = false;
    private Coroutine moveCoroutine, sensorRoutine;
    public FacingDirection.Direction facingDirection;
    [SerializeField]private Vector2 facingSensor = Vector2.up;


    public float radius;
    [Range(0, 360)] public float angle;

    public EnemyState currentState;
    public DormantState dormantState;
    public ChaseState chaseState;
    public WanderState wanderState;
     

    private void Awake()
    {
        currentPos = transform;
        sprite = GetComponent<SpriteRenderer>();
        fov.SetFov(GetBaseAngle(), radius);
    }
    
    void Start()
    {
        dormantState = new DormantState(this);
        chaseState = new ChaseState(this);
        wanderState = new WanderState(this);

        if (isTest)
        {
            ChangeState(wanderState);
        }
        else
        {
            ChangeState(dormantState);
        }
       

        StartCoroutine(SensoryRoutine());
    }

    
    void Update()
    {
        if (targetCooldownTimer > 0f)
            targetCooldownTimer -= Time.deltaTime;

        UpdateFacingSensor();
        currentState.Update();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        currentState.Collision(collision);
    }

    public void ChangeState(EnemyState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
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
        if (targetCooldownTimer > 0f)
        {
           return;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, targetMask);

        if (hits.Length == 0)
        {
            targetPos = null;
            return;
        }

        foreach (Collider2D hit in hits)
        {
            if (hit == null) {
                continue;
            } 

            Transform target = hit.transform;

            if (hit.CompareTag("dummy"))
            {
                if (targetPos == null || !targetPos.CompareTag("dummy"))
                {
                    StopMoving();
                    targetPos = target;
                    return;
                }
            }

            if (targetPos == null)
            {
                targetPos = target;
            }
        }

        if (targetPos != null)
        {
            Vector2 dirToTarget = (targetPos.position - transform.position).normalized;

            if (Vector2.Angle(facingSensor, dirToTarget) < angle / 2)
            {
                float distanceToTarget = Vector2.Distance(transform.position, targetPos.position);
                RaycastHit2D ray = Physics2D.Raycast(transform.position, dirToTarget, distanceToTarget);
                Debug.DrawRay(transform.position, dirToTarget * distanceToTarget, Color.red);
                if (ray.collider != null)
                {
                    targetInRange = true;
                }
                else
                {
                    targetInRange = false;
                }
            }
            else
            {
                targetInRange = false;
            }
        }
    }



    public void StartMoving(List<Node> path)
    {
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(FollowPath(path));
    }

    public void StartSensor()
    {
        if (sensorRoutine != null) StopCoroutine(sensorRoutine);
        sensorRoutine = StartCoroutine(SensoryRoutine());
    }

    public void StopSennsor()
    {
        if (sensorRoutine != null)
        {
            fov.SetFov(0, 0);
            StopCoroutine(sensorRoutine);
            sensorRoutine = null;
        }
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
        isMoving = true;
        foreach (Node node in path)
        {
            Vector3 targetPos = node.WorldPosition;
            
            while ((transform.position - targetPos).sqrMagnitude > 0.05f)
            {
                Vector3 delta = (targetPos - transform.position).normalized;

                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                {
                    if (delta.x > 0f)
                    {
                        facingDirection = FacingDirection.Direction.Right;
                    }
                    else
                    {
                        facingDirection = FacingDirection.Direction.Left;
                    }
                }
                else
                {
                    if (delta.y > 0f)
                    {
                        facingDirection = FacingDirection.Direction.Up;
                    }
                    else
                    {
                        facingDirection = FacingDirection.Direction.Down;
                    }
                }
                //UpdateFacingSensor();
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }
        }

        isMoving = false;
        moveCoroutine = null;

    }

    private void UpdateFacingSensor()
    {
        switch (facingDirection)
        {
            case FacingDirection.Direction.Up:
                facingSensor = Vector2.up;
                break;
            case FacingDirection.Direction.Down:
                facingSensor = Vector2.down;
                break;
            case FacingDirection.Direction.Left:
                facingSensor = Vector2.left;
                break;
            case FacingDirection.Direction.Right:
                facingSensor = Vector2.right;
                break;
        }

        fov.SetFov(GetBaseAngle(), radius);

    }
    public void GetPath(Vector3 start, Vector3 target)
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
        Gizmos.DrawWireSphere(transform.position, radius);
        float baseAngle = GetBaseAngle();

        Vector3 viewAngle1 = DirectionToVector(baseAngle - angle / 2);
        Vector3 viewAngle2 = DirectionToVector(baseAngle + angle / 2);

        Handles.color = Color.blue;
        Handles.DrawLine(transform.position, transform.position + viewAngle1 * radius);
        Handles.DrawLine(transform.position, transform.position + viewAngle2 * radius);

    }

    private Vector3 DirectionToVector(float angleInDegrees)
    {
        float rad = angleInDegrees * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0);
    }

    public float GetBaseAngle()
    {
        switch (facingDirection)
        {
            case FacingDirection.Direction.Right: return 0f;
            case FacingDirection.Direction.Up: return 90f;
            case FacingDirection.Direction.Left: return 180f;
            case FacingDirection.Direction.Down: return 270f;
            default: return 0f;
        }
    }

 

}
