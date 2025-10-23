Shader "Custom/Forcefield"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0.2, 0.8, 1, 0.3)
        _Intensity ("Intensity", Range(0, 2)) = 1
        _Speed ("Animation Speed", Range(0, 5)) = 2
        _NoiseScale ("Noise Scale", Range(0.1, 10)) = 2
        _EdgeGlow ("Edge Glow", Range(0, 2)) = 1
        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 1
        _Distortion ("Distortion", Range(0, 0.1)) = 0.02
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "IgnoreProjector"="True"
        }
        
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Intensity;
            float _Speed;
            float _NoiseScale;
            float _EdgeGlow;
            float _PulseSpeed;
            float _Distortion;
            
            // Simple noise function
            float noise(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }
            
            // Fractal noise
            float fbm(float2 uv)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                
                for (int i = 0; i < 4; i++)
                {
                    value += amplitude * noise(uv * frequency);
                    amplitude *= 0.5;
                    frequency *= 2.0;
                }
                
                return value;
            }
            
            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float time = _Time.y;
                
                // Animated noise for the forcefield effect
                float2 noiseUV = uv * _NoiseScale + time * _Speed;
                float noiseValue = fbm(noiseUV);
                
                // Create animated distortion
                float2 distortion = float2(
                    fbm(noiseUV + float2(time * 0.5, 0)),
                    fbm(noiseUV + float2(0, time * 0.5))
                ) * _Distortion;
                
                // Apply distortion to UV
                float2 distortedUV = uv + distortion;
                
                // Create edge detection for glow effect
                float edge = 1.0 - smoothstep(0.0, 0.1, min(min(uv.x, uv.y), min(1.0 - uv.x, 1.0 - uv.y)));
                
                // Animated pulse
                float pulse = sin(time * _PulseSpeed) * 0.3 + 0.7;
                
                // Combine effects
                float finalNoise = noiseValue * edge * pulse;
                
                // Create the forcefield color
                fixed4 col = _Color;
                col.a *= finalNoise * _Intensity;
                
                // Add edge glow
                float glow = pow(edge, 2.0) * _EdgeGlow;
                col.rgb += glow * _Color.rgb;
                col.a += glow * 0.5;
                
                // Add some sparkle effect
                float sparkle = step(0.95, noiseValue) * edge;
                col.rgb += sparkle * float3(1, 1, 1) * 0.5;
                col.a += sparkle * 0.3;
                
                return col;
            }
            ENDCG
        }
    }
    
    Fallback "Sprites/Default"
}
