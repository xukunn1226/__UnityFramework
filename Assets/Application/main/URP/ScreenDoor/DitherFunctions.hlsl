// https://github.com/gkjohnson/unity-dithered-transparency-shader

#ifndef __DITHER_FUNCTIONS__
#define __DITHER_FUNCTIONS__
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

// pos：屏幕坐标[0, 1]
float isDithered(float2 pos, float alpha)
{
    pos *= _ScreenParams.xy;

    float DITHER_THRESHOLDS[16] =
    {
        1,  9,  3,  11,
        13, 5,  15, 7,
        4,  12, 2,  10,
        16, 8,  14, 6
    };

    return alpha - DITHER_THRESHOLDS[(int(pos.x) % 4) * 4 + int(pos.y) % 4] * 0.0588;   // 1/17 = 0.0588
}

// pos：屏幕坐标[0, 1]
float isDithered(float2 pos, float alpha, sampler2D tex, float scale)
{
    pos *= _ScreenParams.xy;

    // offset so we're centered
    pos.x -= _ScreenParams.x / 2;
    pos.y -= _ScreenParams.y / 2;
    
    // scale the texture
    pos.x /= scale;
    pos.y /= scale;

	// ensure that we clip if the alpha is zero by
	// subtracting a small value when alpha == 0, because
	// the clip function only clips when < 0
    return alpha - tex2D(tex, pos.xy).r - 0.0001 * (1 - ceil(alpha));
}

void ditherClip(float2 pos, float alpha)
{
    clip(isDithered(pos, alpha));
}

void ditherClip(float2 pos, float alpha, sampler2D tex, float scale)
{
    clip(isDithered(pos, alpha, tex, scale));
}
#endif