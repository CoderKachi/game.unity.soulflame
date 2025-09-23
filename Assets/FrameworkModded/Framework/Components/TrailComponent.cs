using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;


[RequireComponent(typeof(LineRenderer))]
public class TrailComponent : MonoBehaviour
{
    [HideInInspector] public LineRenderer lineRenderer;
    int maxSegments = 1000;
    float pointLifeSpan = 0.1f;
    
    List<linePoint> points = new List<linePoint>();
    
    
    public struct linePoint
    {
        public Vector3 pos;
        public float creationTime;
    }

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.sortingOrder = 30;
        pointLifeSpan = 0.1f;
        //maxSegments *= 3;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        List<linePoint> lines = new();
        for (int i = 0; i<points.Count; i++)
        {
            float time = Time.time - points[i].creationTime;            
            if (time < pointLifeSpan)
            {
                lines.Add(points[i]);
            }
        }
        points = lines;
        UpdateLines();

    }
    public void AppendLineVertex(Vector3 pos)
    {
        linePoint line;
        line.pos = pos;
        line.creationTime = Time.time;
        points.Insert(0, line);
        //points.Insert(0, line);
        //points.Insert(0, line);

        if (points.Count > maxSegments)
        {
            for (int i = points.Count-1; i > maxSegments-1;i--) {
                points.RemoveAt(i);
            }
        }
        UpdateLines();
        
        
    }
    public void ClearLines()
    {
        lineRenderer.positionCount = 0;
        points.Clear();
    }
    public void SetMaterial(Material material)
    {
        lineRenderer.material = material;
    }
    public bool IsLineless()
    {
        if (lineRenderer.positionCount > 0)
        {
            return false;
        }
        return true;
    }
    public void UpdateLines()
    {
        lineRenderer.positionCount = points.Count;
        Vector3[] positions = new Vector3[points.Count];
        for (int i = 0; i< points.Count; i++)
        {
            positions[i] = points[i].pos;
        }
        lineRenderer.SetPositions(positions);
    }

}
