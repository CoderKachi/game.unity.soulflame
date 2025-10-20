using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class PathfindingService : MonoBehaviour, IFrameworkService
{
    private PathfindingComponent _PathfindingComponent;
    private GridComponent _GridComponent;
    private PathRequestManager _PathRequestManager;

    public void Setup()
    {
        this.transform.position = new Vector3(0, -0.05f, 0);

        _PathfindingComponent = this.gameObject.AddComponent<PathfindingComponent>();
        _GridComponent = this.gameObject.AddComponent<GridComponent>();

        _PathfindingComponent.SetGrid(_GridComponent);

        GridComponentSettings gridComponentSettings = Resources.Load<GridComponentSettings>("DefaultGridComponentSettings");
        if (gridComponentSettings != null)
        {
            _GridComponent.ApplySettings(gridComponentSettings);
        }
    }

    public async UniTask<List<Vector3>> PathTo(Vector3 start, Vector3 target)
    {
        List<Vector3> waypoints = await _PathfindingComponent.FindPathAsync(start, target);

        if (waypoints.Count > 0)
        {
            Debug.Log($"Path found with {waypoints.Count} waypoints.");
        }
        else
        {
            Debug.Log("No path found");
        }

        return waypoints;
    }

    public Vector3 RoundToGrid(Vector3 worldPosition)
    {
        Node node = _GridComponent.NodeFromWorldPoint(worldPosition);
        return node.worldPosition;
    }

    public void Ping()
    {
        Debug.Log("Pong");
    }
}
