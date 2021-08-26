#include "InstancingCore.hlsl"

half4 skinning(inout Attributes v)
{
	float4 w = v.color;
	half4 bone = half4(v.texcoord2.x, v.texcoord2.y, v.texcoord2.z, v.texcoord2.w);
#if (SHADER_TARGET < 30 || SHADER_API_GLES)
	float curFrame = frameIndex;
	float preAniFrame = preFrameIndex;
	float progress = transitionProgress;
#else
	float curFrame = UNITY_ACCESS_INSTANCED_PROP(frameIndex_arr, frameIndex);
	float preAniFrame = UNITY_ACCESS_INSTANCED_PROP(preFrameIndex_arr, preFrameIndex);
	float progress = UNITY_ACCESS_INSTANCED_PROP(transitionProgress_arr, transitionProgress);
#endif

	//float curFrame = UNITY_ACCESS_INSTANCED_PROP(frameIndex);
	int preFrame = curFrame;
	int nextFrame = curFrame + 1.0f;
	half4x4 localToWorldMatrixPre = loadMatFromTexture(preFrame, bone.x) * w.x;
	localToWorldMatrixPre += loadMatFromTexture(preFrame, bone.y) * max(0, w.y);
	localToWorldMatrixPre += loadMatFromTexture(preFrame, bone.z) * max(0, w.z);
	localToWorldMatrixPre += loadMatFromTexture(preFrame, bone.w) * max(0, w.w);

	half4x4 localToWorldMatrixNext = loadMatFromTexture(nextFrame, bone.x) * w.x;
	localToWorldMatrixNext += loadMatFromTexture(nextFrame, bone.y) * max(0, w.y);
	localToWorldMatrixNext += loadMatFromTexture(nextFrame, bone.z) * max(0, w.z);
	localToWorldMatrixNext += loadMatFromTexture(nextFrame, bone.w) * max(0, w.w);

	half4 localPosPre = mul(v.positionOS, localToWorldMatrixPre);
	half4 localPosNext = mul(v.positionOS, localToWorldMatrixNext);
	half4 localPos = lerp(localPosPre, localPosNext, curFrame - preFrame);

	half3 localNormPre = mul(v.normalOS.xyz, (float3x3)localToWorldMatrixPre);
	half3 localNormNext = mul(v.normalOS.xyz, (float3x3)localToWorldMatrixNext);
	v.normalOS = normalize(lerp(localNormPre, localNormNext, curFrame - preFrame));
	half3 localTanPre = mul(v.tangentOS.xyz, (float3x3)localToWorldMatrixPre);
	half3 localTanNext = mul(v.tangentOS.xyz, (float3x3)localToWorldMatrixNext);
	v.tangentOS.xyz = normalize(lerp(localTanPre, localTanNext, curFrame - preFrame));

	half4x4 localToWorldMatrixPreAni = loadMatFromTexture(preAniFrame, bone.x);
	half4 localPosPreAni = mul(v.positionOS, localToWorldMatrixPreAni);
	localPos = lerp(localPos, localPosPreAni, (1.0f - progress) * (preAniFrame > 0.0f));
	return localPos;
}