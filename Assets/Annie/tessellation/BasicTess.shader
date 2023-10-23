Shader "Unlit/BasicTess"
{ 
	Properties
	{
		//material properties
		_BG("Turn Off BG", Range(0,1)) = 0.75
		_Smoothness("Smoothness", Range(0,1)) = 0.75
		_PlayerSmoothness("Player Smoothness", Range(0,1)) = 0.15
		_PlayerParticlesStrength("Outside Player particle intensity", Range(0,10)) = 1
		_Tiling("Tiling", Float) = 1

		//color
		_Tint("Tint", Color) = (1,1,1,1)
		_BgCol("Background Color in Base", Color) = (0,0,0,0)
		_BgColExploration("Background Color Exploration", Color) = (0,0,0,0)
		_OutCol("Background Color Outside", Color) = (0,0,0,0)
		_OutFungiCol("Fungi Outside of Base Color", Color) = (0,0,0,0)
		_SpecCol("Specular Color", Color) = (1,1,1,1)
		_OutsideSpecCol("Outside Specular Color", Color) = (1,1,1,1)
		_MotherFungiTint("Mother Fungi Tint", Color) = (1,1,1,1)
		_MotherFungiColorAmount("Mother fungi color Strength", Range(0,5)) = 0.75
		_MainRootsTint("Main Roots Tint", Color) = (1,1,1,1)
		_PlayerTint("Player Tint", Color) = (1,1,1,1)

		//Textures
		//_TessMask("tessellation Mask", 2D) = "white" {}
		[NoScaleOffset] _FungiMask("Mycelium Mask", 2D) = "white" {}
		[NoScaleOffset] _AlbedoTexture("Mycelium Texture", 2D) = "white" {}
		[NoScaleOffset] _MyceliumTextureOutside("Mycelium Texture Outside", 2D) = "white" {}
		[NoScaleOffset] _MotherFungiMask("Mother Fungi Mask", 2D) = "white" {}
		_MotherDisStrength("Mother Fungi Displacement", Range(0,5)) = 0.9
		[NoScaleOffset] _DisplacementMask("Displacement Mask", 2D) = "white" {}
		[NoScaleOffset] _GooTex("Goo Texture", 2D) = "white" {}
		_BgDisStrength("Non-Player Displacement Strength", Range(0,1)) = 0.9
		_MainRootsStrength("Main Roots Displacement Strength", Range(0,1)) = 0.9
		[NoScaleOffset] _PlayerParticles("Player Particles", 2D) = "black" {}
		[NoScaleOffset] _DisplacementNoise("Scrolling Noise", 2D) = "white" {}
		_NoiseSpeed("Scrolling Noise Strength", Float) = 2

		[NoScaleOffset] _NormalMap ("Normals", 2D) = "bump" {}
		_BumpScale("Bump Scale", Float) = 1
		_PlayerBumpScale("Player Bump Scale", Range(0,1)) = 0.7
		_MainRootsBumpScale("Main Roots Bump Scale", Range(0,1)) = 0.7
		_OutsideBumpScale("Out Of Base Bump Scale", Float) = 1

		//tessellation related properties
		_Tess("Tessellation", Range(1, 150)) = 20
		_Weight("Displacement Amount", Range(0, 1)) = 0
		_MaxTessDistance("Max Tess Distance", Range(1, 32)) = 20
	}
		//SubShader block > containins Shader code
		SubShader
	{
		//SubShader Tags 
        Tags {"RenderType"="Opaque"}

		//TRANSPARENCY --------------------------------------------------------------------
		//...
			/*
			Tags {"RenderType"="Transparent" "RenderQueue" = "Transparent"} // transparent objects are rendered after opaque (geometry) and skybox 
			ZWrite Off // prevent rasterizer from storing transparent objects in depth buffer so objects behind transparent objects are visible 
			Blend SrcAlpha OneMinusSrcAlpha // > one zero fully opaque 
			*/
		//END TRANSPARENCY ----------------------------------------------------------------


		//SHADOW CASTER PASS --------------------------------------------------------------
		Pass
		{
			Name "ShadowCaster"
			Tags{"LightMode" = "ShadowCaster"}

			//normally ColorMask = rgb > for performance set to in shadow caster pass 0 
			ColorMask 0

			HLSLPROGRAM
				//name of vertex shader
				#pragma vertex vert
				//name of fragment shader
				#pragma fragment frag
				//hlsl file for shadow caster pass
				#include "ShadowCasterPass.hlsl"
			ENDHLSL
		}
		// END SHADOW CASTER PASS ---------------------------------------------------------

		Pass
	{
		Tags{ "LightMode" = "UniversalForward" }

		//Fog {Mode Global}

	HLSLPROGRAM

	// KEYWORDS AND FILES ------------------------------------------------------------------
	//...
		#pragma target 5.0 //5.0 required for tessellation

		#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
		#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
		#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
		#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
		#pragma multi_compile_fragment _ _SHADOWS_SOFT
		#pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
		#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
		#pragma multi_compile _ SHADOWS_SHADOWMASK
		#pragma multi_compile _ DIRLIGHTMAP_COMBINED
		#pragma multi_compile _ LIGHTMAP_ON
		#pragma multi_compile_fog
		#pragma multi_compile_instancing

		//Material keywords
		#pragma shader_feature_local _PARTITIONING_INTEGER _PARTITIONING_FRAC_EVEN _PARTITIONING_FRAC_ODD _PARTITIONING_POW2
		#pragma shader_feature_local _TESSELLATION_SMOOTHING_FLAT _TESSELLATION_SMOOTHING_PHONG _TESSELLATION_SMOOTHING_BEZIER_LINEAR_NORMALS _TESSELLATION_SMOOTHING_BEZIER_QUAD_NORMALS
		#pragma shader_feature_local _TESSELLATION_FACTOR_CONSTANT _TESSELLATION_FACTOR_WORLD _TESSELLATION_FACTOR_SCREEN _TESSELLATION_FACTOR_WORLD_WITH_DEPTH
		#pragma shader_feature_local _TESSELLATION_SMOOTHING_VCOLORS
		#pragma shader_feature_local _TESSELLATION_FACTOR_VCOLORS
		#pragma shader_feature_local _GENERATE_NORMALS_MAP _GENERATE_NORMALS_HEIGHT
		#define _SPECULAR_COLOR

		// Included Files 
		// Core.hlsl file contains definitions of used HLSL macros and functions + #include references to other HLSL files (for example, Common.hlsl, SpaceTransforms.hlsl, etc.).
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"    
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/lighting.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
		#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderVariablesFunctions.hlsl"
		#include "CustomTessellation.hlsl"
	// END KEYWORDS AND FILES---------------------------------------------------------------

	#pragma require tessellation
		//vertex shader
	#pragma vertex TessellationVertexProgram
		//fragment shader
	#pragma fragment frag
		//hull shader
	#pragma hull hull
		//domain shader
	#pragma domain domain


	sampler2D _AlbedoTexture;
    float4 _AlbedoTexture_ST;

	sampler2D _MyceliumTextureOutside;
	float4 _MyceliumTextureOutside_ST;

	sampler2D _DisplacementMask;
    float4 _DisplacementMask_ST;
	sampler2D _MotherFungiMask;
	float4 _MotherFungiMask_ST;
	sampler2D _GooTex;
    float4 _GooTex_ST;
	sampler2D _PlayerParticles;
    float4 _PlayerParticles_ST;
	sampler2D _DisplacementNoise;
    float4 _DisplacementNoise_ST;

	float4 _Tint;
	float4 _BgColExploration;
	float4 _BgCol;
	float4 _MotherFungiTint;
	float4 _SpecCol;
	float4 _OutsideSpecCol;
	float4 _OutCol;
	float4 _OutFungiCol;
	float4 _MainRootsTint;
	float4 _PlayerTint;

	float _Weight;
	float _PlayerParticlesStrength;
	float _BG;
	float _MotherFungiColorAmount;
	float _BgDisStrength;
	float _MotherDisStrength;
	float _MainRootsStrength;
	float _Smoothness;
	float _PlayerSmoothness;
	float _Tiling;
	float _NoiseSpeed;

	sampler2D _NormalMap;
	float4 _NormalMap_TexelSize;
	float _BumpScale;
	float _PlayerBumpScale;
	float _MainRootsBumpScale;
	float _OutsideBumpScale;

	half4 _UnityShadowColor;
	half _UnityShadowPower;
	half _UnityShadowSharpness;

	// pre tesselation vertex program
	ControlPoint TessellationVertexProgram(Attributes v)
	{
		ControlPoint p;
		p.vertex = v.vertex;
		p.uv = v.uv;
		p.normal = v.normal;
		p.color = v.color;
		return p;
	}

	// after tesselation
	Varyings vert(Attributes input)
	{
		Varyings output;

		// Displacement -------------------------------------------------------------------------------------------------------------
		//...
			//render texures > masks
			float fungiMaskOriginal = tex2Dlod(_FungiMask, float4(input.uv, 0, 0)).g; // base
			float motherFungiMaskOriginal = tex2Dlod(_MotherFungiMask, float4(input.uv, 0, 0)).g; // mother fungi
			float gooMaskConnections = tex2Dlod(_GooTex, float4(input.uv, 0, 0)).g; // connections
			float explorationGooMask = tex2Dlod(_GooTex, float4(input.uv, 0, 0)).r; // exploration
			float PlayerParticlesTex = tex2Dlod(_PlayerParticles, float4(input.uv, 0, 0)).g; // player
			float MainRootsTexOriginal = tex2Dlod(_GooTex, float4(input.uv, 0, 0)).b; // mother fungi roots
			float MainRootsTex = (MainRootsTexOriginal) * _MainRootsStrength; // player
			float motherFungiMask = motherFungiMaskOriginal * _MotherDisStrength;// mother fungi 

			//pattern textures
			float TexDisplacementOutside = tex2Dlod(_MyceliumTextureOutside, float4(input.uv, 0, 0)).g; // roots texture outside
			float TexDisplacementInside = tex2Dlod(_DisplacementMask, float4(input.uv * _Tiling, 0, 0)).r;// roots texture inside

			// masking 'pattern Textures'
			float combinedBaseMask = clamp(fungiMaskOriginal + gooMaskConnections,0,1); // base and connecting texture
			float explorationParticlesMasked = clamp(explorationGooMask - combinedBaseMask,0,1); // explorationGooMask without basemask 
			float BaseDisplacement = combinedBaseMask * TexDisplacementInside; // combine mask with tex
			float OutsideDisplacement = explorationParticlesMasked * TexDisplacementOutside; // combine mask with tex
			float combinedAllFungiMasks = clamp(combinedBaseMask + explorationGooMask,0,1);
			float fungiMask = step(0.5, combinedAllFungiMasks);

			//float TexDisplacement1 = step(1,TexDisplacement);
			float movingNoiseTex1 = tex2Dlod(_DisplacementNoise, float4((input.uv * _Tiling) * _NoiseSpeed + _Time.x, 0, 0)).r; // noise 1
			float movingNoiseTex2 = tex2Dlod(_DisplacementNoise, float4((input.uv * _Tiling)* (_NoiseSpeed * -1) + _Time.x, 0, 0)).r; // noise 2
			float combineNoises = movingNoiseTex1 + movingNoiseTex2; // combining noise
			float combTex = combineNoises * (BaseDisplacement + OutsideDisplacement); // combining masked roots with noises
			float combinedDisplacementTexNoPlayer = ((TexDisplacementInside + combTex) * fungiMask) * 0.5; // mask area where Displacement allowed
			float combinedDisplacementTex = (combinedDisplacementTexNoPlayer * _BgDisStrength) + PlayerParticlesTex + MainRootsTex + motherFungiMask; //clamp(PlayerParticlesTex + (MainRootsTex * _MainRootsStrength),0,1); // add player 
			input.vertex.xyz += (input.normal) *  combinedDisplacementTex * _Weight;
		// END Displacement ---------------------------------------------------------------------------------------------------------
		
		// LIGHT --------------------------------------------------------------------------------------------------------------------
		//...
			VertexNormalInputs normInputs = GetVertexNormalInputs(input.normal); // transform from os to ws for light
			VertexPositionInputs posInputs = GetVertexPositionInputs(input.vertex.xyz); // transform position from os to ws for specular

			output.normalWS = normInputs.normalWS; // transform from os to ws for light
			output.positionWS = posInputs.positionWS; // // transform position from os to ws for specular
		// END LIGHT ----------------------------------------------------------------------------------------------------------------

		output.vertex = TransformObjectToHClip(input.vertex.xyz);
		output.color = input.color;
		output.normal = input.normal;

		output.uv = input.uv;

		//fog
		output.fogCoord = ComputeFogFactor(output.vertex.z);

		return output;
	}

	[UNITY_domain("tri")]
	Varyings domain(TessellationFactors factors, OutputPatch<ControlPoint, 3> patch, float3 barycentricCoordinates : SV_DomainLocation)
	{
			Attributes v;
			#define DomainPos(fieldName) v.fieldName = \
			patch[0].fieldName * barycentricCoordinates.x + \
			patch[1].fieldName * barycentricCoordinates.y + \
			patch[2].fieldName * barycentricCoordinates.z;

			DomainPos(vertex)
			DomainPos(uv)
			DomainPos(color)
			DomainPos(normal)

			return vert(v);
	}

	//random for voronoi
	float2 random2(float2 p)
	{
	return frac(sin(float2(dot(p,float2(117.12,341.7)),dot(p,float2(269.5,123.3))))*43458.5453);
	}

	//for normal map
	half3 UnpackScaleNormal (half4 packednormal, half bumpScale) 
	{
	#if defined(UNITY_NO_DXT5nm)
		return packednormal.xyz * 2 - 1;
	#else
		half3 normal;
		normal.xy = (packednormal.wy * 2 - 1);
		#if (SHADER_TARGET >= 30)
			// SM2.0: instruction count limitation
			// SM2.0: normal scaler is not supported
			normal.xy *= bumpScale;
		#endif
		normal.z = sqrt(1.0 - saturate(dot(normal.xy, normal.xy)));
		return normal;
	#endif
	}

	// fragment shader definition.            
	half4 frag(Varyings IN) : SV_Target
	{
		// mask for Mycelium texture inside base
		float fungiMaskOriginal = tex2D(_FungiMask, IN.uv).g; //base render tex
		float fungiMaskOutline = step(0.05, smoothstep(0, 1, (1- pow(fungiMaskOriginal, 1)) * fungiMaskOriginal)); // fungi outlines
		float fungiMask = clamp(step(0.5, fungiMaskOriginal) + fungiMaskOutline,0,1); // stepping the base mask
		float negFungiMask = (1-fungiMask);
		// mother fungi
		float motherFungiMaskOriginal = tex2D(_MotherFungiMask, IN.uv).g; //base render tex
		//player and main roots mask
		float PlayerParticlesTexOriginal = tex2D(_PlayerParticles, IN.uv).g; //player particles
		float PlayerParticlesTex = PlayerParticlesTexOriginal + (PlayerParticlesTexOriginal *0.5);//step(0.5, PlayerParticlesTexOriginal);
		float MainRootsTex = tex2D(_GooTex, IN.uv).b; //main mother fungi roots
		MainRootsTex = clamp((MainRootsTex - PlayerParticlesTex), 0, 1);
		float MainRoots = MainRootsTex - motherFungiMaskOriginal;
		//exploration goo
		float explorationGooTexOriginal = tex2D(_GooTex, IN.uv).r; //exploration growth particle system
		float explorationGooTexPrep = clamp((explorationGooTexOriginal - fungiMask),0,1);//maskingg exploration growth particle system
		float explorationGooTexOutline = step(0.05, smoothstep(0, 1, (1- pow(explorationGooTexPrep, 1)) * explorationGooTexPrep));
		float explorationGooTex = clamp(explorationGooTexOriginal-fungiMask,0,1);//steppping and maskingg exploration growth particle system
		
		float4 MyceliumTextureOutside = tex2D(_MyceliumTextureOutside, IN.uv * _Tiling); //Mycelium texture outside base
		float4 explorationBackground = step(0.5,(explorationGooTexPrep - (MyceliumTextureOutside * _BG)));
		float4 outsideBaseFungiPrep = clamp(explorationGooTexOutline + (step(0.5, explorationGooTexPrep) * MyceliumTextureOutside),0,1); // add goo outline to goo

		//connecting to base
		float connectionMaskOriginal = tex2D(_GooTex, IN.uv).g; //connecting to base particle system
		float connectionMask = clamp(connectionMaskOriginal - fungiMask,0,1); // removing base tex from exploration tex 
		//float4 outsideBaseFungiPrepForBG = clamp(outsideBaseFungiPrep + connectionMask,0,1);

		// NORMAL MAP -----------------------------------------------------------------------------------------------
		//...
			float bumpScale = (_BumpScale * clamp((outsideBaseFungiPrep * _OutsideBumpScale) + (clamp((fungiMask - MainRoots) + connectionMask + fungiMaskOutline+ ((MainRoots - PlayerParticlesTex) * _MainRootsBumpScale + (PlayerParticlesTex * _PlayerBumpScale)),0,1)),0,1));
			IN.normalWS = UnpackScaleNormal(tex2D(_NormalMap, IN.uv * _Tiling), bumpScale);
			IN.normalWS = IN.normalWS.xzy;
			IN.normalWS = normalize(IN.normalWS);
			float3 albedo = tex2D(_AlbedoTexture, IN.uv).rgb * _Tint.rgb;
			//albedo *= tex2D(_NormalMap, IN.uv);
		// END NORMAL MAP -------------------------------------------------------------------------------------------

		// TEXTURES -------------------------------------------------------------------------------------------------
		//...
			//Mycelium texture inside base
			float4 originalAlbedoTex = tex2D(_AlbedoTexture, IN.uv * _Tiling); // Mycelium texture
			float4 AlbedoTexMasked = clamp((originalAlbedoTex * (fungiMask - outsideBaseFungiPrep) + fungiMaskOutline),0,1); // Mycelium texture masked my base tex
			float4 AlbedoTex = clamp((AlbedoTexMasked + MainRoots) + (connectionMask * originalAlbedoTex),0,1); // Mycelium inside base & inside connecting particles 
			float4 AlbedoTexCol = clamp(AlbedoTex + (PlayerParticlesTex*3),0,1) * _Tint + (PlayerParticlesTex * (_PlayerTint * AlbedoTex)); // coloring the inside mycelium and adding the player particles
			// Outside base
			float4 outsideBase = clamp((_OutCol * (negFungiMask - (PlayerParticlesTex))),0,1) - (clamp(outsideBaseFungiPrep + explorationBackground + connectionMask,0,1)); // outside floor color minus exploration mycelium texture
			float4 outsideBaseFungi = clamp((outsideBaseFungiPrep - (PlayerParticlesTex*2))* _OutFungiCol,0,1); // exploration fungi 
			float4 outsideBaseFungiBack = clamp(((1-(outsideBaseFungiPrep - (PlayerParticlesTex))) * (explorationBackground - pow((PlayerParticlesTex * _PlayerParticlesStrength),0.3))) * _BgColExploration,0,1);
			float4 BackgroundPrep = clamp((1 - AlbedoTex) - (PlayerParticlesTex*3),0,1);// inside base background (roots and player = black)
			float4 Background = BackgroundPrep * _BgCol; // color background
			// allgemein
			float4 AlbedoTexAndBG = (Background + AlbedoTexCol) + ((motherFungiMaskOriginal * clamp(_MotherFungiColorAmount * originalAlbedoTex,0.7,1)) * _MotherFungiTint + clamp((PlayerParticlesTex * MainRoots - motherFungiMaskOriginal) * _MainRootsTint * originalAlbedoTex,0,1)) /* < inside visuals*/ + ((clamp(outsideBase - connectionMask - explorationGooTexOutline,0,1) + outsideBaseFungi) + (outsideBaseFungi * _OutFungiCol) + outsideBaseFungiBack); // in and outside visuals
			float maskedSmoothness = clamp(_Smoothness + (PlayerParticlesTexOriginal*_PlayerSmoothness),0,1);

			//specular
			float originalSpecMaskInside = tex2D(_DisplacementMask, IN.uv * _Tiling).g;
			float originalSpecMaskOutside = tex2D(_MyceliumTextureOutside, IN.uv * _Tiling).g;
			float specMaskInsideBase = originalSpecMaskInside * clamp(fungiMask + connectionMask + PlayerParticlesTex,0,1) + PlayerParticlesTex + fungiMaskOutline;
			float specMaskOutsideBase = originalSpecMaskOutside * (1-clamp(fungiMask + connectionMask + PlayerParticlesTex,0,1));
			float specMask = clamp(specMaskOutsideBase + specMaskInsideBase + fungiMaskOutline,0,1);
		// END TEXTURES ---------------------------------------------------------------------------------------------

		// LIGHT ----------------------------------------------------------------------------------------------------
		//...
			//light data
			InputData lightingInput = (InputData)0; // INPUT
			lightingInput.normalWS = normalize(IN.normalWS); // normal > world space
			//float3 lightDir = normalize(IN.normalWS);

			// specular
			lightingInput.positionWS = IN.positionWS;
			lightingInput.viewDirectionWS = GetWorldSpaceNormalizeViewDir(IN.positionWS);

			// shadows
			lightingInput.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
			float4 shadowPos = TransformWorldToShadowCoord(IN.positionWS);

			SurfaceData surfaceInput = (SurfaceData)0; // SURFACE

			//light test-------------------------------------------------------
			//...
				/*
				Light mainLight = GetMainLight(shadowPos);
				float3 Direction = mainLight.direction;
				float shadowAtten = mainLight.shadowAttenuation;
				float distanceAtten = mainLight.distanceAttenuation;
				float atten = distanceAtten * shadowAtten; 

				float NdotL = dot(IN.positionWS, Direction);
				float lightVal = NdotL * atten;
				float4 lightValAndColor = ((lightVal* -1) * 0.1) * _LightColor;
				float4 ShadowValAndColor = (lightVal - (-1)) * _ShadowColor;
				float4 combinedLight = lightValAndColor + ShadowValAndColor;
				float4 combineLightAndTex = AlbedoTex * combinedLight;
				*/
			//END light test---------------------------------------------------
		// END LIGHT ------------------------------------------------------------------------------------------------

		//add light to textures and color 
		surfaceInput.alpha = AlbedoTex.a;
		surfaceInput.specular = clamp((specMaskOutsideBase * _OutsideSpecCol) + (specMaskInsideBase * _SpecCol),0,1);
		surfaceInput.smoothness = maskedSmoothness;
		//surfaceInput.occlusion = PlayerParticlesTex;
		surfaceInput.albedo = AlbedoTexAndBG.rgb; //mix texture with fog

		// lighting + fog 
		float4 color = UniversalFragmentBlinnPhong(lightingInput, surfaceInput); //float4 color = combineLightAndTex;
		color.rgb = MixFog(color.rgb, IN.fogCoord);
		return color;

		/*
		// VORONOI ---------------------------------------------------------------------------------------------------
		//...
				float4 col = float4(0,0,0,1);
				float2 uv = IN.uv;
				uv *= 6.0; //Scaling amount (larger number more cells can be seen)
				float2 iuv = floor(uv); //gets integer values no floating point
				float2 fuv = frac(uv); // gets only the fractional part
				float minDist = 1.0;  // minimun distance
				for (int y = -1; y <= 1; y++)
				{
					for (int x = -1; x <= 1; x++)
					{
						// Position of neighbour on the grid
						float2 neighbour = float2(float(x), float(y));
						// Random position from current + neighbour place in the grid
						float2 pointv = random2(iuv + neighbour);
						// Move the point with time
						pointv = 0.5 + 0.5*sin(_Time.z + 6.2236*pointv);//each point moves in a certain way
																		// Vector between the pixel and the point
						float2 diff = neighbour + pointv - fuv;
						// Distance to the point
						float dist = length(diff);
						// Keep the closer distance
						minDist = min(minDist, dist);
					}
				}
				// Draw the min distance (distance field)
				col.r += minDist * minDist; // squared it to to make edges look sharper
				return col;
		// END VORONOI -----------------------------------------------------------------------------------------------
		*/

		//note to self: color returned by frag function is source color while color stored on render target is Destination color
		// ... > Transparency note: rasterizer adds those color together by multiplying each color by some number and ads the products together 
		// ... > storing result in render target and overwrite what was already there > specify multipliers by blend command 

	}
		ENDHLSL
	}
	}
}
