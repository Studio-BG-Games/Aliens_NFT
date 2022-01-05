Shader "Hexasphere/HexaGridExtrusion" {
    Properties {
        _Color ("Dummy Color", Color) = (1,1,1,1)
        _ExtrusionMultiplier("Extrusion Multiplier", float) = 1.0
        _Center ("Sphere Center", Vector) = (0,0,0)
        _GradientIntensity ("Gradient Intensity", float) = 0.25
		[HideInInspector] _SrcBlend ("__src", Float) = 1.0
		[HideInInspector] _DstBlend ("__dst", Float) = 0.0
		[HideInInspector] _ZWrite ("__zw", Float) = 1.0
    }


        SubShader{
            Tags { "Queue" = "Geometry-1" "RenderType" = "Opaque" "RenderPipeline" = "LightweightPipeline" }
            Pass {
                //  Tags { "LightMode" = "LightweightForward" }
              Blend[_SrcBlend][_DstBlend]
              ZWrite[_ZWrite]

                  CGPROGRAM
                  #pragma vertex vert
                  #pragma fragment frag
                  #pragma geometry geom
                  #pragma fragmentoption ARB_precision_hint_fastest
                  #pragma exclude_renderers gles
        //#pragma multi_compile_fwdbase nolightmap nodynlightmap novertexlight nodirlightmap
        #pragma target 4.0
        #include "UnityCG.cginc"
        #include "AutoLight.cginc"

        fixed4 _Color;
        fixed _GradientIntensity;
        float _ExtrusionMultiplier;
        float3 _Center;

        struct appdata {
            float4 vertex   : POSITION;
            float2 texcoord : TEXCOORD0;
            fixed4 color : COLOR;
        };

        struct v2g {
            float4 vertex   : POSITION;
            float2 uv       : TEXCOORD0;
            float3 worldPos : TEXCOORD1;
            fixed4 color : COLOR;
        };

        struct g2f {
            float4 pos      : SV_POSITION;
            fixed4 color : COLOR;
            SHADOW_COORDS(0)
        };

        struct VertexInfo {
            float4 vertex;
        };

        v2g vert(appdata v) {
            v2g o;
            o.vertex = v.vertex;
            o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
            o.uv = v.texcoord;
            o.color = v.color * _Color;
            return o;
        }

        void Extrude(v2g p0, v2g p1, float extrusion, fixed4 color, fixed4 darkerColor, inout LineStream<g2f> outputStream) {
            g2f tri;
            VertexInfo v;

            v.vertex = p1.vertex;
            v.vertex.xyz *= extrusion;
            tri.pos = UnityObjectToClipPos(v.vertex); //float4(p1.pos.xyz * extrusion, p1.pos.w));
            tri.color = color;
            TRANSFER_SHADOW(tri);
            outputStream.Append(tri);

            v.vertex = p1.vertex;
            tri.pos = UnityObjectToClipPos(v.vertex); //p1.pos);
            tri.color = darkerColor;
            TRANSFER_SHADOW(tri);
            outputStream.Append(tri);

            v.vertex = p1.vertex;
            tri.pos = UnityObjectToClipPos(v.vertex); // p1.pos);
            TRANSFER_SHADOW(tri);
            outputStream.Append(tri);

            v.vertex = p0.vertex;
            tri.pos = UnityObjectToClipPos(v.vertex); // p0.pos);
            tri.color = darkerColor;
            TRANSFER_SHADOW(tri);
            outputStream.Append(tri);

            v.vertex = p0.vertex;
            v.vertex.xyz *= extrusion;
            tri.pos = UnityObjectToClipPos(v.vertex); // float4(p0.pos.xyz * extrusion, p0.pos.w));
            tri.color = color;
            TRANSFER_SHADOW(tri);
            outputStream.Append(tri);
        }

        [maxvertexcount(8)]
        void geom(line v2g input[2], inout LineStream<g2f> outputStream) {

            float3 worldPos = input[0].worldPos;
            float3 v1 = worldPos - _Center;
            float3 v2 = worldPos - _WorldSpaceCameraPos;
            float d = dot(v1,v2);
            if (d > 0) return;

            g2f segment;
            float extrusion = 1.0 + input[0].uv.y * _ExtrusionMultiplier;
            fixed4 color = input[0].color;
            fixed4 darkerColor = color * _GradientIntensity;
            for (int i = 0; i < 2; i++) {
                VertexInfo v;
                v.vertex = input[i].vertex;
                v.vertex.xyz *= extrusion;
                segment.pos = UnityObjectToClipPos(v.vertex);
                segment.color = color;
                TRANSFER_SHADOW(segment);
                outputStream.Append(segment);
            }
            Extrude(input[0], input[1], extrusion, color, darkerColor, outputStream);
        }

        fixed4 frag(g2f i) : SV_Target {
            fixed atten = SHADOW_ATTENUATION(i);
            return i.color * atten;
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
                CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma geometry geom
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma exclude_renderers gles
				#pragma multi_compile_fwdbase nolightmap nodynlightmap novertexlight nodirlightmap
				#pragma target 4.0
				#include "UnityCG.cginc"
				#include "AutoLight.cginc"

				fixed4 _Color;
                fixed _GradientIntensity;
                float _ExtrusionMultiplier;
				float3 _Center;

                struct appdata {
    				float4 vertex   : POSITION;
					float2 texcoord : TEXCOORD0;
					fixed4 color    : COLOR;
    			};

				struct v2g {
	    			float4 vertex   : POSITION;
	    			float2 uv       : TEXCOORD0;
	    			float3 worldPos : TEXCOORD1;
	    			fixed4 color    : COLOR;
				};

				struct g2f {
	    			float4 pos      : SV_POSITION;
	    			fixed4 color    : COLOR;
	    			SHADOW_COORDS(0)
				};

				struct VertexInfo {
					float4 vertex;
				};

				v2g vert(appdata v) {
    				v2g o;
    				o.vertex   = v.vertex;
    				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
    				o.uv       = v.texcoord;
    				o.color    = v.color * _Color;
    				return o;
    			}

    			void Extrude(v2g p0, v2g p1, float extrusion, fixed4 color, fixed4 darkerColor, inout LineStream<g2f> outputStream) {
    			    g2f tri;
    			    VertexInfo v;
    			    v.vertex = p1.vertex;
    			    v.vertex.xyz *= extrusion;
                	tri.pos = UnityObjectToClipPos(v.vertex); // float4(p1.pos.xyz * extrusion, p1.pos.w));
    			    tri.color = color;
    			    TRANSFER_SHADOW(tri);
                	outputStream.Append(tri);
                	v.vertex = p1.vertex;
                	tri.pos = UnityObjectToClipPos(v.vertex); // p1.pos);
                	tri.color = darkerColor;
                	TRANSFER_SHADOW(tri);
                	outputStream.Append(tri);
                	v.vertex = p1.vertex;
                	tri.pos = UnityObjectToClipPos(v.vertex); // p1.pos);
                	TRANSFER_SHADOW(tri);
                	outputStream.Append(tri);
                	v.vertex = p0.vertex;
                	tri.pos = UnityObjectToClipPos(v.vertex); // p0.pos);
                	tri.color = darkerColor;
                	TRANSFER_SHADOW(tri);
                	outputStream.Append(tri);
                	v.vertex = p0.vertex;
                	v.vertex.xyz *= extrusion;
                	tri.pos = UnityObjectToClipPos(v.vertex); // float4(p0.pos.xyz * extrusion, p0.pos.w));
    			    tri.color = color;
    			    TRANSFER_SHADOW(tri);
                	outputStream.Append(tri);
				}

				[maxvertexcount(8)]
				void geom(line v2g input[2], inout LineStream<g2f> outputStream) {

					float3 worldPos = input[0].worldPos;
    				float3 v1 = worldPos - _Center;
    				float3 v2 = worldPos - _WorldSpaceCameraPos;
    				float d = dot(v1,v2);
    				if (d>0) return;

					g2f segment;
					float extrusion = 1.0 + input[0].uv.y * _ExtrusionMultiplier;
					fixed4 color = input[0].color;
					fixed4 darkerColor = color * _GradientIntensity;
                    segment.color = color;
	                for(int i = 0; i < 2; i++) {
	                    VertexInfo v;
	                    v.vertex = input[i].vertex;
	                    v.vertex.xyz *= extrusion;
	                    segment.pos = UnityObjectToClipPos(v.vertex);
	                    TRANSFER_SHADOW(segment);
                    	outputStream.Append(segment);
                	}
                	Extrude(input[0], input[1], extrusion, color, darkerColor, outputStream);
				}

    			fixed4 frag (g2f i) : SV_Target {
    				fixed atten = SHADOW_ATTENUATION(i);
                    return i.color * atten;
                }
                ENDCG
		}

//		Pass {
// 				Name "ShadowCaster"
//				Tags { "LightMode" = "ShadowCaster" }
//                CGPROGRAM
//				#pragma vertex vert
//				#pragma fragment frag
//				#pragma geometry geom
//				#pragma fragmentoption ARB_precision_hint_fastest
//				#pragma multi_compile_shadowcaster
//				#pragma target 4.0
//				#include "UnityCG.cginc"
//				#include "AutoLight.cginc"
//
//                float _ExtrusionMultiplier;
//				float3 _Center;
//
//                struct appdata {
//    				float4 vertex   : POSITION;
//					float2 texcoord : TEXCOORD0;
//					float3 worldPos : TEXCOORD1;
//    			};
//
//				struct v2g {
//	    			float4 pos      : SV_POSITION;
//	    			float2 uv       : TEXCOORD0;
//	    			float3 worldPos : TEXCOORD1;
//				};
//
//				struct g2f {
//					V2F_SHADOW_CASTER;
//				};
//
//				struct vertexInfo {
//					float4 vertex;
//				};
//
//				v2g vert(appdata v) {
//    				v2g o;
//    				o.pos      = v.vertex;
//    				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
//    				o.uv       = v.texcoord;
//    				return o;
//    			}
//
//    			void Extrude(v2g p0, v2g p1, float extrusion, inout LineStream<g2f> outputStream) {
//    				vertexInfo v;
//    			    g2f tri;
//                	v.vertex = float4(p1.pos.xyz * extrusion, p1.pos.w);
//    			    TRANSFER_SHADOW_CASTER(tri);
//                	outputStream.Append(tri);
//                	v.vertex = p1.pos;
//                	TRANSFER_SHADOW_CASTER(tri);
//                	outputStream.Append(tri);
//                	v.vertex = p1.pos;
//                	TRANSFER_SHADOW_CASTER(tri);
//                	outputStream.Append(tri);
//                	v.vertex = p0.pos;
//                	TRANSFER_SHADOW_CASTER(tri);
//                	outputStream.Append(tri);
//                	v.vertex = float4(p0.pos.xyz * extrusion, p0.pos.w);
//    			    TRANSFER_SHADOW_CASTER(tri);
//                	outputStream.Append(tri);
//				}
//
//				[maxvertexcount(8)]
//				void geom(line v2g input[2], inout LineStream<g2f> outputStream) {
//
//					float3 worldPos = input[0].worldPos;
//    				float3 v1 = worldPos - _Center;
//    				float3 v2 = worldPos - _WorldSpaceCameraPos;
//    				float d = dot(v1,v2);
//    				if (d>0) return;
//
//					g2f segment;
//					float extrusion = 1.0 + input[0].uv.y * _ExtrusionMultiplier;
//	                for(int i = 0; i < 2; i++) {
//	                	vertexInfo v;
//	                	v.vertex = input[i].pos;
//	                    v.vertex.xyz *= extrusion;
//	                    TRANSFER_SHADOW_CASTER(segment);
//                    	outputStream.Append(segment);
//                	}
//                	Extrude(input[0], input[1], extrusion, outputStream);
//				}
//
//    			fixed4 frag (g2f i) : SV_Target {
//    				SHADOW_CASTER_FRAGMENT(i)
//                }
//                ENDCG
//		}

    }

   SubShader { // Fallback for old GPUs
    	Tags { "Queue" = "Geometry-1" "RenderType"="Opaque" }

 		Pass {
 				Tags { "LightMode" = "ForwardBase" }
		       	Blend [_SrcBlend] [_DstBlend]
				ZWrite [_ZWrite]

                CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_fwdbase nolightmap nodynlightmap novertexlight nodirlightmap
				#include "UnityCG.cginc"
				#include "AutoLight.cginc"

				fixed4 _Color;
				float _ExtrusionMultiplier;

                struct appdata {
    				float4 vertex   : POSITION;
					float2 texcoord : TEXCOORD0;
    			};

				struct v2f {
	    			float4 pos      : SV_POSITION;
	    			SHADOW_COORDS(0)
				};

				v2f vert(appdata v) {
    				v2f o;
					float extrusion = 1.0 + v.texcoord.y * _ExtrusionMultiplier;
	                v.vertex.xyz *= extrusion;
	                o.pos = UnityObjectToClipPos(v.vertex);
	                TRANSFER_SHADOW(o);
    				return o;
    			}
    		
    			fixed4 frag (v2f i) : SV_Target {
    				fixed atten = SHADOW_ATTENUATION(i);
                    return _Color * atten;
                }
                ENDCG
		}
//		Pass {
// 				Name "ShadowCaster"
//				Tags { "LightMode" = "ShadowCaster" }
//                CGPROGRAM
//				#pragma vertex vert
//				#pragma fragment frag
//				#pragma fragmentoption ARB_precision_hint_fastest
//				#pragma multi_compile_shadowcaster
//				#include "UnityCG.cginc"
//
//				float _ExtrusionMultiplier;
//
//                struct appdata {
//    				float4 vertex   : POSITION;
//					float2 texcoord : TEXCOORD0;
//    			};
//
//				struct v2f {
//					V2F_SHADOW_CASTER;
//				};
//
//				v2f vert(appdata v) {
//    				v2f o;
//					float extrusion = 1.0 + v.texcoord.y * _ExtrusionMultiplier;
//	                v.vertex.xyz *= extrusion;
//					TRANSFER_SHADOW_CASTER(o);
//    				return o;
//    			}
//    		
//    			fixed4 frag (v2f i) : SV_Target {
//					SHADOW_CASTER_FRAGMENT(i)
//                }
//                ENDCG
//		}
    }
       
}