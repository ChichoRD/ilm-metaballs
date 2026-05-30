Shader "Postprocess/MetaballEffect"
{
    Properties
    {
        _Threshold        ("Threshold",         Range(0.1, 2.0)) = 0.5
        _MetaballColor    ("Metaball Color",     Color)           = (1, 0.4, 0.1, 1)
        _SpecularPower    ("Specular Power",     Range(1, 128))   = 48
        _SpecularStrength ("Specular Strength",  Range(0, 1))     = 0.6
        _Ambient          ("Ambient",            Range(0, 1))     = 0.15
        [Toggle] _ShowMaskOnly ("Show Mask Only", Float)          = 0
    }
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #pragma vertex Vert
            #pragma fragment frag

            SAMPLER(sampler_BlitTexture);

            // Maximo de esferas soportadas
            #define MAX_METABALLS 11

            float4 _MetaballColor;
            float  _Threshold;
            float  _SpecularPower;
            float  _SpecularStrength;
            float  _Ambient;
            float  _ShowMaskOnly;

            // Posiciones de las esferas en View Space (coordenadas de camara)
            // xy = posicion en pantalla NDC, z = profundidad, w = radio de influencia
            float4 _MetaballPositions[MAX_METABALLS];
            int    _MetaballCount;

            // Funcion de falloff: da valor alto cerca del centro, cae suavemente a 0
            // Usamos 1/d^2 clasico de metaballs
            float falloff(float dist, float radius)
            {
                float d = dist / radius;
                // Fuera del radio influencia = 0
                if (d > 1.0) return 0.0;
                // Polinomio quintico: suave en los bordes, sin discontinuidades
                return 1.0 - (d * d * d * (6.0 * d * d - 15.0 * d + 10.0));
            }

            // Campo escalar total en un UV dado
            // Separado en funcion para poder llamarlo varias veces (gradiente)
            float computeField(float2 uv)
            {
                float field = 0.0;
                for (int j = 0; j < _MetaballCount; j++)
                {
                    float2 diff = uv - _MetaballPositions[j].xy;
                    diff.x *= _ScreenParams.x / _ScreenParams.y;
                    field += falloff(length(diff), _MetaballPositions[j].w);
                }
                return saturate(field);
            }

            float4 frag(Varyings i) : SV_Target
            {
                float2 uv = i.texcoord.xy;

                float4 sceneColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv);

                float field = computeField(uv);

                // Modo mascara: muestra el campo en escala de grises
                // Se ve la gradiente de influencia de cada esfera
                if (_ShowMaskOnly > 0.5)
                    return float4(field, field, field, 1.0);

                
                float solid = step(_Threshold, field);

                // Fuera de la metaball: escena normal
                if (solid < 0.5)
                    return sceneColor;

                // Muestreamos el campo ligeramente desplazado en X e Y
                float eps = 0.003;
                float gradX = computeField(uv + float2(eps, 0.0))
                            - computeField(uv - float2(eps, 0.0));
                float gradY = computeField(uv + float2(0.0, eps))
                            - computeField(uv - float2(0.0, eps));

                // Negamos XY porque el gradiente apunta hacia dentro, queremos hacia fuera
                float3 normal = normalize(float3(-gradX, -gradY, 0.4));

                // Iluminacion Blinn-Phong
                float3 lightDir = normalize(_MainLightPosition.xyz);
                float3 lightColor = _MainLightColor.rgb;

                // Diffuse (Lambert)
                float diff = max(0.0, dot(normal, lightDir));

                // Specular (Blinn-Phong)
                float3 halfDir = normalize(lightDir + float3(0.0, 0.0, 1.0));
                float spec = pow(max(0.0, dot(normal, halfDir)), _SpecularPower)
                           * _SpecularStrength;

                // Ambient: color base siempre visible
                float3 ambient  = _MetaballColor.rgb * _Ambient;

                // Diffuse: ilumina el color base sin teñirlo con el color de la luz
                float3 diffuse  = _MetaballColor.rgb * diff;

                // Specular: brillo blanco puro encima del color
                float3 specular = float3(1.0, 1.0, 1.0) * spec;

                float3 litColor = ambient + diffuse + specular;

                return float4(litColor, 1.0);
            }
            ENDHLSL
        }
    }
}