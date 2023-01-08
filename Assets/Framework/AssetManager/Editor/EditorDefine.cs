using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.AssetEditorWindow
{
    /// <summary>
    /// 资源文件格式
    /// </summary>
    public enum EAssetFileExtension
    {
        prefab,
        unity,
        fbx,
        anim,
        controller,
        png,
        jpg,
        mat,
        shader,
        ttf,
        cs,
    }

    /// <summary>
    /// 资源搜索类型
    /// </summary>
    public enum EAssetSearchType
    {
        All,
        RuntimeAnimatorController,
        AnimationClip,
        AudioClip,
        AudioMixer,
        Font,
        Material,
        Mesh,
        Model,
        PhysicMaterial,
        Prefab,
        Scene,
        Script,
        Shader,
        Sprite,
        Texture,
        VideoClip,
    }
}