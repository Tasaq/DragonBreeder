Texture2D LightMap;
Texture2D ColorMap;
Texture2D DepthMap;
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


VS_OUT VS( VS_IN input)
{
    VS_OUT  Out;
	Out.position = float4(input.Position.xyz,1);
	Out.texcoord.x = (1+input.Position.x)*0.5f;
	Out.texcoord.y = (1-input.Position.y)*0.5f;
	return Out;
}
float4 PS( VS_OUT input ) : SV_TARGET
{
	float4 light = LightMap.Sample(TexSampler, input.texcoord);
	float4 color = ColorMap.Sample(TexSampler, input.texcoord);
	float d = DepthMap.Sample(TexSampler, input.texcoord).r;
	if(d <= 0)
		return float4(color.rgb,1);
	else
		return max(0,float4(color.rgb * light.rgb + light.a * normalize(light.rgb), 1));
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


technique11 PointLight
{
    pass P0
    {
		SetDepthStencilState( DisableDepth, 0 );
		SetBlendState( AdditiveBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
		 SetRasterizerState(CullingMode);
        SetVertexShader( CompileShader( vs_5_0, VS() ) );
        SetHullShader( NULL );
        SetDomainShader( NULL );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_5_0, PS() ) );
    }
}
