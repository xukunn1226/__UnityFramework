#include "InstancingCore.hlsl"

half4 skinningShadow(inout Attributes v)
{
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
	int preFrame = curFrame;
	int nextFrame = curFrame + 1.0f;
	half4x4 localToWorldMatrixPre = loadMatFromTexture(preFrame, bone.x);
	half4x4 localToWorldMatrixNext = loadMatFromTexture(nextFrame, bone.x);
	half4 localPosPre = mul(v.positionOS, localToWorldMatrixPre);
	half4 localPosNext = mul(v.positionOS, localToWorldMatrixNext);
	half4 localPos = lerp(localPosPre, localPosNext, curFrame - preFrame);
	half4x4 localToWorldMatrixPreAni = loadMatFromTexture(preAniFrame, bone.x);
	half4 localPosPreAni = mul(v.positionOS, localToWorldMatrixPreAni);
	localPos = lerp(localPos, localPosPreAni, (1.0f - progress) * (preAniFrame > 0.0f));
	return localPos;
}