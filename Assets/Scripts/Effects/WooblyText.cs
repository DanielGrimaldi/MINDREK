using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WooblyText : MonoBehaviour
{
    TMP_Text textMesh;
    Mesh mesh;
    Vector3[] vertices;

    public float wobbleAmount = 0.1f;
    public float wobbleSpeed = 2f;

    void Awake()
    {
        textMesh = GetComponent<TMP_Text>();
    }

    void Update()
    {
        textMesh.ForceMeshUpdate();
        mesh = textMesh.mesh;
        vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 offset = Wobble(vertices[i], i);
            vertices[i] += offset;
        }

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
        textMesh.UpdateGeometry(mesh, 0);
    }

    Vector3 Wobble(Vector3 vertex, int index)
    {
        float wobbleX = Mathf.Sin(Time.unscaledTime * wobbleSpeed + index) * wobbleAmount;
        float wobbleY = Mathf.Cos(Time.unscaledTime * wobbleSpeed + index) * wobbleAmount;
        return new Vector3(wobbleX, wobbleY, 0);
    }
}
