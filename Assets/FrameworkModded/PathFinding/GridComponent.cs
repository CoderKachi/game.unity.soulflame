using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

public class GridComponent : MonoBehaviour
{
    public bool displayGridGizmos;
    public LayerMask unwalkableMask, floorMask;
    public Vector2 gridWorldSize = new Vector2(100, 100);
    public float nodeRadius = 0.5f;
    public float nodeSkin = 0.45f;
    public TerrainType[] walkableReigons = new TerrainType[0];
    LayerMask walkableMask;
    Dictionary<int, int> walkableRegionDict = new Dictionary<int, int>();

    Node[,] grid;

    float nodeDiameter = 0.1f;
    int gridSizeX, gridSizeY;

    private void Awake()
    {
        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);
        CreateGrid();

        foreach(TerrainType region in walkableReigons)
        {
            walkableMask.value |= region.terrainMask.value;
            walkableRegionDict.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty);
        }
    }

    public async UniTask<bool> GenerateNewGrid()
    {
        await UniTask.Yield();
        walkableRegionDict.Clear();
        await UniTask.Delay(1);
        CreateGrid();

        foreach (TerrainType region in walkableReigons)
        {
            walkableMask.value |= region.terrainMask.value;
            //.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty);
        }
        return true;
    }

    public void ApplySettings(GridComponentSettings settings)
    {
        if (settings == null)
        {
            Debug.LogError("GridComponentSettings is null!");
            return;
        }

        displayGridGizmos = settings.displayGridGizmos;
        unwalkableMask = settings.unwalkableMask;
        floorMask = settings.floorMask;
        gridWorldSize = settings.gridWorldSize;
        nodeRadius = settings.nodeRadius;
        nodeSkin = settings.nodeSkin;
        walkableReigons = settings.walkableRegions;

        nodeDiameter = nodeRadius * 2;
        gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
        gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

        _ = GenerateNewGrid(); // fire-and-forget
    }

    public int MaxSize
    {
        get
        {
            return gridSizeX * gridSizeY;
        }
    }

    private void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeY];
        Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x/2 - Vector3.forward * gridWorldSize.y/2;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 worldpoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
                bool walkable = !(Physics.CheckSphere(worldpoint, nodeSkin, unwalkableMask)) && (Physics.CheckSphere(worldpoint, nodeSkin, floorMask));
                int movementPenalty = 0;
                if (walkable)
                {
                    RaycastHit hit;
                    Ray ray = new Ray(worldpoint, Vector3.down);
                    if (Physics.SphereCast(ray,nodeSkin*2f,out hit, 1000, walkableMask)){
                        walkableRegionDict.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
                        movementPenalty = 0;
                    }
                    if((Physics.CheckSphere(worldpoint, nodeSkin*2f, unwalkableMask)))
                    {
                        movementPenalty = 0;
                    }
                }
                grid[x, y] = new Node(walkable, worldpoint, x, y, movementPenalty);
            }
        }
    }

    public List<Node> GetNeighbours(Node node)
    {
        List<Node> neighbours = new List<Node>();
        
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;
                int checkX = node.gridX + x;
                int checkY = node.gridY + y;

                if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }
        return neighbours;
    }

    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        float percentX = (worldPosition.x + (gridWorldSize.x / 2));
        float percentY = (worldPosition.z + (gridWorldSize.y / 2));
        percentX /= gridWorldSize.x;
        percentY /= gridWorldSize.y;

        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
        return grid[x, y];
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));


            if (grid != null && displayGridGizmos)
            {
                foreach (Node n in grid)
                {
                    Gizmos.color = (n.walkable) ? Color.white : Color.red;
                    Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter - .1f));
                }
            }
        
    }

    [System.Serializable]
    public class TerrainType
    {
        public LayerMask terrainMask;
        public int terrainPenalty;
    }
}
