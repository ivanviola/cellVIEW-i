Shader "Custom/ColorComposite"
{
	Properties
	{
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
		_ChromaShiftNonSelect("Chroma Shift (not-highlighted)", Int) = 50
		_LumShiftNonSelect("Luminance Shift (not-highlighted)", Int) = 20
		_ChromaShiftSelect("Chroma Shift (highlighted)", Int) = 20
		_LumShiftSelect("Luminance Shift (highlighted)", Int) = 10
		_RNAHue("RNA Hue", Float) = 0
		_RNAChroma("RNA Chroma", Float) = 0
		_RNALuminance("RNA Luminance", Float) = 0
	}
		
	CGINCLUDE

	// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
	#pragma exclude_renderers gles

	#include "UnityCG.cginc"
	#include "Helper.cginc"

	
	float3 getDepthLuminanceManuFormula(float depthvalue, float4 luminances, float4x2 HCs)
	{
		float omega = 0.5;
		int indexlevel = floor((depthvalue) * 3);

		float level = floor((depthvalue)* 3) / 3;
		float lprev = 0;
		float alpha = 1-((level + 0.33) - depthvalue) * 3;
		if (indexlevel > 0) {
			lprev = (1 - alpha)*luminances[indexlevel - 1] + alpha*luminances[indexlevel];
		}
		else {
			lprev = luminances[indexlevel];
		}
		float lnext = 0;
		


		if (indexlevel == 2) {
			lnext = luminances[indexlevel + 1];
		}
		else {
			if(indexlevel<2)
			lnext = (1 - alpha)*luminances[indexlevel + 1] +alpha*luminances[indexlevel+2];
		}


		float lum = (1 - omega)*lprev + omega * 50 + omega*(lnext - 50);

		//calculate hue


		float h1 = HCs[indexlevel][0];
		float h2 = HCs[indexlevel + 1][0];
	//	float angle;
		float c1 = HCs[indexlevel][1];
		float c2 = HCs[indexlevel + 1][1];
	/*	if (abs(alpha*h2 - (1-alpha)*h1)>180)
		{
			angle = (alpha*h2+h1) + 180;
		}
		else {
			angle = (alpha*h2 + h1);
		}*/




		float2 ab1 = HC_ab(h1, c1);
		float2 ab2 = HC_ab(h2, c2);

		float2 abBlend = (1-alpha)*ab1 + alpha*ab2;
		float chroma = ab_chroma(abBlend.x, abBlend.y);
		float d3_radians = 0.01745329252;
		float angle = ab_hue(abBlend.x, abBlend.y)/d3_radians;

//		angle = alpha*h2 + (1-alpha)*h1;


//		chroma =  (1 - alpha)*HCs[indexlevel][1] + alpha*HCs[indexlevel + 1][1];
//		float3 result = float3(angle, chroma, lum);
		return float3(angle, chroma, lum);
	}
	
	StructuredBuffer<ProteinIngredientInfo> _IngredientsInfo;
	
	StructuredBuffer<float4> _LipidAtomInfos;
	StructuredBuffer<AtomInfo> _ProteinAtomInfos;
	StructuredBuffer<float4> _ProteinAtomInfos2;
	StructuredBuffer<ProteinInstanceInfo> _ProteinInstanceInfo;
		
	StructuredBuffer<LipidInstanceInfo> _LipidInstancesInfo;	
	StructuredBuffer<float4> _LipidInstancesInfo2;	

	StructuredBuffer<IngredientGroupColorInfo> _IngredientGroupsColorInfo;
	StructuredBuffer<ProteinIngredientColorInfo> _ProteinIngredientsColorInfo;

	//*****//

	StructuredBuffer<float4> _AtomColors;
	StructuredBuffer<float4> _AminoAcidColors;
	StructuredBuffer<float4> _IngredientsColors;
	StructuredBuffer<float4> _IngredientsChainColors;
	StructuredBuffer<float4> _IngredientGroupsColor;

	//*****//

	StructuredBuffer<float> _IngredientGroupsLerpFactors;
	StructuredBuffer<float4> _IngredientGroupsColorValues;
	StructuredBuffer<float4> _IngredientGroupsColorRanges;
	StructuredBuffer<float4> _ProteinIngredientsRandomValues;

	//*****//

	uniform Texture2D<int> _AtomIdBuffer;
	uniform Texture2D<int> _InstanceIdBuffer;
	
	uniform Texture2D<float> _DepthBuffer;

	uniform int _NumPixels;
	uniform int _DistanceMax;
	uniform int _NumLevelMax;
	uniform int _UseDistanceLevels;
	
	uniform float4x4 _LevelRanges;
	uniform float _LevelLerpFactor;

	
	uniform int _UseHCL;
	uniform int _ShowAtoms;
	uniform int _ShowChains;
	uniform int _ShowResidues;
	uniform int _ShowSecondaryStructures;

	uniform float _ChainDistance;
	uniform float _AtomDistance;
	uniform float _ResidueDistance;
	uniform float _SecondaryStructureDistance;

	//uniform float _depth;
	//*****//

	uniform float4 _FocusSphere;
	uniform float4x4 _ProjectionMatrix;
	uniform float4x4 _InverseViewMatrix;

	/*******/
	uniform int _SomethingIsSelected;
	/********/
	uniform float4 _DistanceLevels;

	int _ChromaShiftNonSelect;
	int	_LumShiftNonSelect;
	int	_ChromaShiftSelect;
	int	_LumShiftSelect;

	float _RNAHue;
	float _RNAChroma;
	float _RNALuminance;

	uniform float _RnaSelected;

	int GetLevelDistance(int level)
	{
		int2 coord = int2(level % 4, level / 4);
		return _LevelRanges[coord.x][coord.y];
	}

	void frag1(v2f_img i, out float4 color : COLOR0)
	{
		color = float4(1,0,0,1);
		//color = float4(0.9, 0.6, 0.4, 1);
		//return; // debug
		int2 uv = i.uv * _ScreenParams.xy;
			
		float vz = LinearEyeDepth(_DepthBuffer[uv]);
		float2 p11_22 = float2(_ProjectionMatrix._11, _ProjectionMatrix._22);
		float3 vpos = float3((i.uv * 2 - 1) / p11_22, -1) * vz;
		float4 wpos = mul(_InverseViewMatrix, float4(vpos, 1));
				
		int atomId = _AtomIdBuffer[uv];
		int instanceId = _InstanceIdBuffer[uv];
		float eyeDepth = min(abs(LinearEyeDepth(_DepthBuffer[uv])), _DistanceMax);

		int level = floor(_LevelLerpFactor);
		float lerpFactor = _LevelLerpFactor - level;

		if(_UseDistanceLevels)
		{
			level = -1;

			int beginRange = 0;
			int endRange = 0;
			for(int i = 0; i < _NumLevelMax; i++)
			{
				if(eyeDepth <= GetLevelDistance(i))
				{
					level = i;
					beginRange = (i == 0) ? 0 : GetLevelDistance(i-1);
					endRange = GetLevelDistance(i);
					break;
				}
			}

			int lengthCurrentEyePosSegment = eyeDepth - beginRange; 
			int lengthTotalSegment = endRange - beginRange; 
			lerpFactor =  (float)lengthCurrentEyePosSegment / (float)lengthTotalSegment;
		}		
		
		if (instanceId == -123) { // RNA instance (yeah it's a very specific...

			float h = _RNAHue;
			float c = _RNAChroma;
			float l = _RNALuminance;

			float rnaState = _RnaSelected;
			//float rnaState = 0;

			if (_SomethingIsSelected == 1) 
			{
				if (rnaState == 1) // means it is Highlighted
				{
					 //c = 500;
					 c += _ChromaShiftSelect / 140.0f;
					 l += _LumShiftSelect / 100.0f;
				}
				else
				{
					c -= _ChromaShiftNonSelect / 140.0f;
					l -= _LumShiftNonSelect / 100.0f;
				}
			}

			if (h >= 0)
			{
				h = h % 360;
			}
			else
			{
				h = 360 - abs(h) % 360;
			}

			color = float4(HSLtoRGB(float3(h / 360.0f ,c, l)), 1);

		} 
		else if (instanceId >= 100000) // LIPIDS
		{
			float4 lipidAtomInfo = _LipidAtomInfos[atomId];

			int lipidInstanceId = instanceId - 100000;
			LipidInstanceInfo lipidInstanceInfo = _LipidInstancesInfo[lipidInstanceId];
			ProteinIngredientInfo lipidIngredientInfo = _IngredientsInfo[lipidInstanceInfo.type];
			
			float4 highlightingInfo = _LipidInstancesInfo2[lipidInstanceId];
			int instanceState = highlightingInfo.x;

			float4 lipidIngredientColor = _IngredientsColors[lipidInstanceInfo.type];

			
			float ingredientGroupsLerpFactors = 1;
			
			float ingredientLocalIndex = _ProteinIngredientsRandomValues[lipidInstanceInfo.type].x;
			float3 ingredientGroupsColorValues = _IngredientGroupsColorValues[lipidIngredientInfo.proteinIngredientGroupId].xyz;
			float3 ingredientGroupsColorRanges = _IngredientGroupsColorRanges[lipidIngredientInfo.proteinIngredientGroupId].xyz;

			float h = ingredientGroupsColorValues.x + (ingredientGroupsColorRanges.x) * (ingredientLocalIndex - 0.5f) * ingredientGroupsLerpFactors;
			float c = ingredientGroupsColorValues.y + (ingredientGroupsColorRanges.y) * (ingredientLocalIndex - 0.5f) * ingredientGroupsLerpFactors;
			float l = ingredientGroupsColorValues.z + (ingredientGroupsColorRanges.z) * (ingredientLocalIndex - 0.5f) * ingredientGroupsLerpFactors;

			if (h >= 0)
			{
				h = h % 360;
			}
			else
			{
				h = 360 - abs(h) % 360;
			}

			if (_SomethingIsSelected == 1) // if something is selected then I need to make sure to "unlocor" Normal instances and "turn up" the color on Highlighted instances
			//instanceState = 1;
			//if (true) // if something is selected then I need to make sure to "unlocor" Normal instances and "turn up" the color on Highlighted instances
			{
				if (instanceState == 1) // means it is Highlighted
				{
					 //c = 500;
					 c += _ChromaShiftSelect;
					 l += _LumShiftSelect;
				}
				else
				{
					c -= _ChromaShiftNonSelect;
					l -= _LumShiftNonSelect;
				}
				// if (this instance has state highlighted) {
				//		turn the saturation higher
				// } else {
				//		turn the saturation lower
				// }
			}

			color = (_UseHCL == 0) ? float4(HSLtoRGB(float3(h / 360.0f ,c, l)), 1) : float4(d3_hcl_lab(h, c, l), 1);
		}
		else if (instanceId >= 0)
		{
			AtomInfo atomInfo = _ProteinAtomInfos[atomId];
			ProteinInstanceInfo proteinInstanceInfo = _ProteinInstanceInfo[instanceId];
			ProteinIngredientInfo proteinIngredientInfo = _IngredientsInfo[proteinInstanceInfo.proteinIngredientType];

			int instanceState = proteinInstanceInfo.state;

			IngredientGroupColorInfo ingredientGroupColorInfo = _IngredientGroupsColorInfo[proteinIngredientInfo.proteinIngredientGroupId];
			ProteinIngredientColorInfo proteinIngredientColorInfo = _ProteinIngredientsColorInfo[proteinInstanceInfo.proteinIngredientType];

			// Predefined colors
			float4 atomColor = _AtomColors[atomInfo.atomSymbolId];
			float4 aminoAcidColor = _AminoAcidColors[atomInfo.residueSymbolId];
			float4 proteinIngredientsChainColor = _IngredientsChainColors[proteinIngredientInfo.chainColorStartIndex + atomInfo.chainSymbolId];
			float4 proteinIngredientsColor = _IngredientsColors[proteinInstanceInfo.proteinIngredientType];
			float4 ingredientGroupColor = _IngredientGroupsColor[proteinIngredientInfo.proteinIngredientGroupId];
			float4 secondaryStructureColor = (atomInfo.secondaryStructure <= 0) ? float4(1,1,1,1) : (round(atomInfo.secondaryStructure) <= 1) ? float4(1,0,0.5,1) : float4(1,0.7843,0,1);

			// Goodsell coloring
			float4 goodsellColor = (atomInfo.atomSymbolId == 0) ? proteinIngredientsChainColor : proteinIngredientsChainColor * (1- 0.25);
			
			float4 atomInfo2 = _ProteinAtomInfos2[atomId];
			float ingredientGroupsLerpFactor = 1;
				
			int groupId = proteinIngredientInfo.proteinIngredientGroupId;

			float3 ingredientLocalIndex = _ProteinIngredientsRandomValues[proteinInstanceInfo.proteinIngredientType].x;
			float3 ingredientGroupsColorValues = _IngredientGroupsColorValues[groupId].xyz;
			float3 ingredientGroupsColorRanges = _IngredientGroupsColorRanges[groupId].xyz;

			float h = ingredientGroupsColorValues.x + (ingredientGroupsColorRanges.x) * (ingredientLocalIndex - 0.5f) * ingredientGroupsLerpFactor;

			float c = ingredientGroupsColorValues.y + (ingredientGroupsColorRanges.y) * (ingredientLocalIndex - 0.5f) * ingredientGroupsLerpFactor;
			float l = ingredientGroupsColorValues.z + (ingredientGroupsColorRanges.z) * (ingredientLocalIndex - 0.5f) * ingredientGroupsLerpFactor;

			float cc = max(eyeDepth - 10, 0);
			float dd = _AtomDistance - 10;	

			float _ChainBeginDistance = 50;
			float _ChainEndDistance = 15;

			float _SSBeginDistance = _ChainEndDistance;
			float _SSEndDistance = 7;

			float _AtomBeginDistance = _SSEndDistance;
			float _AtomEndDistance = 2;

			if(_ShowChains && eyeDepth < _ChainBeginDistance && proteinIngredientInfo.numChains > 1)
			{
				float cc = max(eyeDepth - _ChainEndDistance, 0);
				float dd = _ChainBeginDistance - _ChainEndDistance;
				float ddd = (1-(cc/dd));				

				float wedge = min(50 * proteinIngredientInfo.numChains, 180);
				float hueShift = wedge / proteinIngredientInfo.numChains;
				hueShift *= ddd;
										
				float hueLength = hueShift * (proteinIngredientInfo.numChains - 1);
				float hueOffset = hueLength * 0.5;

				h -=  hueOffset;
				h += (atomInfo.chainSymbolId * hueShift);						
			}

			if(_ShowChains && _ShowSecondaryStructures && eyeDepth < _ChainBeginDistance)
			{
				float cc = max(eyeDepth - _ChainEndDistance, 0);
				float dd = _ChainBeginDistance - _ChainEndDistance;
				float ddd = (1-(cc/dd));				

				float lumaShift = 10;
				lumaShift *= ddd;					
				l = (atomInfo.secondaryStructure == 0) ? l : (round(atomInfo.secondaryStructure) > 0) ? l - lumaShift : l + lumaShift;
			}
			
			if(_ShowSecondaryStructures && eyeDepth < _SSBeginDistance)
			{
				float cc = max(eyeDepth - _SSEndDistance, 0);
				float dd = _SSBeginDistance - _SSEndDistance;
				float ddd = (1-(cc/dd));

				float faktor = 45;
				faktor *= ddd;
						
				h = (atomInfo.secondaryStructure == 0) ? h : (round(atomInfo.secondaryStructure) > 0) ? h - faktor : h + faktor;
			}	

			if(_ShowChains && eyeDepth < _ChainBeginDistance)
			{	
				float cc = max(eyeDepth - _ChainEndDistance, 0);
				float dd = _ChainBeginDistance - _ChainEndDistance;
				float ddd = (1-(cc/dd));					
				if(atomInfo.atomSymbolId > 0) l -= 13 * ddd;	
			}	

			if (h >= 0)
			{
				h = h % 360;
			}
			else
			{
				h = 360 - abs(h) % 360;
			}

			/* HIGHLIGHTING */
			//instanceState = 0;
			if (_SomethingIsSelected == 1) // if something is selected then I need to make sure to "unlocor" Normal instances and "turn up" the color on Highlighted instances
			//if (true) // if something is selected then I need to make sure to "unlocor" Normal instances and "turn up" the color on Highlighted instances
			{
				if (instanceState == 1) // means it is Highlighted
				{
					 //c = 500;
					 c += _ChromaShiftSelect;
					 l += _LumShiftSelect;
				}
				else
				{
					c -= _ChromaShiftNonSelect;
					l -= _LumShiftNonSelect;
				}
				// if (this instance has state highlighted) {
				//		turn the saturation higher
				// } else {
				//		turn the saturation lower
				// }
			}

			color = (_UseHCL == 0) ? float4(HSLtoRGB(float3(h / 360.0f , c, l)), 1) : float4(d3_hcl_lab(h, c, l), 1);
			color.xyz = max(color.xyz, float3(0,0,0));
			color.xyz = min(color.xyz, float3(1,1,1));				

			// if we should start showing atoms
			if(_ShowAtoms && eyeDepth < _AtomBeginDistance)
			{
				float cc = max(eyeDepth - _AtomEndDistance, 0);
				float dd = _AtomBeginDistance - _AtomEndDistance;
				float ddd = (1-(cc/dd));

				color.xyz = (lerp(color.xyz, atomColor.xyz, ddd));		
			}
		}
		else
		{
			// abort the shader for this pixel
			discard;
		}

		float ddd = max(distance(_FocusSphere.xyz, wpos) - _FocusSphere.w, 0);				
		float ddddd = distance(_WorldSpaceCameraPos, _FocusSphere.xyz);

		float _DeturationBeginDistance = 25;
		float _DesaturationEndDistance = 10;

		
		if(	ddd > 0 &&  ddddd < 40 && false)
		{
			float dde = clamp((ddddd - _DesaturationEndDistance) / (_DeturationBeginDistance -_DesaturationEndDistance),0, 1);

			float dddd =  clamp(ddd / 2.5, 0, 1);

			float3 hsv =  RGBtoHSV(color.xyz);
			hsv.y *= max(clamp(1-(dddd * (1-dde)),0, 1),0.15);				
			color.xyz =  HSVtoRGB(hsv);

		}
	}
	ENDCG
	
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }		

		Pass
		{
			ZTest Always

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert_img
			#pragma fragment frag1	

			#pragma target 5.0	
			#pragma only_renderers d3d11	
			
			ENDCG
		}

	}

	FallBack "Diffuse"
}