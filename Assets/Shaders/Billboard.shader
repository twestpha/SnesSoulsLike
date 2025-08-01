Shader "Custom/Billboard" {
    Properties {
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
        _Emit ("Emission", 2D) = "black" {}
        //_EmitAmount ("Emit Amount", Range(0,1)) = "black" {}
    }

    SubShader {
        Tags {
            "Queue"="AlphaTest"
            "IgnoreProjector"="True"
            "RenderType"="TransparentCutout"
            "DisableBatching"="True"
        }

        LOD 100

        CGPROGRAM
            #pragma vertex vert
            #pragma surface surf Lambert alphatest:_Cutoff noinstancing

            struct appdata_t {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
                float2 texcoord1 : TEXCOORD1;
                float2 texcoord2 : TEXCOORD2;
                float4 tangent : TANGENT;
            };

            sampler2D _MainTex;
            sampler2D _Emit;

            struct Input {
                float2 uv_MainTex;
            };

            void Billboard(inout appdata_t v){
                // this is the quad verts as generated by MakeMesh.cs in the localPos list
                const float3 plane = float3(v.vertex.x, v.vertex.y, 0);
                const float3 upVector = half3(0, 1, 0);

                // change this from camera forward to camera -> object vector
                const float3 forwardVector = UNITY_MATRIX_IT_MV[2].xyz; // camera forward
                // this is a littly fucky, so we're just doing inverse camera for now :P

                // float3 modelworld = mul(unity_ObjectToWorld, v.vertex).xyz - v.vertex.xyz;
                // const float3 forwardVector = modelworld - _WorldSpaceCameraPos;
                // const float3 forwardVector = modelworld;

                // float3 forwardVector = _WorldSpaceCameraPos - (mul(unity_ObjectToWorld, v.vertex).xyz - v.vertex.xyz);
                // forwardVector.y = 0.0;

                const float3 rightVector = normalize(cross(forwardVector, upVector));
                float3 position = 0;
                position += plane.x * rightVector;
                position += plane.y * upVector;
                v.vertex = float4(position, 1);

                float3 flatForward = forwardVector;
                flatForward.y = 0.0;
                v.normal = flatForward;
            }

            void vert (inout appdata_t v, out Input o){
                UNITY_INITIALIZE_OUTPUT(Input, o);
                Billboard(v);
            }

            void surf (Input IN, inout SurfaceOutput o) {
                // o.Emission = tex2D(_Emit, IN.uv_MainTex).rgb;
                fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
                //o.Emission = c.rgb;
                o.Albedo = c.rgb;
                o.Alpha = c.a;
            }
        ENDCG
    }
}
