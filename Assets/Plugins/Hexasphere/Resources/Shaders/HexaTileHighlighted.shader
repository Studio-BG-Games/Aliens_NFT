Shader "Hexasphere/HexaTileHighlight" {
    Properties {
        _Color ("Main Color", Color) = (0,0.25,1,0.8)
        _Color2 ("Second Color", Color) = (0,0.25,1,0.2)
        _ColorShift("Color Shift", Float) = 0
        _MainTex ("Main Texture (RGBA)", 2D) = "white"
        
    }


        SubShader{
                Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "LightweightPipeline" }
                Blend SrcAlpha OneMinusSrcAlpha
                Pass {
                // Tags { "LightMode" = "LightweightForward" }

                 CGPROGRAM
                 #pragma vertex vert
                 #pragma fragment frag
                 #pragma fragmentoption ARB_precision_hint_fastest
                 #include "UnityCG.cginc"

                 sampler2D _MainTex;
                 fixed4 _Color, _Color2;
                 fixed _ColorShift;

                 struct appdata {
                     float4 vertex : POSITION;
                     float2 texcoord : TEXCOORD0;
                 };

                 struct v2f {
                     float4 pos : SV_POSITION;
                     float2 uv: TEXCOORD0;
                     fixed4 color : COLOR;
                 };

                 v2f vert(appdata v) {
                     v2f o;
                     o.pos = UnityObjectToClipPos(v.vertex);
                     o.uv = v.texcoord;
                     o.color = lerp(_Color, _Color2, _ColorShift);
                     return o;
                 }

                 fixed4 frag(v2f i) : SV_Target {
                     return i.color * tex2D(_MainTex, i.uv);
                 }
                 ENDCG
           }
    }



   SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Pass {
                CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#include "UnityCG.cginc"

                sampler2D _MainTex;
                fixed4 _Color, _Color2;
                fixed _ColorShift;

                struct appdata {
    				float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
    			};

				struct v2f {
	    			float4 pos : SV_POSITION;
	    			float2 uv: TEXCOORD0;
	    			fixed4 color: COLOR;
				};

				v2f vert(appdata v) {
    				v2f o;
    				o.pos = UnityObjectToClipPos(v.vertex);
    				o.uv = v.texcoord;
    				o.color = lerp(_Color, _Color2, _ColorShift);
    				return o;
    			}

    			fixed4 frag (v2f i) : SV_Target {
                    return i.color * tex2D(_MainTex, i.uv);
                }
                ENDCG
          }
    }


}
