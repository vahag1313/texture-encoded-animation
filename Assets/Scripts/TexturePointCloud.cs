using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexturePointCloud : MonoBehaviour
{
    [SerializeField]
    public Texture2D tex;
    [SerializeField]
    public Vector3 bounds;
    [SerializeField]
    public Mesh mesh;
    [SerializeField]
    public float handleSize;

    [SerializeField]
    public bool textureData;

}
