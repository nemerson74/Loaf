// Erase alpha under a circular brush on the source texture.
// Inputs set from C#: BaseTexture (the scratch canvas), BrushCenter (UV), BrushRadius (UV units)

Texture2D BaseTexture;
SamplerState TextureSampler
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

float2 BrushCenter;  // normalized UV [0..1]
float  BrushRadius;  // normalized radius (relative to max dimension)
float  Feather = 0.0; // optional soft edge in UV units

struct VSInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct PSInput
{
    float4 Position : SV_Position;
    float2 TexCoord : TEXCOORD0;
};

PSInput VSMain(VSInput input)
{
    PSInput o;
    o.Position = input.Position;
    o.TexCoord = input.TexCoord;
    return o;
}

float4 PSMain(PSInput input) : SV_Target
{
    float4 baseCol = BaseTexture.Sample(TextureSampler, input.TexCoord);

    float dist = distance(input.TexCoord, BrushCenter);

    // Hard cut (Feather == 0): fully erase inside radius
    if (Feather <= 0.0001f)
    {
        if (dist <= BrushRadius)
        {
            baseCol.a = 0.0f;
        }
        return baseCol;
    }

    // Soft edge: falloff alpha near the edge
    float inner = BrushRadius - Feather;
    if (dist <= inner)
    {
        baseCol.a = 0.0f;
    }
    else if (dist <= BrushRadius)
    {
        float t = saturate((dist - inner) / Feather);
        baseCol.a *= t; // 0 at inner, 1 at outer
    }
    return baseCol;
}

technique EraseBrush
{
    pass P0
    {
        VertexShader = compile vs_4_0 VSMain();
        PixelShader  = compile ps_4_0 PSMain();
    }
}