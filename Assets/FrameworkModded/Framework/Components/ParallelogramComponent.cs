using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer), typeof(RectTransform))]
public class ParallelogramComponent : Graphic
{
    [SerializeField] private float wingWidthOverride = 0f;

    // Need some sort of anti alias?

    protected override void OnPopulateMesh(VertexHelper vertexHelper)
    {
        vertexHelper.Clear();

        Rect rect = rectTransform.rect;
        float rectWidth = rect.width;
        float rectHeight = rect.height;
        float rectTop = rectHeight * 0.5f;
        float rectBottom = -rectTop;
        float rectRight = rectWidth * 0.5f;
        float rectLeft = -rectRight;

        /* 
            Parallelogram Structure
            [Left Wing] [Body] [Right Wing]
            [Body].width = [rect].width - ([Left Wing].width * 2)
        */

        float wingWidth = wingWidthOverride > 0f ? wingWidthOverride : rectHeight;
        float bodyWidth = MathF.Max(0, rectWidth - (wingWidth * 2)); // Add safety check later

        Vector2[] vertices = new Vector2[]
        {
            // Left Wing
            new Vector2(rectLeft + wingWidth, rectTop),                                      // Top-right    (Triangle) (0)
            new Vector2(rectLeft, rectBottom),                                               // Bottom-left  (Triangle) (1)
            new Vector2(MathF.Min(rectLeft + wingWidth, rectRight - wingWidth), rectBottom), // Bottom-right (Triangle) (2)

            // Center Rectangle
            new Vector2(-bodyWidth/2, rectTop),  // Top-left     (Rectangle) (3)
            new Vector2(bodyWidth/2, rectTop),   // Top-right    (Rectangle) (4)
            new Vector2(-bodyWidth/2, -rectTop), // Bottom-left  (Rectangle) (5)
            new Vector2(bodyWidth/2, -rectTop),  // Bottom-right (Rectangle) (6)

            // Right Wing
            new Vector2(MathF.Max(rectRight - wingWidth, rectLeft + wingWidth), rectTop),    // Top-left    (Triangle) (7)
            new Vector2(rectRight, rectTop),                                                 // Top-right   (Triangle) (8)
            new Vector2(rectRight - wingWidth, rectBottom),                                  // Bottom-left (Triangle) (9)
        };

        for (int i = 0; i < vertices.Length; i++)
        {
            vertexHelper.AddVert(vertices[i], color, Vector2.zero);
        }

        vertexHelper.AddTriangle(0, 1, 2);

        vertexHelper.AddTriangle(3, 4, 5);
        vertexHelper.AddTriangle(4, 5, 6);

        vertexHelper.AddTriangle(7, 8, 9);
    }
}
