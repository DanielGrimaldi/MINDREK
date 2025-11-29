Shader "Custom/ShockWave" {
	Properties
    {
        _DistortionStrength ("Distortion Strength", Range(0, 1)) = 0.1
        _Opacity ("Opacity", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        GrabPass { "_GrabTexture" } // Dichiarazione esplicita del GrabPass

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float2 grabUV : TEXCOORD2;
            };

            float _DistortionStrength;
            float _Opacity;
            sampler2D _GrabTexture; // Dichiarazione esplicita del GrabTexture

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                // Calcolo delle coordinate per il GrabPass
                float4 screenPos = ComputeScreenPos(o.pos);
                o.grabUV = screenPos.xy / screenPos.w;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Normale e vettore di visuale
                float3 worldNormal = normalize(i.worldNormal);
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);

                // Calcolo della distorsione
                float2 distortion = worldNormal.xy * _DistortionStrength;
                float2 distortedUV = i.grabUV + distortion;

                // Cattura il colore dallo schermo usando GrabPass
                fixed4 screenColor = tex2D(_GrabTexture, distortedUV);

                // Applicazione della trasparenza
                screenColor.a *= _Opacity;

                return screenColor;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}
