using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Ghost : MonoBehaviour
{

    public Transform targetPos;
    Transform  currentPos;
    public Grid gridRef;
    public Transform[] wanderSpots;
    public List<Node> openList = new List<Node>();
    public HashSet<Node> closedList = new HashSet<Node>();
    public List<Node> finalPath = new List<Node>();
    //bool isMoving = false;
    public enum GhostState
    {
        Wandering,Seeking,Fleeing
    }
    public GhostState state;

    private void Awake()
    {
        state = GhostState.Wandering;
        currentPos = transform;
    }
    // Start is called before the first frame update
    void Start()
    {
  
       
    }

    // Update is called once per frame
    void Update()
    {
        /*        switch (state)
                {
                    case GhostState.Wandering:
                        break;
                    case GhostState.Seeking:
                        break; 
                    case GhostState.Fleeing:
                        break;
                }
        */
        GetPath(currentPos.position, targetPos.position);

    }

    void MoveAlongPath()
    {

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
            Debug.Log("Start or target node is null!");
            return;
        }
     /*   Debug.Log("Ghost : " + startingNode.GridX + "," + startingNode.GridY);
        Debug.Log("Player : " + targetNode.GridX + "," + targetNode.GridY);*/

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
            {/*
                Debug.Log("Checking neighbor: " + neighborNode.GridX + "," + neighborNode.GridY);*/
                if (neighborNode.IsWall || closedList.Contains(neighborNode))
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

                    Debug.Log("Open List : " + openList.Count);
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
        Debug.Log("Final " + finalPath.Count);
    }

    int GetManhattanDistance(Node nodeA, Node nodeB)
    {
        int distanceX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
        int distanceY = Mathf.Abs(nodeA.GridY - nodeB.GridY);

        return distanceX + distanceY;
    }
}
