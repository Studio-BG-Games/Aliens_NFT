Shader "Hexasphere/HexaTileBackgroundExtrusion" {
    Properties {
        _MainTex ("Main Texture Array", 2DArray) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _AmbientColor ("Ambient Color", Color) = (0,0,0)
        _MinimumLight ("Minimum Light", Float) = 0
        _GradientIntensity ("Gradient Intensity", float) = 0.75
        _ExtrusionMultiplier("Extrusion Multiplier", float) = 1.0
        _Center ("Sphere Center", Vector) = (0,0,0)
        _TileAlpha ("Tile Alpha", Float) = 1
		[HideInInspector] _SrcBlend ("__src", Float) = 1.0
		[HideInInspector] _DstBlend ("__dst", Float) = 0.0
		[HideInInspector] _ZWrite ("__zw", Float) = 1.0
    }


        SubShader{
            Tags { "Queue" = "Geometry-2" "RenderType" = "Opaque" "RenderPipeline" = "LightweightPipeline" }
            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Pass {
                //    Tags { "LightMode" = "LightweightForward" }
                    Offset 2, 2

                        CGPROGRAM
                        #pragma vertex vert
                        #pragma fragment frag
                        #pragma geometry geom
                        #pragma fragmentoption ARB_precision_hint_fastest
            //#pragma multi_compile_fwdbase nolightmap nodynlightmap novertexlight nodirlightmap
            #pragma multi_compile _ HEXA_ALPHA
            #pragma multi_compile _ HEXA_LIT
            #pragma target 4.0
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"

            UNITY_DECLARE_TEX2DARRAY(_MainTex);

            fixed _GradientIntensity;
            float _ExtrusionMultiplier;
            fixed4 _Color;
            fixed3 _AmbientColor;
            float _MinimumLight;
            float3 _Center;
            float4 _MainLightPosition;
            half4 _MainLightColor;

            struct appdata {
                float4 vertex   : POSITION;
                float4 texcoord : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2g {
                float4 vertex   : POSITION;
                float4 uv       : TEXCOORD0;
                fixed4 color : COLOR;
                #if HEXA_ALPHA || HEXA_LIT
                    float3 worldPos : TEXCOORD1;
                #endif
                #if HEXA_LIT
                    float3 norm  : TEXCOORD2;
                #endif
            };

            struct g2f {
                float4 pos   : SV_POSITION;
                float3 uv    : TEXCOORD0;
                fixed4 color : COLOR;
                SHADOW_COORDS(1)
            };

            struct VertexInfo {
                float4 vertex;
            };

            v2g vert(appdata v) {
                v2g o;
                o.vertex = v.vertex;
                o.uv = v.texcoord;
                o.color = v.color * _Color;
                #if HEXA_ALPHA || HEXA_LIT
                    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                #endif
                #if HEXA_LIT
                    o.norm = normalize(o.worldPos - _Center);
                #endif
                return o;
            }

            void Extrude(v2g p0, v2g p1, float extrusion, fixed4 color, inout TriangleStream<g2f> outputStream) {
                g2f tri;
                VertexInfo v;
                v.vertex = p1.vertex;
                v.vertex.xyz *= extrusion;
                tri.pos = UnityObjectToClipPos(v.vertex); // float4(p1.pos.xyz * extrusion, p1.pos.w));
                tri.uv = float3(0.0, 1.0, p1.uv.z);
                tri.color = color;
                TRANSFER_SHADOW(tri);
                outputStream.Append(tri);
                v.vertex = p0.vertex;
                v.vertex.xyz *= extrusion;
                tri.pos = UnityObjectToClipPos(v.vertex); // float4(p0.pos.xyz * extrusion, p0.pos.w));
                tri.uv = float3(1.0, 1.0, p0.uv.z);
                TRANSFER_SHADOW(tri);
                outputStream.Append(tri);
                v.vertex = p0.vertex;
                tri.pos = UnityObjectToClipPos(v.vertex); // p0.pos);
                tri.uv = float3(1.0, 0.0, p0.uv.z);
                fixed4 darkerColor = color * _GradientIntensity;
                tri.color = darkerColor;
                TRANSFER_SHADOW(tri);
                outputStream.Append(tri);
                outputStream.RestartStrip();
                v.vertex = p1.vertex;
                tri.pos = UnityObjectToClipPos(v.vertex); // p1.pos);
                tri.uv = float3(0.0, 0.0, p1.uv.z);
                TRANSFER_SHADOW(tri);
                outputStream.Append(tri);
                v.vertex = p1.vertex;
                v.vertex.xyz *= extrusion;
                tri.pos = UnityObjectToClipPos(v.vertex); // float4(p1.pos.xyz * extrusion, p1.pos.w));
                tri.uv = float3(0.0, 1.0, p1.uv.z);
                tri.color = color;
                TRANSFER_SHADOW(tri);
                outputStream.Append(tri);
                v.vertex = p0.vertex;
                tri.pos = UnityObjectToClipPos(v.vertex); // p0.pos);
                tri.uv = float3(1.0, 0.0, p0.uv.z);
                tri.color = darkerColor;
                TRANSFER_SHADOW(tri);
                outputStream.Append(tri);
                outputStream.RestartStrip();
            }

            [maxvertexcount(21)]
            void geom(triangle v2g input[3], inout TriangleStream<g2f> outputStream) {

                #if HEXA_ALPHA
                    float3 worldPos = input[0].worldPos;
                    float3 v1 = worldPos - _Center;
                    float3 v2 = worldPos - _WorldSpaceCameraPos;
                    float d = dot(v1,v2);
                    if (d > 0) return;
                #endif

                g2f topFace;
                float extrusion = 1.0 + input[0].uv.w * _ExtrusionMultiplier;
                fixed4 color = input[0].color;
                for (int i = 0; i < 3; i++) {
                    VertexInfo v;
                    v.vertex = input[i].vertex;
                    v.vertex.xyz *= extrusion;
                    topFace.pos = UnityObjectToClipPos(v.vertex);
                    topFace.uv = float4(input[i].uv.xyz, 1.0);
                    fixed4 topFaceColor = color;
                    #if HEXA_LIT
                        float3 norm = input[i].norm;
                        float d = saturate(dot(norm, _MainLightPosition.xyz));
                        d = max(d, _MinimumLight);
                        topFaceColor = (topFaceColor * _MainLightColor) * d;
                    #endif
                    topFace.color = topFaceColor;
                    TRANSFER_SHADOW(topFace);
                    outputStream.Append(topFace);
                }
                outputStream.RestartStrip();
                #if HEXA_LIT
                    float3 sideNorm = normalize((input[0].worldPos + input[1].worldPos) * 0.5 - input[2].worldPos);
                    float ndl = saturate(dot(sideNorm, _MainLightPosition.xyz));
                    ndl = max(ndl, _MinimumLight);
                    color = (color * _MainLightColor) * ndl;
                #endif
                Extrude(input[0], input[1], extrusion, color, outputStream);
            }

            fixed4 frag(g2f i) : SV_Target {
                fixed atten = SHADOW_ATTENUATION(i);
                fixed4 color = UNITY_SAMPLE_TEX2DARRAY(_MainTex, i.uv) * i.color;
                color.rgb *= atten;
                color.rgb += _AmbientColor;
                return color;
            }
            ENDCG
    }
        }


    SubShader {
    	Tags { "Queue" = "Geometry-2" "RenderType"="Opaque" }

 		Pass {
 			Tags { "LightMode" = "ForwardBase" }
	      	Blend [_SrcBlend] [_DstBlend]
	      	ZWrite [_ZWrite]
	        Offset 2, 2

                CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma geometry geom
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma exclude_renderers gles
				#pragma multi_compile_fwdbase nolightmap nodynlightmap novertexlight nodirlightmap
				#pragma multi_compile _ HEXA_ALPHA
				#pragma multi_compile _ HEXA_LIT
				#pragma target 4.0
				#include "UnityCG.cginc"
				#include "AutoLight.cginc"
				#include "Lighting.cginc"

                UNITY_DECLARE_TEX2DARRAY(_MainTex); 

                fixed _GradientIntensity;
                float _ExtrusionMultiplier;
                fixed4 _Color;
                fixed3 _AmbientColor;
                float _MinimumLight;
                fixed _TileAlpha;
				float3 _Center;

                struct appdata {
    				float4 vertex   : POSITION;
					float4 texcoord : TEXCOORD0;
					#if HEXA_ALPHA || HEXA_LIT
					float3 gPos     : TEXCOORD1;
					#endif
					fixed4 color    : COLOR;
    			};

				struct v2g {
	    			float4 vertex : POSITION;
	    			float4 uv     : TEXCOORD0;
	    			fixed4 color  : COLOR;
	    			#if HEXA_ALPHA || HEXA_LIT
                        float3 gPos   : TEXCOORD1;
                    #endif
	    			#if  HEXA_LIT
                        float3 worldPos : TEXCOORD2;
                    #endif
				};

				struct g2f {
	    			float4 pos   : SV_POSITION;
	    			float3 uv    : TEXCOORD0;
	    			fixed4 color : COLOR;
	    			SHADOW_COORDS(1)
				};

				struct VertexInfo {
					float4 vertex;
				};

				v2g vert(appdata v) {
    				v2g o;
    				o.vertex   = v.vertex;
    				o.uv    = v.texcoord;
					fixed4 color = v.color * _Color;
    				color.a *= _TileAlpha;
    				o.color  = color;
    				#if HEXA_ALPHA || HEXA_LIT
    				    o.gPos = mul(unity_ObjectToWorld, float4(v.gPos.xyz, 1.0)).xyz;
    				#endif
    				#if HEXA_LIT
    				    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
    				#endif
    				return o;
    			}

    			void Extrude(v2g p0, v2g p1, float extrusion, fixed4 color, inout TriangleStream<g2f> outputStream) {
    			    g2f tri;
    			    VertexInfo v;

    			    // First side triangle
    			    v.vertex = p1.vertex;
    			    v.vertex.xyz *= extrusion;
                	tri.pos   = UnityObjectToClipPos(v.vertex); //float4(p1.pos.xyz * extrusion, p1.pos.w));
                	tri.uv    = float3(0.0, 1.0, p1.uv.z);
                	tri.color = color;

                	TRANSFER_SHADOW(tri);
                	outputStream.Append(tri);

                	v.vertex = p0.vertex;
                	v.vertex.xyz *= extrusion;
                	tri.pos   = UnityObjectToClipPos(v.vertex); //float4(p0.pos.xyz * extrusion, p0.pos.w));
                	tri.uv    = float3(1.0, 1.0, p0.uv.z);
                	TRANSFER_SHADOW(tri);
                	outputStream.Append(tri);

                	v.vertex = p0.vertex;
                	tri.pos = UnityObjectToClipPos(v.vertex); //p0.pos);
                	tri.uv  = float3(1.0, 0.0, p0.uv.z);
                	fixed4 darkerColor = color * _GradientIntensity;
                	tri.color = darkerColor;
                	TRANSFER_SHADOW(tri);
                	outputStream.Append(tri);

                	// Second side triangle (completes the side quad)
                	outputStream.RestartStrip();
                	v.vertex = p1.vertex;
                	tri.pos = UnityObjectToClipPos(v.vertex); //p1.pos);
                	tri.uv  = float3(0.0, 0.0, p1.uv.z);
                	TRANSFER_SHADOW(tri);
                	outputStream.Append(tri);

                	v.vertex = p1.vertex;
                	v.vertex.xyz *= extrusion;
                	tri.pos = UnityObjectToClipPos(v.vertex); // float4(p1.pos.xyz * extrusion, p1.pos.w));
                	tri.uv  = float3(0.0, 1.0, p1.uv.z);
                	tri.color = color;
                	TRANSFER_SHADOW(tri);
                	outputStream.Append(tri);

                	v.vertex = p0.vertex;
                	tri.pos = UnityObjectToClipPos(v.vertex); //p0.pos);
                	tri.uv  = float3(1.0, 0.0, p0.uv.z);
                	tri.color = darkerColor;
                	TRANSFER_SHADOW(tri);
                	outputStream.Append(tri);
                	outputStream.RestartStrip();
				}

				[maxvertexcount(21)]
				void geom(triangle v2g input[3], inout TriangleStream<g2f> outputStream) {

				    #if HEXA_ALPHA || HEXA_LIT
    				float3 gPos = input[0].gPos;
    				#endif

					#if HEXA_ALPHA
    	                float3 v1 = gPos - _Center;
        	            float3 v2 = gPos - _WorldSpaceCameraPos;
            	        float dt = dot(v1, v2);
                	    if (dt>0) return;
                    #endif

					g2f topFace;
					float extrusion = 1.0 + input[0].uv.w * _ExtrusionMultiplier;
					fixed4 color = input[0].color;
					fixed4 topFaceColor = color;
    	            #if HEXA_LIT
						float3 norm = normalize(gPos - _Center);
		   				float d = saturate(dot(norm, _WorldSpaceLightPos0.xyz));
                        d = max(d, _MinimumLight);
    	             	topFaceColor = (topFaceColor * _LightColor0) * d;
    	            #endif
   	                topFace.color = topFaceColor;

	                for(int i = 0; i < 3; i++) {
	                	VertexInfo v;
	                    v.vertex = input[i].vertex;
	                    v.vertex.xyz *= extrusion;
	                    topFace.pos = UnityObjectToClipPos(v.vertex);
    	                topFace.uv = float4(input[i].uv.xyz, 1.0);
    	                TRANSFER_SHADOW(topFace);
                    	outputStream.Append(topFace);
                	}
                	outputStream.RestartStrip();

                	#if HEXA_LIT
                	    float3 sideNorm = normalize( (input[0].worldPos+input[1].worldPos) * 0.5 - input[2].worldPos);
                	    float ndl = saturate(dot(sideNorm, _WorldSpaceLightPos0.xyz));
                        ndl = max(ndl, _MinimumLight);
    	                color = (color * _LightColor0) * ndl;
    	            #endif
                	Extrude(input[0], input[1], extrusion, color, outputStream);
				}

    			fixed4 frag (g2f i) : SV_Target {
    				fixed atten = SHADOW_ATTENUATION(i);
    				fixed4 color = UNITY_SAMPLE_TEX2DARRAY(_MainTex, i.uv) * i.color;
    				color.rgb *= atten;
    				color.rgb += _AmbientColor;
                    return color;
                }
                ENDCG
		}

 		Pass {
 				Name "ShadowCaster"
				Tags { "LightMode" = "ShadowCaster" }
		        Offset 2, 2

                CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma geometry geom
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_shadowcaster
				#pragma target 4.0
				#include "UnityCG.cginc"

                UNITY_DECLARE_TEX2DARRAY(_MainTex); 

                fixed _GradientIntensity;
                float _ExtrusionMultiplier;

                struct appdata {
    				float4 vertex   : POSITION;
					float4 texcoord : TEXCOORD0;
					#if HEXA_ALPHA
					float3 gPos     : TEXCOORD1;
					#endif
    			};

				struct v2g {
	    			float4 pos   : SV_POSITION;
	    			float4 uv    : TEXCOORD0;
	    			#if HEXA_ALPHA
                    float3 gPos  : TEXCOORD1;
                    #endif
				};

				struct g2f {
					V2F_SHADOW_CASTER;
				};

				struct vertexInfo {
					float4 vertex;
				};

				v2g vert(appdata v) {
    				v2g o;
    				o.pos   = v.vertex;
    				o.uv    = v.texcoord;
					#if HEXA_ALPHA
    				o.gPos = mul(unity_ObjectToWorld, float4(v.gPos.xyz, 1.0)).xyz;
    				#endif
    				return o;
    			}

    			void Extrude(v2g p0, v2g p1, float extrusion, inout TriangleStream<g2f> outputStream) {
    				vertexInfo v;
    			    g2f tri;
                	v.vertex = float4(p1.pos.xyz * extrusion, p1.pos.w);
					TRANSFER_SHADOW_CASTER(tri);
                	outputStream.Append(tri);
                	v.vertex = float4(p0.pos.xyz * extrusion, p0.pos.w);
					TRANSFER_SHADOW_CASTER(tri);
                	outputStream.Append(tri);
                	v.vertex = p0.pos;
					TRANSFER_SHADOW_CASTER(tri);
                	outputStream.Append(tri);
                	outputStream.RestartStrip();
                	v.vertex = p1.pos;
					TRANSFER_SHADOW_CASTER(tri);
                	outputStream.Append(tri);
                	v.vertex = float4(p1.pos.xyz * extrusion, p1.pos.w);
					TRANSFER_SHADOW_CASTER(tri);
                	outputStream.Append(tri);
                	v.vertex = p0.pos;
					TRANSFER_SHADOW_CASTER(tri);
                	outputStream.Append(tri);
                	outputStream.RestartStrip();
				}

				[maxvertexcount(21)]
				void geom(triangle v2g input[3], inout TriangleStream<g2f> outputStream) {

					#if HEXA_ALPHA
	    				float3 gPos = input[0].gPos;
    	                float3 v1 = gPos - _Center;
        	            float3 v2 = gPos - _WorldSpaceCameraPos;
            	        float dt = dot(v1, v2);
                	    if (dt>0) return;
                    #endif

					g2f topFace;
					float extrusion = 1.0 + input[0].uv.w * _ExtrusionMultiplier;
	                for(int i = 0; i < 3; i++) {
	                	vertexInfo v;
	                	v.vertex = input[i].pos;
	                	v.vertex.xyz *= extrusion;
    	                TRANSFER_SHADOW_CASTER(topFace);
                    	outputStream.Append(topFace);
                	}
                	outputStream.RestartStrip();
                	Extrude(input[0], input[1], extrusion, outputStream);
				}

    			fixed4 frag (g2f i) : SV_Target {
					SHADOW_CASTER_FRAGMENT(i)
                }
                ENDCG
		}

    }

    SubShader {	// Fallback for SM 3.5
    	Tags { "Queue" = "Geometry-2" "RenderType"="Opaque" }
       	Blend [_SrcBlend] [_DstBlend]
       	ZWrite [_ZWrite]
 		Pass {
 			Tags { "LightMode" = "ForwardBase" }
	        Offset 2, 2

                CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_fwdbase nolightmap nodynlightmap novertexlight nodirlightmap
				#pragma target 3.5
				#include "UnityCG.cginc"
				#include "AutoLight.cginc"

                UNITY_DECLARE_TEX2DARRAY(_MainTex); 

                float _ExtrusionMultiplier;
                fixed4 _Color;

                struct appdata {
    				float4 vertex   : POSITION;
					float4 texcoord : TEXCOORD0;
					fixed4 color    : COLOR;
    			};

				struct v2f {
	    			float4 pos   : SV_POSITION;
	    			float4 uv    : TEXCOORD0;
	    			fixed4 color : COLOR;
	    			SHADOW_COORDS(1)
				};

				v2f vert(appdata v) {
    				v2f o;
    				float extrusion = 1.0 + v.texcoord.w * _ExtrusionMultiplier;
    				v.vertex.xyz *= extrusion;
    				o.pos = UnityObjectToClipPos(v.vertex);
    	            o.uv = float4(v.texcoord.xyz, 1.0);
    	            o.color = v.color * _Color;
    	            TRANSFER_SHADOW(o);
    				return o;
    			}

    			fixed4 frag (v2f i) : SV_Target {
                    fixed4 co = UNITY_SAMPLE_TEX2DARRAY(_MainTex, i.uv.xyz) * i.color;
                    fixed atten = SHADOW_ATTENUATION(i);
                    co.rgb *= i.uv.www * atten;
                    return co;
                }
                ENDCG
		}
    }

    SubShader {	// Fallback for old GPUs
    	Tags { "Queue" = "Geometry-2" "RenderType"="Opaque" }
       	Blend [_SrcBlend] [_DstBlend]
       	ZWrite [_ZWrite]
 		Pass {
 			Tags { "LightMode" = "ForwardBase" }
	        Offset 2, 2

                CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				#pragma multi_compile_fwdbase nolightmap nodynlightmap novertexlight nodirlightmap
				#include "UnityCG.cginc"
				#include "AutoLight.cginc"

                float _ExtrusionMultiplier;

                struct appdata {
    				float4 vertex   : POSITION;
					float4 texcoord : TEXCOORD0;
					fixed4 color    : COLOR;
    			};

				struct v2f {
	    			float4 pos   : SV_POSITION;
	    			fixed4 color : COLOR;
	    			SHADOW_COORDS(0)
				};

				v2f vert(appdata v) {
    				v2f o;
					float extrusion = 1.0 + v.texcoord.w * _ExtrusionMultiplier;
	                v.vertex.xyz *= extrusion;
	                o.pos = UnityObjectToClipPos(v.vertex);
    				o.color = v.color;
    				TRANSFER_SHADOW(o);
    				return o;
    			}

    			fixed4 frag (v2f i) : SV_Target {
    				fixed atten = SHADOW_ATTENUATION(i);
    				return i.color * atten;
                }
                ENDCG
		}
    }


    Fallback "Diffuse"
}