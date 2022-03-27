using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable,VolumeComponentMenu("Post-processing/MyBlur")]
public class MyBlur : VolumeComponent,IPostProcessComponent
{
    public MinFloatParameter blurAmountX = new MinFloatParameter(0.0f, 0.0f);
    public MinFloatParameter blurAmountY = new MinFloatParameter(0.0f, 0.0f);
    public bool IsActive() => blurAmountX.value > 0f && blurAmountY.value >0f;
    public bool IsTileCompatible() => false;
}
