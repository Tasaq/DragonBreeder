float4x4 World;
float4x4 ViewProjection;
float4x4 WorldViewProjection;
float4 Color;
Texture2D DisplacementMap;
Texture2D LayersMap;
Texture2D tex;
float scale =5.1f;
float2 quadID;
float3 CameraPosition;
float3 CameraDirection;
int quadID_MAX;
float dispPow = 0.0f;
SamplerState TexSampler
{
    Filter = MIN_MAG_MIP_LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};
struct HS_PATCH_DATA
{
	float3 b210 : POSITION3;
	float3 b120 : POSITION4;
	float3 b021 : POSITION5;
	float3 b012 : POSITION6;
	float3 b102 : POSITION7;
	float3 b201 : POSITION8;
	float3 b111 : CENTER;

	float3 n110 : NORMAL3;
	float3 n011 : NORMAL4;
	float3 n101 : NORMAL5;

	float edge[3] : SV_TessFactor;
	float inside : SV_InsideTessFactor;
};
struct HS_CONTROL_POINT
{
	float4 pos		: POSITION;
	float3 norm		: NORMAL;
	float2 TexCoord : TEXTURECOORD;
};

struct VS_IN
{
    float4 Position : POSITION;
	float3 Normal : NORMAL;
	float2 TexCoord : TEXTURECOORD;
};
struct VS_OUT
{
    float4 Position : SV_POSITION;
	float3 Normal : NORMAL;
	float2 TexCoord : TEXTURECOORD;
	float4 pos : TEXCOORD1;
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
	float2 TexCoord : TEXTURECOORD;
};


VS_IN VS( VS_IN input )
{
	VS_IN output;
	output.Position = mul(float4(input.Position.xyz,1),World);
	output.Normal = normalize(mul((input.Normal),World));
	output.TexCoord = input.TexCoord;
    return output;
}

