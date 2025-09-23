using System.Collections.Generic;
using UnityEngine;
using static Framework;

[RequireComponent(typeof(MeshCollider))]
[ExecuteInEditMode]
public class CameraBasedColliderComponent : MonoBehaviour
{
    public enum Corner
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
    }

    [SerializeField] private bool debug;
    public Camera targetCamera;

    public float colliderWidth = 1f;
    public float colliderHeight = 1f;
    public float colliderLength = 4f;
    public Vector3 colliderOffset = Vector3.zero;

    private Vector2 screenCenter;
    private float screenHeight;
    private MeshCollider meshCollider;
    private Mesh cachedMesh;
    private Vector3 cachedExtrusionDirection;

    void Awake()
    {
        meshCollider = gameObject.GetComponent<MeshCollider>();
    }

    void LateUpdate()
    {
        if (cachedExtrusionDirection != GetExtrusionDirection())
        {
            UpdateCollider();
        }
    }

    private void UpdateCollider()
    {
        Vector3 fTopLeft = GetCorner(Corner.TopLeft) + (GetExtrusionDirection() * colliderLength);
        Vector3 fTopRight = GetCorner(Corner.TopRight) + (GetExtrusionDirection() * colliderLength);
        Vector3 fBottomLeft = GetCorner(Corner.BottomLeft) + (GetExtrusionDirection() * colliderLength);
        Vector3 fBottomRight = GetCorner(Corner.BottomRight) + (GetExtrusionDirection() * colliderLength);

        Vector3 bTopLeft = GetCorner(Corner.TopLeft) + (GetExtrusionDirection() * -colliderLength);
        Vector3 bTopRight = GetCorner(Corner.TopRight) + (GetExtrusionDirection() * -colliderLength);
        Vector3 bBottomLeft = GetCorner(Corner.BottomLeft) + (GetExtrusionDirection() * -colliderLength);
        Vector3 bBottomRight = GetCorner(Corner.BottomRight) + (GetExtrusionDirection() * -colliderLength);

        cachedMesh = new Mesh();

        // Vertex order
        Vector3[] vertices = new Vector3[8]
        {
            transform.InverseTransformPoint(fTopLeft),
            transform.InverseTransformPoint(fTopRight),
            transform.InverseTransformPoint(fBottomLeft),
            transform.InverseTransformPoint(fBottomRight),

            transform.InverseTransformPoint(bTopLeft),
            transform.InverseTransformPoint(bTopRight),
            transform.InverseTransformPoint(bBottomLeft),
            transform.InverseTransformPoint(bBottomRight)
        };

        int[] triangles = new int[]
        {
            // Front
            2, 1, 0,
            3, 1, 2,

            // Back
            4, 5, 6,
            6, 5, 7,

            // Left
            6, 0, 4,
            2, 0, 6,

            // Right
            3, 5, 1,
            7, 5, 3,

            // Top
            0, 5, 4,
            1, 5, 0,

            // Bottom
            6, 3, 2,
            7, 3, 6
        };

        cachedMesh.vertices = vertices;
        cachedMesh.triangles = triangles;
        cachedMesh.RecalculateNormals();

        meshCollider.sharedMesh = cachedMesh;
    }

    private Vector3 GetCorner(Corner corner)
    {
        Vector3 leftPosition = new Vector3(-colliderWidth * 0.5f, 0, 0) + colliderOffset;
        Vector3 rightPosition = new Vector3(colliderWidth * 0.5f, 0, 0) + colliderOffset;
        Vector3 topPosition = new Vector3(0, colliderHeight * 0.5f, 0) + colliderOffset;
        Vector3 bottomPosition = new Vector3(0, -colliderHeight * 0.5f, 0) + colliderOffset;

        switch (corner)
        {
            case Corner.TopLeft:
                return transform.position + (transform.right * leftPosition.x) + (transform.up * topPosition.y) + (transform.forward * leftPosition.z);
            case Corner.TopRight:
                return transform.position + (transform.right * rightPosition.x) + (transform.up * topPosition.y) + (transform.forward * rightPosition.z);
            case Corner.BottomLeft:
                return transform.position + (transform.right * leftPosition.x) + (transform.up * bottomPosition.y) + (transform.forward * leftPosition.z);
            case Corner.BottomRight:
                return transform.position + (transform.right * rightPosition.x) + (transform.up * bottomPosition.y) + (transform.forward * rightPosition.z);
            default:
                return Vector3.zero;
        }
    }

    private Vector3 GetCenter()
    {
        Vector3 originPosition = transform.position + (transform.right * colliderOffset.x) + (transform.up * colliderOffset.y) + (transform.forward * colliderOffset.z);
        return originPosition;
    }

    private Vector3 GetExtrusionDirection()
    {
        return ResolveCamera().transform.forward.normalized;
    }

    private Camera ResolveCamera()
    {
        if (targetCamera != null)
        {
            return targetCamera;
        }
        else
        {
            return Camera.main;
        }
    }

    void OnDrawGizmos()
    {
        if (!debug) return;
        
        Camera camera = ResolveCamera();
        if (camera == null) return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(GetCenter(), 0.025f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(GetCorner(Corner.TopLeft), 0.05f);
        Gizmos.DrawSphere(GetCorner(Corner.TopRight), 0.05f);
        Gizmos.DrawSphere(GetCorner(Corner.BottomLeft), 0.05f);
        Gizmos.DrawSphere(GetCorner(Corner.BottomRight), 0.05f);

        // Projection
        // Forward
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(GetCorner(Corner.TopLeft) + (GetExtrusionDirection() * colliderLength), 0.025f);
        Gizmos.DrawSphere(GetCorner(Corner.TopRight) + (GetExtrusionDirection() * colliderLength), 0.025f);
        Gizmos.DrawSphere(GetCorner(Corner.BottomLeft) + (GetExtrusionDirection() * colliderLength), 0.025f);
        Gizmos.DrawSphere(GetCorner(Corner.BottomRight) + (GetExtrusionDirection() * colliderLength), 0.025f);

        // Backward
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(GetCorner(Corner.TopLeft) + (GetExtrusionDirection() * -colliderLength), 0.025f);
        Gizmos.DrawSphere(GetCorner(Corner.TopRight) + (GetExtrusionDirection() * -colliderLength), 0.025f);
        Gizmos.DrawSphere(GetCorner(Corner.BottomLeft) + (GetExtrusionDirection() * -colliderLength), 0.025f);
        Gizmos.DrawSphere(GetCorner(Corner.BottomRight) + (GetExtrusionDirection() * -colliderLength), 0.025f);
    }

    void OnGUI()
    {

    }
}