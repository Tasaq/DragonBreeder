float4x4 WorldViewProjection;
float4x4 World;
float4x4 Projection;
float4x4 View;
float4x4 ViewProjection;
float4x4 InvertViewProjection;
float4x4 LightViewProjection;
float4x4 invProjection;
float LightIntensity = 1;
float3 Camera;
int CloudCount = 0;
Texture2DArray Clouds;
Texture2D NormalMap;
Texture2D DepthMap;
Texture2D OcculusionMap;
SamplerState TexSampler
{
    Filter = MIN_MAG_MIP_LINEAR;
};
SamplerState TexSamplerPoint
{
    Filter = MIN_MAG_MIP_POINT;
};



struct VS_INgs
{
    float4 Position	: POSITION;
    float2 TexCoord	: TEXTURECOORD;
};
struct VS_OUTgs
{
	float4 Position	: POSITION;
    float2 TexCoord	: TEXTURECOORD;
};
struct GS_OUT
{
	float4 pos		: SV_POSITION;
	float4 Position	: TEXCOORD2;
    float4 Color	: COLOR0;
    float TextureID	: TEXCOORD1;
    float2 TexCoord	: TEXTURECOORD;
};
#define kPI 3.1415926536f
VS_OUTgs VSgs( VS_INgs input)
{
    VS_OUTgs  Out;
	Out.Position = input.Position;
	Out.TexCoord = input.TexCoord;
	return Out;
}
static const float3 g_positions[4] =
{
float3( -1, 1, 0 ),
float3( 1, 1, 0),
float3( -1, -1, 0 ),
float3( 1, -1, 0 ),
};
float3	CameraDirection;
[maxvertexcount(4)]
void GS( point VS_OUTgs input[1], inout TriangleStream<GS_OUT> stream )
{
  static const float scale = 1.0f;
  GS_OUT output;
  output.Color = float4(1,0,0,1);
  output.TextureID = input[0].TexCoord.x;
  float Distance = scale*input[0].TexCoord.y;
  float NdC = dot(CameraDirection, normalize(input[0].Position.xyz - Camera.xyz));
  float4 Pos = mul(float4(input[0].Position.xyz,1), View);
  float4 v1 = float4(Pos.xyz+ g_positions[0]*Distance,1);
  float4 v2 = float4(Pos.xyz+ g_positions[1]*Distance,1);
  float4 v3 = float4(Pos.xyz+ g_positions[2]*Distance,1);
  float4 v4 = float4(Pos.xyz+ g_positions[3]*Distance,1);

  
  output.pos = mul(v1, Projection);
  output.Position = output.pos;
  output.TexCoord = float2(0,0);
  stream.Append(output);

  output.pos = mul(v2, Projection);
  output.Position = output.pos;
  output.TexCoord = float2(1,0);
  stream.Append(output);
  
  output.pos = mul(v3, Projection);
  output.Position = output.pos;
  output.TexCoord = float2(0,1);
  stream.Append(output);
  
  output.pos = mul(v4, Projection);
  output.Position = output.pos;
  output.TexCoord = float2(1,1);
  stream.Append(output);
}

half3 decode (half3 enc)
{
    half2 ang = enc*2-1;
    half2 scth;
    sincos(ang.x * kPI, scth.x, scth.y);
    half2 scphi = half2(sqrt(1.0 - ang.y*ang.y), ang.y);
    return half3(scth.y*scphi.x, scth.x*scphi.x, scphi.y);
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
#define kPI 3.1415926536f
half2 encode (float3 n)
{
    return half4(
      (half2(atan2(n.y,n.x)/kPI, n.z)+1.0)*0.5,
      0,0);
}
float4 PSgs( GS_OUT input ) : SV_TARGET0
{ 
	float4 result = 0;
	float2 TexCoord = input.pos.xy/float2(1280,720);
	float d = DepthMap.Sample(TexSampler, TexCoord).r;
	float inD = (input.Position.z/input.Position.w);
    float4 depthViewSample = mul( float4( TexCoord, d, 1 ),invProjection  );
    float4 depthViewParticle = mul( float4( TexCoord, inD, 1 ), invProjection );
	float dvs = depthViewSample.z/depthViewSample.w;
	float dvp = depthViewParticle.z/depthViewParticle.w;
	if(d<=0)
	{
		dvp = 1000.0f;
	}
    float depthDiff = dvs - dvp;
    depthDiff=1-depthDiff;
	if( depthDiff < 0 )
	{
        clip(-1);
    }   
	float3 cloudColor = Clouds.Sample(TexSampler,float3(input.TexCoord, input.TextureID)).xyz;
    float  a = saturate( depthDiff / 2 )*0.1f;
	//return float4(a,a,a,1);
	//float3 position = DepthToPosition(TexCoord, d, InvertViewProjection);
	return float4(a* cloudColor,1);
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

technique11 RenderCloudsPost
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_5_0, VSgs() ) );
        SetHullShader( NULL );
        SetDomainShader( NULL );
        SetGeometryShader( CompileShader( gs_5_0, GS() )  );
        SetPixelShader( CompileShader( ps_5_0, PSgs() ) );
		SetDepthStencilState( DisableDepth, 0 );
		SetBlendState( AdditiveBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
		 SetRasterizerState(CullingMode);
    }
}




struct VS_IN
{
    float4 Position: POSITION;
	uint VertexID: SV_VertexID;
	float2 TexCoord  : TEXTURECOORD;
};
struct VS_OUT
{
    float4 position : SV_POSITION;		//Position
	float2 texcoord :TEXTURECOORD;
    float4 pos: TexCoord1;
};



