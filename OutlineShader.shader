Shader "Unlit/OutlineShader"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1, 1, 0, 1)
        _Thickness ("Outline Thickness", Range(0, 10)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            // Draw only objects in the outline layer
            ColorMask 0
            Stencil
            {
                Ref 1
                Comp Equal
                Pass Keep
            }
        }

        Pass
        {
            // Apply outline effect
            ZTest Always
            Cull Front
            ColorMask RGBA
            Stencil
            {
                Ref 1
                Comp Equal
                Pass Keep
            }
            Offset -1, -1
            SetTexture [_OutlineColor] { combine primary }
        }
    }
}