Shader "Custom/Composite" 
{
	Properties {
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	}

	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		
		// *****
		Pass
		{
			ZWrite On
			ZTest Less

			CGPROGRAM
#pragma target 5.0	
			#pragma fragment frag
			#pragma vertex vert_img

			#include "UnityCG.cginc"

			sampler2D _ColorTexture;
			sampler2D_float _DepthTexture;

			void frag(v2f_img i,  out float4 color : COLOR, out float depth : DEPTH)
			{
				color = tex2D(_ColorTexture, i.uv);
				depth = tex2D(_DepthTexture, i.uv);
			}
			ENDCG
		}

		// *****
		Pass
		{
			ZWrite On
			ZTest Always

			CGPROGRAM
#pragma target 5.0	
			#pragma fragment frag
			#pragma vertex vert_img

			sampler2D_float _DepthTexture;
			sampler2D_float _CameraDepthTexture;

			#include "UnityCG.cginc"
			
			void frag(v2f_img i, out float depth : DEPTH)
			{
				float customDepth = tex2D(_DepthTexture, i.uv);
				float cameraDepth = tex2D(_CameraDepthTexture, i.uv);
				depth = customDepth < cameraDepth ? customDepth : cameraDepth;
			}
			ENDCG
		}

		// *****
		Pass
		{
			ZWrite On
			ZTest Always

			CGPROGRAM
#pragma target 5.0	
			#pragma fragment frag
			#pragma vertex vert_img

			sampler2D_float _DepthTexture;
			sampler2D_float _CameraDepthTexture;
			sampler2D_float _CameraDepthNormalsTexture;

			#include "UnityCG.cginc"

			void frag(v2f_img i, out float4 depthNormals : COLOR)
			{
				float customDepth = tex2D(_DepthTexture, i.uv);
				float cameraDepth = tex2D(_CameraDepthTexture, i.uv);
				depthNormals = customDepth < cameraDepth ? float4(0.48,0.52,0,0) : tex2D(_CameraDepthNormalsTexture, i.uv);
			}
			ENDCG
		}

		Pass 
		{
			ZTest Always

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag	
					
			#pragma target 5.0	
			#pragma only_renderers d3d11	

            #include "UnityCG.cginc"
			#include "Helper.cginc"	

			

			uniform Texture2D<int> _IdTexture;
			
			/*StructuredBuffer<int> _IngredientStates;
			StructuredBuffer<float4> _ProteinColors;
			StructuredBuffer<float4> _ProteinInstanceInfo;			
			StructuredBuffer<float4> _LipidInstanceInfo;	*/		

            void frag(v2f_img i, out float4 color : COLOR0) 
			{   
				int2 uv = i.uv * _ScreenParams.xy; 
				int id = _IdTexture[uv];

				color = float4(1, 0, 1, 1);

				if (id >= 0)
				{
					//discard;
					//color = float4(1, 1, 0, 1);
				}
				else
				{
					discard;
				}

				/*color = float4(1, 0, 0, 1);
				return;*/

				//if(id == -3)
				//{
				//	color = float4(1,0,0,0);
				//}
				//else if(id == -2)
				//{
				//	color = float4(1,240.0/255.0,114.0/255.0,0);
				//}				
				
				//if(id >= 0)
				//{
				//	int ingredientId = -1;

				//	// if is lipid
				//	if(id >= 100000)
				//	{		
				//		id -= 100000;
				//		float4 lipidInfo = _LipidInstanceInfo[id];		
				//		color = float4(ColorCorrection((lipidInfo.x > 42) ? float3(0.5,0.4,0.0) : float3(0.5,0.6,0.0)), 1);
				//		
				//		ingredientId = lipidInfo.x;
				//	}
				//	else
				//	{
				//		float4 proteinInfo = _ProteinInstanceInfo[id];
				//		float4 proteinColor = _ProteinColors[proteinInfo.x];

				//		float diffuse = proteinInfo.z;
				//		//color = float4(ColorCorrection(proteinColor.xyz) * diffuse, 1);	
				//		color = float4(ColorCorrection(proteinColor.xyz), 1);	

				//		ingredientId = proteinInfo.x;
				//	}	
				//	
				//	int state = _IngredientStates[ingredientId];					
				//	if(state == 1)
				//	{
				//		color = float4(OffsetHSL(color.xyz, float3(0, 0, 0)), 1);
				//	}
				//	else if(state == 2)
				//	{
				//		color = float4(OffsetHSL(color.xyz, float3(0, 0.5, 0.1)), 1);
				//	}				
				//}
				//else
				//{
				//	discard;
				//}
            }
            
            ENDCG
        }

		Pass
		{
			ZTest Always

			CGPROGRAM
			#pragma target 5.0	
			#pragma fragment frag
			#pragma vertex vert_img

			sampler2D _MainTex;

			#include "UnityCG.cginc"

			void frag(v2f_img i, out float4 color : COLOR)
			{
				float4 c = tex2D(_MainTex, i.uv);
				if(c.z > 0) color = float4(c.z/50,0,0,0);
				else color = float4(0,0,0,0);

				//if(c.z < 0) color = float4(1,0,0,1);
				//else color = float4(0,0,1,1);
			}
			ENDCG
		}


		// Collect Shadows
		Pass
		{
			Cull Off ZWrite Off ZTest Always

			CGPROGRAM

			#pragma target 5.0	
			#pragma fragment frag
			#pragma vertex vert_img
						
			#include "UnityCG.cginc"
			#include "HLSLSupport.cginc"
			#include "Poisson.cginc"
			
			sampler2D _MainTex;
			sampler2D_float _DepthTexture; 
			float4x4 _InverseView;
			
			sampler2D_float _ShadowMap;
			float4x4 _ShadowCameraViewMatrix;
			float4x4 _ShadowCameraViewProjMatrix;
 			
			float _LightSize;  // 2.0
			float _ShadowOffset;  // 0.25 
			float _ShadowFactor; // 2
			float _ShadowBias;

			// Custom texture lookup functions from http://codeflow.org/entries/2013/feb/15/soft-shadow-mapping/
			float texture2DCompare(sampler2D depths, float2 uv, float compare)
			{
				float depth = tex2D(depths, uv).r;

				if(abs(depth-compare) < _ShadowBias) return 1;

				return step(compare, depth);
			}

			float texture2DShadowLerp(sampler2D depths, float2 size, float2 uv, float compare)
			{
				float2 texelSize = float2(1.0,1.0)/size;
				float2 f = frac(uv*size+0.5);
				float2 centroidUV = floor(uv*size+0.5)/size;

				float lb = texture2DCompare(depths, centroidUV+texelSize*float2(0.0, 0.0), compare);
				float lt = texture2DCompare(depths, centroidUV+texelSize*float2(0.0, 1.0), compare);
				float rb = texture2DCompare(depths, centroidUV+texelSize*float2(1.0, 0.0), compare);
				float rt = texture2DCompare(depths, centroidUV+texelSize*float2(1.0, 1.0), compare);
				float a = lerp(lb, lt, f.y);
				float b = lerp(rb, rt, f.y);
				float c = lerp(a, b, f.x);
				return c;
			}
			
			#define PCF_FILTER_STEP_COUNT 5
			#define PCF_DIM (PCF_FILTER_STEP_COUNT * 2 + 1)
			#define PCF_COUNT (PCF_DIM * PCF_DIM)

            float PCF(sampler2D depths, float2 size, float2 uv, float compare) 
			{
                float result = 0.0;
                for(int x=-PCF_FILTER_STEP_COUNT; x<=PCF_FILTER_STEP_COUNT; x++)
				{
                    for(int y=-PCF_FILTER_STEP_COUNT; y<=PCF_FILTER_STEP_COUNT; y++)
					{
                            float2 offset = float2(x,y)/size * _LightSize;
                            result += texture2DShadowLerp(depths, size, uv + offset, compare);
                    }
                }
                return result/PCF_COUNT;
            }

			//#define PCF_POISSON_COUNT 32
			//#define PCF_POISSON Poisson32

			//#define PCF_POISSON_COUNT 64
			//#define PCF_POISSON Poisson64

			//#define PCF_POISSON_COUNT 100
			//#define PCF_POISSON Poisson100

			#define PCF_POISSON_COUNT 128
			#define PCF_POISSON Poisson128

			#define ENABLE_SHADOWS 1

            float PCF_Poisson(sampler2D depths, float2 size, float2 uv, float compare) 
			{
                float result = 0.0;
                for (int i = 0; i < PCF_POISSON_COUNT; ++i)
				{
					float2 offset = PCF_POISSON[i] / size  * _LightSize;					
					result += texture2DShadowLerp(depths, size, uv + offset, compare);
				}	
                return result/PCF_POISSON_COUNT;
            }

			fixed4 frag (v2f_img i) : SV_Target
			{
				float vz = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_DepthTexture, i.uv));
				float2 p11_22 = float2(unity_CameraProjection._11, unity_CameraProjection._22);
				float3 vpos = float3((i.uv * 2 - 1) / p11_22, -1) * vz;
				float4 wpos = mul(_InverseView, float4(vpos, 1));

				float shadow = 1.0;
				half4 source = tex2D(_MainTex, i.uv);

				#if ENABLE_SHADOWS == 1

					//float eyePos = abs(mul(_ShadowCameraViewMatrix, float4(wpos.xyz, 1)).z) - 0.5;
					float eyePos = abs(mul(_ShadowCameraViewMatrix, float4(wpos.xyz, 1)).z) - _ShadowBias;

					float4 fragmentLightProjPos = mul(_ShadowCameraViewProjMatrix, float4(wpos.xyz, 1));
					fragmentLightProjPos /= fragmentLightProjPos.w;
															
					float2 uv = fragmentLightProjPos.xy * 0.5 + 0.5;	
					float z = fragmentLightProjPos.z;	

					//shadowFactor = clamp(PCF(_ShadowMap, float2(1024,1024), uv, z - 0.000001) * 1, 0,1);
					//shadow = PCF(_ShadowMap, float2(1024,1024), uv, eyePos);
					shadow = PCF_Poisson(_ShadowMap, float2(1024,1024), uv, eyePos);

				#endif	

				return half4(source.xyz * min(_ShadowOffset + shadow * _ShadowFactor, 1), 1);	

				//if(false)
				//{
				//	// Find the view position of the current fragment from the ligth point of view
				//	float shadowCameraViewPos = abs(mul(_ShadowCameraViewMatrix, float4(wpos.xyz, 1)).z)- 0.5;

				//	// Find the projected position of the current fragment from the ligth point of view
				//	float4 shadowCameraProjPos = mul(_ShadowCameraViewProjMatrix, float4(wpos.xyz, 1));
					
				//	// Find the UV coordinate of the fragment in the texture map
				//	float2 shadowMapTexCoord =	shadowCameraProjPos.xy /= shadowCameraProjPos.w;
				//	shadowMapTexCoord.xy = shadowMapTexCoord.xy * 0.5 + 0.5;
					
				//	float 
					
				//	//shadowMapTexCoord.y = 1 - shadowMapTexCoord.y;

				//	// Get the depth of the closest fragment to the light camera
				//	float shadowMapBlockerViewPos = tex2D(_ShadowMap, shadowMapTexCoord.xy);		
					
				//	// Find shadow factor					
				//	//shadowFactor = (shadowMapBlockerViewPos > shadowCameraViewPos ) ? 0.5 : 1.0;
				//	shadowFactor = ( shadowMapBlockerViewPos <= shadowCameraViewPos ) ? 0.55 : 0.99;					

				//	float sum = 0;

				//	//float2 stepUV = float2(0.01, 0.01) / PCF_FILTER_STEP_COUNT;
				//	//for (float x = -PCF_FILTER_STEP_COUNT; x <= PCF_FILTER_STEP_COUNT; ++x)
				//	//{
				//	//	for (float y = -PCF_FILTER_STEP_COUNT; y <= PCF_FILTER_STEP_COUNT; ++y)
				//	//	{
				//	//		float2 offset = float2(x, y) * stepUV;
				//	//		float z = BiasedZ(z0, dz_duv, offset);
				//	//		sum += g_shadowMap.SampleCmpLevelZero(PCF_Sampler, uv + offset, z);
				//	//	}
				//	//}

				//	shadowFactor = sum / PCF_COUNT;
				//}	
			}
			ENDCG
		}

		Pass 
		{
			ZTest Always

            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag	
					
			#pragma target 5.0	
			#pragma only_renderers d3d11	

            #include "UnityCG.cginc"
			#include "Helper.cginc"	

			sampler2D _MainTex;
			Texture2D<int> _IdTexture; 
			StructuredBuffer<int> _ProteinInstanceShadowFlags;	
			
			StructuredBuffer<float4> _ProteinInstanceInfo;	
			StructuredBuffer<int> _ProteinAtomCount;		

            void frag(v2f_img i, out float4 color : COLOR0) 
			{   
				int2 uv = i.uv * _ScreenParams.xy; 
				int id = _IdTexture[uv];
			
				if(id >= 0)
				{	
					float4 info = _ProteinInstanceInfo[id];
					int atomCount = _ProteinAtomCount[info.x];

					half4 source = tex2D(_MainTex, i.uv);				
					int shadowFlag = _ProteinInstanceShadowFlags[id];	
					
					int threshold = max(atomCount / 500, 1);

					//float acc = clamp(shadowFlag * 10 / atomCount, 0, 0.5);
					float acc = clamp(shadowFlag * 1000 / atomCount , 0, 0.15);
					//acc = 0;
											
					color = source * (0.8 + acc);	
					//color = atomCount > 4000 ? source * 0.5 : source;	
					//color = source;	
				}
				else
				{
					discard;
				}
            }
            
            ENDCG
        }

			Pass
			{
				ZWrite On
				ZTest Always


				CGPROGRAM
#pragma target 5.0	
#pragma fragment frag
#pragma vertex vert_img

#include "UnityCG.cginc"

				sampler2D_float _DepthTexture;

			void frag(v2f_img i,  out float4 color : COLOR, out float depth : DEPTH)
			{
				color = float4(1,1,1,1);
				depth = tex2D(_DepthTexture, i.uv);
			}
			ENDCG
			}

				Pass
			{
				ZWrite On
				ZTest Always
				//Blend SrcAlpha OneMinusSrcAlpha 

				CGPROGRAM
#pragma target 5.0	
#pragma fragment frag
#pragma vertex vert_img



#include "UnityCG.cginc"
#include "Helper.cginc"

				uniform int _InvertContours;
				uniform float _GhostContours;
				sampler2D _MainTex;
				sampler2D _MainTex2;

			void frag(v2f_img i,  out float4 color : COLOR) //, out float depth : DEPTH)
			{
				//discard;

				float4 srcColor = tex2D(_MainTex, i.uv);
				float4 dstColor = tex2D(_MainTex2, i.uv);

				/*if (srcColor.x == 1 && srcColor.y == 1 && srcColor.z == 1)
				{
					color = dstColor;
				}
				else
				{
					float alpha = 0.25;
					color = dstColor * (1-alpha)+srcColor * (alpha);
				}*/
					
				//color = dstColor;
				//color = srcColor;

				float4 edgeColor = srcColor;
				if (edgeColor.x < 1 && edgeColor.y < 1 && edgeColor.z < 1)
				{
					if(edgeColor.x < 1 && edgeColor.y < 1 && edgeColor.z < 1)

					//color = dstColor * (1 + (srcColor + _GhostContours - srcColor));
					//color = dstColor + (srcColor + _GhostContours - srcColor);
					//color = dstColor * (1+srcColor + _GhostContours - srcColor);
					color = dstColor + (srcColor + _GhostContours - srcColor);
				}
					
				else 
					color = dstColor;
			}
			ENDCG
			}

