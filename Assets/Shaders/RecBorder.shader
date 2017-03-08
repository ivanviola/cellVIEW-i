Shader "Custom/RecBorder"
{
	Properties
	{
		_MainTex ("Main Texture", 2D) = "" {}
		_BorderThickness ("Border Thickness", Float) = 0.1
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			
			#include "UnityCG.cginc"					

			uniform float _BorderThickness;
			sampler2D _MainTex;

			fixed4 frag (v2f_img i) : SV_Target
			{
				//return fixed4(i.uv.r, i.uv.g, 0, 1);
				float xBorderSize = _BorderThickness / _ScreenParams.x;
				float yBorderSize = _BorderThickness / _ScreenParams.y;


				if ((i.uv.x < xBorderSize) || (i.uv.x > 1.0 - xBorderSize) || 
				    (i.uv.y < yBorderSize) || (i.uv.y > 1.0 - yBorderSize)) {
					return fixed4(1,0,0,1);
				}
				return tex2D(_MainTex, i.uv);
			}
			ENDCG
		}
	}
}
