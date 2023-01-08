using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.AssetEditorWindow
{    
    public interface IFilterRule
    {
        /// <summary>
		/// 是否为收集资源
		/// </summary>
		/// <returns>如果收集该资源返回TRUE</returns>
		bool IsCollectAsset(FilterRuleData data);
    }

    public struct FilterRuleData
    {
        public string AssetPath;

        public FilterRuleData(string assetPath)
        {
            AssetPath = assetPath;
        }
    }
}