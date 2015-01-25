float4x4 View;
float4x4 Projection;
float4x4 World;
float3 CameraPosition;
float4 ClipPlane;
float3 SunlightDirection = float3(0.7, 0.7, 0.7);
SamplerState TextureSampler;

float4 TexOffsetAndScale = float4(0, 0, 1, 1);

bool DoShadowMapping = true;
float ShadowMult = 0.3f;
float ShadowBias = 0.001f;
float4x4 ShadowViewProjection;
texture2D ShadowMap;

float Ambient = 0.2;

Texture2D Texture;
Texture2D HeightsMap;

struct MTVertexToPixel
{
	float4 Position         : SV_Position;
	float3 WorldPosition    : TEXCOORD0;
	float2 TextureCoords    : TEXCOORD1;
	float4 PositionCopy     : TEXCOORD2;
	float4 ShadowScreenPos	: TEXCOORD3;
	float3 Normal			: TEXCOORD4;
};


MTVertexToPixel MultiTexturedVS(float4 inPos : SV_Position, float2 inTexCoords : TEXCOORD0)
{
	MTVertexToPixel output;

	output.TextureCoords = float2(
		TexOffsetAndScale.x + inTexCoords.x * TexOffsetAndScale.z,
		TexOffsetAndScale.y + inTexCoords.y * TexOffsetAndScale.w);

	inPos.y += HeightsMap.SampleLevel(TextureSampler, output.TextureCoords, 0).r;

	float4 worldPosition = mul(inPos, World);
	float4x4 viewProjection = mul(View, Projection);

	output.Position = output.PositionCopy = mul(worldPosition, viewProjection);
	output.WorldPosition = worldPosition;

	output.ShadowScreenPos = mul(worldPosition, ShadowViewProjection);
	//output.Normal = 2*(NormalsMap.SampleLevel(TextureSampler, output.TextureCoords, 0).xyz - float3(0.5, 0.5, 0.5));
	return output;
}


float2 sampleShadowMap(float2 UV)
{
	if (UV.x < 0 || UV.x > 1 || UV.y < 0 || UV.y > 1)
		return float2(1, 1);
	return ShadowMap.Sample(TextureSampler, UV).rg;
}

float4 MultiTexturedPS(MTVertexToPixel input) : SV_Target
{
	float lightingFactor = saturate(Ambient + dot(input.Normal, SunlightDirection));

	float4 color = Texture.Sample(TextureSampler, input.TextureCoords);

	if (DoShadowMapping)
	{
		float realDepth = input.ShadowScreenPos.z / input.ShadowScreenPos.w - ShadowBias;
		if (realDepth < 1)
		{
			float2 screenPos = input.ShadowScreenPos.xy / input.ShadowScreenPos.w;
			float2 shadowTexCoord = 0.5f * (float2(screenPos.x, -screenPos.y) + 1);
			float2 moments = sampleShadowMap(shadowTexCoord);
			float lit_factor = (realDepth <= moments.x);
			float E_x2 = moments.y;
			float Ex_2 = moments.x * moments.x;
			float variance = min(max(E_x2 - Ex_2, 0.0) + 1.0f / 10000.0f, 1.0);
			float m_d = (moments.x - realDepth);
			float p = variance / (variance + m_d * m_d);
			lightingFactor *= clamp(max(lit_factor, p), ShadowMult, 1.0f);
		}
	}

	color *= lightingFactor;
	color.w = 1;
	return color;
}

float4 MultiTexturedPSClipPlane(MTVertexToPixel input) : SV_Target
{
	clip(dot(float4(input.WorldPosition, 1), ClipPlane));
	return MultiTexturedPS(input);
}

float4 MultiTexturedPSDepth(MTVertexToPixel input) : SV_Target
{
	float depth = clamp(input.PositionCopy.z / input.ShadowScreenPos.w, 0, 1);
	return float4(depth, depth * depth, 0, 1);
}

technique TechStandard
{
	pass Pass0
	{
		SetGeometryShader(0);
		SetVertexShader(CompileShader(vs_4_0, MultiTexturedVS()));
		SetPixelShader(CompileShader(ps_4_0, MultiTexturedPS()));
	}
}

technique TechClipPlane
{
	pass Pass0
	{
		SetGeometryShader(0);
		SetVertexShader(CompileShader(vs_4_0, MultiTexturedVS()));
		SetPixelShader(CompileShader(ps_4_0, MultiTexturedPSClipPlane()));
	}
}

technique TechDepthMap
{
	pass Pass0
	{
		SetGeometryShader(0);
		SetVertexShader(CompileShader(vs_4_0, MultiTexturedVS()));
		SetPixelShader(CompileShader(ps_4_0, MultiTexturedPSDepth()));
	}
}

