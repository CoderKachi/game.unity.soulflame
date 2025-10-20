using UnityEngine;

[CreateAssetMenu(fileName = "GridComponentSettings", menuName = "Pathfinding/Grid Settings")]
public class GridComponentSettings : ScriptableObject
{
    [Header("Grid Setup")]
    public bool displayGridGizmos = false;
    public LayerMask unwalkableMask;
    public LayerMask floorMask;
    public Vector2 gridWorldSize = new Vector2(100, 100);
    public float nodeRadius = 1f;
    public float nodeSkin = 0.9f;

    [Header("Walkable Regions")]
    public GridComponent.TerrainType[] walkableRegions;
}
