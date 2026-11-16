Shader "EraserWars/EraserToon"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _Color("Color", Color) = (1, 1, 1, 1)
        [HDR] _EmissionColor("Emission", Color) = (0, 0, 0, 1)
        _Shade("Shade", Range(0, 1)) = 0.58
        _OutlineWidth("Outline Width", Range(0, 0.01)) = 0.0012
        _OutlineColor("Outline Color", Color) = (0.09, 0.06, 0.08, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex OutlineVertex
            #pragma fragment OutlineFragment
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _Color;
                float4 _EmissionColor;
                float4 _OutlineColor;
                float _Shade;
                float _OutlineWidth;
            CBUFFER_END

            struct OutlineAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct OutlineVaryings
            {
                float4 positionCS : SV_POSITION;
            };

            OutlineVaryings OutlineVertex(OutlineAttributes input)
            {
                OutlineVaryings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float lenSq = dot(normalWS, normalWS);
                if (lenSq > 1e-8)
                {
                    positionWS += normalWS * (_OutlineWidth * rsqrt(lenSq));
                }

                output.positionCS = TransformWorldToHClip(positionWS);
                return output;
            }

            half4 OutlineFragment(OutlineVaryings input) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex ToonVertex
            #pragma fragment ToonFragment
            #pragma multi_compile_local _ _EMISSION
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _Color;
                float4 _EmissionColor;
                float4 _OutlineColor;
                float _Shade;
                float _OutlineWidth;
            CBUFFER_END

            struct ToonAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct ToonVaryings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
            };

            ToonVaryings ToonVertex(ToonAttributes input)
            {
                ToonVaryings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 ToonFragment(ToonVaryings input) : SV_Target
            {
                Light mainLight = GetMainLight();
                float3 normalWS = normalize(input.normalWS);
                half ndotl = saturate(dot(normalWS, mainLight.direction));
                half hasLight = step(0.001h, mainLight.color.r + mainLight.color.g + mainLight.color.b);
                half lit = lerp(1.0h, step(0.35h, ndotl), hasLight);
                half3 color = _BaseColor.rgb * lerp(_Shade, 1.0h, lit);
                #if defined(_EMISSION)
                color += _EmissionColor.rgb;
                #endif
                return half4(color, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _Color;
                float4 _EmissionColor;
                float4 _OutlineColor;
                float _Shade;
                float _OutlineWidth;
            CBUFFER_END

            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }

    FallBack Off
}
