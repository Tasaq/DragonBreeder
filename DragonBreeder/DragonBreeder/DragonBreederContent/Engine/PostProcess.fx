Texture2D SourceTexture;
Texture2DArray SourceTextureArray;
Texture2D AnotherTexture;
Texture2D NoiseTexture;
Texture2D DepthBuffer;
float DistortPower =1;
Texture2DArray t_array;
int slice;
SamplerState TexSampler
{
    Filter = MIN_MAG_MIP_LINEAR;

};

struct VS_IN
{
    float4 Position: POSITION;
	uint VertexID: SV_VertexID;
};
struct VS_OUT
{
    float4 position : SV_POSITION;		//Position
	float2 texcoord : TexCoord0;
};
struct GS_OUT
{
    float4 position : SV_POSITION;		//Position
	float2 texcoord : TexCoord0;
    uint RTid : SV_RenderTargetArrayIndex;
};


VS_OUT VS( VS_IN input)
{
    VS_OUT  Out;
	Out.position = float4(input.Position.xyz,1);
	Out.texcoord.x = (1+input.Position.x)*0.5f;
	Out.texcoord.y = (1-input.Position.y)*0.5f;
	return Out;
}
[maxvertexcount(12)]
void GS( triangle VS_OUT input[3], inout TriangleStream<GS_OUT> triStream )
{
	GS_OUT g;
	for(int i = 0; i<3;i++)
	{
		g.RTid = i;
		for(int j = 0; j<3; j++)
		{
			g.position = input[j].position;
			g.texcoord = input[j].texcoord;
			triStream.Append(g);
		}
		triStream.RestartStrip();
	}
}

float2 blurSize = (1/1280.0f, 1/720.0f);
float4 BlurH_PS( VS_OUT input ) : SV_TARGET
{
	float4 sum = 0;
	sum += SourceTexture.Sample(TexSampler, float2(input.texcoord.x - 4.0*blurSize.x, input.texcoord.y)) * 0.05;
    sum += SourceTexture.Sample(TexSampler, float2(input.texcoord.x - 3.0*blurSize.x, input.texcoord.y)) * 0.09;
    sum += SourceTexture.Sample(TexSampler, float2(input.texcoord.x - 2.0*blurSize.x, input.texcoord.y)) * 0.12;
    sum += SourceTexture.Sample(TexSampler, float2(input.texcoord.x -     blurSize.x, input.texcoord.y)) * 0.15;
    sum += SourceTexture.Sample(TexSampler, float2(input.texcoord.x,				  input.texcoord.y)) * 0.16;
    sum += SourceTexture.Sample(TexSampler, float2(input.texcoord.x +     blurSize.x, input.texcoord.y)) * 0.15;
    sum += SourceTexture.Sample(TexSampler, float2(input.texcoord.x + 2.0*blurSize.x, input.texcoord.y)) * 0.12;
    sum += SourceTexture.Sample(TexSampler, float2(input.texcoord.x + 3.0*blurSize.x, input.texcoord.y)) * 0.09;
    sum += SourceTexture.Sample(TexSampler, float2(input.texcoord.x + 4.0*blurSize.x, input.texcoord.y)) * 0.05;
	return float4(sum.xyz,1);
}
float4 BlurV_PS( VS_OUT input ) : SV_TARGET
{
	   float4 sum = 0;
 
	sum += SourceTexture.Sample(TexSampler, float2(input.texcoord.x, input.texcoord.y - 4.0*blurSize.y)) * 0.05;
	sum += SourceTexture.Sample(TexSampler, float2(input.texcoord.x, input.texcoord.y - 3.0*blurSize.y)) * 0.09;
	sum += SourceTexture.Sample(TexSampler, float2(input.texcoord.x, input.texcoord.y - 2.0*blurSize.y)) * 0.12;
	sum += SourceTexture.Sample(TexSampler, float2(input.texcoord.x, input.texcoord.y - blurSize.y)) * 0.15;
	sum += SourceTexture.Sample(TexSampler, float2(input.texcoord.x, input.texcoord.y)) * 0.16;
	sum += SourceTexture.Sample(TexSampler, float2(input.texcoord.x, input.texcoord.y + blurSize.y)) * 0.15;
	sum += SourceTexture.Sample(TexSampler, float2(input.texcoord.x, input.texcoord.y + 2.0*blurSize.y)) * 0.12;
	sum += SourceTexture.Sample(TexSampler, float2(input.texcoord.x, input.texcoord.y + 3.0*blurSize.y)) * 0.09;
	sum += SourceTexture.Sample(TexSampler, float2(input.texcoord.x, input.texcoord.y + 4.0*blurSize.y)) * 0.05;
	return float4(sum.xyz,1);
}
float4 NoiseDistort_PS( VS_OUT input ) : SV_TARGET
{
	 float2 TexCoord = input.texcoord + DistortPower * (NoiseTexture.Sample(TexSampler, input.texcoord).xy)/15;
	 return SourceTexture.Sample(TexSampler, TexCoord);
}
float scale = 0.2f;
float2 Center = float2(0.5f, 0.5f);
float4 RadialBlur_PS( VS_OUT input ) : SV_TARGET
{
	float nsamples = 16;
	float2 UV = input.texcoord - Center;
    float4 c = 0;
    for(int i=0; i <nsamples; i++)
	{
    	float scale = 1 + (-0.1f)*(i/(float) (nsamples-1));
    	c += SourceTexture.Sample(TexSampler, UV * scale + Center );
   	}
   	c /= nsamples;
	return c;
}
float4 Copy_PS( VS_OUT input ) : SV_TARGET
{
	 return SourceTexture.Sample(TexSampler, input.texcoord);
}
float4 RTarrayCopy( VS_OUT input ) : SV_TARGET
{
	 return t_array.Sample(TexSampler, float3(input.texcoord,slice));
}
float4 PS_writeArray( GS_OUT input ) : SV_TARGET
{
	 return float4(1,1,0,1);
}

