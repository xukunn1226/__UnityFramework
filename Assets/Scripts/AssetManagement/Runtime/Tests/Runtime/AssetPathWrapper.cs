using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

/// <summary>
/// 展示如何把assetbundle name，asset name封装为assetPath
/// </summary>
public class AssetPathWrapper : MonoBehaviour
{
    public string assetBundleName;
    public string assetName;
}


#if UNITY_EDITOR
[CustomEditor(typeof(AssetPathWrapper))]
public class AssetPathWrapperInspector : Editor
{
    SerializedProperty assetBundleNameProp;
    SerializedProperty assetNameProp;

    string assetPath;
    UnityEngine.Object assetObj;

    private void OnEnable()
    {
        InitAssetPathProp("assetBundleName", "assetName");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawAssetPathProp();

        serializedObject.ApplyModifiedProperties();
    }

    void InitAssetPathProp(string assetBundleName_Variant, string assetName_Variant)
    {
        assetBundleNameProp = serializedObject.FindProperty(assetBundleName_Variant);
        assetNameProp = serializedObject.FindProperty(assetName_Variant);

        assetPath = Path.Combine(assetBundleNameProp.stringValue.Replace(".ab", ""), assetNameProp.stringValue);
        assetObj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
    }

    void DrawAssetPathProp()
    {
        EditorGUILayout.Space();

        assetObj = EditorGUILayout.ObjectField("Object", assetObj, typeof(UnityEngine.Object), false);

        if (assetObj != null)
        {
            assetPath = AssetDatabase.GetAssetPath(assetObj);

            assetNameProp.stringValue = assetPath.Substring(assetPath.LastIndexOf("/") + 1).ToLower();
            assetBundleNameProp.stringValue = (assetPath.Substring(0, assetPath.LastIndexOf("/")).ToLower() + ".ab");

            assetObj = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        }

        EditorGUILayout.Space();

        GUIStyle style = new GUIStyle();
        style.fontSize = 15;

        EditorGUILayout.BeginHorizontal();
        if (assetObj == null)
        {
            GUILayout.Label("序列化数据如下：", style);

            style.normal.textColor = Color.red;
            GUILayout.Label("资源加载失败，请检查以下路径", style);
        }
        else
        {
            EditorGUILayout.LabelField("序列化数据如下：", style);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("AssetBundleName");
        EditorGUILayout.LabelField(assetBundleNameProp.stringValue);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("AssetName");
        EditorGUILayout.LabelField(assetNameProp.stringValue);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("AssetPath");
        EditorGUILayout.LabelField(assetPath);
        EditorGUILayout.EndHorizontal();
    }
}
#endif