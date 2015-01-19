float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;
float4 ClipPlane;
float3 SunlightDirection = float3(-10, 20, 5);

Texture2D Texture;
SamplerState TextureSampler;

float3 RotateAxis = float3(0,-1,0);
float4 DiffuseColor = float4(255, 255, 255, 1);

struct VertexShaderInput
{
	float4 Position : SV_Position;
	float2 UV : TEXCOORD0;
};

struct VertexShaderOutput
{
 	float4 Position : SV_Position;
	float2 UV : TEXCOORD0;
	float3 WorldPosition : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);
    float4x4 viewProjection = mul(View, Projection);
    
    output.WorldPosition = worldPosition;
    output.Position = mul(worldPosition, viewProjection);

	output.UV = input.UV;

    return output;
}


float4 PixelShaderFunction(VertexShaderOutput input) : SV_Target
{
	float4 sampledColor = Texture.Sample(TextureSampler, input.UV) * DiffuseColor.a;
	return sampledColor * float4(DiffuseColor.xyz,1);
}

 
float4 PixelShaderFunctionClipPlane(VertexShaderOutput input) : SV_Target
{
	clip(dot(float4(input.WorldPosition,1), ClipPlane));
	return PixelShaderFunction(input);
}


technique TechStandard
{
    pass Pass1
    {
		SetGeometryShader(0);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderFunction()));
	}
}

technique TechClipPlane
{
    pass Pass1
    {
		SetGeometryShader(0);
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetPixelShader(CompileShader(ps_4_0, PixelShaderFunctionClipPlane()));
	}
}