[domain("tri")]
[outputtopology("triangle_cw")]
[outputcontrolpoints(3)]
[partitioning("fractional_odd")]
[patchconstantfunc("HS_PatchConstant")]
HS_CONTROL_POINT HS_ControlPointPhase(InputPatch<VS_IN, 3> inputPatch,
									  uint tid : SV_OutputControlPointID,
									  uint pid : SV_PrimitiveID)
{
	HS_CONTROL_POINT output;
	output.pos = inputPatch[tid].Position;
	output.norm = inputPatch[tid].Normal;
	output.TexCoord = inputPatch[tid].TexCoord;
	return output;
}
float2 rand_2_10(in float2 uv) {
    float noiseX = (frac(sin(dot(uv, float2(12.9898,78.233) * 2.0)) * 43758.5453));
    float noiseY = sqrt(1 - noiseX * noiseX);
    return float2(noiseX, noiseY);
}
float TessellationFactor = 16;
[domain("tri")]
HS_PATCH_DATA HS_PatchConstant(OutputPatch<HS_CONTROL_POINT, 3> controlPoints, uint pid : SV_PrimitiveID)
{
	HS_PATCH_DATA patch = (HS_PATCH_DATA)1;
	float3 normal = normalize(controlPoints[0].norm + controlPoints[1].norm + controlPoints[2].norm);
	float culling = dot(normal, CameraDirection);
	if (culling >= -0.5)
	{
		patch.edge[0] = patch.edge[1] = patch.edge[2] = TessellationFactor;
		patch.inside = ( patch.edge[0] + patch.edge[1] + patch.edge[2] ) / 3.0f;
	}
	else
	{
		patch.edge[0] = patch.edge[1] = patch.edge[2] = 0.0f;
		patch.inside = ( patch.edge[0] + patch.edge[1] + patch.edge[2] ) / 3.0f;
	}
	float3 b003 = controlPoints[0].pos;
	float3 b030 = controlPoints[1].pos;
	float3 b300 = controlPoints[2].pos;
	// And Normals
	float3 n002 = controlPoints[0].norm;
	float3 n020 = controlPoints[1].norm;
	float3 n200 = controlPoints[2].norm;

	// Compute the cubic geometry control points
	// Edge control points
	patch.b210 = ( ( 2.0f * b003 ) + b030 - ( dot( ( b030 - b003 ), n002 ) * n002 ) ) / 3.0f;
	patch.b120 = ( ( 2.0f * b030 ) + b003 - ( dot( ( b003 - b030 ), n020 ) * n020 ) ) / 3.0f;
	patch.b021 = ( ( 2.0f * b030 ) + b300 - ( dot( ( b300 - b030 ), n020 ) * n020 ) ) / 3.0f;
	patch.b012 = ( ( 2.0f * b300 ) + b030 - ( dot( ( b030 - b300 ), n200 ) * n200 ) ) / 3.0f;
	patch.b102 = ( ( 2.0f * b300 ) + b003 - ( dot( ( b003 - b300 ), n200 ) * n200 ) ) / 3.0f;
	patch.b201 = ( ( 2.0f * b003 ) + b300 - ( dot( ( b300 - b003 ), n002 ) * n002 ) ) / 3.0f;
	// Center control point
	float3 E = ( patch.b210 + patch.b120 + patch.b021 + patch.b012 + patch.b102 + patch.b201 ) / 6.0f;
	float3 V = ( b003 + b030 + b300 ) / 3.0f;
	patch.b111 = E + ( ( E - V ) / 2.0f );

	// Compute the quadratic normal control points, and rotate into world space
	float V12 = 2.0f * dot( b030 - b003, n002 + n020 ) / dot( b030 - b003, b030 - b003 );
	patch.n110 = normalize( n002 + n020 - V12 * ( b030 - b003 ) );
	float V23 = 2.0f * dot( b300 - b030, n020 + n200 ) / dot( b300 - b030, b300 - b030 );
	patch.n011 = normalize( n020 + n200 - V23 * ( b300 - b030 ) );
	float V31 = 2.0f * dot( b003 - b300, n200 + n002 ) / dot( b003 - b300, b003 - b300 );
	patch.n101 = normalize( n200 + n002 - V31 * ( b003 - b300 ) );

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
[domain("tri")]
DS_OUTPUT DS_PNtriangles(HS_PATCH_DATA patchData, const OutputPatch<HS_CONTROL_POINT, 3> input, float3 uvw : SV_DOMAINLOCATION)
{
	DS_OUTPUT output;
	float u = uvw.x;
	float v = uvw.y;
	float w = uvw.z;
	
	float uu = u * u;
	float vv = v * v;
	float ww = w * w;
	float uu3 = uu * 3.0f;
	float vv3 = vv * 3.0f;
	float ww3 = ww * 3.0f;
	
	float3 position = input[0].pos * ww * w +
					  input[1].pos * uu * u +
					  input[2].pos * vv * v +
					  patchData.b210 * ww3 * u +
					  patchData.b120 * w * uu3 +
					  patchData.b201 * ww3 * v +
					  patchData.b021 * uu3 * v +
					  patchData.b102 * w * vv3 +
					  patchData.b012 * u * vv3 +
					  patchData.b111 * 6.0f * w * u * v;

					  
	float3 normal = input[0].norm * ww +
					input[1].norm * uu +
					input[2].norm * vv +
					patchData.n110 * w*u +
					patchData.n011 * u*v +
					patchData.n101 * w*v;

	float4 final_position = float4(position.xyz, 1.0f);
	output.Position = mul(final_position, ViewProjection);
	output.Normal = normalize(normal);
	output.TexCoord = (input[0].TexCoord*u+input[1].TexCoord*v+input[2].TexCoord*w)/3.0f;
	return output;		
}
[domain("tri")]
DS_OUTPUT DS_PNtrianglesDisplace(HS_PATCH_DATA patchData, const OutputPatch<HS_CONTROL_POINT, 3> input, float3 uvw : SV_DOMAINLOCATION)
{
	DS_OUTPUT output;
	float u = uvw.x;
	float v = uvw.y;
	float w = uvw.z;
	
	float uu = u * u;
	float vv = v * v;
	float ww = w * w;
	float uu3 = uu * 3.0f;
	float vv3 = vv * 3.0f;
	float ww3 = ww * 3.0f;
	
	float3 position = input[0].pos * ww * w +
					  input[1].pos * uu * u +
					  input[2].pos * vv * v +
					  patchData.b210 * ww3 * u +
					  patchData.b120 * w * uu3 +
					  patchData.b201 * ww3 * v +
					  patchData.b021 * uu3 * v +
					  patchData.b102 * w * vv3 +
					  patchData.b012 * u * vv3 +
					  patchData.b111 * 6.0f * w * u * v;

					  
	float3 normal = input[0].norm * ww +
					input[1].norm * uu +
					input[2].norm * vv +
					patchData.n110 * w*u +
					patchData.n011 * u*v +
					patchData.n101 * w*v;
	output.TexCoord = (input[0].TexCoord*w+input[1].TexCoord*u+input[2].TexCoord*v);
	float disp = (DisplacementMap.SampleLevel(TexSampler, output.TexCoord.xy, 0)).r* dispPow;
	output.Normal = normalize(normal);
	float3 final_position = position.xyz + disp * output.Normal ;
	output.Normal = normalize(normal* (1-disp/0.1f) + normalFromTexture(output.TexCoord)*(disp/0.1f) );
	output.Position = mul(float4(final_position,1.0f), ViewProjection);
	return output;		
}
[maxvertexcount(12)]
void GS( triangle DS_OUTPUT input[3], inout TriangleStream<VS_OUT> triStream )
{
	VS_OUT output;
	float4 v0 = input[0].Position;
	float4 v1 = input[1].Position;
	float4 v2 = input[2].Position;

	float3 n0 = input[0].Normal;
	float3 n1 = input[1].Normal;
	float3 n2 = input[2].Normal;
	
	float2 t0 = input[0].TexCoord;
	float2 t1 = input[1].TexCoord;
	float2 t2 = input[2].TexCoord;
	float4 res = v0 + v1 + v2;

	VS_OUT input2;
	input2.Position = v0;
	input2.Normal	= n0;
	input2.TexCoord = t0;
	input2.pos = input2.Position;
	triStream.Append(input2);
	input2.Position = v1;
	input2.Normal	= n1;
	input2.TexCoord = t1;
	input2.pos = input2.Position;
	triStream.Append(input2);
	input2.Position = v2;
	input2.Normal	= n2;
	input2.TexCoord = t2;
	input2.pos = input2.Position;
	triStream.Append(input2);
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
	output.Color = float4(1,0,0,1);
	return output;
}
PS_OUT PS_Textured( VS_OUT input ) : SV_TARGET
{

	PS_OUT output;
	output.Normal = 0;
	output.Color = 0;
	output.Depth = input.pos.z/input.pos.w;
	output.Normal.rg = encode(input.Normal);
	
	output.Color = float4(0,0.1f,0.1f,1)+float4(DisplacementMap.Sample(TexSampler, input.TexCoord).rgb,1.0f)*float4(1,0,0,1);
	//output.Color = float4(input.TexCoord.rg,0,1);
	return output;
}

technique11 Render
{
    pass P0
    {
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
        SetVertexShader( CompileShader( vs_5_0, VS() ) );
        SetHullShader( CompileShader( hs_5_0, HS_ControlPointPhase() ) );
        SetDomainShader( CompileShader( ds_5_0, DS_PNtriangles() ) );
        SetGeometryShader(CompileShader(gs_5_0, GS() ));
        SetPixelShader( CompileShader( ps_5_0, PS_Textured() ) );
    }
}
technique11 RenderTexturedDisplaced
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_5_0, VS() ) );
        SetHullShader( CompileShader( hs_5_0, HS_ControlPointPhase() ) );
        SetDomainShader( CompileShader( ds_5_0, DS_PNtrianglesDisplace() ) );
        SetGeometryShader(CompileShader(gs_5_0, GS() ));
        SetPixelShader( CompileShader( ps_5_0, PS_Textured() ) );
    }
}