using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using UnityEngine.U2D;
using UnityEditor.U2D;
using UnityEngine.UI;
using TMPro;

namespace Framework.UI.Editor
{
    static public class UIEditorHelper
    {
        [InitializeOnLoadMethod]
        static void AutoPackAllAtlases()
        {
            EditorApplication.playModeStateChanged += DoPack;
        }

        private static void DoPack(PlayModeStateChange state)
        {
            // 为了编辑器下涉及到图集的业务逻辑的正确性，每次进入Play Mode时重新打图集
            if(state == PlayModeStateChange.EnteredPlayMode)
                SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget);
        }

        /// <summary>
        /// 自动取消RatcastTarget
        /// </summary>
        [MenuItem("GameObject/UI/zImage")]
        static void CreatImage()
        {
            if (Selection.activeTransform && Selection.activeTransform.GetComponentInParent<Canvas>())
            {
                GameObject go = new GameObject("Image", typeof(Image));
                go.GetComponent<Image>().raycastTarget = false;
                go.transform.SetParent(Selection.activeTransform, false);
            }
        }
        
        [MenuItem("GameObject/UI/zText")]
        static void CreatText()
        {
            if (Selection.activeTransform && Selection.activeTransform.GetComponentInParent<Canvas>())
            {
                GameObject go = new GameObject("Text", typeof(Text));
                go.GetComponent<Text>().raycastTarget = false;
                go.transform.SetParent(Selection.activeTransform, false);
            }
        }

        [MenuItem("GameObject/UI/zText - TextMeshPro")]
        static void CreatTextMeshPro()
        {
            if (Selection.activeTransform && Selection.activeTransform.GetComponentInParent<Canvas>())
            {
                GameObject go = new GameObject("Text(TMP)", typeof(TextMeshProUGUI));
                go.GetComponent<TextMeshProUGUI>().raycastTarget = false;
                go.transform.SetParent(Selection.activeTransform, false);
            }
        }

        [MenuItem("GameObject/UI/zRaw Image")]
        static void CreatRawImage()
        {
            if (Selection.activeTransform && Selection.activeTransform.GetComponentInParent<Canvas>())
            {
                GameObject go = new GameObject("RawImage", typeof(RawImage));
                go.GetComponent<RawImage>().raycastTarget = false;
                go.transform.SetParent(Selection.activeTransform, false);
            }
        }

        [MenuItem("GameObject/UI/List Details")]
        static private void ListDetails()
        {
            if(Selection.activeGameObject == null)
                return;

            RectTransform rectTransform = Selection.activeGameObject.GetComponent<RectTransform>();
            if(rectTransform == null)
                return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<b>rect: </b> {rectTransform.rect}");
            sb.AppendLine($"<b>anchoredPosition: </b> {rectTransform.anchoredPosition}");
            sb.AppendLine($"<b>pivot: </b> {rectTransform.pivot}");
            sb.AppendLine($"<b>sizeDelta: </b> {rectTransform.sizeDelta}");
            sb.AppendLine($"<b>anchorMin: </b> {rectTransform.anchorMin}");
            sb.AppendLine($"<b>anchorMax: </b> {rectTransform.anchorMax}");            
            sb.AppendLine($"<b>offsetMin: </b> {rectTransform.offsetMin}");
            sb.AppendLine($"<b>offsetMax: </b> {rectTransform.offsetMax}");

            Vector3[] fourCornersArray = new Vector3[4];
            rectTransform.GetLocalCorners(fourCornersArray);
            sb.AppendLine($"<b>LocalCorners: </b> {fourCornersArray[0]} {fourCornersArray[1]} {fourCornersArray[2]} {fourCornersArray[3]}");

            rectTransform.GetWorldCorners(fourCornersArray);
            sb.AppendLine($"<b>WorldCorners: </b> {fourCornersArray[0]} {fourCornersArray[1]} {fourCornersArray[2]} {fourCornersArray[3]}");

            Debug.Log($"List details: {rectTransform.name}");
            Debug.Log(sb.ToString());
        }
    }

    [InitializeOnLoad]
    public class Factory
    {
        static Factory()
        {
            ObjectFactory.componentWasAdded += ComponentWasAdded;
        }

        private static void ComponentWasAdded(Component component)
        {
            Image image = component as Image;
            if (image != null)
                image.raycastTarget = false;
            RawImage rawImage = component as RawImage;
            if (rawImage != null)
                rawImage.raycastTarget = false;
            Text text = component as Text;
            if (text != null)
                text.raycastTarget = false;
        }
    }
}