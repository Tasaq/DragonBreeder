float4x4 World;
float4x4 ViewProjection;
float4 Color;
Texture2D ModelTexture;
Texture2D NormalTexture;
SamplerState TexSampler
{
    Filter = MIN_MAG_MIP_LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;

};
struct VS_IN
{
    float4 Position: POSITION;
	float3 Normal  : NORMAL;
    uint Pos	   : SV_VertexID ;
};
struct VS_INtexture
{
    float4 Position: POSITION;
	float3 Normal  : NORMAL;
	float2 TexCoord  : TEXTURECOORD;
    uint Pos	   : SV_VertexID ;
};
struct VS_INtexture_normal
{
    float4 Position: POSITION;
	float3 Normal  : NORMAL;
	float2 TexCoord  : TEXTURECOORD;
    uint Pos	   : SV_VertexID ;
	float3 Tangent  : TANGENT0;
	float3 BiNormal : BINORMAL0;
};
struct VS_OUT
{
    float4 Pos : SV_Position;		//Position
    float4 Position: TEXCOORD1;
	float3 Normal: TEXCOORD0;
};
struct VS_OUT_texture
{
    float4 Pos : SV_Position;		//Position
    float4 Position: TEXCOORD2;
	float3 Normal: TEXCOORD1;
	float2 TexCoord: TEXCOORD0;
};
struct VS_OUT_texture_normal
{
    float4 Pos : SV_Position;		//Position
    float4 Position: TEXCOORD2;
	float3 Normal: TEXCOORD1;
	float2 TexCoord: TEXCOORD0;
	float3 Tangent: TEXCOORD3;
	float3 BiNormal: TEXCOORD4;
};
struct PS_OUT
{
    float4 Depth :SV_Target0;		//Position
    float4 Normal :SV_Target1;
	float4 Color :SV_Target2;
};


VS_OUT VS( VS_IN input )
{
    VS_OUT output;
    output.Pos = mul(input.Position, mul(World,ViewProjection));
    output.Position = mul(input.Position, mul(World,ViewProjection));
	output.Normal = normalize(mul(input.Normal, World));
    return output;
}

VS_OUT_texture_normal VS_textured( VS_INtexture input )
{
    VS_OUT_texture_normal output;
    output.Pos = mul(input.Position, mul(World,ViewProjection));
    output.Position = mul(input.Position, mul(World,ViewProjection));
	output.Normal = normalize(mul(input.Normal, World));
	output.TexCoord = input.TexCoord;
    return output;
}
VS_OUT_texture_normal VS_texturedNormal( VS_INtexture_normal input )
{
    VS_OUT_texture_normal output;
    output.Pos = mul(input.Position, mul(World,ViewProjection));
    output.Position = mul(input.Position, mul(World,ViewProjection));
	output.Normal = normalize(mul(input.Normal, World));
	output.TexCoord = input.TexCoord;
	output.Tangent = normalize(mul(input.Tangent, World).rgb);
	output.BiNormal = normalize(mul(input.BiNormal, World).rgb);
    return output;
}
#define kPI 3.1415926536f
half2 encode (float3 n)
{
    return half4(
      (half2(atan2(n.y,n.x)/kPI, n.z)+1.0)*0.5,
      0,0);
}
PS_OUT PS( VS_OUT input ) : SV_TARGET
{	
	PS_OUT output;
	output.Normal = 0;
	output.Depth =  input.Position.z/input.Position.w;
	output.Normal.rg = encode(input.Normal);
	output.Color =  Color;
    return output;
}
PS_OUT PS_textureNoBlend( VS_OUT_texture input ) : SV_TARGET
{	
	PS_OUT output;
	output.Normal = 0;
	output.Depth =  input.Position.z/input.Position.w;
	output.Normal.rg = encode(input.Normal);
	output.Color =  ModelTexture.Sample(TexSampler, input.TexCoord);
    return output;
}

PS_OUT PS_textureBlend( VS_OUT_texture input ) : SV_TARGET
{	
	PS_OUT output;
	output.Normal = 0;
	output.Depth =  input.Position.z/input.Position.w;
	output.Normal.rg = encode(input.Normal);
	float4 tex = ModelTexture.Sample(TexSampler, input.TexCoord);
	output.Color =  float4(Color.rgb * tex.a + tex.rgb*(1-tex.a), 1);
    return output;
}


float BumpConstant = 1.0f;
PS_OUT PS_textureNoBlendNormal( VS_OUT_texture_normal input ) : SV_TARGET
{	
	PS_OUT output;
	output.Normal = 0;
	output.Depth =  input.Position.z/input.Position.w;
	float3 bump = BumpConstant * (NormalTexture.Sample(TexSampler, input.TexCoord) - (0.5, 0.5, 0.5));
	input.Normal = input.Normal + (bump.x * input.Tangent + bump.y * input.BiNormal);
	input.Normal = normalize(input.Normal);
	output.Normal.rg = encode(input.Normal);
	output.Color =  ModelTexture.Sample(TexSampler, input.TexCoord);
    return output;
}

PS_OUT PS_textureBlendNormal( VS_OUT_texture_normal input ) : SV_TARGET
{	
	PS_OUT output;
	output.Normal = 0;
	output.Depth =  input.Position.z/input.Position.w;
	output.Normal.rg = encode(input.Normal);
	float4 tex = ModelTexture.Sample(TexSampler, input.TexCoord);
	output.Color =  float4(Color.rgb * tex.a + tex.rgb*(1-tex.a), 1);
    return output;
}


technique11 Render
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_5_0, VS() ) );
        SetHullShader( NULL );
        SetDomainShader( NULL );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_5_0, PS() ) );
    }
}

technique11 RenderTextureNoBlend
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_5_0, VS_textured() ) );
        SetHullShader( NULL );
        SetDomainShader( NULL );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_5_0, PS_textureNoBlend() ) );
    }
}

technique11 RenderTextureBlend
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_5_0, VS_textured() ) );
        SetHullShader( NULL );
        SetDomainShader( NULL );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_5_0, PS_textureBlend() ) );
    }
}



technique11 RenderTextureNoBlendNormal
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_5_0, VS_texturedNormal() ) );
        SetHullShader( NULL );
        SetDomainShader( NULL );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_5_0, PS_textureNoBlendNormal() ) );
    }
}

technique11 RenderTextureBlendNormal
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_5_0, VS_texturedNormal() ) );
        SetHullShader( NULL );
        SetDomainShader( NULL );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_5_0, PS_textureBlend() ) );
    }
}