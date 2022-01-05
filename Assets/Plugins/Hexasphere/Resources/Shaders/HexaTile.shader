Shader "Hexasphere/HexaTile" {
    Properties {
        _Color ("Main Color", Color) = (1,0.5,0.5,1)
        _TileAlpha ("Tile Alpha", Float) = 1
		[HideInInspector] _SrcBlend ("__src", Float) = 1.0
		[HideInInspector] _DstBlend ("__dst", Float) = 0.0
		[HideInInspector] _ZWrite ("__zw", Float) = 1.0
    }


        SubShader{
            Tags { "Queue" = "Geometry-2" "RenderPipeline" = "LightweightPipeline" }
            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]

            Pass {
                //    Tags { "LightMode" = "LightweightForward" }
                    Offset 1, 1

                        CGPROGRAM
                        #pragma vertex vert
                        #pragma fragment frag
                        #pragma fragmentoption ARB_precision_hint_fastest
        //#pragma multi_compile_fwdbase nolightmap nodynlightmap novertexlight nodirlightmap
        #include "UnityCG.cginc"
        #include "AutoLight.cginc"
        #include "Lighting.cginc"

        fixed4 _Color;
        fixed _TileAlpha;
        float4 _MainLightPosition;
half4 _MainLightColor;
        struct appdata {
            float4 vertex   : POSITION;
            #if HEXA_LIT
                float3 normal   : NORMAL;
            #endif
        };

        struct v2f {
            float4 pos      : SV_POSITION;
            SHADOW_COORDS(0)
            #if HEXA_LIT
                float3 norm  : NORMAL;
            #endif
        };

        v2f vert(appdata v) {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            TRANSFER_SHADOW(o);
            #if HEXA_LIT
                o.norm = UnityObjectToWorldNormal(v.normal);
            #endif
            return o;
        }

        fixed4 frag(v2f i) : SV_Target {
            fixed atten = SHADOW_ATTENUATION(i);
            fixed4 color = _Color;
            #if HEXA_LIT
                float d = saturate(dot(normalize(i.norm), _MainLightPosition.xyz));
                color = (color * _MainLightColor) * d;
            #endif
            color.rgb *= atten;
            color.a *= _TileAlpha;
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
            Offset 1, 1

                CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_fwdbase nolightmap nodynlightmap novertexlight nodirlightmap
                #pragma multi_compile _ HEXA_LIT
				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
                #include "Lighting.cginc"

				fixed4 _Color;
				fixed _TileAlpha;

                struct appdata {
    				float4 vertex   : POSITION;
                    #if HEXA_LIT
                        float3 normal   : NORMAL;
                    #endif
    			};

				struct v2f {
	    			float4 pos      : SV_POSITION;
	    			SHADOW_COORDS(0)
                    #if HEXA_LIT
                        float3 norm  : NORMAL;
                    #endif
				};

				v2f vert(appdata v) {
    				v2f o;
	                o.pos = UnityObjectToClipPos(v.vertex);
	                TRANSFER_SHADOW(o);
                    #if HEXA_LIT
                        o.norm = UnityObjectToWorldNormal(v.normal);
                    #endif
    				return o;
    			}
    		
    			fixed4 frag (v2f i) : SV_Target {
    				fixed atten = SHADOW_ATTENUATION(i);
    				fixed4 color = _Color;
                    #if HEXA_LIT
                        float d = saturate(dot(normalize(i.norm), _WorldSpaceLightPos0.xyz));
                        color = (color * _LightColor0) * d;
                    #endif
    				color.rgb *= atten;
    				color.a *= _TileAlpha;
    				return color;
                }
                ENDCG

        }
    }

    Fallback "Diffuse"
}