float4 PS_RTArrayBlurH( GS_OUT input ) : SV_TARGET
{
	float4 sum = 0;
	sum += SourceTextureArray.Sample(TexSampler, float3(input.texcoord.x - 4.0*blurSize.x, input.texcoord.y, input.RTid)) * 0.05;
    sum += SourceTextureArray.Sample(TexSampler, float3(input.texcoord.x - 3.0*blurSize.x, input.texcoord.y, input.RTid)) * 0.09;
    sum += SourceTextureArray.Sample(TexSampler, float3(input.texcoord.x - 2.0*blurSize.x, input.texcoord.y, input.RTid)) * 0.12;
    sum += SourceTextureArray.Sample(TexSampler, float3(input.texcoord.x -     blurSize.x, input.texcoord.y, input.RTid)) * 0.15;
    sum += SourceTextureArray.Sample(TexSampler, float3(input.texcoord.x,				  input.texcoord.y, input.RTid)) * 0.16;
    sum += SourceTextureArray.Sample(TexSampler, float3(input.texcoord.x +     blurSize.x, input.texcoord.y, input.RTid)) * 0.15;
    sum += SourceTextureArray.Sample(TexSampler, float3(input.texcoord.x + 2.0*blurSize.x, input.texcoord.y, input.RTid)) * 0.12;
    sum += SourceTextureArray.Sample(TexSampler, float3(input.texcoord.x + 3.0*blurSize.x, input.texcoord.y, input.RTid)) * 0.09;
    sum += SourceTextureArray.Sample(TexSampler, float3(input.texcoord.x + 4.0*blurSize.x, input.texcoord.y, input.RTid)) * 0.05;
	return float4(sum.xyz,1);
}
float4 PS_RTArrayBlurV( GS_OUT input ) : SV_TARGET
{
	   float4 sum = 0;
 
	sum += SourceTextureArray.Sample(TexSampler, float3(input.texcoord.x, input.texcoord.y - 4.0*blurSize.y, input.RTid)) * 0.05;
	sum += SourceTextureArray.Sample(TexSampler, float3(input.texcoord.x, input.texcoord.y - 3.0*blurSize.y, input.RTid)) * 0.09;
	sum += SourceTextureArray.Sample(TexSampler, float3(input.texcoord.x, input.texcoord.y - 2.0*blurSize.y, input.RTid)) * 0.12;
	sum += SourceTextureArray.Sample(TexSampler, float3(input.texcoord.x, input.texcoord.y - blurSize.y	  , input.RTid)) * 0.15;
	sum += SourceTextureArray.Sample(TexSampler, float3(input.texcoord.x, input.texcoord.y				  , input.RTid)) * 0.16;
	sum += SourceTextureArray.Sample(TexSampler, float3(input.texcoord.x, input.texcoord.y + blurSize.y    , input.RTid)) * 0.15;
	sum += SourceTextureArray.Sample(TexSampler, float3(input.texcoord.x, input.texcoord.y + 2.0*blurSize.y, input.RTid)) * 0.12;
	sum += SourceTextureArray.Sample(TexSampler, float3(input.texcoord.x, input.texcoord.y + 3.0*blurSize.y, input.RTid)) * 0.09;
	sum += SourceTextureArray.Sample(TexSampler, float3(input.texcoord.x, input.texcoord.y + 4.0*blurSize.y, input.RTid)) * 0.05;
	return float4(sum.xyz,1);
}
DepthStencilState DisableDepth
{
    DepthEnable = FALSE;
    DepthWriteMask = ALL;
};
RasterizerState CullingMode
{
	CULLMODE = NONE;
};
BlendState AdditiveBlend
{
    AlphaToCoverageEnable = false;
    BlendEnable[0] = true;
    SrcBlend = ONE;
    DestBlend = ONE;
    BlendOp = ADD;
    SrcBlendAlpha = ONE;
    DestBlendAlpha = ZERO;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
};


