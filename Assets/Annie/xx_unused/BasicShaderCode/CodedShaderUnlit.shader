Shader "Unlit/CodedShaderUnlit"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _AlphaMask ("Alpha Mask", 2D) = "white" {}
        _TintColor("Tint", Color) = (1,1,1,1)
        _TintEmissionStrength("Tint Emission strength", Float) = 1
        _Transparency("Transparency", Float) = 1
        _CutoutThresh("Cutout Threshold", Range(0.0,1.0)) = 0.2
        _Distance("Distance", Float) = 1
        _Amplitude("Amplitude", Float) = 1
        _Speed("Speed", Float) = 1
        _Amount("Amount", Float) = 1
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha 

        Pass
        {
            CGPROGRAM
            //pre-processor directives
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                //vertex passed on in packed array > 4 floating point numbers > x,y,z,w
                float4 vertex : POSITION; //semantic binding > specifying that it's a position > coordinates in it's local space
                float2 uv : TEXCOORD0;
            };

            struct v2f //vertex to fragment 
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION; //semantic binding > specifying that it's a screen space position
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _AlphaMask;
            float4 _AlphaMask_ST;

            //Color
            float4 _TintColor;
            float _TintEmissionStrength;
            float _Transparency;
            float _CutoutThresh;
            float _Distance;
            float _Amplitude;
            float _Speed;
            float _Amount;

            v2f vert (appdata v) // vertex function > passing appdata into vertex function
            {
                v2f o;
                v.vertex.x += sin(_Time.y * _Speed + v.vertex.y * _Amplitude) * _Distance * _Amount; //still in object space> relative to object
                o.vertex = UnityObjectToClipPos(v.vertex); // v.vertex coordinates in model's local space > make screenspace
                o.uv = TRANSFORM_TEX(v.uv, _MainTex); // uv info from model and data from texture > where tiling and offset parameters get applied
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;  // return the struct
            }

            fixed4 frag (v2f i) : SV_Target // fragment function > takes vertex to fragment struct called "i" > SV_Target is render target, out put is a render target -> frame buffer for the screen
            {
                // sample the texture 
                fixed4 alphaMaskCol = tex2D(_AlphaMask, i.uv);
                _Transparency = alphaMaskCol.a;
                fixed4 col = tex2D(_MainTex, i.uv) * _TintColor * _TintEmissionStrength; // rgba > fixed4 can also be x,y,z,w / read in texture and the uv from v2f struct
                col.a = _Transparency;
                clip(col.r - _CutoutThresh);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
     
                return col;
            }
            ENDCG
        }
    }
}
