using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Tilemaps;



public class Grid : MonoBehaviour
{
    public Tilemap tiles;

    public float gizmoSize = 0.7f;

    Node[,] grids;
    public List<Node> finalPath;

    public Vector3Int origin,end;
    public int width, height;
    public Transform start, ends;

    private void Awake()
    {
        GenerateGrid();
    }
    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateGrid()
    {
        BoundsInt bounds = tiles.cellBounds;
        origin = bounds.min;
        width = bounds.size.x;
        height = bounds.size.y;

        grids = new Node[width, height];

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                Vector3 worldPos = tiles.GetCellCenterWorld(cell);

                int arrayX = x - bounds.xMin;
                int arrayY = y - bounds.yMin;

                bool isWall = tiles.HasTile(cell);
                grids[arrayX, arrayY] = new Node(isWall, worldPos, arrayX, arrayY);

            }
        }
    }

    public Node CellFromWorld(Vector3 position)
    {
        Vector3Int cell = tiles.WorldToCell(position);
       
        int x = cell.x - origin.x;
        int y = cell.y - origin.y;

        Debug.Log("Convert" + grids[x,y].GridX + grids[x, y].GridX);
        return grids[x,y];

    }

    public List<Node> GetNeightboringNodes(Node neighbourNode)
    {
        List<Node> neighbors = new List<Node>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                {
                    continue;
                }

                int checkX = neighbourNode.GridX + x;
                int checkY = neighbourNode.GridY + y;

                if (checkX >= 0 && checkX < width && checkY >= 0 && checkY < height)
                {
                    neighbors.Add(grids[checkX, checkY]);
                }

            }
        }
        Debug.Log("Neighbor " + neighbors.Count);
        return neighbors;

    }

    private void OnDrawGizmos()
    {
        if (grids != null) 
        {
            Node playerNode = CellFromWorld(ends.position);
            Node targetNode = CellFromWorld(start.position);

            Debug.Log("PlayerNode Pos: " + playerNode.WorldPosition);
            Debug.Log("TargetNode Pos: " + targetNode.WorldPosition);

            foreach (Node node in grids)
            {
                
                Gizmos.color = !node.IsWall ? Color.green : Color.red;
              
                if (finalPath != null)
                {
                    if (finalPath.Contains(node))
                    {
                        Gizmos.color = Color.black;
                    }
                }
                else
                {
                    Debug.Log("No Path");
                }
                if (playerNode == node)
                    Gizmos.color = Color.blue;

                if (targetNode == node)
                    Gizmos.color = Color.cyan;

                Gizmos.DrawCube(node.WorldPosition, Vector3.one * gizmoSize);

            }



        }
       
    }


}
