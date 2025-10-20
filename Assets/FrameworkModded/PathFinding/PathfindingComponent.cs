using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using Cysharp.Threading.Tasks;

public class PathfindingComponent : MonoBehaviour
{
    [SerializeField] private LayerMask wallMask;

    private GridComponent grid;

    public void SetGrid(GridComponent setGrid)
    {
        grid = setGrid;
    }

    public async UniTask<List<Vector3>> FindPathAsync(Vector3 startPos, Vector3 targetPos)
    {
        return await UniTask.RunOnThreadPool(() =>
        {
            List<Vector3> waypoints = new List<Vector3>();
            bool pathSuccess = false;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Node startNode = grid.NodeFromWorldPoint(startPos);
            Node targetNode = grid.NodeFromWorldPoint(targetPos);
            if (startNode.walkable && targetNode.walkable)
            {
                Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
                HashSet<Node> closedSet = new HashSet<Node>();
                openSet.Add(startNode);

                while (openSet.Count > 0)
                {
                    Node currentNode = openSet.RemoveFirst();
                    closedSet.Add(currentNode);

                    if (currentNode == targetNode)
                    {
                        sw.Stop();
                        UnityEngine.Debug.Log($"Path Found: {sw.ElapsedMilliseconds}ms");
                        pathSuccess = true;
                        break;
                    }

                    foreach (Node neighbour in grid.GetNeighbours(currentNode))
                    {
                        if (!neighbour.walkable || closedSet.Contains(neighbour))
                            continue;

                        int newMoveCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
                        if (newMoveCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                        {
                            neighbour.gCost = newMoveCostToNeighbour;
                            neighbour.hCost = GetDistance(neighbour, targetNode);
                            neighbour.parent = currentNode;

                            if (!openSet.Contains(neighbour))
                                openSet.Add(neighbour);
                            else
                                openSet.UpdateItem(neighbour);
                        }
                    }
                }
            }

            if (pathSuccess)
            {
                waypoints = new List<Vector3>(RetracePath(startNode, targetNode));
            }

            return waypoints;
        });
    }

    private Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        Vector3[] waypoints = CullPath(path);
        Array.Reverse(waypoints);
        return waypoints;
    }

    private Vector3[] CullPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        if (path.Count == 0) return waypoints.ToArray();

        Vector3 locationOld = path[0].worldPosition;
        waypoints.Add(path[0].worldPosition);
        for (int i = 1; i < path.Count; i++)
        {
            waypoints.Add(path[i].worldPosition);
            locationOld = path[i].worldPosition;
        }
        return waypoints.ToArray();
    }

    private int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
        {
            return 14 * dstY + 10 * (dstX - dstY);
        }
        else
        {
           return 14 * dstX + 10 * (dstY - dstX); 
        }  
    }
}
