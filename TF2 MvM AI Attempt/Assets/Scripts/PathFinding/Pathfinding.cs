using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Pathfinding : MonoBehaviour
{
    PathRequestManager requestManager;
    PFGrid grid;

    public Transform seeker;
    public Transform target;
    private void Awake()
    {
        grid = GetComponent<PFGrid>();
        requestManager = GetComponent<PathRequestManager>();
    }

    private void Update()
    {
        FindPath(seeker.position, target.position);
    }

    public void FindPath(Vector3 startPosition, Vector3 targetPosition)
    {
        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;
        PFNode startNode = grid.NodeFromWorldPoint(startPosition);
        PFNode targetNode = grid.NodeFromWorldPoint(targetPosition);

        if(startNode.walkable && targetNode.walkable) 
        {
            List<PFNode> openSet = new List<PFNode>();
            HashSet<PFNode> closedSet = new HashSet<PFNode>();
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                PFNode currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentNode.fCost || (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                    {
                        currentNode = openSet[i];
                    }
                }
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode == targetNode) { pathSuccess = true; break; }

                foreach (PFNode neighbour in grid.GetNeighbours(currentNode))
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour)) { continue; }

                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;
                    }

                    if (!openSet.Contains(neighbour)) { openSet.Add(neighbour); }
                }
            }
        }
        
        if (pathSuccess) 
        {
            waypoints = RetracePath(startNode, targetNode);
        }
        requestManager.FinishedProcessingPath(waypoints,pathSuccess);
        
    }

    Vector3[] RetracePath(PFNode startNode, PFNode endNode)
    {
        List<PFNode> path = new List<PFNode>();
        PFNode currentNode = endNode;

        while(currentNode != startNode) 
        { 
            path.Add(currentNode);
            currentNode = currentNode.parent; 
        }
        path.Reverse();
        grid.path = path;

        Vector3[] waypoints = SimplfyPath(path);
        Array.Reverse(waypoints);
        return waypoints;
    }

    Vector3[] SimplfyPath(List<PFNode> path) 
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if(directionNew != directionOld) { waypoints.Add(path[i].worldPosition); }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
    }

    int GetDistance(PFNode nodeA,PFNode nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if(dstX > dstY) { return 14 * dstY + 10 * (dstX - dstY); }
        return 14 * dstX + 10 * (dstY - dstX);
    }
}
