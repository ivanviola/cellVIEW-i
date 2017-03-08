Shader "Custom/RenderLipids" 
{	
	CGINCLUDE
		
	#include "UnityCG.cginc"	

	uniform float _Scale;	
	uniform StructuredBuffer<float4> _LipidAtomPositions;	
	uniform StructuredBuffer<float4> _LipidInstancePositions;	

	uniform StructuredBuffer<int4> _LipidSphereBatches;	

	struct vs2ds
	{
		int id : INT0;
		int type : INT1;
		int state : INT2;	
		int sphereStart : INT3;	
		int sphereCount : INT4;	
		int lodLevel : INT5;	
		int decimationFactor : INT6;		
		float radiusScale : FLOAT0;
				
		float3 pos : FLOAT30;
		float3 color : FLOAT31;				
		float4 rot : FLOAT40;	
	};

	void vs_lipid(uint id : SV_VertexID, out vs2ds output)
	{		
		float4 batchInfo = _LipidSphereBatches[id.x];	
		float4 position = _LipidInstancePositions[batchInfo.x];

		output.id = batchInfo.x; // Offset id to get unique id
		output.type = batchInfo.y;		
		output.state = 0;
		output.rot = float4(0,0,0,1);	
		output.color = float3(1,1,0); // Read color here and pass it to the next levels to avoid unnecessary buffer reads
		output.pos = position.xyz * _Scale;
			
		output.lodLevel = 0;
		output.radiusScale = 1;
		output.decimationFactor = 1;
		output.sphereStart = batchInfo.z;
		output.sphereCount = batchInfo.w;		
	}		

	struct hsConst
	{
		float tessFactor[2] : SV_TessFactor;
	};

	void HSConst(InputPatch<vs2ds, 1> input, uint patchID : SV_PrimitiveID, out hsConst output)
	{
		output.tessFactor[0] = output.tessFactor[1] = ( input[0].sphereCount <= 0 ) ? 0 : ceil(sqrt(input[0].sphereCount));									
		return;
	}

	[domain("isoline")]
	[partitioning("integer")]
	[outputtopology("point")]
	[outputcontrolpoints(1)]				
	[patchconstantfunc("HSConst")]
	vs2ds hs (InputPatch<vs2ds, 1> input, uint ID : SV_OutputControlPointID)
	{
		return input[0];
	} 

	struct ds2gs
	{
		int id : INT0;		
		int atomId : INT3;		
		int type : INT1;
		int state : INT2;								
		float radius : FLOAT0;	
		
		float3 pos : FLOAT30;	
		float3 color : FLOAT31;		
	};	

	static const float atomsRadius[7] =
	{
		1.548f, 1.100f, 1.400f, 1.348f, 1.880f, 1.808f, 1.5f
	};

	[domain("isoline")]
	void ds_lipid(hsConst input, const OutputPatch<vs2ds, 1> op, float2 uv : SV_DomainLocation, out ds2gs output)
	{
		int x = round(uv.y * input.tessFactor[0]);
		int y = round(uv.x * input.tessFactor[0]);		
		int sphereId = x + y * input.tessFactor[0];	
		int sphereIndex = sphereId * op[0].decimationFactor;		
							
		float4 atom = _LipidAtomPositions[op[0].sphereStart + sphereIndex];			
		atom.xyz = atom.xyz * _Scale + op[0].pos;

		float radius = atomsRadius[atom.w];

		output.id = op[0].id;
		output.atomId = op[0].sphereStart + sphereIndex;
		output.type = op[0].type;
		output.state = op[0].state;
		output.color = op[0].color;
		output.pos = atom.xyz ;		
		output.radius = (y >= input.tessFactor[0] || sphereId >= op[0].sphereCount) ? 0 : radius * _Scale;		
	}

	struct gs2fs
	{
		nointerpolation int id : INT0;	
		nointerpolation int atomId : INT1;	
			
		nointerpolation float radius : FLOAT0;	
		nointerpolation float lambertFalloff : FLOAT1;
		nointerpolation float3 color : FLOAT30;		
				
		float2 uv: TEXCOORD0;	
		centroid float4 pos : SV_Position;	
	};

	//--------------------------------------------------------------------------------------
							
	[maxvertexcount(3)]
	void gs_lipid(point ds2gs input[1], inout TriangleStream<gs2fs> triangleStream)
	{
		// Discard unwanted atoms
		if( input[0].radius <= 0 ) return;

		//float minl = 15;
		//float maxl = 50;
		//float d = min(distance(_WorldSpaceCameraPos, input[0].pos), maxl);
		//output.lambertFalloff = 1-( max(d - minl, 0) / (maxl -minl));
	
		//float shadowFactor = 1;
		//if(_EnableShadows)
		//{
		//	float4 shadowProj = mul(_ShadowCameraViewProjMatrix, float4(input[0].pos,1));

		//	shadowProj.xyz /= shadowProj.w;
		//	shadowProj.xy = shadowProj.xy * 0.5 + 0.5;
		//	shadowProj.y = 1 - shadowProj.y;

		//	float shadowCameraEyePos = mul(_ShadowCameraViewMatrix, float4(input[0].pos,1)).z + 0.75;
		//	float shadowMapCameraEyePos = tex2Dlod(_ShadowMap, float4(shadowProj.xy, 0, 0));				
		//	shadowFactor = (shadowMapCameraEyePos > shadowCameraEyePos ) ? 0.75 : 1.0;
		//}			
		//output.color = SetHSL(_IngredientColors[output.type].rgb, float3(-1, (output.state == 0) ? 0.35 : 0.5 + (sin(_Time.z * 3) + 1) / 4 , -1)) * shadowFactor;		
	
		float4 viewPos = mul(UNITY_MATRIX_MV, float4(input[0].pos, 1));
		viewPos -= normalize( viewPos ) * input[0].radius;
		float4 projPos = mul(UNITY_MATRIX_P, float4(viewPos.xyz, 1));
		float4 offset = mul(UNITY_MATRIX_P, float4(input[0].radius, input[0].radius, 0, 0));

		gs2fs output;	
		output.id = input[0].id;		
		output.atomId = input[0].atomId;		
		output.color = input[0].color;			
		output.radius = input[0].radius;
		output.lambertFalloff = 0;		

		//*****//
		
		float triBase = 3.464;
		float triHeigth = 3;
		float triBaseHalf = triBase * 0.5;
		float2 triOffset = float2(triBaseHalf, 1.0);

		output.uv = float2(0, 0) - triOffset;
		output.pos = projPos + float4(output.uv * offset.xy, 0, 0);
		triangleStream.Append(output);

		output.uv = float2(triBaseHalf, triHeigth) - triOffset;
		output.pos = projPos + float4(output.uv * offset.xy, 0, 0);
		triangleStream.Append(output);	
								
		output.uv = float2(triBase,0) - triOffset;
		output.pos = projPos + float4(output.uv * offset.xy, 0, 0);
		triangleStream.Append(output);
	}

	//--------------------------------------------------------------------------------------
	
	void fs_lipid(gs2fs input, out int id : COLOR0, out float depth : sv_depthgreaterequal) 
	//void fs_lipid(gs2fs input, out float4 color : COLOR0) 
	{	
		float lensqr = dot(input.uv, input.uv);   
		if(lensqr > 1) discard;			

		// Find normal
		float3 normal = normalize(float3(input.uv, sqrt(1.0 - lensqr)));		
						
		//color = float4(1,0,0,1);	
								
		// Set id to idbuffer
		id = 100000 + input.id;		

		// Find depth
		float eyeDepth = LinearEyeDepth(input.pos.z) + input.radius * (1-normal.z);
		depth = 1 / (eyeDepth * _ZBufferParams.z) - _ZBufferParams.w / _ZBufferParams.z;			
	}
	
	// Shadow map using eye depth
	void fs_shadow(gs2fs input, out float eyeDepth : SV_TARGET0) //, out float depth : sv_depthgreaterequal)
	{		
		float lensqr = dot(input.uv, input.uv);   
		if(lensqr > 1) discard;
			
		eyeDepth = abs(LinearEyeDepth(input.pos.z));

		//// Find normal
		//float3 normal = normalize(float3(input.uv, sqrt(1.0 - lensqr)));		

		//// Find depth
		//float eyeDepth = LinearEyeDepth(input.pos.z) + input.radius * (1-normal.z);
		//depth = 1 / (eyeDepth * _ZBufferParams.z) - _ZBufferParams.w / _ZBufferParams.z;
	}

	ENDCG

	SubShader 
	{	
		Pass 
	    {
			ZWrite On

	    	CGPROGRAM			
	    		
			#include "UnityCG.cginc"
			
			#pragma only_renderers d3d11
			#pragma target 5.0				
			
			#pragma vertex vs_lipid
			#pragma hull hs
			#pragma domain ds_lipid				
			#pragma geometry gs_lipid			
			#pragma fragment fs_lipid
						
			ENDCG
		}

		Pass 
	    {
			ZWrite On

	    	CGPROGRAM			
	    		
			#include "UnityCG.cginc"
			
			#pragma only_renderers d3d11
			#pragma target 5.0				
			
			#pragma vertex vs_lipid
			#pragma hull hs
			#pragma domain ds_lipid				
			#pragma geometry gs_lipid			
			#pragma fragment fs_shadow
						
			ENDCG
		}
	}
	Fallback Off
}	