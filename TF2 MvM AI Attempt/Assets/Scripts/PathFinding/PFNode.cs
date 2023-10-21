using UnityEngine;

public class PFNode
{
    public bool walkable;
    public Vector3 worldPosition;

    public PFNode(bool _walkable, Vector3 _worldPosition) 
    {
        walkable = _walkable;
        worldPosition = _worldPosition;
    }

}
