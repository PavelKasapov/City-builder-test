Shader "Custom/GridShader"
{
    Properties
    {
        _FreeColor ("Free Color", Color) = (0.2, 0.8, 0.2, 0.5)
        _OccupiedColor ("Occupied Color", Color) = (0.8, 0.2, 0.2, 0.5)
        _GridSize ("Grid Size", Vector) = (32, 32, 0, 0)
        _CellColors ("Cell Colors", 2D) = "white" {}
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
            
            float4 _FreeColor;
            float4 _OccupiedColor;
            float2 _GridSize;
            sampler2D _CellColors;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float2 cellUV = i.uv;
                float cellState = tex2D(_CellColors, cellUV).r;
                return lerp(_FreeColor, _OccupiedColor, cellState);
            }
            ENDCG
        }
    }
}