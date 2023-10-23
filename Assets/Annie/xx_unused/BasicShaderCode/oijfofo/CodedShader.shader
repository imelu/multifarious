Shader "Unlit/CodedShader"
{
    SubShader{
        Tags{"RenderPipeline" = "UniversalPipeline"}
        Pass{
            Name "ForwardLit"
            Tags{"LightMode" = "UniversalForward"}

            HLSLPROGRAM
            #include "CodedShaderForwardLitPass.hlsl"
            ENDHLSL
            }
        }
}
