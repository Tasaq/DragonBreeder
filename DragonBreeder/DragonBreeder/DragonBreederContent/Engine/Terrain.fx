float4x4 World;
float4x4 ViewProjection;
float4x4 WorldViewProjection;
float4 Color;
Texture2D DisplacementMap;
Texture2D LayersMap;
Texture2D Textures[5];
Texture2D TexturesNormals[5];
float scale =20.1f;
float2 quadID;
float3 CameraPosition;
int quadID_MAX;
float clipSpace[3];
float4x4 ViewProjectionSplits[3];
SamplerState TexSampler
{
    Filter = MIN_MAG_MIP_LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};
struct HS_PATCH_DATA
{
	float edges[4] : SV_TessFactor;
	float inside[2] : SV_InsideTessFactor;
};
struct HS_CONTROL_POINT
{
	float4 pos		: POSITION;
	float3 norm		: NORMAL;
	float4 TexCoord : TEXTURECOORD;
};

struct VS_IN
{
    float4 Position : POSITION;
	float3 Normal : NORMAL;
	float4 TexCoord : TEXTURECOORD;
};
struct VS_OUT
{
    float4 Position : SV_POSITION;
	float3 Normal : NORMAL;
	float2 TexCoord : TEXTURECOORD;
	float4 pos : TEXCOORD1;
};
struct VS_OUT_TEST
{
    float4 Position : SV_POSITION;
	float3 Normal : NORMAL;
	float2 TexCoord : TEXTURECOORD;
	float4 pos : TEXCOORD1;
	float3 Tangent  : TANGENT0;
	float3 BiNormal : BINORMAL0;
};
struct PS_OUT
{
    float4 Depth : SV_Target0; //Position
    float4 Normal : SV_Target1;
	float4 Color : SV_Target2;
};
struct DS_OUTPUT
{
    float4 Position : POSITION;
	float3 Normal : NORMAL;
	float4 TexCoord : TEXTURECOORD;
};
struct GS_OUT
{
    float4 Position : SV_POSITION;
	float3 Normal : NORMAL;
	float2 TexCoord : TEXTURECOORD;
	float4 pos : TEXCOORD1;
    uint RTIndex : SV_RenderTargetArrayIndex;
};


VS_IN VS( VS_IN input )
{
    return input;
}

