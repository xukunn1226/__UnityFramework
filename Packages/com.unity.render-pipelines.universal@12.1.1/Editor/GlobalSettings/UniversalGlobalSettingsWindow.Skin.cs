using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Scripting.APIUpdating;
using UnityEditorInternal;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System.Linq;

namespace UnityEditor.Rendering.Universal
{
    internal partial class UniversalGlobalSettingsPanelIMGUI
    {
        internal class Styles
        {
            public const int labelWidth = 260;

            public static readonly GUIContent lightLayersLabel = EditorGUIUtility.TrTextContent("Light Layer Names (3D)", "If the Light Layers feature is enabled in the URP Asset, Unity allocates memory for processing Light Layers. In the Deferred Rendering Path, this allocation includes an extra render target in GPU memory, which reduces performance.");
            public static readonly GUIContent lightLayerName0 = EditorGUIUtility.TrTextContent("Light Layer 0", "The display name for Light Layer 0.");
            public static readonly GUIContent lightLayerName1 = EditorGUIUtility.TrTextContent("Light Layer 1", "The display name for Light Layer 1.");
            public static readonly GUIContent lightLayerName2 = EditorGUIUtility.TrTextContent("Light Layer 2", "The display name for Light Layer 2.");
            public static readonly GUIContent lightLayerName3 = EditorGUIUtility.TrTextContent("Light Layer 3", "The display name for Light Layer 3.");
            public static readonly GUIContent lightLayerName4 = EditorGUIUtility.TrTextContent("Light Layer 4", "The display name for Light Layer 4.");
            public static readonly GUIContent lightLayerName5 = EditorGUIUtility.TrTextContent("Light Layer 5", "The display name for Light Layer 5.");
            public static readonly GUIContent lightLayerName6 = EditorGUIUtility.TrTextContent("Light Layer 6", "The display name for Light Layer 6.");
            public static readonly GUIContent lightLayerName7 = EditorGUIUtility.TrTextContent("Light Layer 7", "The display name for Light Layer 7.");
            public static readonly GUIContent userLayerName8 = EditorGUIUtility.TrTextContent("User Layer 8", "The display name for User Layer 8.");
            public static readonly GUIContent userLayerName9 = EditorGUIUtility.TrTextContent("User Layer 9", "The display name for User Layer 9.");
            public static readonly GUIContent userLayerName10 = EditorGUIUtility.TrTextContent("User Layer 10", "The display name for User Layer 10.");
            public static readonly GUIContent userLayerName11 = EditorGUIUtility.TrTextContent("User Layer 11", "The display name for User Layer 11.");
            public static readonly GUIContent userLayerName12 = EditorGUIUtility.TrTextContent("User Layer 12", "The display name for User Layer 12.");
            public static readonly GUIContent userLayerName13 = EditorGUIUtility.TrTextContent("User Layer 13", "The display name for User Layer 13.");
            public static readonly GUIContent userLayerName14 = EditorGUIUtility.TrTextContent("User Layer 14", "The display name for User Layer 14.");
            public static readonly GUIContent userLayerName15 = EditorGUIUtility.TrTextContent("User Layer 15", "The display name for User Layer 15.");
            public static readonly GUIContent userLayerName16 = EditorGUIUtility.TrTextContent("User Layer 16", "The display name for User Layer 16.");
            public static readonly GUIContent userLayerName17 = EditorGUIUtility.TrTextContent("User Layer 17", "The display name for User Layer 17.");
            public static readonly GUIContent userLayerName18 = EditorGUIUtility.TrTextContent("User Layer 18", "The display name for User Layer 18.");
            public static readonly GUIContent userLayerName19 = EditorGUIUtility.TrTextContent("User Layer 19", "The display name for User Layer 19.");
            public static readonly GUIContent userLayerName20 = EditorGUIUtility.TrTextContent("User Layer 20", "The display name for User Layer 20.");
            public static readonly GUIContent userLayerName21 = EditorGUIUtility.TrTextContent("User Layer 21", "The display name for User Layer 21.");
            public static readonly GUIContent userLayerName22 = EditorGUIUtility.TrTextContent("User Layer 22", "The display name for User Layer 22.");
            public static readonly GUIContent userLayerName23 = EditorGUIUtility.TrTextContent("User Layer 23", "The display name for User Layer 23.");
            public static readonly GUIContent userLayerName24 = EditorGUIUtility.TrTextContent("User Layer 24", "The display name for User Layer 24.");
            public static readonly GUIContent userLayerName25 = EditorGUIUtility.TrTextContent("User Layer 25", "The display name for User Layer 25.");
            public static readonly GUIContent userLayerName26 = EditorGUIUtility.TrTextContent("User Layer 26", "The display name for User Layer 26.");
            public static readonly GUIContent userLayerName27 = EditorGUIUtility.TrTextContent("User Layer 27", "The display name for User Layer 27.");
            public static readonly GUIContent userLayerName28 = EditorGUIUtility.TrTextContent("User Layer 28", "The display name for User Layer 28.");
            public static readonly GUIContent userLayerName29 = EditorGUIUtility.TrTextContent("User Layer 29", "The display name for User Layer 29.");
            public static readonly GUIContent userLayerName30 = EditorGUIUtility.TrTextContent("User Layer 30", "The display name for User Layer 30.");
            public static readonly GUIContent userLayerName31 = EditorGUIUtility.TrTextContent("User Layer 31", "The display name for User Layer 31.");

            public static readonly GUIContent miscSettingsLabel = EditorGUIUtility.TrTextContent("Shader Stripping", "Shader Stripping settings");
            public static readonly GUIContent stripDebugVariantsLabel = EditorGUIUtility.TrTextContent("Strip Debug Variants", "When disabled, all debug display shader variants are removed when you build for the Unity Player. This decreases build time, but prevents the use of Rendering Debugger in Player builds.");
            public static readonly GUIContent stripUnusedPostProcessingVariantsLabel = EditorGUIUtility.TrTextContent("Strip Unused Post Processing Variants", "Controls whether strips automatically post processing shader variants based on VolumeProfile components. It strips based on VolumeProfiles in project and not scenes that actually uses it.");
            public static readonly GUIContent stripUnusedVariantsLabel = EditorGUIUtility.TrTextContent("Strip Unused Variants", "Controls whether strip disabled keyword variants if the feature is enabled.");

            public static readonly string warningUrpNotActive = "Project graphics settings do not refer to a URP Asset. Check the settings: Graphics > Scriptable Render Pipeline Settings, Quality > Render Pipeline Asset.";
            public static readonly string warningGlobalSettingsMissing = "The Settings property does not contain a valid URP Global Settings asset. There might be issues in rendering. Select a valid URP Global Settings asset.";
            public static readonly string infoGlobalSettingsMissing = "Select a URP Global Settings asset.";

            public static readonly GUIContent newAssetButtonLabel = EditorGUIUtility.TrTextContent("New", "Create a URP Global Settings asset in the Assets folder.");
            public static readonly GUIContent cloneAssetButtonLabel = EditorGUIUtility.TrTextContent("Clone", "Clone a URP Global Settings asset in the Assets folder.");
            public static readonly GUIContent fixAssetButtonLabel = EditorGUIUtility.TrTextContent("Fix", "Ensure a URP Global Settings Asset is assigned.");
        }
    }
}
