using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PencilEffect : MonoBehaviour
{
    public Shader pencilShader;
    private Material pencilMat;

    [Header("Pencil Settings")]
    public Color lineColor = Color.black;
    [Range(50f, 500f)] public float lineDensity = 150f;
    [Range(0.1f, 3f)] public float lineThickness = 1f;
    [Range(0f, 180f)] public float lineAngle = 45f;
    [Range(0f, 1f)] public float strength = 1f;
    public Vector3 mainLightDir = new Vector3(0, -1, 0);

    void Start()
    {
        if (pencilShader == null)
            pencilShader = Shader.Find("Hidden/PencilShadedOnly");

        if (pencilShader != null)
            pencilMat = new Material(pencilShader);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (pencilMat == null)
        {
            Graphics.Blit(src, dest);
            return;
        }

        pencilMat.SetColor("_LineColor", lineColor);
        pencilMat.SetFloat("_LineDensity", lineDensity);
        pencilMat.SetFloat("_LineThickness", lineThickness);
        pencilMat.SetFloat("_LineAngle", lineAngle);
        pencilMat.SetFloat("_Strength", strength);
        pencilMat.SetVector("_LightDir", mainLightDir.normalized);

        Graphics.Blit(src, dest, pencilMat);
    }
}
