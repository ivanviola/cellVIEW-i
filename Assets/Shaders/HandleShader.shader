Shader "Custom/Handle" 
{
	SubShader
	{
		//Tags{ "Queue" = "Overlay" }
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}

		Pass
		{
			Cull Off
			ZWrite Off
			ZTest Always

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0

			#include "UnityCG.cginc"
		
			uniform float4 _HandleColor;

			struct v2f
			{
				float4 pos : SV_POSITION;
			};

			v2f vert(appdata_base v) 
			{
				v2f output;
				output.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				return output;
			}

			float4 frag(v2f input) : COLOR
			{
				return _HandleColor;
			}

			ENDCG
		}

		Pass
		{
			ZWrite Off
			ZTest Always

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0

			#include "UnityCG.cginc"

			uniform float4 _HandleColor;

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 viewNormal : FLOAT30;
			};

			v2f vert(appdata_base v)
			{
				v2f output;

				output.pos = mul(UNITY_MATRIX_MVP, v.vertex);
				output.viewNormal = normalize(mul(UNITY_MATRIX_MV, float4(v.normal, 0.0)).xyz);
				return output;
			}

			float4 frag(v2f input) : COLOR
			{
				float ndotl = 1.25 * pow(max(dot(input.viewNormal, float3(0,0,1)), 0.1), 0.5);
				return _HandleColor * ndotl;
			}

			ENDCG
		}

		Pass 
		{  
			Cull Off
			ZWrite Off
			ZTest Lequal			
			Blend SrcAlpha OneMinusSrcAlpha 
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
			};

			uniform float4 _HandleColor;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				return o;
			}
			
			uniform float _CutObjectAlpha;

			fixed4 frag (v2f i) : SV_Target
			{
				return float4(_HandleColor.xyz, (_CutObjectAlpha * 2) - 0.15);
			}
			ENDCG
		}

		Pass 
		{  
			Cull Off
			ZWrite Off
			ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha 
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
			};

			uniform float4 _HandleColor;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				return float4(_HandleColor.xyz, 0.15);
			}
			ENDCG
		}
	}
}