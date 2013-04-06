float4x4 World;
float4x4 ViewProjection;
float4x4 WorldViewProjection;
float4 Color;
Texture2D DisplacementMap;
float scale = 0.5f;
SamplerState TexSampler
{
    Filter = MIN_MAG_MIP_POINT;
	AddressU = Wrap;
	AddressV = Wrap;

};
struct HS_PATCH_DATA
{
	float edges[3]	: SV_TessFactor;
	float inside	: SV_InsideTessFactor;
	float center[3] : CENTER;
};
struct HS_CONTROL_POINT
{
	float3	pos1		: POSITION1;
	float3	pos2		: POSITION2;
	float3	pos3		: POSITION3;
	float3	norm1		: NORMAL0;
	float3	norm2		: NORMAL1;
	float2	TexCoord	: TEXCOORD0;
};

struct VS_IN
{
    float4 Position  : POSITION0;
	float3 Normal	 : NORMAL;
	float2 TexCoord  : TEXCOORD;
};
struct VS_OUT
{
    float4 Position  : SV_POSITION;
	float3 Normal	 : NORMAL;
	float2 TexCoord  : TEXCOORD;
};
struct PS_OUT
{
    float4 Depth	: SV_Target0;		//Position
    float4 Normal	: SV_Target1;
	float4 Color	: SV_Target2;
};
struct DS_OUTPUT
{
    float4 Position	: POSITION;
	float3 Normal	: NORMAL;
	float2 TexCoord	: TEXCOORD;
};


VS_IN VS( VS_IN input )
{
    return input;
}
float edgeLOD(float3 p1, float3 p2)
{
	return dot(p1, p2);
}
void getTessFact(inout HS_PATCH_DATA patch, OutputPatch<HS_CONTROL_POINT, 3> controlPoints, uint tid : SV_InstanceID)
{
	int next = (1 << tid) & 3;
	patch.edges[tid] = 2;//0.1f+edgeLOD((float3) controlPoints[tid].pos1,(float3) controlPoints[next].pos1);
	return;
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
	
	int next = (tid+1)%3;

	float3 p1 = inputPatch[tid].Position;	
	float3 p2 = inputPatch[next].Position;
		
	float3 n1 = inputPatch[tid].Normal;	
	float3 n2 = inputPatch[next].Normal;

	output.pos1 = p1;
	output.pos2 = (2 * p1+p2 - dot(p2-p1, n1) * n1);
	output.pos3 = (2 * p2+p1 - dot(p1-p2, n2) * n2);

	float3 v12 = 4 * dot(p2-p1, n1+n2) / dot(p2-p1, p2-p1);
	output.norm1 = n1;
	output.norm2 = n1+n2 - v12*(p2 - p1);

	output.TexCoord = inputPatch[tid].TexCoord;
	return output;
}
[domain("tri")]
HS_PATCH_DATA HS_PatchConstant(OutputPatch<HS_CONTROL_POINT, 3> controlPoints)
{
	HS_PATCH_DATA patch = (HS_PATCH_DATA)1;
	
	getTessFact(patch, controlPoints, 0);
	getTessFact(patch, controlPoints, 1);
	getTessFact(patch, controlPoints, 2);
	patch.inside = max(max(patch.edges[0], patch.edges[1]), patch.edges[2]);

	float3 Center =   (controlPoints[0].pos2 + controlPoints[0].pos3) * 0.5 -
					  controlPoints[0].pos1 + 
					  (controlPoints[1].pos2 + controlPoints[1].pos3) * 0.5 -
					  controlPoints[1].pos1 + 
					  (controlPoints[2].pos2 + controlPoints[2].pos3) * 0.5 -
					  controlPoints[2].pos1;
	patch.center = (float[3])Center;
	return patch;
}				
[domain("tri")]		
DS_OUTPUT DS_PNtriangles(HS_PATCH_DATA patchData, const OutputPatch<HS_CONTROL_POINT, 3> input, float3 uvw : SV_DOMAINLOCATION)
{
	DS_OUTPUT output;

	float u = uvw.x;
	float v = uvw.y;
	float w = uvw.z;

	float3 pos = (float3)input[0].pos1 * w*w*w + (float3)input[1].pos1 * u*u*u + (float3)input[2].pos1 * v*v*v +
				 (float3)input[0].pos2 * w*w*u + (float3)input[0].pos3 * w*u*u + (float3)input[1].pos2 * u*u*v +
				 (float3)input[1].pos3 * u*v*v + (float3)input[2].pos2 * v*v*w + (float3)input[2].pos3 * v*w*w +
				 (float3)patchData.center*u*v*w;

	float3 norm= input[0].norm1 * w*w + input[1].norm1 *u*u + input[2].norm1 * v*v +
				 input[0].norm2 * w*u + input[1].norm2 *u*v + input[2].norm2 * v*w;

	
    output.Position = mul(float4(uvw,1), WorldViewProjection);
	output.Normal = normalize(mul(norm, World));
	output.TexCoord = input[0].TexCoord *w+ input[1].TexCoord*v + input[2].TexCoord*u;

	//output.Position.xyz += DisplacementMap.SampleLevel(TexSampler, output.TexCoord,0).rgb * scale * output.Normal;
	return output;

}

[maxvertexcount(3)]
void GS( triangle DS_OUTPUT input[3], inout TriangleStream<VS_OUT> triStream )
{
	VS_OUT output;
	for(uint i = 0; i < 3; i++)
	{
		output.Normal	= input[i].Normal;
		output.Position = input[i].Position;
		output.TexCoord = input[i].TexCoord;
		triStream.Append(output);
	}
	triStream.RestartStrip();
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

technique11 Render
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_5_0, VS() ) );
        SetHullShader( CompileShader(	hs_5_0, HS_ControlPointPhase() ) );
        SetDomainShader( CompileShader( ds_5_0, DS_PNtriangles() ) );
        SetGeometryShader(CompileShader(gs_5_0, GS() ));
        SetPixelShader( CompileShader(	ps_5_0, PS() ) );
    }
}

technique11 RenderTextured
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_5_0, VS() ) );
        SetHullShader( CompileShader(	hs_5_0, HS_ControlPointPhase() ) );
        SetDomainShader( CompileShader( ds_5_0, DS_PNtriangles() ) );
        SetGeometryShader(CompileShader(gs_5_0, GS() ));
        SetPixelShader( CompileShader(	ps_5_0, PS() ) );
    }
}