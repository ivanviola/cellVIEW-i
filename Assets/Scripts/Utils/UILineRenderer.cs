using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class UILineRenderer : Graphic
{
    public float LineThikness = 2;
    public bool UseMargins;
    public Vector2 Margin;
    
    public float Decay;
    public float Gamma;
    public int CurveResolution;

    protected override void OnPopulateMesh(Mesh m)
    {
        var Points = new List<Vector2>();

        if (Decay < 0.0f)
            Decay = 0.0f;
        if (Decay > 1.0f)
            Decay = 1.0f;

        if (Gamma < 0.1f)
            Gamma = 0.1f;
        if (Gamma > 10.0f)
            Gamma = 10.0f;
        
        for (int i = 0; i < CurveResolution; i++)
        {
            float zeroOne = (float) i/(float) (CurveResolution - 1);
            float v = Mathf.Pow(zeroOne, Gamma) * Decay;
            Points.Add(new Vector2((float) i / (float) (CurveResolution - 1), 1.0f - v));
        }
        
        var sizeX = rectTransform.rect.width;
        var sizeY = rectTransform.rect.height;
        var offsetX = -rectTransform.pivot.x * rectTransform.rect.width;
        var offsetY = -rectTransform.pivot.y * rectTransform.rect.height;

        if (UseMargins)
        {
            sizeX -= Margin.x;
            sizeY -= Margin.y;
            offsetX += Margin.x / 2f;
            offsetY += Margin.y / 2f;
        }

        var vh = new VertexHelper();

        for (int i = 0; i < Points.Count-1; i++)
        {
            var prev = Points[i];
            var cur = Points[i+1];
            prev = new Vector2(prev.x * sizeX + offsetX, prev.y * sizeY + offsetY);
            cur = new Vector2(cur.x * sizeX + offsetX, cur.y * sizeY + offsetY);

            var normal = new Vector3(cur.x - prev.x, cur.y - prev.y);
            var perp_vector = Vector3.Cross(normal, Vector3.forward).normalized;

            var halfThikness = LineThikness/2;

            var v1 = prev + new Vector2(perp_vector.x * -halfThikness, perp_vector.y * -halfThikness);
            var v2 = prev + new Vector2(perp_vector.x * halfThikness, perp_vector.y * halfThikness);
            var v3 = cur + new Vector2(perp_vector.x * halfThikness, perp_vector.y * halfThikness);
            var v4 = cur + new Vector2(perp_vector.x * -halfThikness, perp_vector.y * -halfThikness);

            vh.AddVert(v1, color, new Vector2(0f, 0f));
            vh.AddVert(v2, color, new Vector2(0f, 1f));
            vh.AddVert(v3, color, new Vector2(1f, 1f));
            vh.AddVert(v4, color, new Vector2(1f, 0f));

            vh.AddTriangle(0 + i*4, 1 + i * 4, 2 + i * 4);
            vh.AddTriangle(2 + i * 4, 3 + i * 4, 0 + i * 4);
            
        }

        vh.FillMesh(m);
    }
}