VS_OUT VS( VS_IN input )
{
    VS_OUT output;
    output.position = mul(float4(input.Position.xyz,1), mul(World,ViewProjection));
    output.pos =mul(float4(input.Position.xyz,1), World);
	output.texcoord = input.TexCoord;
    return output;
}
float3 lpos = float3(1,1,1);
float4 PS( VS_OUT input ) : SV_TARGET
{
	float2 TexCoord = input.position.xy/float2(1280,720);
	float4 result = 0;
	float d = DepthMap.Sample(TexSampler, TexCoord).r;
	if(d<=0)
	{
		float3 color = lerp(float3(0.7,0.2,0.2),float3(0.4,0.45,0.8f),(saturate((input.pos.y-0.1)*0.1f)));
		return float4(color,1);
	}
	discard;
	return 1;
}



struct VS_IN_FS
{
    float4 Position: POSITION;
	uint VertexID: SV_VertexID;
};
struct VS_OUT_FS
{
    float4 position : SV_POSITION;		//Position
	float2 texcoord : TexCoord0;
};


VS_OUT VS_FS( VS_IN input)
{
    VS_OUT  Out;
	Out.position = float4(input.Position.xyz,1);
	Out.texcoord.x = (1+input.Position.x)*0.5f;
	Out.texcoord.y = (1-input.Position.y)*0.5f;
	return Out;
}
float3 LightPosition = float3(40,3,5);
float LightDistance = 50.0f;
float4 PS_FS( VS_OUT input ) : SV_TARGET
{
	float4 lightScreenPos = mul(float4(LightPosition, 1.0f), ViewProjection);
	float depth_data = DepthMap.Sample(TexSampler, input.texcoord);
	float depth_data2 = DepthMap.Sample(TexSampler, input.texcoord+0.01f);
	//lightScreenPos.xy /= lightScreenPos.w;
	//lightScreenPos.x += 0.45f;
	lightScreenPos.x = ((lightScreenPos.x / lightScreenPos.w) * 0.5 ) + 0.5;
	lightScreenPos.y = ((lightScreenPos.y / lightScreenPos.w) * -0.5 ) + 0.5;
	lightScreenPos.z = ((lightScreenPos.z / lightScreenPos.w) * 0.5 ) + 0.5;
	float test =  (depth_data.r < lightScreenPos.z/lightScreenPos.w);
	//float2 deltaCoord = (lightScreenPos.xy - input.TexCoord);
	float L2C = 1/distance(Camera, LightPosition);
	float dts = distance(lightScreenPos*float2(1.6,0.9), input.texcoord*float2(1.6,0.9));
	if(test && ( dts < LightDistance *L2C* 0.05f))
	{
		//return float4(1,1,1,1);
		return float4(float3(1,0.8,1)*(1-dts/(LightDistance*L2C * 0.05f)),1);
	}
    return float4(0,0,0,0);
}
int NUM_SAMPLES = 32;
float g_density = 0.75f;
float g_weight = 0.1f;
float g_exposure = 0.14f;
float g_decay = 0.95f;
float4 ShaftMaker(VS_OUT input) : SV_TARGET
{
    // TODO: add your pixel shader code here.
	//float test = 1-(.g > 3.0f/512.0f);
	float4 lightScreenPos = mul(float4(LightPosition, 1.0f), ViewProjection);
	lightScreenPos.x = ((lightScreenPos.x / lightScreenPos.w) * 0.5 ) + 0.5;
	lightScreenPos.y = ((lightScreenPos.y / lightScreenPos.w) * -0.5 ) + 0.5;
	float2 texCoord = input.texcoord;
	float2 deltaCoord = (lightScreenPos.xy - input.texcoord);

	deltaCoord *= 1.0f / NUM_SAMPLES * g_density;
	float illuminationDecay = 1.0f;
	float4 color = OcculusionMap.Sample(TexSampler, texCoord);
	for (int i = 0; i < NUM_SAMPLES; ++i)
	{
		texCoord += deltaCoord;
		float4 sample = OcculusionMap.Sample(TexSampler, texCoord);
		sample *= illuminationDecay * g_weight;
		color += sample;
		illuminationDecay *= g_decay;
	}
	color.a = 1.0f;

	return float4(color.xyz * g_exposure*10,1);
	//float2 deltaCoord = (lightScreenPos.xy - input.TexCoord);
    //return tex2D(depth, input.TexCoord);
}

technique11 FullScreenSun
{
    pass P0
    {
		SetDepthStencilState( DisableDepth, 0 );
	//	SetBlendState( AdditiveBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
		 SetRasterizerState(CullingMode);
        SetVertexShader( CompileShader( vs_5_0, VS_FS() ) );
        SetHullShader( NULL );
        SetDomainShader( NULL );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_5_0, PS_FS() ) );
    }
}

technique11 FullScreenShaft
{
    pass P0
    {
		SetDepthStencilState( DisableDepth, 0 );
	//	SetBlendState( AdditiveBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
		 SetRasterizerState(CullingMode);
        SetVertexShader( CompileShader( vs_5_0, VS_FS() ) );
        SetHullShader( NULL );
        SetDomainShader( NULL );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_5_0, ShaftMaker() ) );
    }
}
technique11 RenderSky
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_5_0, VS() ) );
        SetHullShader( NULL );
        SetDomainShader( NULL );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_5_0, PS() ) );
		SetDepthStencilState( DisableDepth, 0 );
		 SetRasterizerState(CullingMode);
    }
}