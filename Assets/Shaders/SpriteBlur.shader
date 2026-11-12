Shader "Custom/SpriteBlur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BlurSize ("Blur Size", Range(0, 0.05)) = 0.015
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    struct appdata_t
    {
        float4 vertex   : POSITION;
        float4 color    : COLOR;
        float2 texcoord : TEXCOORD0;
    };

    struct v2f
    {
        float4 vertex   : SV_POSITION;
        fixed4 color    : COLOR;
        float2 texcoord : TEXCOORD0;
    };

    fixed4 _Color;
    fixed4 _RendererColor;
    sampler2D _MainTex;
    float _BlurSize;

    v2f vert(appdata_t IN)
    {
        v2f OUT;
        OUT.vertex = UnityObjectToClipPos(IN.vertex);
        OUT.texcoord = IN.texcoord;
        OUT.color = IN.color * _Color;
        return OUT;
    }

    fixed4 SampleSpriteBlur(float2 uv, float b, fixed4 vertexColor)
    {
        // Alpha-weighted Gaussian blur so transparent (0,0,0,0) pixels never darken colors
        float2 offsets[13] = {
            float2(0, 0),
            float2(b, 0), float2(-b, 0), float2(0, b), float2(0, -b),
            float2(b * 0.707, b * 0.707), float2(-b * 0.707, b * 0.707),
            float2(b * 0.707, -b * 0.707), float2(-b * 0.707, -b * 0.707),
            float2(b * 1.6, 0), float2(-b * 1.6, 0), float2(0, b * 1.6), float2(0, -b * 1.6)
        };
        float weights[13] = {
            4.0,
            2.0, 2.0, 2.0, 2.0,
            1.5, 1.5, 1.5, 1.5,
            1.0, 1.0, 1.0, 1.0
        };

        float3 rgbSum = float3(0, 0, 0);
        float alphaWeightSum = 0.0;
        float alphaSum = 0.0;
        float totalWeight = 0.0;

        for (int i = 0; i < 13; i++)
        {
            fixed4 s = tex2D(_MainTex, clamp(uv + offsets[i], 0.0, 1.0));
            float w = weights[i];
            rgbSum += s.rgb * s.a * w;
            alphaWeightSum += s.a * w;
            alphaSum += s.a * w;
            totalWeight += w;
        }

        float outAlpha = alphaSum / totalWeight;
        float3 outRgb = (alphaWeightSum > 0.0001) ? (rgbSum / alphaWeightSum) : float3(0, 0, 0);

        fixed4 finalCol = fixed4(outRgb, outAlpha) * vertexColor;
        return finalCol;
    }

    fixed4 frag(v2f IN) : SV_Target
    {
        return SampleSpriteBlur(IN.texcoord, _BlurSize, IN.color);
    }
    ENDCG

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "SpriteBlurUniversal2D"
            Tags { "LightMode"="Universal2D" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            ENDCG
        }

        Pass
        {
            Name "SpriteBlurSRPDefaultUnlit"
            Tags { "LightMode"="SRPDefaultUnlit" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            ENDCG
        }
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            ENDCG
        }
    }
}
