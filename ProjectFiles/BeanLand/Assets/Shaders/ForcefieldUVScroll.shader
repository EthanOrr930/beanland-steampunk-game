Shader "Custom/ForcefieldUVScroll"
{
    Properties
    {
        _MainTex ("Forcefield Texture", 2D) = "white" {}
        _Color ("Color", Color) = (0, 0.5, 1, 0.5)
        _ScrollSpeed ("UV Scroll Speed", Vector) = (1, 0, 0, 0)
        _PulseSpeed ("Alpha Pulse Speed", Float) = 2
        _PulseIntensity ("Pulse Intensity", Range(0, 1)) = 0.3
        _BaseAlpha ("Base Alpha", Range(0, 1)) = 0.7
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
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float4 _ScrollSpeed;
            float _PulseSpeed;
            float _PulseIntensity;
            float _BaseAlpha;
            
            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate UV scrolling with continuous time and wrap with frac
                float2 scrolledUV = frac(i.uv + _ScrollSpeed.xy * _Time.y);
                
                // Sample the texture with scrolled UVs
                fixed4 col = tex2D(_MainTex, scrolledUV);
                
                // Apply base color
                col *= _Color;
                
                // Calculate pulsing alpha
                float pulse = sin(_Time.y * _PulseSpeed) * _PulseIntensity + _BaseAlpha;
                col.a *= pulse;
                
                return col;
            }
            ENDCG
        }
    }
    
    Fallback "Sprites/Default"
}
