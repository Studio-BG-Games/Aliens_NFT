Shader "Hexasphere/HexaGridNoExtrusion" {
    Properties {
        _Color ("Main Color", Color) = (1,0.5,0.5,1)
		[HideInInspector] _SrcBlend ("__src", Float) = 1.0
		[HideInInspector] _DstBlend ("__dst", Float) = 0.0
		[HideInInspector] _ZWrite ("__zw", Float) = 1.0
    }


    SubShader {
            Tags { "Queue" = "Geometry-2" "RenderPipeline" = "LightweightPipeline" }
            Pass {
                    Blend[_SrcBlend][_DstBlend]
                    ZWrite[_ZWrite]
                    CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag
                    #pragma fragmentoption ARB_precision_hint_fastest
        //#pragma multi_compile_fwdbase nolightmap nodynlightmap novertexlight nodirlightmap
        #include "UnityCG.cginc"
        #include "AutoLight.cginc"

        fixed4 _Color;

        struct appdata {
            float4 vertex   : POSITION;
        };

        struct v2f {
            float4 pos      : SV_POSITION;
            SHADOW_COORDS(0)
        };

        v2f vert(appdata v) {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            TRANSFER_SHADOW(o);
            return o;
        }

        fixed4 frag(v2f i) : SV_Target {
            fixed atten = SHADOW_ATTENUATION(i);
            fixed4 color = _Color;
            color.rgb *= atten;
            return color;
        }
        ENDCG
        }
    }


    SubShader {
    	Tags { "Queue" = "Geometry-2" }
        Pass {
        		Tags { "LightMode" = "ForwardBase" }
   		       	Blend [_SrcBlend] [_DstBlend]
   		       	ZWrite [_ZWrite]
                Offset 2, 2

                CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_fwdbase nolightmap nodynlightmap novertexlight nodirlightmap
				#include "UnityCG.cginc"
				#include "AutoLight.cginc"

				fixed4 _Color;

                struct appdata {
    				float4 vertex   : POSITION;
    			};

				struct v2f {
	    			float4 pos      : SV_POSITION;
	    			SHADOW_COORDS(0)
				};

				v2f vert(appdata v) {
    				v2f o;
	                o.pos = UnityObjectToClipPos(v.vertex);
	                TRANSFER_SHADOW(o);
    				return o;
    			}
    		
    			fixed4 frag (v2f i) : SV_Target {
    				fixed atten = SHADOW_ATTENUATION(i);
    				fixed4 color = _Color;
    				color.rgb *= atten;
    				return color;
                }
                ENDCG
         }
    }


    Fallback "Diffuse"
}