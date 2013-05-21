float2 TextureDim;
float2 ClickPosition;
float PixelDistance;
float Strength=0.1f;
float Smoothen=2;
float4x4 invViewProjection;
float4 Rectangle;
Texture2D<float4> inputTex;
Texture2D<float> Depth;
RWTexture2D<float4> outputTex;// : register(u0);

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
[numthreads(32, 32, 1)]
void Paint2D(uint3 DTid : SV_DispatchThreadID)
{
	float2 TexCoord = float2(DTid.xy)/TextureDim;
	float2 ClickCoord = ClickPosition/TextureDim;
	float4 input = inputTex[DTid.xy];
	float ModValue=0;
	
	float2 dd = ClickCoord - TexCoord;
	float d = sqrt(dd.x*dd.x+dd.y*dd.y);
	d = distance(ClickCoord, TexCoord);
	//if(d < PixelDistance)
	{
		ModValue = pow(Strength*pow(saturate(1-(d/PixelDistance)),Smoothen),1/2.2);
	}
	outputTex[DTid.xy] = float4(input.xyz+ModValue,1);
}
[numthreads(32, 32, 1)]
void Paint3D(uint3 DTid : SV_DispatchThreadID)
{
	float2 TexCoord = float2(DTid.xy)/TextureDim;
	float2 ClickCoord = ClickPosition/float2(1280/2,720);
	float zbuf = Depth[ClickPosition.xy].r;
	float3 Position = DepthToPosition(ClickCoord ,zbuf, invViewProjection);
	ClickCoord = float2((Position.x+100.0f)/200.0f, (Position.z+100.0f)/200.0f);
	float4 input = pow(inputTex[DTid.xy],1);
	if(zbuf <=0)
	{
		outputTex[DTid.xy] = pow(input,1);
		return;
	}
	//ClickCoord.x= -ClickCoord.x;
	float ModValue=0;
	outputTex[DTid.xy] = input;
	float2 dd = ClickCoord - TexCoord;
	float d = sqrt(dd.x*dd.x+dd.y*dd.y);
	d = distance(ClickCoord, TexCoord);
	if(d < PixelDistance)
	{
		ModValue = pow(Strength*pow(saturate(1-(d/PixelDistance)),Smoothen),1/2.2);
	}
	outputTex[DTid.xy] = pow(float4(input.xyz+ModValue,1),1);
}


[numthreads(20, 20, 1)]
void CopyAtoB(uint3 DTid : SV_DispatchThreadID)
{
	outputTex[DTid.xy] = inputTex[DTid.xy];
}

technique11 paint2D
{
	pass paint
	{
		SetComputeShader(CompileShader(cs_5_0, Paint2D()));
	}
}
technique11 paint3D
{
	pass paint
	{
		SetComputeShader(CompileShader(cs_5_0, Paint3D()));
	}
}
technique11 copyAtoB
{
	pass copyAtoB
	{
		SetComputeShader(CompileShader(cs_5_0, CopyAtoB()));
	}
}