using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static Framework;

// Requires
[RequireComponent(typeof(MovementComponent), typeof(ModelComponent))]

public class IsoUnitController : IsoCharacterController
{
    // Services
    private UnitService UnitService;
    private PathfindingService PathfindingService;

    // Components

    // Variables
    public Vector3 targetNode; // The node the Unit always tries to move towards
    public Vector3 currentNode; // The node the Unit is currently on
    public List<Vector3> waypoints;
    public List<Vector3> visitedWaypoints;

    // Configuration

    // Debugging
    [Header("Debugging")]
    [SerializeField] protected Mesh _waypointMesh;
    [SerializeField] protected Material _waypointMaterial;
    [SerializeField] protected Material _visitedWaypointMaterial;

    protected override void Awake()
    {
        base.Awake();

        UnitService = Game.GetService<UnitService>();
        PathfindingService = Game.GetService<PathfindingService>();
    }

    protected override void Start()
    {
        base.Start();
        UnitService.RegisterUnit(this);
    }

    protected override void Update()
    {
        currentNode = PathfindingService.RoundToGrid(this.transform.position);

        if (waypoints.Count != 0)
        {
            Vector3 currentWaypointPosition = waypoints[0];
            float distance = Vector3.Distance(this.transform.position, currentWaypointPosition);

            if (distance > 0.125)
            {
                Move(-(this.transform.position - currentWaypointPosition).normalized);
            }
            else
            {
                visitedWaypoints.Add(waypoints[0]);
                waypoints.RemoveAt(0);
            }
        }
        else
        {
            Move(_moveDirection);
        }
    }

    public async void TryPathTo(Vector3 targetPosition)
    {
        Debug.Log("Attempting to move!");
        waypoints = await PathfindingService.PathTo(this.transform.position, targetPosition);
        if (waypoints.Count != 0)
        {
            targetNode = waypoints[waypoints.Count - 1];
        }
    }

    void OnDestroy()
    {
        // Disconnect from connections
    }

    private void OnRenderObject()
    {
        if (_waypointMesh == null) return;
        if (_waypointMaterial == null) return;
        
        _waypointMaterial.SetPass(0);

        foreach (Vector3 waypoint in waypoints)
        {
            Graphics.DrawMeshNow
            (
                _waypointMesh, // built once, or use a shared primitive
                Matrix4x4.TRS
                (
                    waypoint,
                    Quaternion.identity,
                    Vector3.one * 0.25f
                )
            );
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        foreach (Vector3 waypoint in waypoints)
        {
            Gizmos.DrawSphere(waypoint, 0.25f);
        }
    }
}
