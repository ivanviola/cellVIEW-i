Shader "Custom/RenderCurveIngredients" 
{	
	CGINCLUDE

	#include "UnityCG.cginc" 
	#include "Helper.cginc"		
	
	#define NUM_STEPS_PER_SEGMENT_MAX 16
	#define NUM_ROOT_POINTS_FLOAT (NUM_STEPS_PER_SEGMENT_MAX / 4)
		
	uniform float _Scale;
	uniform int _EnableTwist;	
	uniform int _EnableCrossSection;
	uniform float4 _CrossSectionPlane;	

	uniform	StructuredBuffer<int> _CurveIngredientsToggleFlags;
	uniform	StructuredBuffer<int> _CurveIngredientsAtomCount;
	uniform	StructuredBuffer<int> _CurveIngredientsAtomStart;
	uniform	StructuredBuffer<float4> _CurveIngredientsAtoms;
	uniform	StructuredBuffer<float4> _CurveIngredientsInfos;
	uniform	StructuredBuffer<float4> _CurveIngredientsColors;

	uniform	StructuredBuffer<float4> _DnaControlPoints;
	uniform	StructuredBuffer<float4> _DnaControlPointsNormals;
	uniform	StructuredBuffer<float4> _DnaControlPointsInfos;

	uniform float4 _FrustrumPlane_0; 	
	uniform float4 _FrustrumPlane_1; 
	uniform float4 _FrustrumPlane_2; 
	uniform float4 _FrustrumPlane_3; 
	uniform float4 _FrustrumPlane_4; 
	uniform float4 _FrustrumPlane_5; 

	bool SphereFrustrumTest_2(float4 sphere)
	{
		bool inFrustrum = true;

		inFrustrum = inFrustrum & SpherePlaneTest(_FrustrumPlane_0, sphere);
		inFrustrum = inFrustrum & SpherePlaneTest(_FrustrumPlane_1, sphere);
		inFrustrum = inFrustrum & SpherePlaneTest(_FrustrumPlane_2, sphere);
		inFrustrum = inFrustrum & SpherePlaneTest(_FrustrumPlane_3, sphere);
		inFrustrum = inFrustrum & SpherePlaneTest(_FrustrumPlane_4, sphere);
		inFrustrum = inFrustrum & SpherePlaneTest(_FrustrumPlane_5, sphere);	

		return !inFrustrum;
	}

	uniform int _NumCutObjects;
	uniform	int _NumIngredientTypes;
	uniform	int _IngredientIdOffset;
	uniform StructuredBuffer<CutInfoStruct> _CutInfos;
	uniform	StructuredBuffer<float4> _CutScales;
	uniform	StructuredBuffer<float4> _CutPositions;	
	uniform StructuredBuffer<float4> _CutRotations;

	float CutAwayTest(float4 sphere, int ingredientType, int instanceId)
	{
		bool cut = false;
		for (int i = 0; i < _NumCutObjects; i++)
		{
			CutInfoStruct cutInfo = _CutInfos[(_IngredientIdOffset + ingredientType) + _NumIngredientTypes * i];
			float3 position = _CutPositions[i].xyz;
			float4 rotation = _CutRotations[i];
			float3 scale = _CutScales[i].xyz;

			float distance = GetSignedDistance(sphere.xyz, cutInfo.info.x, position, rotation, scale);
					
			//inverse
			if (cutInfo.info.w == 1) distance *= -1;
		
			float value1 = cutInfo.info.y;
			//value1 = 1.0f;
			//if(cutInfo.info.y < 0.5) cut = true;	
			//if(cutInfo.info.y > 0.5) cut = cut | false;	
			if(value1 < 0.5) cut = true;
			if (value1 > 0.5) cut = cut | false;
			else if(distance <= 0.0) cut = true;				
			     
		}
		return cut;
	}

	//--------------------------------------------------------------------------------------

	struct ds_input
	{					
		int numSteps : INT1;	
		int segmentId : INT0;			
		int curveType : INT5;	
		int localSphereCount : INT2;
		int localSphereStart : INT6;			
		int globalSphereCount : INT3;
			
		float radius : FLOAT0;		
		float twist : FLOAT1;
	
		float3 pos0 : FLOAT30;
		float3 pos1 : FLOAT31;
		float3 pos2 : FLOAT32;
		float3 pos3 : FLOAT33;	
	
		float3 n1 : FLOAT34;
		float3 n2 : FLOAT35;	
		float3 color : FLOAT36;	
							
		float4 rootPoints[NUM_ROOT_POINTS_FLOAT] : FLOAT4;
	};

	//--------------------------------------------------------------------------------------

	void VS(uint id : SV_VertexID, out ds_input output)
	{			 
		float4 pos0 = _DnaControlPoints[id]; 
		float4 pos1 = _DnaControlPoints[id + 1];
		float4 pos2 = _DnaControlPoints[id + 2];
		float4 pos3 = _DnaControlPoints[id + 3];
    
		float4 info0 = _DnaControlPointsInfos[id]; 
		float4 info1 = _DnaControlPointsInfos[id + 1];
		float4 info2 = _DnaControlPointsInfos[id + 2];
		float4 info3 = _DnaControlPointsInfos[id + 3];
			    
		int curveType = info1.y;	
		float4 ingredientInfo = _CurveIngredientsInfos[curveType];
		float3 ingredientColor = _CurveIngredientsColors[curveType].xyz;
				
		float segmentLength = distance(pos1.xyz, pos2.xyz);
		int numSteps = min(ingredientInfo.x, NUM_STEPS_PER_SEGMENT_MAX);
		float4 boundingSphere = float4((pos1.xyz + (pos2.xyz - pos1.xyz) * 0.5) * _Scale, segmentLength * 0.5);

		// Check if must discard
		bool frustrumTest = SphereFrustrumTest_2(boundingSphere);
		bool endCurve = info0.x != info1.x || info1.x != info2.x || info2.x != info3.x;

		bool cutAway = CutAwayTest(boundingSphere, curveType, id);

		//bool toggleIngredient = _CurveIngredientsToggleFlags[curveType] == 0;
		//bool crossSection = SpherePlaneTest(_CrossSectionPlane, float4(pos1.xyz  * _Scale, 0)) && _EnableCrossSection == 1;		

		output.segmentId = id.x;	  	
		output.numSteps = numSteps;	
		output.curveType = curveType;	
		output.twist = ingredientInfo.y;
		output.color = ColorCorrection(ingredientColor);
		output.localSphereCount = (endCurve || frustrumTest || cutAway) ? 0 : _CurveIngredientsAtomCount[curveType];	
		output.localSphereStart = _CurveIngredientsAtomStart[curveType];		
		output.globalSphereCount = numSteps * output.localSphereCount;
			
		output.pos0 = pos0.xyz;
		output.pos1 = pos1.xyz;
		output.pos2 = pos2.xyz;
		output.pos3 = pos3.xyz;
	
		output.n1 = _DnaControlPointsNormals[id].xyz;
		output.n2 = _DnaControlPointsNormals[id + 1].xyz;
		
		/*****/

		// First pass to find out the size of the last interval 
		
		int numLinearSeachStep = 4;
		int numBinarySearchStep = 8;	
		
		float linearStepSize = 0.5f / numSteps;	
		float stepLength = segmentLength / (float)numSteps;

		float3 current;	
		int stepsCount = 0;
		float3 previous = pos1;	
		float interpolationValue = linearStepSize;

		float rootPoints[NUM_STEPS_PER_SEGMENT_MAX];		
		for(int i = 0; i < NUM_STEPS_PER_SEGMENT_MAX; i++)
		{
			rootPoints[i] = 0;
		}

		// Find the number of segments
		for(int i = 1; i < numSteps; i++)
		{	
			// Linear search
			[unroll(4)]
			while(true)
			{					
				current = CubicInterpolate(pos0, pos1, pos2, pos3, interpolationValue);						
				interpolationValue += (distance(current, previous) < stepLength) ? linearStepSize : 0;
			}
	
			float binaryStepSize = linearStepSize * 0.5;
			interpolationValue -= binaryStepSize;

			// Binary search
			[unroll(8)]
			while(true)
			{
				binaryStepSize *= 0.5;	
				current = CubicInterpolate(pos0, pos1, pos2, pos3, interpolationValue);				
				interpolationValue += (stepLength - distance(current, previous) < 0) ? -binaryStepSize : binaryStepSize;									
			}	
				
			stepsCount ++;				
			previous = current;
			interpolationValue += linearStepSize;			
		}
			
		// Second pass with corrected step length to normalize the spacing between each steps 
			
		stepsCount = 0;
		previous = pos1;	
		interpolationValue = linearStepSize;
	
		float stepLengthOffset = distance(current, pos2) - stepLength;
		stepLength += stepLengthOffset / (float)numSteps;	// Correct segment length to fill the last blank
	
		// Find the number of segments
		for(int i = 1; i < numSteps; i++)
		{	
			// Linear search
			[unroll(4)]
			while(true)
			{					
				current = CubicInterpolate(pos0, pos1, pos2, pos3, interpolationValue);						
				interpolationValue += (distance(current, previous) < stepLength) ? linearStepSize : 0;
			}
	
			float binaryStepSize = linearStepSize * 0.5;
			interpolationValue -= binaryStepSize;

			// Binary search
			[unroll(8)]
			while(true)
			{
				binaryStepSize *= 0.5;	
				current = CubicInterpolate(pos0, pos1, pos2, pos3, interpolationValue);				
				interpolationValue += (stepLength - distance(current, previous) < 0) ? -binaryStepSize : binaryStepSize;									
			}	
		
			rootPoints[i] = interpolationValue;	
				
			stepsCount ++;				
			previous = current;
			interpolationValue += linearStepSize;					
		}	

		for(int i = 0; i < numSteps; i++)
		{
			output.rootPoints[i/4][i%4] = rootPoints[i];
		}
	}	

	//--------------------------------------------------------------------------------------

	struct hsConst
	{
		float tessFactor[2] : SV_TessFactor;
	};

	void HSConst(InputPatch<ds_input, 1> input, uint patchID : SV_PrimitiveID, out hsConst output)
	{
		output.tessFactor[0] = output.tessFactor[1] = ( input[0].globalSphereCount <= 0 ) ? 0 : ceil(sqrt(input[0].globalSphereCount));									
		return;
	}

	[domain("isoline")]
	[partitioning("integer")]
	[outputtopology("point")]
	[outputcontrolpoints(1)]				
	[patchconstantfunc("HSConst")]
	ds_input HS (InputPatch<ds_input, 1> input, uint ID : SV_OutputControlPointID)
	{
		return input[0];
	} 

	//--------------------------------------------------------------------------------------

	struct gs_input
	{	
		int id : INT0;
		float radius : FLOAT0;
		float3 color : FLOAT30;		
		float3 position : FLOAT31;
	};

	[domain("isoline")]
	void DS(hsConst input, const OutputPatch<ds_input, 1> op, float2 uv : SV_DomainLocation, out gs_input output)
	{
		int x = round(uv.y * input.tessFactor[0]);
		int y = round(uv.x * input.tessFactor[0]);		
		int sphereId = x + y * input.tessFactor[0];	
			
		int atomId = sphereId / op[0].numSteps;				
		int stepId = (sphereId % op[0].numSteps);	
    			
		// Find begin step pos	
		int beingStepId = stepId;	
		float beingStepLerp =  op[0].rootPoints[beingStepId / 4][beingStepId % 4];
		float3 beginStepPos = CubicInterpolate(op[0].pos0, op[0].pos1, op[0].pos2, op[0].pos3, beingStepLerp); 		
	
		// Find end step pos
		int endStepId = beingStepId + 1;	
		float endStepLerp =  (endStepId < op[0].numSteps) ? op[0].rootPoints[endStepId / 4][endStepId % 4] : 1; // if this is the last step use 1 for the lerp value
		float3 endStepPos = CubicInterpolate(op[0].pos0, op[0].pos1, op[0].pos2, op[0].pos3, endStepLerp);
	
		// Find mid step pos
		float3 diff = endStepPos - beginStepPos;	
		float3 tangent = normalize(diff);
		float3 baseCenter = beginStepPos + diff * 0.5;	
		float midStepLerp = beingStepLerp + (endStepLerp - beingStepLerp) * 0.5;
	
		// Find binormal
		float3 crossDirection = float3(0,1,0);	
		float3 normal = lerp(op[0].n1, op[0].n2, midStepLerp);	
	
		//float3 binormal = normalize(cross(op[0].pos2 - op[0].pos1, normal));
		float3 binormal = normalize(cross(tangent, normal));
		normal = normalize(cross(tangent, binormal));

		// Do helix rotation of the binormal arround the tangent
		float angleStep = -op[0].twist * (3.14 / 180);
		float angleStart = op[0].segmentId * op[0].numSteps * angleStep;
		float rotationAngle = stepId * angleStep; 
		float4 q = QuaternionFromAxisAngle(tangent, (_EnableTwist == 1) ? rotationAngle : 0 );		
	
		normal = QuaternionTransform(q, normal);	
		binormal = QuaternionTransform(q, binormal);	

		// Get rotation to align with the normal
		float3 from = float3(0,1,0);	// Assuming that the nucleotide is pointing in the up direction
		float3 to = normal;	
		float3 axis = normalize(cross(from, to));
		float cos_theta = dot(normalize(from), normalize(to));
		float angle = acos(cos_theta);
		float4 quat = QuaternionFromAxisAngle(axis, angle);
	
		// Get rotation to align with the binormal
		float3 from2 = QuaternionTransform(quat, float3(1,0,0));	
		float3 to2 = binormal;	
		float3 axis2 = normalize(cross(from2, to2));
		float cos_theta2 = dot(normalize(from2), normalize(to2));
		float angle2 = acos(cos_theta2);
		float4 quat2 = QuaternionFromAxisAngle(axis2, angle2);
	
		// Fetch sphere data
		float4 sphere = _CurveIngredientsAtoms[op[0].localSphereStart + atomId];
		float3 sphereCenter = sphere.xyz;		
	
		sphereCenter = QuaternionTransform(quat, sphereCenter.xyz);
		sphereCenter = QuaternionTransform(quat2, sphereCenter.xyz);
	
		//sphereCenter.xyz = normal * atomId /  op[0].localSphereCount * 8;

		// Use this to draw the coordinate frame for debug
		//int halfSphereCount = op[0].localSphereCount * 0.5;
		//float hdd = atomId - halfSphereCount;
			
		//if(hdd < 0) sphereCenter.xyz = normal * abs(hdd) / halfSphereCount * 5;
		//else sphereCenter.xyz = binormal * abs(hdd) / halfSphereCount * 5;
	
		//baseCenter = lerp(op[0].pos1, op[0].pos2, beingStepLerp);
		//sphereCenter = float3(0,0,0);
	
		//if(beingStepLerp == 0) 
		//{
		//	sphereOffset.xyz = op[0].n1 * atomId / op[0].localSphereCount * 10;
		//}
	
		//if(endStepLerp == 1)
		//{
		//	sphereOffset.xyz = op[0].n2 * atomId / op[0].localSphereCount * 10;
		//}
				
		output.radius = (y >= input.tessFactor[0] || sphereId >= op[0].globalSphereCount) ? 0 : sphere.w * _Scale; // Discard unwanted spheres	
		output.position = baseCenter * _Scale + sphereCenter * _Scale; 
		output.id = op[0].segmentId; //* op[0].numSteps + stepId;
		output.color = op[0].color;
	}

	//--------------------------------------------------------------------------------------

	struct fs_input
	{				
		int id : INT0;
		float2 uv: TEXCOORD0;	
		nointerpolation float radius : FLOAT0;	
		nointerpolation float3 color : FLOAT30;		
		centroid float4 position : SV_Position;	
	};
							
	[maxvertexcount(3)]
	void GS(point gs_input input[1], inout TriangleStream<fs_input> triangleStream)
	{	
		if(input[0].radius <= 0) return;	
		
		float radius = input[0].radius;

		float4 viewPos = mul(UNITY_MATRIX_MV, float4(input[0].position, 1));
		viewPos -= normalize( viewPos ) * input[0].radius;
		float4 projPos = mul(UNITY_MATRIX_P, float4(viewPos.xyz, 1));
		float4 offset = mul(UNITY_MATRIX_P, float4(input[0].radius, input[0].radius, 0, 0));

		//*****//
		
		float triBase = 3.464;
		float triHeigth = 3;
		float triBaseHalf = triBase * 0.5;
		float2 triOffset = float2(triBaseHalf, 1.0);

		fs_input output;
		output.color = input[0].color;
		output.radius = radius;
		output.id = input[0].id;
		output.uv = float2(0, 0) - triOffset;
		output.position = projPos + float4(output.uv * offset.xy, 0, 0);
		triangleStream.Append(output);

		output.uv = float2(triBaseHalf, triHeigth) - triOffset;
		output.position = projPos + float4(output.uv * offset.xy, 0, 0);
		triangleStream.Append(output);	
								
		output.uv = float2(triBase,0) - triOffset;
		output.position = projPos + float4(output.uv * offset.xy, 0, 0);
		triangleStream.Append(output);
	}

	//--------------------------------------------------------------------------------------
	
	//void fs_curve_ingredient(fs_input input, out float4 color : SV_TARGET0, out int id : SV_TARGET1, out float depth : sv_depthgreaterequal) 
	void fs_curve_ingredient(fs_input input, out int color : SV_TARGET0, out int id : SV_TARGET1, out float depth : sv_depthgreaterequal) 
	{					
		float lensqr = dot(input.uv, input.uv);   
		if(lensqr > 1) discard;

		// Find normal
		float3 normal = normalize(float3(input.uv, sqrt(1.0 - lensqr)));		
		
		// Find depth
		float eyeDepth = LinearEyeDepth(input.position.z) + input.radius * (1-normal.z);
		depth = 1 / (eyeDepth * _ZBufferParams.z) - _ZBufferParams.w / _ZBufferParams.z;	
		
		color = float4(input.color,0);
		//color = float4(1,1,1,0);
		color = -123;

		id = -input.id;	
		//id = 12;	
	}

	ENDCG

	SubShader 
	{	
		Pass 
	    {
			ZWrite On

	    	CGPROGRAM				    		
			
			#pragma only_renderers d3d11
			#pragma target 5.0				
			
			#pragma vertex VS
			#pragma hull HS
			#pragma domain DS				
			#pragma geometry GS			
			#pragma fragment fs_curve_ingredient
						
			ENDCG
		}			
	}
	Fallback Off
}	