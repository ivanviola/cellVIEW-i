Shader "Custom/ColorComposite_backup"
{
	Properties
	{
		_MainTex("Base (RGB) Trans (A)", 2D) = "white" {}
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

	uniform float4 _DistanceLevels;

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
		
		

		if (instanceId >= 100000)
		{
			float4 lipidAtomInfo = _LipidAtomInfos[atomId];

			int lipidInstanceId = instanceId - 100000;
			LipidInstanceInfo lipidInstanceInfo = _LipidInstancesInfo[lipidInstanceId];
			ProteinIngredientInfo lipidIngredientInfo = _IngredientsInfo[lipidInstanceInfo.type];

			float4 lipidIngredientColor = _IngredientsColors[lipidInstanceInfo.type];

			
			float ingredientGroupsLerpFactors = 1;
			//float ingredientGroupsLerpFactors = _IngredientGroupsLerpFactors[lipidIngredientInfo.proteinIngredientGroupId];

			float ingredientLocalIndex = _ProteinIngredientsRandomValues[lipidInstanceInfo.type].x;
			float3 ingredientGroupsColorValues = _IngredientGroupsColorValues[lipidIngredientInfo.proteinIngredientGroupId].xyz;
			float3 ingredientGroupsColorRanges = _IngredientGroupsColorRanges[lipidIngredientInfo.proteinIngredientGroupId].xyz;

			float h = ingredientGroupsColorValues.x + (ingredientGroupsColorRanges.x) * (ingredientLocalIndex - 0.5f) * ingredientGroupsLerpFactors;
			float c = ingredientGroupsColorValues.y + (ingredientGroupsColorRanges.y) * (ingredientLocalIndex - 0.5f) * ingredientGroupsLerpFactors;
			float l = ingredientGroupsColorValues.z + (ingredientGroupsColorRanges.z) * (ingredientLocalIndex - 0.5f) * ingredientGroupsLerpFactors;

			//color = float4(d3_hcl_lab(h, c, l), 1);
			//color = float4(HSLtoRGB(float3((h%360)/360.0f, 0.75, 0.55)), 1);
			//color = float4(HSVtoRGB(float3((h%360)/360.0f, 0.75f, 1.0f)), 1);

			if (h >= 0)
			{
				h = h % 360;
			}
			else
			{
				h = 360 - abs(h) % 360;
			}

			color = (_UseHCL == 0) ? float4(HSLtoRGB(float3(h / 360.0f ,c, l)), 1) : float4(d3_hcl_lab(h, c, l), 1);
		}
		else if (instanceId >= 0)
		{
			int colorChoice = 3;
			//int colorChoice = 0;

			AtomInfo atomInfo = _ProteinAtomInfos[atomId];
			ProteinInstanceInfo proteinInstanceInfo = _ProteinInstanceInfo[instanceId];
			ProteinIngredientInfo proteinIngredientInfo = _IngredientsInfo[proteinInstanceInfo.proteinIngredientType];

			IngredientGroupColorInfo ingredientGroupColorInfo = _IngredientGroupsColorInfo[proteinIngredientInfo.proteinIngredientGroupId];
			ProteinIngredientColorInfo proteinIngredientColorInfo = _ProteinIngredientsColorInfo[proteinInstanceInfo.proteinIngredientType];

			// To debug color infos
			//if(_ProteinIngredientsColorInfo[proteinInstanceInfo.proteinIngredientType].numProteinInstances <  1000) discard;	
			//if((float)proteinIngredientColorInfo.screenCoverage / (float)_NumPixels < 0.25) discard;
			//if(proteinIngredientColorInfo.numProteinInstancesVisible < 3000) discard;					

			//color = float4(1,0,0,1);
			//return;

			// Predefined colors
			float4 atomColor = _AtomColors[atomInfo.atomSymbolId];
			float4 aminoAcidColor = _AminoAcidColors[atomInfo.residueSymbolId];
			float4 proteinIngredientsChainColor = _IngredientsChainColors[proteinIngredientInfo.chainColorStartIndex + atomInfo.chainSymbolId];
			float4 proteinIngredientsColor = _IngredientsColors[proteinInstanceInfo.proteinIngredientType];
			float4 ingredientGroupColor = _IngredientGroupsColor[proteinIngredientInfo.proteinIngredientGroupId];
			float4 secondaryStructureColor = (atomInfo.secondaryStructure <= 0) ? float4(1,1,1,1) : (round(atomInfo.secondaryStructure) <= 1) ? float4(1,0,0.5,1) : float4(1,0.7843,0,1);

			// Goodsell coloring
			float4 goodsellColor = (atomInfo.atomSymbolId == 0) ? proteinIngredientsChainColor : proteinIngredientsChainColor * (1- 0.25);
						
			if(colorChoice == 0)
			{
				color = atomColor;
				return;
			}
			else if(colorChoice == 1)
			{
				float4 beginColor = float4(0,0,0,0);
				float4 endColor = float4(0,0,0,0);

				beginColor = (level == 0) ? atomColor : beginColor;
				endColor = (level == 0) ? goodsellColor : endColor;

				beginColor = (level == 1) ? goodsellColor : beginColor;
				endColor = (level == 1) ? proteinIngredientsChainColor : endColor;

				beginColor = (level == 2) ? proteinIngredientsChainColor : beginColor;
				endColor = (level == 2) ? proteinIngredientsColor : endColor;
			
				beginColor = (level == 3) ? proteinIngredientsColor : beginColor;
				endColor = (level == 3) ? ingredientGroupColor : endColor;

				beginColor = (level == 4) ? ingredientGroupColor : beginColor;
				endColor = (level == 4) ? ingredientGroupColor : endColor;
			
				color = lerp(beginColor, endColor, lerpFactor);
				return;
			}
			else if(colorChoice == 2)
			{
				////ingredientgroupcolor is of type color in C#, so i needed to rescale, kind of a hack. Should eventually be changed to vector.
				//ingredientGroupColor.x = ingredientGroupColor.x * 360;
				//ingredientGroupColor.y = ingredientGroupColor.y * 100;
				//ingredientGroupColor.z = ingredientGroupColor.z * 100;
									
				//////*******//
				////test compartment color
				//float h = proteinIngredientsColor.x;
				//float c = proteinIngredientsColor.y;
				//float l = proteinIngredientsChainColor.z;

				//float4 luminances;
				//luminances[0] = ingredientGroupColor.z;
				//luminances[1] = proteinIngredientsColor.z;
				//luminances[2] = proteinIngredientsChainColor.z;
				//luminances[3] = 70;
				//float4x2 HCs = float4x2(ingredientGroupColor.xy, proteinIngredientsColor.xy, proteinIngredientsChainColor.xy, atomColor.xy);
			
				////tbd, temporary replacement because atomcolor is rgb
				//HCs[3][0] = 100;
				//HCs[3][1] =70;

	   // 		float3 hclMelded = getDepthLuminanceManuFormula(_depth , luminances, HCs);
				////tbd, temporary replacement because atomcolor is rgb
				////		HCs[3][0] = 100;
				////		HCs[3][1] =50;
							
				//color = float4(d3_hcl_lab(hclMelded.x, hclMelded.y, hclMelded.z), 1);
			}
			else if(colorChoice == 3)
			{
				float4 atomInfo2 = _ProteinAtomInfos2[atomId];
				float ingredientGroupsLerpFactor = 1;
				//float ingredientGroupsLerpFactors = _IngredientGroupsLerpFactors[proteinIngredientInfo.proteinIngredientGroupId];
				
				int groupId = proteinIngredientInfo.proteinIngredientGroupId;

				float3 ingredientLocalIndex = _ProteinIngredientsRandomValues[proteinInstanceInfo.proteinIngredientType].x;
				float3 ingredientGroupsColorValues = _IngredientGroupsColorValues[groupId].xyz;
				float3 ingredientGroupsColorRanges = _IngredientGroupsColorRanges[groupId].xyz;

				float h = ingredientGroupsColorValues.x + (ingredientGroupsColorRanges.x) * (ingredientLocalIndex - 0.5f) * ingredientGroupsLerpFactor;
				//float h = ingredientGroupsColorValues.x ;
				//float c = ingredientGroupsColorValues.y + (ingredientGroupsColorRanges.y) * (ingredientLocalIndex - 0.5f) * ingredientGroupsLerpFactor;
				//float l = ingredientGroupsColorValues.z + (ingredientGroupsColorRanges.z) * (ingredientLocalIndex - 0.5f) * ingredientGroupsLerpFactor;

				//float s = 0.75f + 0.25f * (ingredientLocalIndex - 0.5f);
				//float l = 1.0f - 0.4f * (ingredientLocalIndex);
				//float l = 0.5f + (ingredientGroupsColorRanges.z) * (ingredientLocalIndex - 0.5f) * ingredientGroupsLerpFactor;

				
				//float l = 0.55;
				//float c = 75;
				//float l = 75;

				//float c = (_UseHCL == 0) ? 0.75 :45;
				//float c = (_UseHCL == 0) ? 0.75 :60;
				//float l = (_UseHCL == 0) ? 0.55 : 75;

				float c = ingredientGroupsColorValues.y + (ingredientGroupsColorRanges.y) * (ingredientLocalIndex - 0.5f) * ingredientGroupsLerpFactor;
				float l = ingredientGroupsColorValues.z + (ingredientGroupsColorRanges.z) * (ingredientLocalIndex - 0.5f) * ingredientGroupsLerpFactor;

				float cc = max(eyeDepth - 10, 0);
				float dd = _AtomDistance - 10;

				//l = 50;
				
				//this is debug
			//	if(_ShowSecondaryStructures)
			//	{				
			//		int secondaryStructure = round(atomInfo.secondaryStructure);
			//		h = (secondaryStructure == 0) ? 0 : (secondaryStructure > 0) ? secondaryStructure * 30 : 0;
			//	}	

				
					
				
				//if(eyeDepth < _AtomDistance)
				//{
				//	if(_ShowAtoms)
				//	{
				//		//l = (atomInfo.atomSymbolId == 0) ? l : l * (0.85 + 0.15 * (eyeDepth/_AtomDistance));
				//		c = (atomInfo.atomSymbolId == 0) ? c : (1 + 0.5 * (1-(eyeDepth/_AtomDistance))) * c;
						
				//		//c = (atomInfo.atomSymbolId == 0) ? c : c * (0.5 + 0.5 * (eyeDepth/40.0f));
				//		//l = (atomInfo.atomSymbolId == 0) ? l : l * (0.85 + 0.15 * (eyeDepth/30.0f));
				//		//c = (atomInfo.atomSymbolId == 0) ? c : c * (1 + 0.25 * (eyeDepth/40.0f));
				//	}
				//}		

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

				//if(true && _ShowAtoms && eyeDepth < _SSBeginDistance)
				////if(_ShowChains && eyeDepth < _ChainBeginDistance)
				//{
				//	float cc = max(eyeDepth - _SSEndDistance, 0);
				//	float dd = _SSBeginDistance - _SSEndDistance;
				//	float ddd = (1-(cc/dd));
										
				//	float lg = l;
				//	lg -= (1-(atomColor.x * 0.5 +  atomColor.y * 0.5 + atomColor.z * 0.0)) * 50;
				//	l = lerp(l, lg, ddd);	
				//}	

				if(_ShowChains && eyeDepth < _ChainBeginDistance)
				{	
					float cc = max(eyeDepth - _ChainEndDistance, 0);
					float dd = _ChainBeginDistance - _ChainEndDistance;
					float ddd = (1-(cc/dd));					
					if(atomInfo.atomSymbolId > 0) l -= 13 * ddd;	
				}	

				//if(_ShowSecondaryStructures && eyeDepth < _ChainBeginDistance)
				//{
				//	float cc = max(eyeDepth - _ChainEndDistance, 0);
				//	float dd = _ChainBeginDistance - _ChainEndDistance;
										

				//	float wedge = min(20 * proteinIngredientInfo.numChains, 180);
				//	float hueShift = wedge / proteinIngredientInfo.numChains;
				//	hueShift *= (1-(cc/dd));
										
				//	float hueLength = hueShift * (proteinIngredientInfo.numChains - 1);
				//	float hueOffset = hueLength * 0.5;

				//	h -=  hueOffset;
				//	h += (atomInfo.chainSymbolId * hueShift);	
				//}


				//if(atomInfo.chainSymbolId == 0) 
				//{					
				//	discard;
				//}

				//if(_ProteinAtomInfos2[atomId].x !=5) 
				//{					
				//	discard;
				//}

				

				//this is old
				//if(_ShowSecondaryStructures && eyeDepth < _SecondaryStructureDistance)
				//{
				//	float factor = (1-(eyeDepth / _SecondaryStructureDistance)) * 35;
				//		//color = lerp(aminoAcidColor, atomColor, 1-factor);	
						
				//		//c = (atomInfo.secondaryStructure <= 0) ? c : (round(atomInfo.secondaryStructure) <= 1) ? c - factor : c + factor;
				//		// MWa: changed this with hue
				//		//l = (atomInfo.secondaryStructure <= 0) ? l : (round(atomInfo.secondaryStructure) <= 1) ? l - factor : l + factor;

				//		float totalnumsecondary = _ProteinAtomInfos2[atomId].y + _ProteinAtomInfos2[atomId].w + 1; //+1 so not zero

				//		float wedge = min(20 * totalnumsecondary, 180);
				//		float hueShift = wedge / totalnumsecondary;

				//		float myvalue = 0;
				//		if(atomInfo2.x < 0){
				//		color = float4(1,1,0,1);
				//			return;}
				//			if(atomInfo2.z > 0){
				//		color = float4(1,1,1,1);
				//			return;}
				//			if(atomInfo2.y < 0){
				//		color = float4(1,1,1,1);
				//			return;}
				//			if(atomInfo2.w < 0){
				//		color = float4(1,1,1,1);
				//			return;}
				//		if(atomInfo2.x - atomInfo2.z == 0){
				//			myvalue = (atomInfo2.y + atomInfo2.w)/2;
				//			}
				//		else{
				//			myvalue = atomInfo2.x>0 ? atomInfo2.x : atomInfo2.y - atomInfo2.z; //z is sheet and negative
				//			if (myvalue >= (atomInfo2.y + atomInfo2.w)/2-0.000001){
				//				myvalue += 1;}

				//			}

						

				//		float cc = max(eyeDepth - 10, 0);
				//		float dd = _SecondaryStructureDistance - 10;
				//		hueShift *= (1-(cc/dd));
				//		float hueLength = hueShift * (totalnumsecondary-1);
				//		float hueOffset = hueLength * 0.5;
										
				//	//	hueLength = hueShift;
				//	//	hueOffset = hueLength * 0.5;

				//		h -=  hueOffset;
				//		h += (myvalue * hueShift);	

				//		float lFactor  = (1-(eyeDepth / _SecondaryStructureDistance)); 
				//		float lg = 60;
				//		lg += atomColor.x * 10;
				//		lg += atomColor.y * 40;
				//		lg += atomColor.z * 2;
				//		//l = lerp(l, lg, lFactor);	

				//		//if(atomInfo.secondaryStructure == 0) l=0;
				//		//MWa: end
				//}

				if (h >= 0)
				{
					h = h % 360;
				}
				else
				{
					h = 360 - abs(h) % 360;
				}
				
								

				color = (_UseHCL == 0) ? float4(HSLtoRGB(float3(h / 360.0f , c, l)), 1) : float4(d3_hcl_lab(h, c, l), 1);
				color.xyz = max(color.xyz, float3(0,0,0));
				color.xyz = min(color.xyz, float3(1,1,1));
							

				if(_ShowAtoms && eyeDepth < _AtomBeginDistance)
				{
					float cc = max(eyeDepth - _AtomEndDistance, 0);
					float dd = _AtomBeginDistance - _AtomEndDistance;
					float ddd = (1-(cc/dd));

					color.xyz = (lerp(color.xyz, atomColor.xyz, ddd));		
				}

				

				//if(eyeDepth < _AtomDistance)
				//{
				//	if(_ShowAtoms)
				//	{
				//		float factor = eyeDepth / _AtomDistance;
				//		color = lerp(aminoAcidColor, atomColor, 1-factor);						
				//	}
				//}

				//if(eyeDepth > _AtomDistance && eyeDepth < _ResidueDistance)
				//{
				//	if(_ShowResidues)
				//	{
				//		float dd = _ResidueDistance - _AtomDistance;
				//		float factor = (eyeDepth -_AtomDistance)/ dd;
				//		color = lerp(secondaryStructureColor, aminoAcidColor, 1-factor);						
				//	}
				//}

				//if(eyeDepth > _ResidueDistance && eyeDepth < _SecondaryStructureDistance)
				//{
				//	if(_ShowSecondaryStructures)
				//	{
				//		float dd = _SecondaryStructureDistance - _ResidueDistance;
				//		float factor = (eyeDepth -_ResidueDistance)/ dd;
				//		color = lerp(color, secondaryStructureColor, 1-factor);						
				//	}
				//}

				//if(eyeDepth < _SecondaryStructureDistance)
				//{
				//	if(_ShowSecondaryStructures)
				//	{
				//		float factor = (1-((eyeDepth - _AtomDistance) / (_SecondaryStructureDistance - _AtomDistance)));
				//		color = lerp(color, secondaryStructureColor, factor);							
				//		//c = (atomInfo.secondaryStructure <= 0) ? c : (round(atomInfo.secondaryStructure) <= 1) ? c - factor : c + factor;
				//		//l = (atomInfo.secondaryStructure <= 0) ? l : (round(atomInfo.secondaryStructure) <= 1) ? l - factor : l + factor;
				
				//	}
				//}										
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
		//if(reuw)
		{
			float dde = clamp((ddddd - _DesaturationEndDistance) / (_DeturationBeginDistance -_DesaturationEndDistance),0, 1);
			//color = float4(dde,0,0,1);

			float dddd =  clamp(ddd / 2.5, 0, 1);

			float3 hsv =  RGBtoHSV(color.xyz);
			hsv.y *= max(clamp(1-(dddd * (1-dde)),0, 1),0.15);				
			color.xyz =  HSVtoRGB(hsv);
		}
	}
	
	//void frag2(v2f_img i, out float4 color : COLOR0)
	//{
	//	int2 uv = i.uv * _ScreenParams.xy;

	//	int id = _IdTexture[uv];

	//	color = float4(1, 0, 0, 1);

	//	if (id >= 0)
	//	{
	//		float4 proteinInfo = _ProteinInstanceInfo[id];
	//		float4 ingredientGroupInfo = _IngredientProperties[proteinInfo.x];

	//		////*******//

	//		float ingredientGroupsLerpFactors = _IngredientGroupsLerpFactors[ingredientGroupInfo.x];

	//		float3 proteinRandomValues = _ProteinIngredientsRandomValues[proteinInfo.x].xyz;
	//		float3 ingredientGroupsColorValues = _IngredientGroupsColorValues[ingredientGroupInfo.x].xyz;
	//		float3 ingredientGroupsColorRanges = _IngredientGroupsColorRanges[ingredientGroupInfo.x].xyz;

	//		float h = ingredientGroupsColorValues.x + (ingredientGroupsColorRanges.x) * (proteinRandomValues.x - 0.5f) * ingredientGroupsLerpFactors;
	//		float c = ingredientGroupsColorValues.y + (ingredientGroupsColorRanges.y) * (proteinRandomValues.x - 0.5f) * ingredientGroupsLerpFactors;
	//		float l = ingredientGroupsColorValues.z + (ingredientGroupsColorRanges.z) * (proteinRandomValues.x - 0.5f) * ingredientGroupsLerpFactors;

	//		color = float4(d3_hcl_lab(h, c, l), 1);
	//	}
	//	else
	//	{
	//		discard;
	//	}
	//}
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

		/*Pass
		{
			ZTest Always

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert_img
			#pragma fragment frag2	

			#pragma target 5.0	
			#pragma only_renderers d3d11	

			ENDCG
				
		}*/
	}

	FallBack "Diffuse"
}