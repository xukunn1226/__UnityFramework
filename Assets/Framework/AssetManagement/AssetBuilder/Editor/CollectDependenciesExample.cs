using UnityEngine;
using UnityEditor;

public class CollectDependenciesExample : EditorWindow
{
    static UnityEngine.Object obj = null;


    [MenuItem("Example/Collect Dependencies")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        CollectDependenciesExample window = (CollectDependenciesExample)EditorWindow.GetWindow(typeof(CollectDependenciesExample));
        window.Show();
    }

    void OnGUI()
    {
        obj = EditorGUI.ObjectField(new Rect(3, 3, position.width - 6, 20), "Find Dependency", obj, typeof(UnityEngine.Object)) as UnityEngine.Object;

        if (obj)
        {
            Object[] roots = new Object[] { obj };

            if (GUI.Button(new Rect(3, 25, position.width - 6, 20), "Collect Dependencies"))
            {
                Selection.objects = EditorUtility.CollectDependencies(roots);
                foreach(var o in Selection.objects)
                {
                    Debug.Log($"{AssetDatabase.GetAssetPath(o.GetInstanceID())}");
                }
            }

            if (GUI.Button(new Rect(3, 55, position.width - 6, 20), "Get Dependencies"))
            {
                string[] paths = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(obj.GetInstanceID()), true);
                foreach(var path in paths)
                {
                    Debug.Log($"{path}");
                }
            }
        }
        else
            EditorGUI.LabelField(new Rect(3, 25, position.width - 6, 20), "Missing:", "Select an object first");
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }
}