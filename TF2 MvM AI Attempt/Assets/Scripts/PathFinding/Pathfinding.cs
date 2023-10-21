using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour
{
    PFGrid grid;
    private void Awake()
    {
        grid = GetComponent<PFGrid>();
    }
    void FindPatch(Vector3 startPosition, Vector3 targetPosition)
    {
        PFNode startNode = grid.NodeFromWorldPoint(startPosition);
        PFNode targetNode = grid.NodeFromWorldPoint(targetPosition);

        List<PFNode> openSet = new List<PFNode>();
        HashSet<PFNode> closedSet = new HashSet<PFNode>();
        openSet.Add(startNode);

    }
}
