Shader "Hexasphere/HexaTileBackgroundNoExtrusion" {
    Properties {
        _MainTex ("Main Texture Array", 2DArray) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _AmbientColor ("Ambient Color", Color) = (0,0,0)
        _MinimumLight ("Minimum Light", Float) = 0
        _SpecularTint ("Specular Tint", Color) = (0,0,0)
        _Smoothness ("Smoothness", Float) = 0.7
        _TileAlpha ("Tile Alpha", Float) = 1
		[HideInInspector] _SrcBlend ("__src", Int) = 1
		[HideInInspector] _DstBlend ("__dst", Int) = 0
		[HideInInspector] _ZWrite ("__zw", Int) = 1
        [HideInInspector] _Cull ("__cull", Int) = 2
    }


    SubShader {
              Tags { "Queue" = "Geometry-1" "RenderType" = "Opaque" "RenderPipeline" = "LightweightPipeline" }
              Pass {
                //   Tags { "LightMode" = "LightweightForward" }
                   Offset 2, 2
                   Blend[_SrcBlend][_DstBlend]
                   ZWrite[_ZWrite]

                       CGPROGRAM
                       #pragma vertex vert
                       #pragma fragment frag
                       #pragma fragmentoption ARB_precision_hint_fastest
            #pragma multi_compile _ HEXA_LIT
            #pragma target 3.5
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"

            UNITY_DECLARE_TEX2DARRAY(_MainTex);
            fixed4 _Color;
            fixed _TileAlpha;
            fixed3 _AmbientColor;
            float _MinimumLight;
            fixed3 _SpecularTint;
            fixed _Smoothness;
            float4 _MainLightPosition;
            half4 _MainLightColor;

            struct appdata {
                float4 vertex   : POSITION;
                float4 texcoord : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f {
                float4 pos   : SV_POSITION;
                float3 uv    : TEXCOORD0;
                fixed4 color : COLOR;
                SHADOW_COORDS(1)
                #if HEXA_LIT
                    float3 norm  : TEXCOORD2;
                    float3 wpos  : TEXCOORD3;
                #endif
            };

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord.xyz;
                o.color = v.color * _Color;
                o.color.a *= _TileAlpha;
                TRANSFER_SHADOW(o);
                #if HEXA_LIT
                    o.norm = UnityObjectToWorldNormal(v.vertex.xyz);
                    o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
                #endif
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 color = UNITY_SAMPLE_TEX2DARRAY(_MainTex, i.uv) * i.color;
                #if HEXA_LIT
                    float3 norm = normalize(i.norm);
                    float d = saturate(dot(norm, _MainLightPosition.xyz));
                    d = max(d, _MinimumLight);
                    color = (color * _MainLightColor) * d;
                       
                    float3 viewDir = normalize(_WorldSpaceCameraPos - i.wpos);
                    float3 halfVector = normalize(_WorldSpaceLightPos0.xyz + viewDir);
 				    float3 specular = _SpecularTint * _LightColor0 * (pow(saturate(dot(halfVector, norm)), _Smoothness));
                    color.rgb += specular;
                #endif

                fixed atten = SHADOW_ATTENUATION(i);
                color.rgb *= atten;
                color.rgb += _AmbientColor;
                return color;
            }
            ENDCG
        }
    }

    SubShader {
    	Tags { "Queue" = "Geometry-1" "RenderType"="Opaque" }
 		Pass {
 			Tags { "LightMode" = "ForwardBase" }
	      	Blend [_SrcBlend] [_DstBlend]
	      	ZWrite [_ZWrite]
            Cull [_Cull]
	        Offset 2, 2

                CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_fwdbase nolightmap nodynlightmap novertexlight nodirlightmap
				#pragma multi_compile _ HEXA_LIT
				#pragma target 3.5
				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
				#include "Lighting.cginc"

                UNITY_DECLARE_TEX2DARRAY(_MainTex); 
                fixed4 _Color;
                fixed _TileAlpha;
                fixed3 _AmbientColor;
                float _MinimumLight;
                fixed3 _SpecularTint;
                fixed _Smoothness;

                struct appdata {
    				float4 vertex   : POSITION;
					float4 texcoord : TEXCOORD0;
					fixed4 color    : COLOR;
    			};

				struct v2f {
	    			float4 pos   : SV_POSITION;
	    			float3 uv    : TEXCOORD0;
	    			fixed4 color : COLOR;
	    			SHADOW_COORDS(1)
	    			#if HEXA_LIT
                        float3 norm  : TEXCOORD2;
                        float3 wpos  : TEXCOORD3;
                    #endif
				};

				v2f vert(appdata v) {
    				v2f o;
    				o.pos   = UnityObjectToClipPos(v.vertex);
    				o.uv    = v.texcoord.xyz;
    				fixed4 color = v.color * _Color;
    				color.a *= _TileAlpha;
    				o.color = color;
    				TRANSFER_SHADOW(o);
    				#if HEXA_LIT
    				    o.norm = UnityObjectToWorldNormal(v.vertex.xyz);
                        o.wpos = mul(unity_ObjectToWorld, v.vertex).xyz;
    				#endif
    				return o;
    			}
    		
    			fixed4 frag (v2f i) : SV_Target {
    				fixed4 color = UNITY_SAMPLE_TEX2DARRAY(_MainTex, i.uv) * i.color;
					#if HEXA_LIT
                        float3 norm = normalize(i.norm);
                	    float d = saturate(dot(norm, _WorldSpaceLightPos0.xyz));
                        d = max(d, _MinimumLight);
    	                color = (color * _LightColor0) * d;
                       
                        float3 viewDir = normalize(_WorldSpaceCameraPos - i.wpos);
                        float3 halfVector = normalize(_WorldSpaceLightPos0.xyz + viewDir);
    				    float3 specular = _SpecularTint * _LightColor0 * (pow(DotClamped(halfVector, norm), _Smoothness));
                        color.rgb += specular;

    	            #endif
    				fixed atten = SHADOW_ATTENUATION(i);
    				color.rgb *= atten;
    	            color.rgb += _AmbientColor;
    				return color;
                }
                ENDCG
		}
    }

    SubShader {	// Fallback for old GPUs
    	Tags { "Queue" = "Geometry-1" "RenderType"="Opaque" }
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

                float _ExtrusionMultiplier;
                fixed4 _Color;
                fixed _TileAlpha;

                struct appdata {
    				float4 vertex   : POSITION;
					fixed4 color    : COLOR;
    			};

				struct v2f {
	    			float4 pos   : SV_POSITION;
	    			fixed4 color : COLOR;
	    			SHADOW_COORDS(0)
				};

				v2f vert(appdata v) {
    				v2f o;
	                o.pos = UnityObjectToClipPos(v.vertex);
    				o.color = v.color * _Color;
    				o.color.a *= _TileAlpha;
    				TRANSFER_SHADOW(o);
    				return o;
    			}

    			fixed4 frag (v2f i) : SV_Target {
    				fixed atten = SHADOW_ATTENUATION(i);
    				fixed4 color = i.color;
    				color.rgb *= atten;
    				return atten;
                }
                ENDCG
		}
    }

    Fallback "Diffuse"
}