Shader "Custom/DebugShadow"
{
	Properties 
	{
		_ShadowMapTexture ("Shadow Map", 2D) = "" {}
	}

	SubShader 
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		LOD 100
	
		//Cull Off
		Cull Front
		ZWrite Off
		ZTest LEqual
	
		Pass
		{  
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
			
				#include "UnityCG.cginc"

				struct appdata_t
				 {
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					half2 texcoord : TEXCOORD0;
				};

				//UNITY_DECLARE_SHADOWMAP(_ShadowMapTexture);
				//Texture2D _ShadowMapTexture;
				//SamplerState _LinearClamp;
				
				UNITY_DECLARE_TEX2D(_ShadowMapTexture);
				

				v2f vert (appdata_t v)
				{
					v2f o;
					o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
					o.texcoord = v.texcoord;
					return o;
				}

				fixed4 frag (v2f i) : SV_Target
				{
					//float4 depth = UNITY_SAMPLE_TEX2D(_ShadowMapTexture,i.texcoord);
					//if(depth.x < 1) return float4(0,0,0,1) ;
					
					return float4(1,1,1,1);					
				}
			ENDCG
		}

		//Pass
		//{  
		//	CGPROGRAM
		//		#pragma vertex vert
		//		#pragma fragment frag
			
		//		#include "UnityCG.cginc"

		//		struct appdata_t
		//		 {
		//			float4 vertex : POSITION;
		//			float2 texcoord : TEXCOORD0;
		//		};

		//		struct v2f 
		//		{
		//			float4 vertex : SV_POSITION;
		//			half2 texcoord : TEXCOORD0;
		//		};

		//		sampler2D_float _MainTex;
			
		//		v2f vert (appdata_t v)
		//		{
		//			v2f o;
		//			o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
		//			o.texcoord = v.texcoord;
		//			return o;
		//		}

		//		fixed4 frag (v2f i) : SV_Target
		//		{
		//			float col = tex2D(_MainTex, i.texcoord);
		//			return float4(col.x / 500, 0, 0, 1);
		//		}
		//	ENDCG
		//}
	}
}