Shader "Minecraft/Transparent Blocks"{
    Properties{
        _MainTex("Block Texture Atlas", 2D) = "white"{}
    }

    SubShader{
        Tags{"Queue" = "AlphaTest" "IgnoreProjector"="ture" "RenderType" = "TransparentCutout"}
        LOD 100
        Lighting Off
        Pass{
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0
                #include "UnityCG.cginc"

                struct a2v{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float4 color : COLOR;
				};

				struct v2f {
					float4 vertex : SV_POSITION;
					float2 uv : TEXCOORD0;
					float4 color : COLOR;
				};

				sampler2D _MainTex;
				float GlobalLightLevel;
				float minGlobalLightLevel;
				float maxGlobalLightLevel;

				v2f vert (a2v v) {
				
					v2f o;

					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					o.color = v.color;

					return o;

				}

				fixed4 frag (v2f i) : SV_Target {
				
					fixed4 col = tex2D (_MainTex, i.uv);

					float shader = (maxGlobalLightLevel - minGlobalLightLevel) * GlobalLightLevel + minGlobalLightLevel;
					shader *= i.color.a;
					shader = clamp(1 - shader, minGlobalLightLevel, maxGlobalLightLevel);

					// float localLightLevel = clamp(GlobalLightLevel + i.color.a, 0, 1);
					clip(col.a - 1);
					col = lerp(col, float4(0, 0, 0, 1), shader);
					return col;
				}

            ENDCG
        }

    }
}
