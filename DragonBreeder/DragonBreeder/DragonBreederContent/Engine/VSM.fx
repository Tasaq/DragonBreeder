Texture2DArray CSM_Depth;
Texture2D DepthMap;
float4x4 InvertViewProjection;
float4x4 LightInvertViewProjection[3];
float2 Resolution = float2(1280.0f, 720.0f);
float3 LightPosition;
int slice;
float clipSpace1 = 70.0f;
float clipSpace2 = 70.0f;
float clipSpace3 = 70.0f;
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
float3 DepthToPosition(float2 TexCoord, float d, float4x4 InvertViewProjection)
{
	float4 position;
	if(d==-1)
		return -1;
	position.x =   TexCoord.x * 2.0f - 1.0f;
	position.y = -(TexCoord.y * 2.0f - 1.0f);
	position.z = d;
	position.w = 1.0f;
	position = mul(position, InvertViewProjection);
	position /= position.w;
	return position;
}
float4 getTexCoordNoBackProj(float3 position, float4x4 VP)
{

	float4 texcoord = mul(float4(position, 1.0), VP);
	if(texcoord.z < 0)
		return 0;
	texcoord.x = ((texcoord.x / texcoord.w) * 0.5 ) + 0.5;
	texcoord.y = ((texcoord.y / texcoord.w) * -0.5 ) + 0.5;
	//texcoord.z = ((texcoord.z / texcoord.w) * 0.5 ) + 0.5;
	texcoord.w = (texcoord.w / texcoord.w);
	return  texcoord;
}
   float linstep(float mini, float maxi, float v)  
{  
  return clamp((v - mini) / (maxi- mini),0, 1);  
}  
float g_MinVariance = 0.001;
float ChebyshevUpperBound(float2 M, float t)  
{  
	float p = (t <= M.x);
	float Variance = M.y - (M.x*M.x);
	Variance = max(Variance, g_MinVariance);
	float d = t-M.x;
	float p_max = Variance/(Variance+d*d);
	p_max = linstep(0.8, 1, p_max);
	return pow(max(p, p_max),10);
} 
float4 VSM_PS( VS_OUT input ) : SV_TARGET
{
	float2 Moments = 1;
	float2 TexCoord = input.texcoord;
	float depth = DepthMap.Sample(TexSampler, TexCoord);
	float3 position = DepthToPosition(TexCoord, depth, InvertViewProjection);
	float4 lightTexCoord = getTexCoordNoBackProj(position, LightInvertViewProjection[0]);
	float4 colorDebugger = 0;
	if(lightTexCoord.x < 0 || lightTexCoord.x > 0.99 || lightTexCoord.y < 0 || lightTexCoord.y > 0.99 || lightTexCoord.z > 0.99)
	{
		lightTexCoord = getTexCoordNoBackProj(position, LightInvertViewProjection[1]);
		if(lightTexCoord.x < 0 || lightTexCoord.x > 1 || lightTexCoord.y < 0 || lightTexCoord.y > 1 || lightTexCoord.z > 1)
		{
			lightTexCoord = getTexCoordNoBackProj(position, LightInvertViewProjection[2]);
			Moments = CSM_Depth.Sample(TexSampler,float3( lightTexCoord.xy, 2))*clipSpace3;
			colorDebugger= float4(0,0,1,1);
		}
		else
		{
			Moments = CSM_Depth.Sample(TexSampler,float3( lightTexCoord.xy, 1))*clipSpace2;
			colorDebugger= float4(1,0,0,1);
		}
	}
	else
	{
		Moments = CSM_Depth.Sample(TexSampler,float3( lightTexCoord.xy, 0))*clipSpace1;
		colorDebugger= float4(0,1,0,1);
	}
	float3 ans = ChebyshevUpperBound(Moments, lightTexCoord.z);

    return float4(saturate(ans*1000), 1);//*colorDebugger+(colorDebugger*0.1f);
} 
float4 VSM_PS_NOVAR( VS_OUT input ) : SV_TARGET
{
	float2 Moments = 1;
	float2 TexCoord = input.texcoord;
	float depth = DepthMap.Sample(TexSampler, TexCoord).r;
	float3 position = DepthToPosition(TexCoord, depth, InvertViewProjection);
	float4 lightTexCoord = getTexCoordNoBackProj(position, LightInvertViewProjection[0]);
	float4 colorDebugger = 0;
	lightTexCoord = getTexCoordNoBackProj(position, LightInvertViewProjection[2]);
	Moments = CSM_Depth.Sample(TexSampler,float3( lightTexCoord.xy, 2))*clipSpace3;

	float3 ans = ChebyshevUpperBound(Moments, lightTexCoord.z);

    return float4(ans, 1);//*colorDebugger+(colorDebugger*0.1f);
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


technique11 Directional
{
    pass P0
    {
		SetDepthStencilState( DisableDepth, 0 );
		 SetRasterizerState(CullingMode);
        SetVertexShader( CompileShader( vs_5_0, VS() ) );
        SetHullShader( NULL );
        SetDomainShader( NULL );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_5_0, VSM_PS() ) );
    }
}