technique11 BlurV
{
    pass P0
    {
		SetDepthStencilState( DisableDepth, 0 );
		 SetRasterizerState(CullingMode);
        SetVertexShader( CompileShader( vs_5_0, VS() ) );
        SetHullShader( NULL );
        SetDomainShader( NULL );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_5_0, BlurV_PS() ) );
    }
}
technique11 BlurH
{
    pass P0
    {
		SetDepthStencilState( DisableDepth, 0 );
		 SetRasterizerState(CullingMode);
        SetVertexShader( CompileShader( vs_5_0, VS() ) );
        SetHullShader( NULL );
        SetDomainShader( NULL );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_5_0, BlurH_PS() ) );
    }
}
technique11 Copy
{
    pass P0
    {
		SetDepthStencilState( DisableDepth, 0 );
		 SetRasterizerState(CullingMode);
        SetVertexShader( CompileShader( vs_5_0, VS() ) );
        SetHullShader( NULL );
        SetDomainShader( NULL );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_5_0, Copy_PS() ) );
    }
}
technique11 NoiseDistort
{
    pass P0
    {
		SetDepthStencilState( DisableDepth, 0 );
		 SetRasterizerState(CullingMode);
        SetVertexShader( CompileShader( vs_5_0, VS() ) );
        SetHullShader( NULL );
        SetDomainShader( NULL );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_5_0, NoiseDistort_PS() ) );
    }
}
technique11 RadialBlur
{
    pass P0
    {
		SetDepthStencilState( DisableDepth, 0 );
		 SetRasterizerState(CullingMode);
        SetVertexShader( CompileShader( vs_5_0, VS() ) );
        SetHullShader( NULL );
        SetDomainShader( NULL );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_5_0, RadialBlur_PS() ) );
    }
}

technique11 RTarray2RT
{
    pass P0
    {
		SetDepthStencilState( DisableDepth, 0 );
		 SetRasterizerState(CullingMode);
        SetVertexShader( CompileShader( vs_5_0, VS() ) );
        SetHullShader( NULL );
        SetDomainShader( NULL );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_5_0, RTarrayCopy() ) );
    }
}
technique11 RTArrayBlurH
{
    pass P0
    {
		SetDepthStencilState( DisableDepth, 0 );
		 SetRasterizerState(CullingMode);
        SetVertexShader( CompileShader( vs_5_0, VS() ) );
        SetHullShader( NULL );
        SetDomainShader( NULL );
        SetGeometryShader( CompileShader( gs_5_0, GS() ) );
        SetPixelShader( CompileShader( ps_5_0, PS_RTArrayBlurH() ) );
    }
}
technique11 RTArrayBlurV
{
    pass P0
    {
		SetDepthStencilState( DisableDepth, 0 );
		 SetRasterizerState(CullingMode);
        SetVertexShader( CompileShader( vs_5_0, VS() ) );
        SetHullShader( NULL );
        SetDomainShader( NULL );
        SetGeometryShader( CompileShader( gs_5_0, GS() ) );
        SetPixelShader( CompileShader( ps_5_0, PS_RTArrayBlurV() ) );
    }
}
