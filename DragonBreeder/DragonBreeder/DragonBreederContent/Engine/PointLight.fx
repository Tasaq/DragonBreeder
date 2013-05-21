float4x4 WorldViewProjection;
float4x4 Projection;
float4x4 View;
float4x4 ViewProjection;
float4x4 InvertViewProjection;
float4x4 LightViewProjection;

float LightIntensity = 1;
float3 Camera;
Texture2D NormalMap;
Texture2D DepthMap;
SamplerState TexSampler
{
    Filter = MIN_MAG_MIP_POINT;
};



struct VS_INgs
{
    float4 Position	: POSITION;
    float4 Color	: COLOR;
    float2 TexCoord	: TEXTURECOORD;
};
struct VS_OUTgs
{
	float4 Position	: POSITION;
    float3 Color	: COLOR;
    float2 TexCoord	: TEXTURECOORD;
};
struct GS_OUT
{
	float4 pos		: SV_POSITION;
	float3 Position	: TEXCOORD2;
    float3 Color	: COLOR0;
    float Distance	: TEXCOORD1;
    float2 TexCoord	: TEXTURECOORD;
};
#define kPI 3.1415926536f
VS_OUTgs VSgs( VS_INgs input)
{
    VS_OUTgs  Out;
	Out.Position = input.Position;
	Out.Color = input.Color;
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
  static const float scale = 1;
  GS_OUT output;
  output.Color = input[0].Color;
  output.Position = input[0].Position.xyz;
  output.Distance = input[0].Position.w;
  float NdC = dot(CameraDirection, normalize(input[0].Position - Camera));
  float3 Pos = mul(float4(input[0].Position.xyz,1), View).xyz;
  float4 v1 = float4(Pos+ g_positions[0]*output.Distance,1);
  float4 v2 = float4(Pos+ g_positions[1]*output.Distance,1);
  float4 v3 = float4(Pos+ g_positions[2]*output.Distance,1);
  float4 v4 = float4(Pos+ g_positions[3]*output.Distance,1);

  
  output.pos = mul(v1, Projection);
  output.TexCoord = float2(0,0);
  stream.Append(output);

  output.pos = mul(v2, Projection);
  output.TexCoord = float2(1,0);
  stream.Append(output);
  
  output.pos = mul(v3, Projection);
  output.TexCoord = float2(0,1);
  stream.Append(output);
  
  output.pos = mul(v4, Projection);
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
float4 Phong(float3 Color, float3 LightPosition, float LightDistance, float3 position, float3 Normal)
{
	float3 LightDirection = normalize(-position.xyz+LightPosition);
	float NdL = saturate(dot(LightDirection, Normal));
	float3 H = normalize(normalize(Camera - position) + LightDirection);
	float NdH =  pow( saturate( dot( H, Normal ) ), 60.0f );
	//return float4(NdL * Color.rgb, 1);
	float4 result = saturate(1 -  (distance(position, LightPosition) / LightDistance));
	result *= LightIntensity*float4(NdL * Color.rgb, NdH);
	return result;
}
float4 Phong(float3 Color, float3 LightPosition, float LightDistance, float3 position, float3 Normal, float specularExp, float specularInt)
{
	float3 LightDirection = normalize(LightPosition-position.xyz);
	float NdL = saturate(dot(LightDirection, Normal));
	float3 H = normalize(normalize(Camera - position) + LightDirection);
	float NdH =  specularInt*pow( saturate( dot( H, Normal ) ), 24*specularExp );
	float4 result = saturate(1 - (distance(position, LightPosition) / LightDistance));
	result *= LightIntensity*float4(NdL * Color.rgb, NdH);
	return result;
}
float2 Resolution;
float4 PSgs( GS_OUT input ) : SV_TARGET0
{
	//return float4(1.0,0.5,0.5,1); 
	float4 result = 0;
	float2 TexCoord = input.pos.xy/Resolution;
	//return float4(TexCoord, 0, 1);
	float3 LightPosition = input.Position.xyz;
	float d = DepthMap.Sample(TexSampler, TexCoord);
	if(d.r <= 0)
	{
		clip(-1);
		return float4(0,0,0,0);
	}
	float3 position = DepthToPosition(TexCoord, d, InvertViewProjection);
	float3 normal = decode(float3(NormalMap.Sample(TexSampler, TexCoord).rg,1));
	if(length(input.Color.rgb) == 0)
		input.Color.rgb = float3(1,0,0);
	result = Phong(input.Color.rgb, LightPosition, input.Distance, position.xyz, normal);
	return max(0,result);
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
    DestBlendAlpha = ONE;
    BlendOpAlpha = ADD;
    RenderTargetWriteMask[0] = 0x0F;
};

technique11 PointLightGS
{
    pass P0
    {
		 SetDepthStencilState( DisableDepth, 0 );
		 SetBlendState( AdditiveBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF );
		 SetRasterizerState(CullingMode);
        SetVertexShader( CompileShader( vs_5_0, VSgs() ) );
        SetHullShader( NULL );
        SetDomainShader( NULL );
        SetGeometryShader( CompileShader( gs_5_0, GS() )  );
        SetPixelShader( CompileShader( ps_5_0, PSgs() ) );
    }
}