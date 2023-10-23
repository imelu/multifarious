
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

struct Attributes{
	float3 positionOS : POSITION;
	}

	void Vertex(Attributes input)
	{
		VertexPositionInputs posnInputs = GetVertexPositionInputs(input.positionOS);
		float4 positionCS = posnInputs.positionCS;
	}