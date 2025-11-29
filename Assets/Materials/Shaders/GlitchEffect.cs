using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlitchEffect : MonoBehaviour {

	public Material glitchMaterial;
    [Range(0, 4)] public float glitchIntensity = 0.5f;
    public float waveSpeed = 10f;
    public float waveFrequency = 100f;
    public Color channelColor = Color.white;

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (glitchMaterial != null)
        {
            glitchMaterial.SetFloat("_GlitchIntensity", glitchIntensity);
            glitchMaterial.SetFloat("_WaveSpeed", waveSpeed);
            glitchMaterial.SetFloat("_WaveFrequency", waveFrequency);
            glitchMaterial.SetColor("_ChannelColor", channelColor);

            Graphics.Blit(src, dest, glitchMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}
