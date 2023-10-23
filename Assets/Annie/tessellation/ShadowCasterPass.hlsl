#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/lighting.hlsl"

	//fix shadow acne---------------------------------------------------------------------------------------
	float _LightDirection;

	float4 GetShadowCasterPositionCS(float3 positionWS, float3 normalWS)
	{
		float3 lightDirectionWS = _LightDirection;
		float4 vertexCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
	#if UNITY_REVERSED_Z
		vertexCS.z = min(vertexCS.z, UNITY_NEAR_CLIP_VALUE);
	#else
		vertexCS.z = max(vertexCS.z, UNITY_NEAR_CLIP_VALUE);
	#endif
		return vertexCS;
	}
	//END fix shadow acne------------------------------------------------------------------------------------

	struct Attributes
	{
		float4 vertex : POSITION;
		float3 normalOS : NORMAL;
	};

	struct Varyings //interpolator 
	{
		float4 vertexCS : SV_POSITION;
		//float3 normalCS : NORMAL; //remove later if not needed  
	};

	Varyings vert(Attributes input)
	{
		Varyings output;

		VertexPositionInputs posnInputs = GetVertexPositionInputs(input.vertex.xyz);
		//fix shadow acne 
		VertexNormalInputs normInputs = GetVertexNormalInputs(input.normalOS);
		output.vertexCS = GetShadowCasterPositionCS(posnInputs.positionWS, normInputs.normalWS);

		return output;
	}

	half4 frag(Varyings IN) : SV_Target
	{
		return 0;
	}