[domain("quad")]
[outputtopology("triangle_ccw")]
[outputcontrolpoints(4)]
[partitioning("fractional_odd")]
[patchconstantfunc("HS_PatchConstant")]
HS_CONTROL_POINT HS_ControlPointPhase(InputPatch<VS_IN, 4> inputPatch,
									  uint tid : SV_OutputControlPointID,
									  uint pid : SV_PrimitiveID)
{
	HS_CONTROL_POINT output;
	output.pos = inputPatch[tid].Position;
	output.norm = inputPatch[tid].Normal;
	output.TexCoord = (inputPatch[tid].Position+1)*0.5f;
	return output;
}
float2 rand_2_10(in float2 uv) {
    float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43758.5453));
    float noiseY = sqrt(1 - noiseX * noiseX);
    return float2(noiseX, noiseY);
}
float3 CameraDirection;
float tfact=10;
[domain("quad")]
HS_PATCH_DATA HS_PatchConstant(OutputPatch<HS_CONTROL_POINT, 4> controlPoints)
{
	HS_PATCH_DATA patch = (HS_PATCH_DATA)1;
	float3 sum = controlPoints[0].pos.xyz+controlPoints[1].pos.xyz+controlPoints[2].pos.xyz+controlPoints[3].pos.xyz;
	sum/=4;
	float e0 = distance(CameraPosition.xyz, mul(float4(controlPoints[0].pos.xyz,1),World).xyz);
	float e1 = distance(CameraPosition.xyz, mul(float4(controlPoints[1].pos.xyz,1),World).xyz);
	float e2 = distance(CameraPosition.xyz, mul(float4(controlPoints[2].pos.xyz,1),World).xyz);
	float e3 = distance(CameraPosition.xyz, mul(float4(controlPoints[3].pos.xyz,1),World).xyz);
	int mtessfact = tfact;
	float center = distance(CameraPosition.xz, mul(float4(sum,1), World).xz);
	center =pow((1-saturate(center/300.0f)),10);
	e0 =max(2,mtessfact*pow((1-saturate(e0/100.0f)),3));
	e1 =max(2,mtessfact*pow((1-saturate(e1/100.0f)),3));
	e2 =max(2,mtessfact*pow((1-saturate(e2/100.0f)),3));
	e3 =max(2,mtessfact*pow((1-saturate(e3/100.0f)),3));
	patch.edges[0]  =min(e0, e3);
	patch.edges[1]  =min(e0, e1);
	patch.edges[2]  =min(e1, e2);
	patch.edges[3]  =min(e2, e3);
	patch.inside[0] =1;//max(3,20*mtessfact*center);
	patch.inside[1] =1;//max(3,20*mtessfact*center);//max(max(patch.edges[0], patch.edges[1]), patch.edges[2]);
	float minTess = min(patch.edges[1], patch.edges[3]);
	patch.inside[0] = minTess;
	patch.inside[1] = minTess;
	return patch;
}
float heightMapSizeX=512.0f;
float3 normalFromTexture(float2 texCoord)
{
		float3 norm = float3(0,1,0);
		float me =length(DisplacementMap.SampleLevel(TexSampler,texCoord,0));
		float n = length(DisplacementMap.SampleLevel(TexSampler,float2(texCoord.x,texCoord.y+1.0/heightMapSizeX),0));
		float s = length(DisplacementMap.SampleLevel(TexSampler,float2(texCoord.x,texCoord.y-1.0/heightMapSizeX),0));
		float e = length(DisplacementMap.SampleLevel(TexSampler,float2(texCoord.x+1.0/heightMapSizeX,texCoord.y),0));
		float w = length(DisplacementMap.SampleLevel(TexSampler,float2(texCoord.x-1.0/heightMapSizeX,texCoord.y),0));                

	//find perpendicular vector to norm:        
	float3 temp = norm; //a temporary vector that is not parallel to norm
	if(norm.x==1)
		temp.y+=0.5;
	else
		temp.x+=0.5;

	//form a basis with norm being one of the axes:
	float3 perp1 = normalize(cross(norm,temp));
	float3 perp2 = normalize(cross(norm,perp1));

	//use the basis to move the normal in its own space by the offset        
	float3 normalOffset = 50*(((n-me)-(s-me))*perp1 + ((e-me)-(w-me))*perp2);
	norm += normalOffset;
	norm = normalize(norm);
	return norm;
}
struct NTB
{
	float3 N;
	float3 T;
	float3 B;
};
NTB NTBfromHeightMap(float2 texCoord)
{
		NTB result;
		result.N=0;
		result.T=0;
		result.B=0;
		float3 norm = float3(0,1,0);
		float me =length(DisplacementMap.SampleLevel(TexSampler,texCoord,0));
		float n = length(DisplacementMap.SampleLevel(TexSampler,float2(texCoord.x,texCoord.y+1.0/heightMapSizeX),0));
		float s = length(DisplacementMap.SampleLevel(TexSampler,float2(texCoord.x,texCoord.y-1.0/heightMapSizeX),0));
		float e = length(DisplacementMap.SampleLevel(TexSampler,float2(texCoord.x+1.0/heightMapSizeX,texCoord.y),0));
		float w = length(DisplacementMap.SampleLevel(TexSampler,float2(texCoord.x-1.0/heightMapSizeX,texCoord.y),0));                

	//find perpendicular vector to norm:        
	float3 temp = norm; //a temporary vector that is not parallel to norm
	temp.x+=0.5;

	//form a basis with norm being one of the axes:
	float3 perp1 = normalize(cross(norm,normalize(temp)));
	float3 perp2 = normalize(cross(norm,perp1));

	//use the basis to move the normal in its own space by the offset        
	float3 normalOffset = 50*(((n-me)-(s-me))*perp1 + ((e-me)-(w-me))*perp2);
	norm += normalOffset;
	result.N = normalize(norm);
	return result;
}
[domain("quad")]
DS_OUTPUT DS_PNtriangles(HS_PATCH_DATA patchData, const OutputPatch<HS_CONTROL_POINT, 4> input, float2 uvw : SV_DOMAINLOCATION)
{
	DS_OUTPUT output;

	float u = uvw.x;
	float v = uvw.y;

    float4 topMidpoint = lerp(input[0].pos, input[1].pos, uvw.x);
    float4 bottomMidpoint = lerp(input[3].pos, input[2].pos, uvw.x);
    output.Position = float4(lerp(topMidpoint, bottomMidpoint, uvw.y));

	
    topMidpoint = lerp(input[0].TexCoord, input[1].TexCoord, uvw.x);
    bottomMidpoint = lerp(input[3].TexCoord, input[2].TexCoord, uvw.x);
    output.TexCoord = float4(lerp(topMidpoint, bottomMidpoint, uvw.y));

    //output.Normal = normalize(normalFromTexture(output.TexCoord));
    output.Normal = normalize(NTBfromHeightMap(output.TexCoord).N);
	output.Position = mul(float4(output.Position.xyz,1), World);
	output.Position.y += DisplacementMap.SampleLevel(TexSampler, output.TexCoord, 0).y*scale;



	//output.Position = mul(output.Position, ViewProjection);
	return output;

}

