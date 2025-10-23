Shader "Custom/AfterImageWorldReflection"
{
    Properties
    {
        _MainColor ("Base Color", Color) = (1,1,1,1)
        _MainTexture ("Base Texture", 2D) = "white" {}
        _SecondMap ("Normal Map", 2D) = "bump" {}
        _SecondMapScale ("Normal Scale", Range(0,2)) = 1.0
        _ReflectionPower ("Reflection Strength", Range(0,5)) = 2.5
        _FresnelPow  ("Fresnel Power", Range(1, 10)) = 5
        _Mettalic ("Mettalic", Range(0,1)) = 0.5
        _Smoothness ("Smoothness", Range(0,1)) = 0.8 
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "RenderPipeline"="UniversalRenderPipeline" }
        LOD 200

        Pass
        {
            Name "Unlit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS_
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"

            TEXTURE2D(_MainTexture);
            SAMPLER(sampler_MainTexture);
            TEXTURE2D(_SecondMap);
            SAMPLER(sampler_SecondMap);

            float4 _MainColor;
            half _SecondMapScale;
            half _FresnelPow;
            half _ReflectionPower;
            half _Mettalic;
            half _Smoothness;



            // ===== Attributes / Varyings =====
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 tangentWS   : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
                float2 uv          : TEXCOORD4;
                float FogFactor    : TEXCOORD5;
            };

            // Vertex
            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 nrmWS = TransformObjectToWorldNormal(IN.normalOS);

                OUT.positionWS  = posWS;
                OUT.normalWS    = nrmWS;
                OUT.positionHCS = TransformWorldToHClip(posWS);
                OUT.tangentWS = normalize(TransformObjectToWorldDir(IN.tangentOS.xyz));
                OUT.bitangentWS = cross(OUT.normalWS, OUT.tangentWS) * IN.tangentOS.w;
                OUT.uv = IN.uv;
                OUT.FogFactor = ComputeFogFactor(OUT.positionHCS.z);
                return OUT;
            }

            // ===== Fragment =====
            half4 frag (Varyings IN) : SV_Target
            {
                // Base layer (optional)
                half4 baseCol = SAMPLE_TEXTURE2D(_MainTexture, sampler_MainTexture, IN.uv) * _MainColor;
                half3 tangentCol = UnpackNormalScale(SAMPLE_TEXTURE2D(_SecondMap, sampler_SecondMap, IN.uv), _SecondMapScale);
                
                // View direction (WS)
                float3 ViewDirWS = normalize(GetCameraPositionWS() - IN.positionWS);

                // TBN & World Normal (WS)
                float3x3 TBN = float3x3(normalize(IN.tangentWS), normalize(IN.bitangentWS), normalize(IN.normalWS));
                float3 N = SafeNormalize(mul(IN.normalWS, TBN));

                // Reflection vector (WS)
                float3 R = reflect( -1 * ViewDirWS, N);

                // Fresnel
                float fresnel = pow(1.0 - saturate(dot(ViewDirWS, N)), _FresnelPow);

                //SkyBox / Reflection Probe
                half3 reflectionColor = GlossyEnvironmentReflection(R, _Smoothness, N);
                reflectionColor *= _ReflectionPower;

                //Lighting
                Light mainLight = GetMainLight();
                half3 LightDir = normalize(mainLight.direction);
                half3 LightColor = mainLight.color;
                half NdotL = saturate(dot(N, LightDir));
                half3 diffuse = NdotL * LightColor; 

                // Blend env over base (unlit)
                half3 finalCol = baseCol.rgb * diffuse + reflectionColor * fresnel;
                finalCol = MixFog(finalCol, IN.FogFactor);
                return half4(finalCol, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
