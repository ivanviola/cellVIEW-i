Shader "Custom/DrawBillboards"
{
	CGINCLUDE
	
	#include "UnityCG.cginc"	
	#include "Helper.cginc" 

	int _UseHCL;
	int _ShowAtoms;
	int _ShowChains;

	float _GlyphRadius;
	float _HueCircleRadius;

	float4 _CameraUp;
	float4 _CameraRight;

	StructuredBuffer<float2> _QuadUVs;
	StructuredBuffer<int> _QuadIndices;
	StructuredBuffer<float3> _QuadVertices;
		
	StructuredBuffer<float2> _WedgesInfo;
	StructuredBuffer<float4> _NodesInfo;
	StructuredBuffer<float4> _InstancePositions;
	

	struct v2f
	{
		float2 worldPos : TEXCOORD1;
		float2 uv : TEXCOORD0;
		int order : INT0;
		float radius : FLOAT0;
		float4 color : COLOR0;
		float4 vertex : SV_POSITION;
	};
			
	v2f vert_group(uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
	{
		int index = _QuadIndices[vertexId];
		float3 vertexPos = _QuadVertices[index] * 2;
				
		float4 billboardWorldPos = _InstancePositions[instanceId];
		float4 billboardVertexWorldPos = float4(billboardWorldPos.xy,0,0) + _CameraRight * vertexPos.x + _CameraUp * vertexPos.y;
		float2 circlePos = normalize(billboardWorldPos.xy);

		v2f output;
		output.radius = _GlyphRadius;
		output.uv = (_QuadUVs[index] - 0.5) * 2;
		output.order = round(billboardWorldPos.z);
		output.color = float4(atan2(circlePos.y, circlePos.x) * 57.29578f,75,75,1); //;
		output.vertex = mul(UNITY_MATRIX_MVP, float4(billboardVertexWorldPos.xyz,1));

		output.vertex.w = (billboardWorldPos.w < 0) ? 0 : output.vertex.w;
		return output;
	}
			
	void frag_group (v2f input, out float4 color : SV_TARGET0)//, out float depth : sv_depth)
	{
		float lensqr = dot(input.uv, input.uv);   
		if(lensqr > 1) discard;	

		// Find depth
		//float eyeDepth = LinearEyeDepth(input.vertex.z) + 1;
		//depth = 1 / (eyeDepth * _ZBufferParams.z) - _ZBufferParams.w / _ZBufferParams.z;	

		float angle = input.color.x;
		if (angle >= 0)
        {
            angle = angle % 360;
        }
		else
		{
			angle = 360 - abs(angle) % 360;
		}

		color = (_UseHCL == 0) ? float4(HSLtoRGB(float3(angle / 360.0f , 0.75, 0.55)), 1) : float4(d3_hcl_lab(input.color.x, 75, 75), 1);
	}

	//****************************************//

	v2f vert_ingredient(uint vertexId : SV_VertexID, uint instanceId : SV_InstanceID)
	{
		int index = _QuadIndices[vertexId];
		float4 nodeInfo = _NodesInfo[instanceId];
		float3 vertexPos = _QuadVertices[index] * nodeInfo.z * 2;
				
		float4 billboardWorldPos = _InstancePositions[instanceId];
		float4 billboardVertexWorldPos = float4(billboardWorldPos.xyz,0) + _CameraRight * vertexPos.x + _CameraUp * vertexPos.y;
		
		

		float2 wedgeInfo = _WedgesInfo[nodeInfo.x];	
		float2 circlePos = normalize(billboardWorldPos.xy);
		float distCircle = length(billboardWorldPos.xy);
		
		float minDistCircle = wedgeInfo.x;
		float maxDistCircle = wedgeInfo.y;

		float localWegdgePos = distCircle - minDistCircle;
		float wedgeWidth = maxDistCircle - minDistCircle;

		float ratio = (abs(wedgeWidth) < 0.001) ? 0.5 : 1-( localWegdgePos / wedgeWidth);

		float chroma = 100 - (ratio * 60);
		//float chroma = 100 - distCircle;

		v2f output;
		output.radius = nodeInfo.z;
		output.uv = (_QuadUVs[index] - 0.5) * 2;
		output.order = round(billboardWorldPos.z);
		output.worldPos = billboardVertexWorldPos.xy;
		output.color = float4(atan2(circlePos.y, circlePos.x) * 57.29578f,chroma,75,1); //;
		output.vertex = mul(UNITY_MATRIX_MVP, float4(billboardVertexWorldPos.xyz,1));

		output.vertex.w = (nodeInfo.z < 0) ? 0 : output.vertex.w;
		return output;
	}

	void frag_ingredient1 (v2f input, out float4 color : SV_TARGET0) //, out float depth : sv_depth)
	{
		float lensqr = dot(input.uv, input.uv);   
		if(lensqr > 1) discard;
		normalize(input.worldPos);
		float angle = atan2(input.worldPos.y, input.worldPos.x) * 57.29578f;

		// Find normal
		float3 normal = normalize(float3(input.uv, sqrt(1.0 - lensqr)));	

		// Find depth
		//float eyeDepth = LinearEyeDepth(input.vertex.z) + input.radius * (1-normal.z);
		//depth = 1 / (eyeDepth * _ZBufferParams.z) - _ZBufferParams.w / _ZBufferParams.z;	

		//float light = 1;
		float light = max(dot(normal, float3(0,0,1)), 0);
		light = pow(light, 0.2);
		
		if (angle >= 0)
        {
            angle = angle % 360;
        }
		else
		{
			angle = 360 - abs(angle) % 360;
		}

		//color = float4(HSLtoRGB(float3(angle / 360.0f , 0.5, 0.5)), 1);
		//color = float4(light * d3_hcl_lab(input.color.x, input.color.y, input.color.z), 1);
	}

	void frag_ingredient2 (v2f input, out float4 color : SV_TARGET0, out float depth : sv_depth)
	{
		float lensqr = dot(input.uv, input.uv);   
		if(lensqr > 1) discard;
		normalize(input.worldPos);
		input.color = float4(atan2(input.worldPos.y, input.worldPos.x) * 57.29578f,75,75,1);

		// Find normal
		float3 normal = normalize(float3(input.uv, sqrt(1.0 - lensqr)));	

		// Find depth
		float eyeDepth = LinearEyeDepth(input.vertex.z) + input.radius * (1-normal.z) - 2;
		depth = 1 / (eyeDepth * _ZBufferParams.z) - _ZBufferParams.w / _ZBufferParams.z;	

		//float light = 1;
		float light = max(dot(normal, float3(0,0,1)), 0);
		light = pow(light, 0.2);
		
		//color = float4(HSLtoRGB(float3((input.color.x % 360)/360.0f, 0.5, 0.5)), 1);
		color = float4(light * d3_hcl_lab(input.color.x, input.color.y, input.color.z), 1.0);
	}

	ENDCG

	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{	
			Cull Back
			//Blend SrcAlpha OneMinusSrcAlpha // Alpha blending

			//Cull Off
			CGPROGRAM
			#pragma vertex vert_group
			#pragma fragment frag_group			
			ENDCG
		}

		Pass
		{	
			Cull Back
			Blend SrcAlpha OneMinusSrcAlpha // Alpha blending

			//Cull Off
			CGPROGRAM
			#pragma vertex vert_ingredient
			#pragma fragment frag_ingredient1			
			ENDCG
		}

		Pass
		{	
			Cull Back
			Blend SrcAlpha OneMinusSrcAlpha // Alpha blending

			//Cull Off
			CGPROGRAM
			#pragma vertex vert_ingredient
			#pragma fragment frag_ingredient2		
			ENDCG
		}
	}
}
