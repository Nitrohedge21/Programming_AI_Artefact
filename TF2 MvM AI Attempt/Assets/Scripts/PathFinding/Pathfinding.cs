using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour
{
    PFGrid grid;

    public Transform seeker;
    public Transform target;
    private void Awake()
    {
        grid = GetComponent<PFGrid>();
    }

    private void Update()
    {
        FindPath(seeker.position, target.position);
    }

    void FindPath(Vector3 startPos, Vector3 targetPos)
    {
        PFNode startNode = grid.NodeFromWorldPoint(startPos);
        PFNode targetNode = grid.NodeFromWorldPoint(targetPos);

        List<PFNode> openSet = new List<PFNode>();
        HashSet<PFNode> closedSet = new HashSet<PFNode>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            PFNode node = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < node.fCost || openSet[i].fCost == node.fCost)
                {
                    if (openSet[i].hCost < node.hCost)
                        node = openSet[i];
                }
            }

            openSet.Remove(node);
            closedSet.Add(node);

            if (node == targetNode)
            {
                RetracePath(startNode, targetNode);
                return;
            }

            foreach (PFNode neighbour in grid.GetNeighbours(node))
            {
                if (!neighbour.walkable || closedSet.Contains(neighbour))
                {
                    continue;
                }

                int newCostToNeighbour = node.gCost + GetDistance(node, neighbour);
                if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                {
                    neighbour.gCost = newCostToNeighbour;
                    neighbour.hCost = GetDistance(neighbour, targetNode);
                    neighbour.parent = node;

                    if (!openSet.Contains(neighbour))
                        openSet.Add(neighbour);
                }
            }
        }
    }

    void RetracePath(PFNode startNode, PFNode endNode)
    {
        List<PFNode> path = new List<PFNode>();
        PFNode currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();

        grid.path = path;

    }


    int GetDistance(PFNode nodeA, PFNode nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }
}
