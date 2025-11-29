Shader "Custom/Shader" {
	Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GlitchIntensity ("Glitch Intensity", Range(0,4)) = 0.5
        _WaveSpeed ("Wave Speed", Range(0, 50)) = 10
        _WaveFrequency ("Wave Frequency", Range(10, 300)) = 100
        _ChannelColor ("Glitch Tint Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _GlitchIntensity;
            float _WaveSpeed;
            float _WaveFrequency;
            float4 _ChannelColor;

            float4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;

                // Horizontal wave distortion
                float offset = sin(uv.y * _WaveFrequency + _Time.y * _WaveSpeed) * 0.01 * _GlitchIntensity;
                uv.x += offset;

                // Sample once and tint
                float4 texColor = tex2D(_MainTex, uv);
                return texColor * _ChannelColor;
            }
            ENDCG
        }
    }
}