[maxvertexcount(32)]
void GS( triangle DS_OUTPUT input[3], inout TriangleStream<VS_OUT_TEST> triStream )
{
	VS_OUT_TEST output;
	for(uint i = 0; i < 3; i++)
	{
		output.Normal = input[i].Normal;
		output.TexCoord = input[i].TexCoord;
		output.Position = mul(input[i].Position, ViewProjection);
		output.pos = output.Position;
		output.BiNormal = normalize(cross(normalize(input[i].Normal), float3(-1,0,0)));
		output.Tangent = normalize(cross(normalize(input[i].Normal), output.BiNormal));
		triStream.Append(output);
	}
	triStream.RestartStrip();
}
[maxvertexcount(32)]
void GS_Depth( triangle DS_OUTPUT input[3], inout TriangleStream<GS_OUT> triStream )
{
	//GS_OUT output;
	for(uint j = 0; j < 3; j++)
	{
		GS_OUT output;
		output.RTIndex = j;
		for(uint i = 0; i < 3; i++)
		{
			output.Normal = input[i].Normal;
			output.TexCoord = input[i].TexCoord;
			output.Position = mul(input[i].Position, ViewProjectionSplits[j]);
			output.pos = output.Position;
			triStream.Append(output);
		}
		triStream.RestartStrip();
	}
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
	output.Depth = input.pos.z/input.pos.w;
	output.Normal.rg = encode(input.Normal);
	output.Color = float4(input.TexCoord.rg,0,1);
	return output;
}
float BumpConstant = 1.0f;
PS_OUT PS_Textured( VS_OUT_TEST input ) : SV_TARGET
{
	PS_OUT output;
	output.Normal = 0;
	output.Color = 0;
	output.Depth = input.pos.z/input.pos.w;
	float3 a = LayersMap.Sample(TexSampler, input.TexCoord.rg).rgb;
	float val =  length(DisplacementMap.Sample(TexSampler, (input.TexCoord.rg)).rgb);
	val += 1.0f - input.Normal.y;
	val/=2.0f;
	float3 wsp = float3(0.01, 0.3, 0.5);
	a.r = min((val > wsp.x) * (val-wsp.x)* (val-wsp.x) * 50,1);
	a.g = min((val > wsp.y) * (val-wsp.y)* (val-wsp.y) * 10,1);
	a.b = min((val > wsp.z) * (val-wsp.z)* (val-wsp.z) * 10,1);
	input.TexCoord.rg *= 500.0f;
	output.Color.rgb =  Textures[4].Sample(TexSampler, (input.TexCoord.rg)).rgb;
	output.Color.rgb = output.Color.rgb*(1-a.r) + a.r*Textures[0].Sample(TexSampler, (input.TexCoord.rg)).rgb;
	output.Color.rgb = output.Color.rgb*(1-a.g) + a.g*Textures[1].Sample(TexSampler, (input.TexCoord.rg)).rgb;
	output.Color.rgb = output.Color.rgb*(1-a.b) + a.b*Textures[2].Sample(TexSampler, (input.TexCoord.rg)).rgb;
	output.Color.a=1;
	float3 outNorm = 0;
	float3 bump = 0;
	bump = BumpConstant * (TexturesNormals[4].Sample(TexSampler, input.TexCoord.rg) - (0.5, 0.5, 0.5));
	bump =bump*(1-a.r)+ BumpConstant * a.r*(TexturesNormals[0].Sample(TexSampler, input.TexCoord.rg) - (0.5, 0.5, 0.5));
	bump =bump*(1-a.g)+ BumpConstant * a.g*(TexturesNormals[1].Sample(TexSampler, input.TexCoord.rg) - (0.5, 0.5, 0.5));
	bump =bump*(1-a.b)+ BumpConstant * a.b*(TexturesNormals[2].Sample(TexSampler, input.TexCoord.rg) - (0.5, 0.5, 0.5));
	input.Normal = input.Normal + (bump.x * input.Tangent + bump.y * input.BiNormal);
	input.Normal = normalize(input.Normal);

	output.Normal.rg = encode(input.Normal);


	return output;
}
float2 PS_Depth( GS_OUT input ) : SV_Target
{

	float2 Moments;  
	float Depth = input.pos.z;
	//return input.pos.z/input.pos.w;
	float2 normal = float2(Depth, Depth*Depth);
	Moments.x = Depth; 
	float dx = ddx(Depth);  
	float dy = ddy(Depth);
	Moments.y = Depth*Depth + 0.25*(dx*dx + dy*dy);
	return (Moments.xy/clipSpace[input.RTIndex]);
}
DepthStencilState EnableDepth
{
    DepthEnable = TRUE;
    DepthWriteMask = ALL;
};
technique11 Render
{
    pass P0
    {
		SetDepthStencilState( EnableDepth, 0 );
        SetVertexShader( CompileShader( vs_5_0, VS() ) );
        SetHullShader( CompileShader( hs_5_0, HS_ControlPointPhase() ) );
        SetDomainShader( CompileShader( ds_5_0, DS_PNtriangles() ) );
        SetGeometryShader(CompileShader(gs_5_0, GS() ));
        SetPixelShader( CompileShader( ps_5_0, PS() ) );
    }
}

technique11 RenderTextured
{
    pass P0
    {
		SetDepthStencilState( EnableDepth, 0 );
        SetVertexShader( CompileShader( vs_5_0, VS() ) );
        SetHullShader( CompileShader( hs_5_0, HS_ControlPointPhase() ) );
        SetDomainShader( CompileShader( ds_5_0, DS_PNtriangles() ) );
        SetGeometryShader(CompileShader(gs_5_0, GS() ));
        SetPixelShader( CompileShader( ps_5_0, PS_Textured() ) );
    }
}
technique11 RenderShadows
{
    pass P0
    {
		SetDepthStencilState( EnableDepth, 0 );
        SetVertexShader( CompileShader( vs_5_0, VS() ) );
        SetHullShader( CompileShader( hs_5_0, HS_ControlPointPhase() ) );
        SetDomainShader( CompileShader( ds_5_0, DS_PNtriangles() ) );
        SetGeometryShader(CompileShader(gs_5_0, GS_Depth() ));
        SetPixelShader( CompileShader( ps_5_0, PS_Depth() ) );
    }
}