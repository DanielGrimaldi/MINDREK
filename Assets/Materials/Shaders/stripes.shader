Shader "Custom/stripes" {
	Properties
    {
        _StripeCount ("Stripe Count", Range(1, 200)) = 20
        _StripeSpeed ("Stripe Speed", Range(-10, 10)) = 1
        _StripeColor ("Stripe Color", Color) = (1,1,1,1)
        _UnscaledTime ("Unscaled Time", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _StripeCount;
            float _StripeSpeed;
            float4 _StripeColor;
            float _UnscaledTime;

            float4 frag(v2f_img i) : SV_Target
            {
                float2 uv = i.uv;

                // Diagonal movement (uv.x + uv.y creates 45° stripes)
                float stripeValue = (uv.x + uv.y + _UnscaledTime * _StripeSpeed) * _StripeCount;

                // Convert to 0–1 repeating pattern
                stripeValue = frac(stripeValue);

                // Alternating stripes: transparent when < 0.5
                float alpha = step(0.5, stripeValue);  // 0 = transparent, 1 = visible

                return float4(_StripeColor.rgb, _StripeColor.a * alpha);
            }
            ENDCG
        }
    }
}
