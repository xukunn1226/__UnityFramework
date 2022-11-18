using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEditor.AssetImporters;
using UnityEditor.Presets;
using UnityEditorInternal;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace Framework.AssetManagement.AssetChecker
{

    public class MyPresetSelectorReceiver : PresetSelectorReceiver
    {
        public static MyPresetSelectorReceiver Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = CreateInstance<MyPresetSelectorReceiver>();
                }
                return mInstance;
            }
        }
        private static MyPresetSelectorReceiver mInstance;

        public Action<Preset> OnFinishSelection = null;

        public override void OnSelectionChanged(Preset selection)
        {
            if (selection != null)
            {
                Debug.Log(selection.name);
            }
        }

        public override void OnSelectionClosed(Preset selection)
        {
            if (selection != null)
            {
                OnFinishSelection?.Invoke(selection);
                Debug.Log($"Close: {selection.name}");
            }
            else
            {
                OnFinishSelection?.Invoke(selection);
            }
        }
    }

    [Serializable]
    public class PresetReference
    {
        [JsonProperty]
        [SerializeField]
        private string presetGUID;

        [JsonIgnore]
        public Preset value
        {
            set
            {
                mvalue = value;
                if(mvalue!=null)
                    presetGUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(mvalue));
                if (value == null)
                    presetGUID = null;
            }
            get
            {
                if (mvalue == null && !string.IsNullOrEmpty(presetGUID))
                {
                    mvalue = AssetDatabase.LoadAssetAtPath<Preset>(AssetDatabase.GUIDToAssetPath(presetGUID));
                }else if (string.IsNullOrEmpty(presetGUID))
                {
                    mvalue = null;
                }
                return mvalue;
            }
        }

        private Preset mvalue = null;
    }

    public class AssetProcessor_Preset : IAssetProcessor
    {
        [HideInInspector]
        public PresetReference AnimationClipPreset = new PresetReference();
        [HideInInspector]
        public PresetReference AudioClipPreset= new PresetReference();
        [HideInInspector]
        public PresetReference FontPreset= new PresetReference();
        [HideInInspector]
        public PresetReference MaterialPreset= new PresetReference();
        [HideInInspector]
        public PresetReference PhysicMaterialPreset= new PresetReference();
        //public Preset SpritePreset;
        [HideInInspector]
        public PresetReference VideoClipPreset= new PresetReference();
        [HideInInspector]
        public PresetReference TexturePreset= new PresetReference();
        [HideInInspector]
        public PresetReference ModelPreset= new PresetReference();
        //public Preset MeshPreset;

        [OnInspectorGUI]
        void OnInspectorGUI()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("TexturePreset");
            GUI.enabled = false;
            EditorGUILayout.ObjectField(TexturePreset.value, typeof(Preset), false);
            GUI.enabled = true;
            SelectPreset<TextureImporter>(preset => TexturePreset.value = preset);
            GUILayout.EndHorizontal();
            
            //todo: 现在还没有video clip
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("VideoClipPreset");
            GUI.enabled = false;
            EditorGUILayout.ObjectField(VideoClipPreset.value, typeof(Preset), false);
            GUI.enabled = true;
            //SelectPreset<VideoClip>(preset => VideoClipPreset = preset);
            SelectPreset<VideoClipImporter>(preset => VideoClipPreset.value = preset);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("FontPreset");
            GUI.enabled = false;
            EditorGUILayout.ObjectField(FontPreset.value, typeof(Preset), false);
            GUI.enabled = true;
            SelectPreset<TrueTypeFontImporter>(preset => FontPreset.value = preset);
            GUILayout.EndHorizontal();
            //todo:
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("ModelPreset");
            GUI.enabled = false;
            EditorGUILayout.ObjectField(ModelPreset.value, typeof(Preset), false);
            GUI.enabled = true;
            SelectPreset<ModelImporter>(preset => ModelPreset.value = preset);
            GUILayout.EndHorizontal();
            // no need importer
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("MaterialPreset");
            GUI.enabled = false;
            EditorGUILayout.ObjectField(MaterialPreset.value, typeof(Preset), false);
            GUI.enabled = true;
            SelectPreset<Material>(preset => MaterialPreset.value = preset);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("PhysicMaterialPreset");
            GUI.enabled = false;
            EditorGUILayout.ObjectField(PhysicMaterialPreset.value, typeof(Preset), false);
            GUI.enabled = true;
            SelectPreset<PhysicMaterial>(preset => PhysicMaterialPreset.value = preset);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("AudioClipPreset");
            GUI.enabled = false;
            EditorGUILayout.ObjectField(AudioClipPreset.value, typeof(Preset), false);
            GUI.enabled = true;
            SelectPreset<AudioImporter>(preset => AudioClipPreset.value = preset);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("AnimationClipPreset");
            GUI.enabled = false;
            EditorGUILayout.ObjectField(AnimationClipPreset.value, typeof(Preset), false);
            GUI.enabled = true;
            SelectPreset<AnimationClip>(preset => AnimationClipPreset.value = preset);
            GUILayout.EndHorizontal();
        }

        public void SelectPreset<T>(Action<Preset> onSelectOver)
        {
            //if (GUILayout.Button($"Select {typeof(T).Name} Preset"))
            if (GUILayout.Button($"Select Preset", GUILayout.Width(120)))
            {
                PresetType temp = new PresetType();
                if (typeof(T) == typeof(ModelImporter))
                {
                    var Presets = Preset.GetAllDefaultTypes();
                    temp = Presets[0];
                }
                else
                {
                    var constructors = typeof(PresetType).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
                    foreach (var constructorInfo in constructors)
                    {
                        var param = new object[1] { typeof(T) };
                        try
                        {
                            temp = (PresetType)constructorInfo.Invoke(param);
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
                MyPresetSelectorReceiver.Instance.OnFinishSelection = result =>
                {
                    onSelectOver?.Invoke(result);
                };
                PresetSelector.ShowSelector(temp, null, true, MyPresetSelectorReceiver.Instance);
            }
        }

        public string DoProcess(string assetPath)
        {
            AssetImporter importer = AssetImporter.GetAtPath(assetPath);
            var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            string errorInfo = "";

            if (CanApplyPresetTo<AudioClip>(assetPath, AudioClipPreset.value, importer, asset, out errorInfo))
                return errorInfo;
            if (CanApplyPresetTo<Font>(assetPath, FontPreset.value, importer, asset, out errorInfo))
                return errorInfo;
            if (CanApplyPresetTo<Material>(assetPath, MaterialPreset.value, importer, asset, out errorInfo))
                return errorInfo;
            if (CanApplyPresetTo<PhysicMaterial>(assetPath, PhysicMaterialPreset.value, importer, asset, out errorInfo))
                return errorInfo;
            //if (CanApplyPresetTo<Sprite>(SpritePreset,importer, asset,out errorInfo))
            //    return errorInfo;
            if (CanApplyPresetTo<VideoClip>(assetPath, VideoClipPreset.value, importer, asset, out errorInfo))
                return errorInfo;
            if (CanApplyPresetTo<Texture>(assetPath, TexturePreset.value, importer, asset, out errorInfo))
                return errorInfo;
            //if (CanApplyPresetTo<Mesh>(MeshPreset,importer, asset,out errorInfo))
            //    return errorInfo;
            //if (CanApplyPresetTo<ModelImporter>(assetPath, ModelPreset.value, importer, asset, out errorInfo))
            //    return errorInfo;
            if (CanApplyPresetTo<AnimationClip>(assetPath, AnimationClipPreset.value, importer, asset, out errorInfo))
                return errorInfo;

            // FBX
            if (CanApplyPresetTo<GameObject>(assetPath, ModelPreset.value, importer, asset, out errorInfo))
                return errorInfo;
            
            return null;
        }

        public bool CanApplyPresetTo<T>(string assetPath,Preset preset,AssetImporter importer, Object asset, out string errorInfo)
        {
            if(
                (asset is T && preset != null) 
                //|| (importer.GetType() == typeof(ModelImporter) && preset != null)
               )
            {
                bool res = false;
                if (preset.CanBeAppliedTo(importer))
                {
                    res = preset.ApplyTo(importer);
                    AssetDatabase.ImportAsset(assetPath);
                }
                else
                {
                    res = preset.ApplyTo(asset);
                    AssetDatabase.ImportAsset(assetPath);
                }
                
                if (res)
                {
                    errorInfo = null;
                }
                else
                {
                    errorInfo = $"Failed to apply preset {preset.name} to {asset.name}";
                }
                
                return true;
            }
            errorInfo = null;
            return false;
        }
        
    }
}
