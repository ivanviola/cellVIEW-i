Shader "Custom/OcclusionQueries"
{
	CGINCLUDE

	#include "UnityCG.cginc"
	#include "Helper.cginc"		


	float _Scale;
	StructuredBuffer<float> _ProteinRadii;
	StructuredBuffer<float4> _ProteinInstanceInfo;
	StructuredBuffer<float4> _ProteinInstancePositions;
		
	StructuredBuffer<float4> _LipidInstanceInfo;
	StructuredBuffer<float4> _LipidInstancePositions;

	StructuredBuffer<int4> _OccludeeSphereBatches;
	RWStructuredBuffer<int> _FlagBuffer : register(u1);
		
	int _CutObjectIndex;
	int _NumIngredients;
	sampler2D _DistanceField;
	StructuredBuffer<CutInfoStruct> _CutInfo;

	struct gs_input
	{
		int id : INT0;
		float aperture : FLOAT0;
		float4 sphere : FLOAT40;
	};

	struct fs_input
	{
		nointerpolation int id : INT0;
		nointerpolation float radius : FLOAT0;
		nointerpolation float aperture : FLOAT1;
		nointerpolation	float pixelradius : FLOAT2;

		float2 uv: TEXCOORD0;
		float2 uv2: TEXCOORD1;
		float4 pos : SV_POSITION;
	};

	//--------------------------------------------------------------------------------------

	void vs_protein(uint id : SV_VertexID, out gs_input output)
	{		
		int idx = _OccludeeSphereBatches[id].x;

		float4 info = _ProteinInstanceInfo[idx];
		float radius = _ProteinRadii[info.x] * _Scale * 1;
		float3 pos = _ProteinInstancePositions[idx].xyz * _Scale;
		
		output.id = idx;
		output.aperture = 1-_CutInfo[info.x + _CutObjectIndex * _NumIngredients].info2.w;
		output.sphere = float4(pos, radius);
	}
	
	void vs_lipid(uint id : SV_VertexID, out gs_input output)
	{		
		int idx = _OccludeeSphereBatches[id].x;

		float4 info = _LipidInstanceInfo[idx];
		float4 sphere = _LipidInstancePositions[idx] * _Scale;
		
		output.id = idx;
		output.aperture = 1-_CutInfo[info.x + _CutObjectIndex * _NumIngredients].info2.w;
		output.sphere = sphere;
	}

	//--------------------------------------------------------------------------------------

	[maxvertexcount(4)]
	void gs_sphere(point gs_input input[1], inout TriangleStream<fs_input> triangleStream)
	{
		// Discard unwanted atoms
		if (input[0].sphere.w <= 0) return;

		float4 viewPos = mul(UNITY_MATRIX_MV, float4(input[0].sphere.xyz, 1));
		viewPos -= normalize(viewPos) * input[0].sphere.w;
		float4 projPos = mul(UNITY_MATRIX_P, float4(viewPos.xyz, 1));
		float4 offset = mul(UNITY_MATRIX_P, float4(input[0].sphere.w, input[0].sphere.w, 0, 0));
		
		float2 centerPixel = ((projPos.xy / projPos.w) * 0.5 + 0.5) * _ScreenParams.xy;		
		
		float4 corner = projPos + float4(offset.xy, 0, 0);
		float2 cornerPixel = ((corner.xy / corner.w) * 0.5 + 0.5) * _ScreenParams.xy;		

		// Find pixel radius
		float pixelradius = abs(centerPixel.x - cornerPixel.x);

		fs_input output;
		output.id = input[0].id;
		output.radius = input[0].sphere.w;
		output.aperture = input[0].aperture;
		output.uv2 = (projPos.xy / projPos.w) * 0.5 + 0.5;
		output.uv2.y = 1-output.uv2.y;
		output.pixelradius = pixelradius;

		output.uv = float2(1.0f, 1.0f);
		output.pos = projPos + float4(output.uv * offset.xy, 0, 0);
		triangleStream.Append(output);

		output.uv = float2(1.0f, -1.0f);
		output.pos = projPos + float4(output.uv * offset.xy, 0, 0);
		triangleStream.Append(output);

		output.uv = float2(-1.0f, 1.0f);
		output.pos = projPos + float4(output.uv * offset.xy, 0, 0);
		triangleStream.Append(output);

		output.uv = float2(-1.0f, -1.0f);
		output.pos = projPos + float4(output.uv * offset.xy, 0, 0);
		triangleStream.Append(output);
	}

	//--------------------------------------------------------------------------------------
	
	void fs_sphere(fs_input input, out float4 color: COLOR)
	{
		float lensqr = dot(input.uv, input.uv);
		if (lensqr > 1) discard;
		color = float4(1, 0, 0, 1);		
	}	

	[earlydepthstencil] // Necessary when writing to UAV's otherwise the depth stencil test will happen after the fragment shader
	void fs_sphere2(fs_input input)
	{	
		//float2 uv = input.pos.xy / float2(512,512);
		float2 uv = input.pos.xy / _ScreenParams.xy;
		
		//float4 d = tex2D(_DistanceField, uv);
		float4 d = tex2D(_DistanceField, input.uv2) + input.pixelradius;
		
		//_FlagBuffer[input.id] = 1;
		//input.aperture = max(input.aperture, 0.025);
		if(d.z < 0)
		{
			_FlagBuffer[input.id] = 0;
		}
		else if(d.z < input.aperture * 256 )
		{
			_FlagBuffer[input.id] = 1;
		}
		else
		{
			_FlagBuffer[input.id] = 2;
		}
	}

	ENDCG		

	SubShader
	{
		// Protein MASK LEQUAL

		Pass
		{
			ZWrite On
			ZTest Lequal

			// These stencil values will write 1 in the stencil channel for each instance drawn
			Stencil
			{
				Ref 1
				Comp always
				Pass replace
			}

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma only_renderers d3d11
			#pragma target 5.0				

			#pragma vertex vs_protein			
			#pragma geometry gs_sphere			
			#pragma fragment fs_sphere

			ENDCG
		}

		Pass
		{
			ZWrite Off
			ZTest Lequal
			//ZTest Always

			// These stencil values will discard instances drawn outiside of the mask
			Stencil
			{
				Ref 1
				Comp equal
			}

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma only_renderers d3d11
			#pragma target 5.0				

			#pragma vertex vs_protein			
			#pragma geometry gs_sphere		
			#pragma fragment fs_sphere2

			ENDCG
		}

		// Lipid MASK LEQUAL

		Pass
		{
			ZWrite On
			ZTest Lequal

			// These stencil values will write 1 in the stencil channel for each instance drawn
			Stencil
			{
				Ref 1
				Comp always
				Pass replace
			}

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma only_renderers d3d11
			#pragma target 5.0				

			#pragma vertex vs_lipid			
			#pragma geometry gs_sphere			
			#pragma fragment fs_sphere

			ENDCG
		}

		Pass
		{
			ZWrite Off
			ZTest Less

			// These stencil values will discard instances drawn outiside of the mask
			Stencil
			{
				Ref 1
				Comp equal
			}

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma only_renderers d3d11
			#pragma target 5.0				

			#pragma vertex vs_lipid			
			#pragma geometry gs_sphere		
			#pragma fragment fs_sphere2

			ENDCG
		}

		// Protein mask GEQUAL

		Pass
		{
			ZWrite On
			ZTest Gequal

			// These stencil values will write 1 in the stencil channel for each instance drawn
			Stencil
			{
				Ref 1
				Comp always
				Pass replace
			}

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma only_renderers d3d11
			#pragma target 5.0				

			#pragma vertex vs_protein			
			#pragma geometry gs_sphere			
			#pragma fragment fs_sphere

			ENDCG
		}

		// Lipid mask GEQUAL

		Pass
		{
			ZWrite On
			ZTest Gequal

			// These stencil values will write 1 in the stencil channel for each instance drawn
			Stencil
			{
				Ref 1
				Comp always
				Pass replace
			}

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma only_renderers d3d11
			#pragma target 5.0				

			#pragma vertex vs_lipid			
			#pragma geometry gs_sphere			
			#pragma fragment fs_sphere

			ENDCG
		}

	}
	Fallback Off
}