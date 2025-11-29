using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class RageText : MonoBehaviour
{
    private TMP_Text tmpText;
    private TMP_TextInfo textInfo;

    [Header("Rage Settings")]
    public float intensity = 2f;     // how far letters move
    public float speed = 30f;        // how fast the jitter changes
    public bool useUnscaledTime = true;

    void Awake()
    {
        tmpText = GetComponent<TMP_Text>();
    }

    void Update()
    {
        tmpText.ForceMeshUpdate();
        textInfo = tmpText.textInfo;

        int characterCount = textInfo.characterCount;
        if (characterCount == 0) return;

        float t = useUnscaledTime ? Time.unscaledTime : Time.time;

        for (int i = 0; i < characterCount; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;

            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            int meshIndex = textInfo.characterInfo[i].materialReferenceIndex;

            Vector3[] vertices = textInfo.meshInfo[meshIndex].vertices;

            // Random jitter based on sine/cosine for smoother rage shake
            float offsetX = (Mathf.PerlinNoise(i, t * speed) - 0.5f) * intensity;
            float offsetY = (Mathf.PerlinNoise(i + 100, t * speed) - 0.5f) * intensity;

            Vector3 offset = new Vector3(offsetX, offsetY, 0);

            vertices[vertexIndex + 0] += offset;
            vertices[vertexIndex + 1] += offset;
            vertices[vertexIndex + 2] += offset;
            vertices[vertexIndex + 3] += offset;
        }

        // Apply updated mesh
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            TMP_MeshInfo meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            tmpText.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}
