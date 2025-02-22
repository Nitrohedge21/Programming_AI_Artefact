using UnityEngine;

public class PFNode
{
    public bool walkable;
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;

    public int gCost;
    public int hCost;
    public PFNode parent;

    public PFNode(bool _walkable, Vector3 _worldPosition, int _gridX, int _gridY) 
    {
        walkable = _walkable;
        worldPosition = _worldPosition;
        gridX = _gridX;
        gridY = _gridY;
    }

    public int fCost { get { return gCost + hCost; } }

}
