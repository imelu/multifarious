
#if defined(SHADER_API_D3D11) || defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE) || defined(SHADER_API_VULKAN) || defined(SHADER_API_METAL) || defined(SHADER_API_PSSL)
#define UNITY_CAN_COMPILE_TESSELLATION 1
#   define UNITY_domain                 domain
#   define UNITY_partitioning           partitioning
#   define UNITY_outputtopology         outputtopology
#   define UNITY_patchconstantfunc      patchconstantfunc
#   define UNITY_outputcontrolpoints    outputcontrolpoints
#endif


// The structure definition defines which variables it contains.
// > uses the Attributes structure as an input structure in the vertex shader.

// the original vertex struct
struct Attributes
{
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	float2 uv : TEXCOORD0;
	float3 normalWS : TEXCOORD2;
	float2 lightmapUV : TEXCOORD4;
	float4 color : COLOR;
};

// vertex to fragment struct
struct Varyings
{
	float4 color : COLOR;
	float3 normal : NORMAL;
	float4 vertex : SV_POSITION;
	float2 uv : TEXCOORD0;
	float3 positionWS : TEXCOORD1; //for specular
	float3 normalWS : TEXCOORD2;
	float fogCoord : TEXCOORD3; // fog
};


// tessellation data
struct TessellationFactors
{
	float edge[3] : SV_TessFactor;
	float inside : SV_InsideTessFactor;
};

// Extra vertex struct
struct ControlPoint
{
	float4 vertex : INTERNALTESSPOS;
	float2 uv : TEXCOORD0;
	float4 color : COLOR;
	float3 normal : NORMAL;
};

// tessellation variables, add these to your shader properties
float _Tess;
float _MaxTessDistance;

	sampler2D _FungiMask;
    float4 _FungiMask_ST;
	sampler2D _GooMask;
    float4 _GooMask_ST;


// info so the GPU knows what to do (triangles) and how to set it up, clockwise, fractional division
// hull takes the original vertices and outputs more
[UNITY_domain("tri")]
[UNITY_outputcontrolpoints(3)]
[UNITY_outputtopology("triangle_cw")]
//[UNITY_partitioning("fractional_odd")]
//[UNITY_partitioning("fractional_even")]
[UNITY_partitioning("pow2")]
//[UNITY_partitioning("integer")]
[UNITY_patchconstantfunc("patchConstantFunction")]


ControlPoint hull(InputPatch<ControlPoint, 3> patch, uint id : SV_OutputControlPointID)
{
	return patch[id];
}


// fade tessellation at a distance
float CalcDistanceTessFactor(float4 vertex, float minDist, float maxDist, float tess)
{
    float3 worldPosition = TransformObjectToWorld(vertex.xyz);
    float dist = distance(worldPosition, _WorldSpaceCameraPos);
	float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
	return (f);
}


// tessellation
TessellationFactors patchConstantFunction(InputPatch<ControlPoint, 3> patch)
{
	// values for distance fading the tessellation
	float minDist = 1.0;
	float maxDist = _MaxTessDistance;

	//prepare for green values
	float allTheGreens;
	float newAllG;
	float green0;
	float green1;
	float green2;

	float clampedTessVal;

	float fungiMask0 = tex2Dlod(_FungiMask, float4(patch[0].uv, 0, 0)).g;
	float fungiMask0b = step(1, fungiMask0);
	float fungiMask1 = tex2Dlod(_FungiMask, float4(patch[1].uv, 0, 0)).g;
	float fungiMask1b = step(1, fungiMask1);
	float fungiMask2 = tex2Dlod(_FungiMask, float4(patch[2].uv, 0, 0)).g;
	float fungiMask2b = step(1, fungiMask2);
	float fungiMaskVal = clamp((fungiMask0b + fungiMask1b + fungiMask2b) /3, 0,1);
	float newFungiMaskVal;

	/*
	float gooMask0 = tex2Dlod(_GooMask, float4(patch[0].uv, 0, 0)).g;
	float gooMask0b = step(1, gooMask0);
	float gooMask1 = tex2Dlod(_GooMask, float4(patch[1].uv, 0, 0)).g;
	float gooMask1b = step(1, gooMask1);
	float gooMask2 = tex2Dlod(_GooMask, float4(patch[2].uv, 0, 0)).g;
	float gooMask2b = step(1, gooMask2);
	float gooMaskVal = clamp((gooMask0b + gooMask1b + gooMask2b) /3, 0,1);
	float newGooMaskVal;

	if(gooMaskVal > 0.1)
	{
		newGooMaskVal = 1;//(floor((fungiMaskVal+1.0)*10))/10;
	}
	if(gooMaskVal < 0.09)
	{
		newGooMaskVal = 0.0;
	}

	float combinedMaskVal = clamp(newFungiMaskVal + newGooMaskVal,0,1);
	*/

	if(fungiMaskVal >0.1)
	{
		newFungiMaskVal = 1;//(floor((fungiMaskVal+1.0)*10))/10;
	}
	if(fungiMaskVal < 0.09)
	{
		newFungiMaskVal = 0.0;
	}

	/*
	green0 = patch[0].color.g;
	green1 = patch[1].color.g;
	green2 = patch[2].color.g;
	
	//take green value from each vertex and take the average value of those for the final green value
	allTheGreens = clamp(green0 + green1 + green2 / 3, 0, 1);
	if(allTheGreens >0.1){
			newAllG = (floor((allTheGreens+0.2)*10))/10;
		}
	if(allTheGreens < 0.1){
			newAllG = 0.01;
		}
	*/
	clampedTessVal = _Tess;//clamp(_Tess * newFungiMaskVal, 1, _Tess);

	TessellationFactors f;

	float edge0 = CalcDistanceTessFactor(patch[0].vertex, minDist, maxDist, clampedTessVal);
	float edge1 = CalcDistanceTessFactor(patch[1].vertex, minDist, maxDist, clampedTessVal);
	float edge2 = CalcDistanceTessFactor(patch[2].vertex, minDist, maxDist, clampedTessVal);


	// make sure there are no gaps between different tessellated distances, by averaging the edges out.
	f.edge[0] = (edge1 + edge2) / 2;
	f.edge[1] = (edge2 + edge0) / 2;
	f.edge[2] = (edge0 + edge1) / 2;
	f.inside = (edge0 + edge1 + edge2) / 3;
	return f;
}








