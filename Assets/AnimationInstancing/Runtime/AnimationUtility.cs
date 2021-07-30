using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationInstancingModule.Runtime
{
    public class AnimationUtility
    {
        // Merge all bones to a single array and merge all bind pose
        public static Transform[] MergeBone(SkinnedMeshRenderer[] meshRender, ref List<Matrix4x4> bindPose)
        {
            UnityEngine.Profiling.Profiler.BeginSample("MergeBone()");
            bindPose.Clear();
            List<Transform> listTransform = new List<Transform>(150);
            for (int i = 0; i != meshRender.Length; ++i)
            {
                Transform[] bones = meshRender[i].bones;
                Matrix4x4[] checkBindPose = meshRender[i].sharedMesh.bindposes;
                for (int j = 0; j != bones.Length; ++j)
                {
#if UNITY_EDITOR
                    Debug.Assert(checkBindPose[j].determinant != 0, "The bind pose can't be 0 matrix.");
#endif
                    // the bind pose is correct base on the skinnedMeshRenderer, so we need to replace it
                    int index = listTransform.FindIndex(q => q == bones[j]);
                    if (index < 0)
                    {
                        listTransform.Add(bones[j]);
                        if (bindPose != null)
                        {
                            bindPose.Add(checkBindPose[j]);
                        }
                    }
                    else
                    {
                        bindPose[index] = checkBindPose[j];
                    }
                }
            }
            UnityEngine.Profiling.Profiler.EndSample();

            return listTransform.ToArray();
        }
    }
}