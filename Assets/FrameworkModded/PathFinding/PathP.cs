using UnityEngine;

public class PathP 
{

    public readonly Vector3[] lookPoints;
    public readonly int finishLineIndex;

    public PathP(Vector3[] waypoints, Vector3 startPos)
    {
        lookPoints = waypoints;
        finishLineIndex = waypoints.Length - 1;

        Vector2 previousPoint = V3ToV2(startPos);
    }

    Vector2 V3ToV2 (Vector3 v3)
    {
        return new Vector2(v3.x, v3.z);
    }
    public void DrawWithGizmos()
    {
        Gizmos.color = Color.cyan;
        foreach(Vector3 p in lookPoints)
        {
            Gizmos.DrawCube(p + Vector3.up, Vector3.one);
        }
        Gizmos.color = Color.black;
    }
}
