Shader "Zen/Metallic"
{
    Properties
    {
        _DiffuseColor ("DiffuseColor", Color) = (1,1,1,1)
        _DiffuseColorForce("DiffuseColorForce", Range(0,3)) = 1
        _MainTex ("Albedo (RGB)", 2D) = "white" {}

        [Toggle(_METALLIC_ON)] _UseMetallic("Use Metallic", Float) = 0
        _Glossiness ("Smoothness", Range(0,1)) = 0.9
        _Metallic ("Metallic", Range(0,1)) = 0.25

        [Toggle(_EMISSION_ON)] _UseEmission("Use Emission", Float) = 0
        _EmissionTexture("EmissionTexture", 2D) = "white" {}
		[HDR]_EmissionColor("EmissionColor", Color) = (1,1,1,1)
        _EmissionForce("EmissionForce",  Range(0,3)) = 1

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        #pragma shader_feature _METALLIC_ON
        #pragma shader_feature _EMISSION_ON
        #pragma shader_feature _NORMAL_ON

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            #ifdef _NORMAL_ON
                float2 uv_NormalMap;
            #endif
        };

        #ifdef _METALLIC_ON
            half _Glossiness;
            half _Metallic;
        #endif
        uniform float _DiffuseColorForce;

        #ifdef _EMISSION_ON
            uniform sampler2D _EmissionTexture;
		    uniform float4 _EmissionTexture_ST;	
            uniform float _EmissionForce;
        #endif

        #ifdef _NORMAL_ON
            sampler2D _NormalMap;
        #endif

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        UNITY_INSTANCING_BUFFER_START(ColorProps)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _DiffuseColor)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _EmissionColor)
        UNITY_INSTANCING_BUFFER_END(ColorProps)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            half4 c = tex2D (_MainTex, IN.uv_MainTex) * UNITY_ACCESS_INSTANCED_PROP(ColorProps, _DiffuseColor * _DiffuseColorForce);    
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables

            #ifdef _METALLIC_ON
                o.Metallic = _Metallic;
                o.Smoothness = _Glossiness;
            #endif

			#ifdef _EMISSION_ON
                float2 uv_EmissionTexture = IN.uv_MainTex * _EmissionTexture_ST.xy + _EmissionTexture_ST.zw;
				float4 staticSwitch58 = ( ( tex2D( _EmissionTexture, uv_EmissionTexture ) *  UNITY_ACCESS_INSTANCED_PROP(ColorProps, _EmissionColor ) ) * _EmissionForce );
			    o.Emission = staticSwitch58.rgb;
			#endif
        }
        ENDCG
    }
    FallBack "Diffuse"
}
