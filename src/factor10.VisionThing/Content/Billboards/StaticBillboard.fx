float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;
float4 ClipPlane;
float3 LightDirection;

//------- Texture Samplers --------
Texture2D Texture;
SamplerState TextureSampler;

// Lighting parameters.
float3 LightColor = 0.8;
float3 AmbientColor = 0.5;

// 1 means we should only accept opaque pixels.
// -1 means only accept transparent pixels.
float AlphaTestDirection = 1;
float AlphaTestThreshold = 0.95;

// Parameters describing the billboard itself.
float BillboardWidth = 10;
float BillboardHeight = 10;

struct VertexToPixel
{
    float4 Position : SV_Position;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
	float4 WorldPosition: TEXCOORD1;
	float4 PositionCopy : TEXCOORD2;
};

VertexToPixel VSStandard(
  float4 inPosition : SV_Position,
  float3 inNormal: NORMAL0,
  float2 inTexCoord : TEXCOORD0,
  float3 inDirection: TEXCOORD1
 )
{
	VertexToPixel output = (VertexToPixel)0;

	float4 center = mul(inPosition, World);
	float3 eyeVector = center - CameraPosition;

	float3 sideVector = cross(inNormal, inDirection);
	sideVector = normalize(sideVector);

	float3 finalPosition = center;
	finalPosition += (inTexCoord.x - 0.5f)*sideVector*BillboardWidth;
	finalPosition += (1.5f - inTexCoord.y*1.5f)*inNormal*BillboardHeight;

	output.WorldPosition = float4(finalPosition, 1);

	float4x4 preViewProjection = mul(View, Projection);
	output.Position = output.PositionCopy = mul(output.WorldPosition, preViewProjection);

	output.TexCoord = inTexCoord;

    // Compute lighting.
    float diffuseLight = max(-dot(inNormal, LightDirection), 0);
    
    output.Color.rgb = diffuseLight * LightColor + AmbientColor;
    output.Color.a = 1;

	return output;
}


float4 PSStandard(VertexToPixel input) : SV_Target
{
	float4 color = input.Color * Texture.Sample(TextureSampler, input.TexCoord);
    clip((color.a - AlphaTestThreshold) * AlphaTestDirection);
    return color;
}


float4 PSClipPlane(VertexToPixel input) : SV_Target
{
	clip(dot(input.WorldPosition, ClipPlane));
	float4 color = input.Color * Texture.Sample(TextureSampler, input.TexCoord);
	clip((color.a - AlphaTestThreshold) * AlphaTestDirection);
	return color;
}


float4 PSDepthMap(VertexToPixel input) : SV_Target
{
    float4 color = input.Color * Texture.Sample(TextureSampler, input.TexCoord);
    clip((color.a - AlphaTestThreshold) * AlphaTestDirection);
    float depth = clamp(input.PositionCopy.z / input.PositionCopy.w, 0, 1);
    return float4(depth, depth * depth, 0, 1);
}

technique TechStandard
{
	pass Pass0
    {
		SetGeometryShader(0);
		SetVertexShader(CompileShader(vs_4_0, VSStandard()));
		SetPixelShader(CompileShader(ps_4_0, PSStandard()));
	}
}

technique TechClipPlane
{
	pass Pass0
    {
		SetGeometryShader(0);
		SetVertexShader(CompileShader(vs_4_0, VSStandard()));
		SetPixelShader(CompileShader(ps_4_0, PSClipPlane()));
	}
}

technique TechDepthMap
{
	pass Pass0
	{
		SetGeometryShader(0);
		SetVertexShader(CompileShader(vs_4_0, VSStandard()));
		SetPixelShader(CompileShader(ps_4_0, PSDepthMap()));
	}
}