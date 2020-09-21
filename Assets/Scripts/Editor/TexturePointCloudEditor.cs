using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TexturePointCloud))]
public class TexturePointCloudEditor : Editor
{
    private TexturePointCloud tpc;

    private void OnEnable()
    {
        tpc = target as TexturePointCloud;
    }

    private void OnSceneGUI()
    {
        if(tpc.textureData)
        {
            for (int i = 0; i < tpc.tex.height; i++)
            {
                Color c = tpc.tex.GetPixel(0, i);
                Vector3 pos = new Vector3((c.r * 2.0f - 1.0f) * tpc.bounds.x, (c.g * 2.0f - 1.0f) * tpc.bounds.y, (c.b * 2.0f - 1.0f) * tpc.bounds.z);
                Handles.Label(pos, i.ToString());
            }
            return;
        }
        for (int i = 0; i < tpc.mesh.vertexCount; i++)
        {
            var pos = tpc.mesh.vertices[i];
            Handles.Label(pos, i.ToString());
        }
    }
}
