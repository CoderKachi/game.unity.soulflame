using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Graphic))]
public class ShadowComponent : BaseMeshEffect
{
    [SerializeField] private Color shadowColor = new Color(0, 0, 0, 0.5f);
    [SerializeField] private Vector2 offset = new Vector2(5f, -5f);

    public override void ModifyMesh(VertexHelper vertexHelper)
    {
        if (!IsActive() || vertexHelper.currentVertCount == 0)
            return;

        List<UIVertex> originalVertices = new();
        List<UIVertex> shadowVertices = new();
        vertexHelper.GetUIVertexStream(originalVertices);

        for (int i = 0; i < originalVertices.Count; i++)
        {
            UIVertex shadowVertex = originalVertices[i];
            shadowVertex.position += (Vector3) offset;
            shadowVertex.color = shadowColor;
            shadowVertices.Add(shadowVertex);
        }

        // Add new shadow vertices
        shadowVertices.AddRange(originalVertices);

        vertexHelper.Clear();
        vertexHelper.AddUIVertexTriangleStream(shadowVertices);
    }
}
