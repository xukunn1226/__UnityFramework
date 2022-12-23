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

            if (GUI.Button(new Rect(3, 25, position.width - 6, 20), "Check Dependencies"))
            {
                Selection.objects = EditorUtility.CollectDependencies(roots);
                foreach(var o in Selection.objects)
                {
                    Debug.Log($"{AssetDatabase.GetAssetPath(o.GetInstanceID())}");
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