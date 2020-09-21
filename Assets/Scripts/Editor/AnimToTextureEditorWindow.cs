using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class AnimToTextureEditorWindow : EditorWindow
{
    private static readonly Vector2 WINDOW_SIZE = new Vector2(300, 400);

    private GameObject referenceAnimatedObject;
    private AnimationClip referenceAnimationClip;
    private int frameRate = 60;

    private Bounds animationSpaceBounds;
    private List<Mesh> frameMeshes;
    private Texture2D animationTexture;
    private Material animationMaterial;
    private Animator animator;

    [MenuItem("Window/Animation To Texture")]
    private static void CreateWindow()
    {
        // Get existing open window or if none, make a new one:
        var window = (AnimToTextureEditorWindow)EditorWindow.GetWindow(typeof(AnimToTextureEditorWindow));
        window.position = new Rect() { size = WINDOW_SIZE };
        window.minSize = WINDOW_SIZE;
        window.minSize = WINDOW_SIZE;
        window.titleContent = new GUIContent("Animation To Texture");
        window.Show();
    }

    private void OnGUI()
    {
        referenceAnimatedObject = (GameObject)EditorGUILayout.ObjectField("Reference Object", referenceAnimatedObject, typeof(GameObject), false);
        referenceAnimationClip = (AnimationClip)EditorGUILayout.ObjectField("Animation Clip", referenceAnimationClip, typeof(AnimationClip), false);
        frameRate = EditorGUILayout.IntField("Frame Rate", frameRate);
        if (GUILayout.Button("Bake to Texture"))
        {
            BakeAnimationTexture();
        }
    }

    private void BakeAnimationTexture()
    {
        BakeAnimationFrameMeshes();
        DetermineAnimationSpaceBounds();
        EncodeAnimationIntoTexture();
        SaveAnimationTexture();
        SaveAnimationMaterial();
    }

    private void BakeAnimationFrameMeshes()
    {
        GameObject animatedObject = Instantiate(referenceAnimatedObject);
        //animator = animatedObject.AddComponent<Animator>();
        //animator.runtimeAnimatorController = animatorController;
        frameMeshes = new List<Mesh>();
        AnimationMode.StartAnimationMode();
        AnimationMode.BeginSampling();
        var skinnedRenderer = animatedObject.GetComponentInChildren<SkinnedMeshRenderer>();
        AnimationMode.SampleAnimationClip(animatedObject, referenceAnimationClip, 0);
        var bakedMesh = new Mesh();
        skinnedRenderer.BakeMesh(bakedMesh);
        AssetDatabase.CreateAsset(bakedMesh, "Assets/BackedMesh.mesh");
        for (float t = 0.0f; t <= referenceAnimationClip.length; t += 1f / frameRate)
        {
            AnimationMode.SampleAnimationClip(animatedObject, referenceAnimationClip, t);
            skinnedRenderer.BakeMesh(bakedMesh);
            frameMeshes.Add(bakedMesh);
        }
        AssetDatabase.SaveAssets();
        AnimationMode.EndSampling();
        AnimationMode.StopAnimationMode();
        GameObject.DestroyImmediate(animatedObject);
        //SceneView.RepaintAll();
    }

    private void DetermineAnimationSpaceBounds()
    {
        animationSpaceBounds = new Bounds(Vector3.zero, Vector3.zero);
        for (int i = 0; i < frameMeshes.Count; ++i)
        {
            animationSpaceBounds.Encapsulate(frameMeshes[i].bounds);
        }
    }

    private void EncodeAnimationIntoTexture()
    {
        int texWidth = GetCeilPowerOfTwo(frameMeshes.Count);
        int texHeight = GetCeilPowerOfTwo(frameMeshes[0].vertexCount);
        animationTexture = new Texture2D(texWidth, texHeight);
        for (int i = 0; i < frameMeshes.Count; ++i)
        {
            List<Vector3> vertices = new List<Vector3>();
            frameMeshes[i].GetVertices(vertices);
            for (int j = 0; j < vertices.Count; ++j)
            {
                animationTexture.SetPixel(i, j, GetColorEncodedVertexPos(vertices[j]));
            }
        }
    }

    private void SaveAnimationTexture()
    {
        string defaultFileName = string.Format("{0}_{1}_anim_tex.png", referenceAnimatedObject.name, referenceAnimationClip.name);
        var savePath = EditorUtility.SaveFilePanelInProject("Save animation texture",  defaultFileName, "png", "Animation Texture Saved!");
        if (savePath.Length != 0)
        {
            var pngData = animationTexture.EncodeToPNG();
            if (pngData != null)
                File.WriteAllBytes(savePath, pngData);
            animationTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(savePath, typeof(Texture2D));
            AssetDatabase.Refresh();
        }
    }

    private void SaveAnimationMaterial()
    {
        animationMaterial = new Material(Shader.Find("ArmNomads Games/Texture Baked Animation"));
        animationMaterial.SetTexture("_AnimTex", animationTexture);
        animationMaterial.SetFloat("_AnimFrameCount", frameMeshes.Count);
        animationMaterial.SetFloat("_VertexCount", frameMeshes[0].vertexCount);
        animationMaterial.SetFloat("_FrameRate", frameRate);
        animationMaterial.SetVector("_AnimBounds", new Vector4(animationSpaceBounds.size.x,
                                                               animationSpaceBounds.size.y,
                                                               animationSpaceBounds.size.z,
                                                               0));
        string defaultFileName = string.Format("{0}_{1}_anim.mat", referenceAnimatedObject.name, referenceAnimationClip.name);
        var savePath = EditorUtility.SaveFilePanelInProject("Save animation material", defaultFileName, "mat", "Animation Material Saved!");
        if (savePath.Length != 0)
        {
            AssetDatabase.CreateAsset(animationMaterial, savePath);
            AssetDatabase.Refresh();
        }
    }

    private int GetCeilPowerOfTwo(int v)
    {
        return v;
        int ceil = 1;
        while (v > 0)
        {
            v = v >> 1;
            ceil = ceil << 1;
        }
        return ceil;
    }

    private Color GetColorEncodedVertexPos(Vector3 pos)
    {
        float x = ((pos.x / animationSpaceBounds.size.x) + 1.0f) / 2.0f;
        float y = ((pos.y / animationSpaceBounds.size.y) + 1.0f) / 2.0f;
        float z = ((pos.z / animationSpaceBounds.size.z) + 1.0f) / 2.0f;

        return new Color(x, y, z, 1f);
    }
}


