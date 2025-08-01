Shader "Custom/Triplanar" 
{
    Properties 
    {
        _DiffuseMapA ("Diffuse Map A", 2D)  = "white" {}
        _DiffuseMapB ("Diffuse Map B", 2D)  = "white" {}
        _TextureScale ("Texture Scale",float) = 1
        _TriplanarBlendSharpness ("Blend Sharpness",float) = 1
    }
    SubShader 
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma target 3.0
        #pragma surface surf Lambert

        sampler2D _DiffuseMapA;
        sampler2D _DiffuseMapB;
        
        float _TextureScale;
        float _TriplanarBlendSharpness;

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
        }; 

        void surf (Input IN, inout SurfaceOutput o) 
        {
            // Find our UVs for each axis based on world position of the fragment.
            half2 yUV = IN.worldPos.xz / (_TextureScale);
            half2 xUV = IN.worldPos.zy / (_TextureScale);
            half2 zUV = IN.worldPos.xy / (_TextureScale);
            
            // I have no idea why this is the case, but basically it fixes the vertical mappings
            xUV.y = xUV.y / 3.0;
            zUV.y = zUV.y / 3.0;
            
            // Now do texture samples from our diffuse map with each of the 3 UV set's we've just made.
            half3 yDiff = tex2D (_DiffuseMapA, yUV);
            half3 xDiff = tex2D (_DiffuseMapB, xUV);
            half3 zDiff = tex2D (_DiffuseMapB, zUV);
            // Get the absolute value of the world normal.
            // Put the blend weights to the power of BlendSharpness, the higher the value, 
            // the sharper the transition between the planar maps will be.
            half3 blendWeights = pow(abs(IN.worldNormal), _TriplanarBlendSharpness);
            // Divide our blend mask by the sum of it's components, this will make x+y+z=1
            blendWeights = blendWeights / (blendWeights.x + blendWeights.y + blendWeights.z);
            // Finally, blend together all three samples based on the blend mask.
            o.Albedo = xDiff * blendWeights.x + yDiff * blendWeights.y + zDiff * blendWeights.z;
        }
        ENDCG
    }
}