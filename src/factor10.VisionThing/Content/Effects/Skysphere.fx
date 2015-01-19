float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;
float4 ClipPlane;
SamplerState TextureSampler;

TextureCube Texture;

struct VertexShaderInput
{
	float4 Position : SV_Position;
};

struct VertexShaderOutput
{
	float4 Position : SV_Position;
	float3 WorldPosition : TEXCOORD0;
	float3 PosL : TEXCOORD1;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

	float4 worldPosition = mul(input.Position, World);

	output.WorldPosition = worldPosition;
	output.Position = mul(worldPosition, mul(View, Projection)).xyzw;
	output.PosL = input.Position;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : SV_Target
{
	float3 viewDirection = normalize(input.WorldPosition - CameraPosition);

	//if ( viewDirection.y < 0.001 )
	//{
	//	float3 normalVector = float3(0,1,0);
	//	float3 LightDirection = float3(11, -2, -6);
	//    float3 reflectionVector = -reflect(LightDirection, normalVector);
	//	float specular = dot(normalize(reflectionVector), viewDirection);
	//	specular = pow(abs(specular), 256);  

	//	viewDirection.y = abs(viewDirection.y);
	//	float4 dullColor = float4(0.1, 0.1, 0.3, 1.0f);
	//	return lerp(Texture.Sample(CubeMapSampler, viewDirection), dullColor, 0.4) + specular;
	//}

	input.PosL.y = abs(input.PosL.y);
	return Texture.Sample(TextureSampler, input.PosL);
}

float4 PixelShaderFunctionClipPlane(VertexShaderOutput input) : SV_Target
{
	clip(dot(float4(input.WorldPosition, 1), ClipPlane));
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
		SetPixelShader(CompileShader(ps_4_0, PixelShaderFunction()));

    }
}
