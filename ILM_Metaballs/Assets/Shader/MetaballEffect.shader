Shader "Postprocess/MetaballEffect"
{
    Properties
    {
        _Threshold     ("Threshold",      Range(0.1, 2.0)) = 0.5
        _MetaballColor ("Metaball Color", Color)           = (1, 0.4, 0.1, 1)
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

            float4 frag(Varyings i) : SV_Target
            {
                float2 uv = i.texcoord.xy;

                // Color original de la escena
                float4 sceneColor = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_BlitTexture, uv);

                // Acumular campo escalar de todas las esferas
                float field = 0.0;
                for (int j = 0; j < _MetaballCount; j++)
                {
                    // Distancia en UV entre este pixel y la proyeccion de la esfera
                    float2 diff = uv - _MetaballPositions[j].xy;
                    // Corregir aspecto para que sea circular y no eliptico
                    diff.x *= _ScreenParams.x / _ScreenParams.y;
                    float dist = length(diff);
                    field += falloff(dist, _MetaballPositions[j].w);
                }

                // Threshold: si el campo supera el umbral, pixel solido
                float solid = step(_Threshold, field);

                // Mezclar escena con color metaball
                float4 finalColor = lerp(sceneColor, _MetaballColor, solid);
                return finalColor;
            }
            ENDHLSL
        }
    }
}