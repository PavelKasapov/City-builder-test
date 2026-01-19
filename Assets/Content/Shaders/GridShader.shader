Shader "Custom/GridShader"
{
    Properties
    {
        // Цвета
        _FreeColor ("Free Color", Color) = (0.2, 0.8, 0.2, 0.5)
        _OccupiedColor ("Occupied Color", Color) = (0.8, 0.2, 0.2, 0.5)
        _HoverValidColor ("Hover Valid Color", Color) = (0, 1, 0, 0.8)
        _HoverInvalidColor ("Hover Invalid Color", Color) = (1, 0, 0, 0.8)
        
        // Данные сетки в ОДНОЙ текстуре
        // R канал: занятость (0=свободно, 1=занято)
        // G канал: наведение (0=нет, 1=есть)
        _GridData ("Grid Data (R=Occupancy, G=Hover)", 2D) = "white" {}
        _GridSize ("Grid Size", Vector) = (32, 32, 0, 0)
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            // Цвета
            float4 _FreeColor;
            float4 _OccupiedColor;
            float4 _HoverValidColor;
            float4 _HoverInvalidColor;
            
            // Данные
            sampler2D _GridData;
            float2 _GridSize;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                
                // 1. ЧИТАЕМ ВСЕ ДАННЫЕ ОДНИМ ЧТЕНИЕМ
                float2 gridData = tex2D(_GridData, uv).rg;
                float occupancy = gridData.r;   // 0 = free, 1 = occupied
                float isHovered = gridData.g;   // 0 = no hover, 1 = hovered
                
                // 2. БАЗОВЫЙ ЦВЕТ
                fixed4 baseColor = lerp(_FreeColor, _OccupiedColor, occupancy);
                
                // 3. ПРОВЕРКА НАВЕДЕНИЯ
                if (isHovered > 0.0)
                {
                    // Выбираем цвет наведения на основе занятости
                    // Тернарный оператор читаемее, чем if/else для простых случаев
                    return occupancy > 0.5 ? _HoverInvalidColor : _HoverValidColor;
                }
                
                // 4. ВОЗВРАТ БАЗОВОГО ЦВЕТА
                return baseColor;
            }
            ENDCG
        }
    }
}