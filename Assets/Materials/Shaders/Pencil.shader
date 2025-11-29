Shader "Custom/Pencil"
{
    Properties
    {
        _LineColor("Pencil Line Color", Color) = (0,0,0,1)
        _LineDensity("Line Density", Float) = 150
        _LineThickness("Line Thickness", Range(0.1,3)) = 1
        _LineAngle("Line Angle", Range(0,180)) = 45
        _Strength("Effect Strength", Range(0,1)) = 1
        _LightDir("Main Light Direction", Vector) = (0,-1,0)
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _LineColor;
            float _LineDensity;
            float _LineThickness;
            float _LineAngle;
            float _Strength;
            float3 _LightDir;

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; float3 normal : TEXCOORD1; };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = normalize(UnityObjectToWorldNormal(v.normal));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // Calculate light intensity on this pixel
                float ndotl = saturate(dot(normalize(_LightDir), i.normal));

                // If facing away from light, apply pencil effect
                float shadowFactor = 1.0 - ndotl; // 0 = lit, 1 = fully dark

                if (shadowFactor > 0.01)
                {
                    // Rotate UVs for pencil stripes
                    float rad = radians(_LineAngle);
                    float2x2 rot = float2x2(cos(rad), -sin(rad), sin(rad), cos(rad));
                    float2 uv = mul(rot, i.uv * _LineDensity);

                    // Procedural stripes
                    float lineVal = fmod(uv.y, _LineThickness*2.0) < _LineThickness ? 0.0 : 1.0;

                    float hatchBlend = lerp(1.0, lineVal, shadowFactor * _Strength);

                    col.rgb = lerp(col.rgb, _LineColor.rgb, 1.0 - hatchBlend);
                }

                return col;
            }
            ENDCG
        }
    }
}
