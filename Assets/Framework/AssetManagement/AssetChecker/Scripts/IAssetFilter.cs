using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.IO;

namespace Framework.AssetManagement.AssetChecker
{
    public interface IAssetFilter
    {
        List<string> Run(List<string> input, string pattern);
    }

    /// <summary>
    /// ·��������
    /// </summary>
    public class AssetChecker_PathFilter : IAssetFilter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="directories">��Ҫɸѡ�ĸ�Ŀ¼</param>
        /// <param name="pattern">������ʽ</param>
        /// <returns>���з��������·����ȥ���˹���Ŀ¼ǰ׺</returns>
        public List<string> Run(List<string> directories, string pattern)
        {
            if (directories == null || directories.Count != 1)
                throw new System.ArgumentNullException(@"PathFilter: unsupport EMPTY directory or input directories count > 1");

            DirectoryInfo di = new DirectoryInfo(directories[0]);
            DirectoryInfo[] dis = di.GetDirectories("*", SearchOption.AllDirectories);

            List<string> result = new List<string>();
            try
            {
                Regex regex = new Regex(pattern);

                foreach (var dir in dis)
                {
                    string path = dir.FullName.Replace(@"\", @"/");
                    if (regex.IsMatch(path))
                        result.Add(AssetCheckerUtility.TrimProjectFolder(path));
                }
            }
            catch(System.Exception e)
            {
                Debug.LogException(e);
            }
            return result;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class AssetChecker_FilenameFilter : IAssetFilter
    {
        public List<string> Run(List<string> directories, string pattern)
        {
            return null;
        }
    }

    static public class AssetChecker_Test
    {
        [UnityEditor.MenuItem("Tools/AssetChecker_Test/Foo")]
        static private void Foo()
        {
            IAssetFilter filter = new AssetChecker_PathFilter();
            List<string> ret = filter.Run(new List<string> { "Assets/Resources" }, @"s/Wind");

            int count = 0;
            foreach(string s in ret)
            {
                Debug.Log(string.Format($"{count++}: {s}"));
            }
        }
    }
}