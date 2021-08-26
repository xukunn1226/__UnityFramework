#ifndef ANIMATION_INSTANCING_BASE
#define ANIMATION_INSTANCING_BASE

//#pragma target 3.0

sampler2D _boneTexture;
int _boneTextureBlockWidth;
int _boneTextureBlockHeight;
int _boneTextureWidth;
int _boneTextureHeight;

#if (SHADER_TARGET < 30 || SHADER_API_GLES)
uniform float frameIndex;
uniform float preFrameIndex;
uniform float transitionProgress;
#else
UNITY_INSTANCING_BUFFER_START(Props)
	UNITY_DEFINE_INSTANCED_PROP(float, preFrameIndex)
#define preFrameIndex_arr Props
	UNITY_DEFINE_INSTANCED_PROP(float, frameIndex)
#define frameIndex_arr Props
	UNITY_DEFINE_INSTANCED_PROP(float, transitionProgress)
#define transitionProgress_arr Props
UNITY_INSTANCING_BUFFER_END(Props)
#endif

half4x4 loadMatFromTexture(uint frameIndex, uint boneIndex)
{
	uint blockCount = 1.0 * _boneTextureWidth / _boneTextureBlockWidth;
	int2 uv;
	uv.y = frameIndex / blockCount * _boneTextureBlockHeight;
	uv.x = _boneTextureBlockWidth * (frameIndex % blockCount);

	int matCount = _boneTextureBlockWidth * 0.25;
	uv.x = uv.x;
	uv.y = uv.y + boneIndex;

	float2 uvFrame;
	uvFrame.x = ((float)uv.x + 0.5) / (float) (_boneTextureWidth);
    uvFrame.y = ((float)uv.y + 0.5) / (float) (_boneTextureHeight);
	half4 uvf = half4(uvFrame, 0, 0);

	half offset = 1.0f / (half) _boneTextureWidth;
	half4 c1 = tex2Dlod(_boneTexture, uvf);
	uvf.x = uvf.x + offset;
	half4 c2 = tex2Dlod(_boneTexture, uvf);
	uvf.x = uvf.x + offset;
	half4 c3 = tex2Dlod(_boneTexture, uvf);
	uvf.x = uvf.x + offset;
	//half4 c4 = tex2Dlod(_boneTexture, uvf);
	half4 c4 = half4(0, 0, 0, 1);
	//float4x4 m = float4x4(c1, c2, c3, c4);
	half4x4 m;
	m._11_21_31_41 = c1;
	m._12_22_32_42 = c2;
	m._13_23_33_43 = c3;
	m._14_24_34_44 = c4;
	return m;
}
#endif