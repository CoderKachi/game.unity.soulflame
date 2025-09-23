using UnityEngine;
using System;

public class ScreenToPlaneComponent : MonoBehaviour
{
    [SerializeField] private LayerMask _planeMask;

    public void SetMask(GameObject plane)
    {
        if (plane == null)
        {
            return;
        }

        _planeMask = plane.layer;
    }

    public void SetMask(LayerMask setMask)
    {
        _planeMask = setMask;
    }

    public Vector3 ScreenToPlane(Vector2 screenPosition)
    {
        return ScreenToPlane(new Vector3(screenPosition.x, screenPosition.y, 0));
    }

    public Vector3 ScreenToPlane(Vector3 screenPosition)
    {
        Ray raycast = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit raycastResult;
        bool raycastCollided = Physics.Raycast(raycast, out raycastResult, Mathf.Infinity, _planeMask);

        if (raycastCollided == true)
        {
            return raycastResult.point;
        }
        else
        {
            return Vector3.zero;
        }
    }

    public Vector3 WorldToPlane(Vector3 worldPosition)
    {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        screenPosition = new Vector3(screenPosition.x, screenPosition.y, 0);

        return ScreenToPlane(screenPosition);
    }
}