//		Pass
//		{
//			ZWrite On
//			ZTest Always
//
//
//			CGPROGRAM
//#pragma target 5.0	
//			#pragma fragment frag
//			#pragma vertex vert_img
//
//			#include "UnityCG.cginc"
//
//			sampler2D_float _DepthTexture;
//
//			void frag(v2f_img i,  out float4 color : COLOR, out float depth : DEPTH)
//			{
//				color = float4(1,1,1,1);
//				depth = tex2D(_DepthTexture, i.uv);
//			}
//			ENDCG
//		}
//
//		Pass
//		{
//			ZWrite On
//			ZTest Always
//			Blend SrcAlpha OneMinusSrcAlpha 
//
//			CGPROGRAM
//			#pragma target 5.0	
//			#pragma fragment frag
//			#pragma vertex vert_img
//
//			
//
//			#include "UnityCG.cginc"
//
//			sampler2D _MainTex;
//
//			void frag(v2f_img i,  out float4 color : COLOR) //, out float depth : DEPTH)
//			{				
//				float4 edgeColor = tex2D(_MainTex, i.uv);
//
//				if(edgeColor.x != 1  && edgeColor.y != 1 && edgeColor.z != 1)
//					color = float4(edgeColor.xyz,0.25);
//				else discard;
//			}
//			ENDCG
//		}		
	}

	FallBack "Diffuse"
}