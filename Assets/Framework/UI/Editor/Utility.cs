using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

namespace Framework.UI.Editor
{
    static public class Utility
    {
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
}