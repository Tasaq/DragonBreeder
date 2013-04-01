float4x4 World;
float4x4 ViewProjection;
float4 Color;

struct VS_IN
{
    float4 Position: POSITION;
	float3 Normal  : NORMAL;
    uint Pos	   : SV_VertexID ;
};
struct VS_OUT
{
    float4 Pos : SV_Position;		//Position
    float4 Position: TEXCOORD1;
	float3 Normal: TEXCOORD0;
};
struct PS_OUT
{
    float4 Depth :SV_Target0;		//Position
    float2 Normal :SV_Target1;
	float4 Color :SV_Target2;
};


VS_OUT VS( VS_IN input )
{
    VS_OUT output;
    output.Pos = mul(input.Position, mul(World,ViewProjection));
    output.Position = mul(input.Position, mul(World,ViewProjection));
	output.Normal = normalize(mul(input.Normal, World));
	//output.Pos = output.Position;
    return output;
}
#define kPI 3.1415926536f
half2 encode (float3 n)
{
    half scale = 1.7777;
    half2 enc = n.xy / (n.z+1);
    enc /= scale;
    enc = enc*0.5+0.5;
    return half4(enc,0,0);
}
PS_OUT PS( VS_OUT input ) : SV_TARGET
{	
	PS_OUT output;
	output.Normal = 0;
	output.Depth =  input.Position.z/input.Position.w;
	output.Normal = encode(input.Normal);
	output.Color =  Color;
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
