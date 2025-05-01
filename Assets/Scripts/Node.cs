using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    private int heuristicCost;
    private bool isWall;
    private Vector3 worldPosition;
    private Node parentNode;
    private int gridX;
    private int gridY;
    private int moveCost;

    public Node(bool isWall, Vector3 worldPosition, int gridX, int gridY)
    {
        this.GridX = gridX;
        this.GridY = gridY;
        this.IsWall = isWall;
        this.WorldPosition = worldPosition;
    }
    public int TotalCost
    {
        get
        {
            return MoveCost * HeuristicCost;
        }
    }

    public int GridX { get => gridX; set => gridX = value; }
    public int GridY { get => gridY; set => gridY = value; }
    public int MoveCost { get => moveCost; set => moveCost = value; }
    public int HeuristicCost { get => heuristicCost; set => heuristicCost = value; }
    public bool IsWall { get => isWall; set => isWall = value; }
    public Vector3 WorldPosition { get => worldPosition; set => worldPosition = value; }
    public Node ParentNode { get => parentNode; set => parentNode = value; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
