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

// Parameters controlling the wind effect.
float WindWaveSize = 0.00001;
float WindRandomness = 1;
float WindSpeed = 2;
float WindAmount = 0.05;
float WindTime;

// 1 means we should only accept opaque pixels.
// -1 means only accept transparent pixels.
float AlphaTestDirection = 1;
float AlphaTestThreshold = 0.95;

// Parameters describing the billboard itself.
float BillboardWidth = 10;
float BillboardHeight = 10;

float2 TextureCoordinates[4] =
{
	float2(0.0f, 1.0f),
	float2(0.0f, 0.0f),
	float2(1.0f, 1.0f),
	float2(1.0f, 0.0f)
};

struct VertexToGeo
{
	float4 Position : POSITION;
	float3 Up		: TEXCOORD0;
	float2 Random	: TEXCOORD1;
};

struct GeoToPixel
{
	float4 Position : SV_Position;
	float4 Color	: COLOR0;
	float2 TexCoord : TEXCOORD0;
	float4 WorldPosition: TEXCOORD1;
	float4 PositionCopy : TEXCOORD2;
	uint   PrimID	: SV_PrimitiveID;
};


VertexToGeo VS(
	float4 inPosition : SV_Position,
	float3 inUp : NORMAL0,
	float2 inRandom : TEXCOORD0
	)
{
	VertexToGeo vout;

	// Just pass data over to geometry shader.
	vout.Position = inPosition;
	vout.Up = inUp;
	vout.Random = inRandom;

	return vout;
}

[maxvertexcount(4)]
void GS(
	point VertexToGeo gin[1],
	uint primID : SV_PrimitiveID,
	inout TriangleStream<GeoToPixel> triStream
	)
{
	// Apply a scaling factor to make some of the billboards
	// shorter and fatter while others are taller and thinner.
	float squishFactor = 0.75 + gin[0].Random.x / 2;

	float halfWidth = 0.5 * BillboardWidth * squishFactor;
	float height = BillboardHeight / squishFactor;

	// Flip half of the billboards from left to right. This gives visual variety
	// even though we are actually just repeating the same texture over and over.
	halfWidth *= sign(gin[0].Random.x);

	float3 center = (float3)mul(gin[0].Position, World);
		float3 eyeVector = center - CameraPosition;

		float3 sideVector = cross(eyeVector, gin[0].Up);
		sideVector = normalize(sideVector);

	// Work out how this vertex should be affected by the wind effect.
	float3 windDirection = sideVector + float3(gin[0].Random.x, frac(gin[0].Position.x), frac(gin[0].Position.y));
		float waveOffset = dot(center, windDirection) * WindWaveSize;

	waveOffset += gin[0].Random.x * WindRandomness;
	float wind = sin(WindTime * WindSpeed + waveOffset) * WindAmount;

	float4 v[4];
	v[0] = float4(center - halfWidth*sideVector, 1.0f);
	v[1] = float4(center - halfWidth*sideVector + height*gin[0].Up + windDirection*wind, 1.0f);
	v[2] = float4(center + halfWidth*sideVector, 1.0f);
	v[3] = float4(center + halfWidth*sideVector + height*gin[0].Up + windDirection*wind, 1.0f);

	float4x4 preViewProjection = mul(View, Projection);

		// Compute lighting.
		float diffuseLight = max(-dot(eyeVector, LightDirection), 0);
	float4 color = float4(diffuseLight * LightColor + AmbientColor, 1);

		GeoToPixel gout;
	[unroll]
	for (int i = 0; i < 4; ++i)
	{
		gout.Position = gout.PositionCopy = mul(v[i], preViewProjection);
		gout.WorldPosition = v[i];
		gout.Color = color;
		gout.TexCoord = TextureCoordinates[i];
		gout.PrimID = primID;

		triStream.Append(gout);
	}
}


float4 PSStandard(GeoToPixel input) : SV_Target
{
	float4 color = input.Color * Texture.Sample(TextureSampler, input.TexCoord);
	clip((color.a - AlphaTestThreshold) * AlphaTestDirection);
	return color;
}


float4 PSClipPlane(GeoToPixel input) : SV_Target
{
	clip(dot(input.WorldPosition, ClipPlane));
	float4 color = input.Color * Texture.Sample(TextureSampler, input.TexCoord);
		clip((color.a - AlphaTestThreshold) * AlphaTestDirection);
	return color;
}

float4 PSDepthMap(GeoToPixel input) : SV_Target
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
		SetVertexShader(CompileShader(vs_4_0, VS()));
		SetGeometryShader(CompileShader(gs_4_0, GS()));
		SetPixelShader(CompileShader(ps_4_0, PSStandard()));
	}
}

technique TechClipPlane
{
	pass Pass0
	{
		SetVertexShader(CompileShader(vs_5_0, VS()));
		SetGeometryShader(CompileShader(gs_5_0, GS()));
		SetPixelShader(CompileShader(ps_5_0, PSClipPlane()));
	}
}

technique TechDepthMap
{
	pass Pass0
	{
		SetVertexShader(CompileShader(vs_5_0, VS()));
		SetGeometryShader(CompileShader(gs_5_0, GS()));
		SetPixelShader(CompileShader(ps_5_0, PSDepthMap()));
	}
}