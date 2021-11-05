using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Framework.AssetManagement.Runtime;
using Framework.Core;
using Mono.Data.Sqlite;

namespace Application.Runtime
{
    public partial class ConfigManager : Singleton<ConfigManager>
    {
        private SqlData         m_Sql;
        static private string   m_DataPath;
        static public string    dataPath
        {
            get
            {
                if(string.IsNullOrEmpty(m_DataPath))
                {
                    switch(Launcher.GetLauncherMode())
                    {
                        case LoaderType.FromEditor:
                            m_DataPath = ConfigBuilderSetting.DatabaseFilePath;
                            break;
                        case LoaderType.FromStreamingAssets:
                        case LoaderType.FromPersistent:
                            m_DataPath = string.Format($"{UnityEngine.Application.persistentDataPath}/{Utility.GetPlatformName()}/{System.IO.Path.GetFileName(ConfigBuilderSetting.DatabaseFilePath)}");
                            break;
                    }
                }
                return m_DataPath;
            }
        }

        protected override void InternalInit()
        {
            m_Sql = new SqlData(dataPath);
        }

        protected override void OnDestroy()
        {
            m_Sql?.Close();
        }

        // 提取配置数据库从streamingAssets至persistentDataPath
        static public IEnumerator ExtractDatabase()
        {
            string srcPath = string.Format($"{UnityEngine.Application.streamingAssetsPath}/{Utility.GetPlatformName()}/{System.IO.Path.GetFileName(ConfigBuilderSetting.DatabaseFilePath)}");
            string dstPath = string.Format($"{UnityEngine.Application.persistentDataPath}/{Utility.GetPlatformName()}/{System.IO.Path.GetFileName(ConfigBuilderSetting.DatabaseFilePath)}");

            DownloadTask task = new DownloadTask(new byte[1024]);
            DownloadTaskInfo info           = new DownloadTaskInfo();
                    info.srcUri             = new System.Uri(srcPath);
                    info.dstURL             = dstPath;
                    info.verifiedHash       = null;
                    info.retryCount         = 3;
                    // info.onProgress         = OnProgress;
                    info.onCompleted        = OnCompleted;
                    info.onRequestError     = OnRequestError;
                    info.onDownloadError    = OnDownloadError;
            return task.Run(info);
        }

        // private void OnProgress(DownloadTaskInfo taskInfo, ulong downedLength, ulong totalLength, float downloadSpeed)
        // {
        //     Debug.Log($"OnProgress: {System.IO.Path.GetFileName(taskInfo.dstURL)}     {downedLength}/{totalLength}    downloadSpeed({downloadSpeed})");
        // }

        static private void OnCompleted(DownloadTaskInfo taskInfo, bool success, int tryCount)
        {
            Debug.Log($"配置数据库下载：{taskInfo.dstURL} {(success ? "成功" : "失败")}");
        }

        static private void OnRequestError(DownloadTaskInfo taskInfo, string error)
        {
            Debug.LogError(string.Format($"配置数据库  OnRequestError: {error} : {taskInfo.srcUri}"));
        }

        static private void OnDownloadError(DownloadTaskInfo taskInfo, string error)
        {
            Debug.LogError(string.Format($"配置数据库  OnDownloadError: {error} : {taskInfo.srcUri}"));
        }
        
        private void Parse(ref List<string> ret, string content)
        {
            string[] list = content.Split(';');
            for(int i = 0; i < list.Length; ++i)
            {
                if(string.IsNullOrEmpty(list[i])) continue;
                
                ret.Add(list[i]);
            }
        }

        private void Parse(ref List<int> ret, string content)
        {
            string[] list = content.Split(';');
            for(int i = 0; i < list.Length; ++i)
            {
                if(string.IsNullOrEmpty(list[i])) continue;
                
                ret.Add(int.Parse(list[i]));
            }
        }

        private void Parse(ref List<float> ret, string content)
        {
            string[] list = content.Split(';');
            for(int i = 0; i < list.Length; ++i)
            {
                if(string.IsNullOrEmpty(list[i])) continue;
                
                ret.Add(float.Parse(list[i]));
            }
        }

        private void Parse(ref Dictionary<int, int> ret, string content)
        {
            string[] pairs = content.Split(';');
            for(int i = 0; i < pairs.Length; ++i)
            {
                string[] items = pairs[i].Split(':');
                Debug.Assert(items.Length == 2);
                ret.Add(int.Parse(items[0]), int.Parse(items[1]));
            }
        }

        private void Parse(ref Dictionary<int, string> ret, string content)
        {
            string[] pairs = content.Split(';');
            for(int i = 0; i < pairs.Length; ++i)
            {
                if(string.IsNullOrEmpty(pairs[i])) continue;

                string[] items = pairs[i].Split(':');
                Debug.Assert(items.Length == 2);
                ret.Add(int.Parse(items[0]), items[1]);
            }
        }

        private void Parse(ref Dictionary<int, float> ret, string content)
        {
            string[] pairs = content.Split(';');
            for(int i = 0; i < pairs.Length; ++i)
            {
                string[] items = pairs[i].Split(':');
                Debug.Assert(items.Length == 2);
                ret.Add(int.Parse(items[0]), float.Parse(items[1]));
            }
        }

        private void Parse(ref Dictionary<string, int> ret, string content)
        {
            string[] pairs = content.Split(';');
            for(int i = 0; i < pairs.Length; ++i)
            {
                string[] items = pairs[i].Split(':');
                Debug.Assert(items.Length == 2);
                ret.Add(items[0], int.Parse(items[1]));
            }
        }

        private void Parse(ref Dictionary<string, string> ret, string content)
        {
            string[] pairs = content.Split(';');
            for(int i = 0; i < pairs.Length; ++i)
            {
                string[] items = pairs[i].Split(':');
                Debug.Assert(items.Length == 2);
                ret.Add(items[0], items[1]);
            }
        }

        private void Parse(ref Dictionary<string, float> ret, string content)
        {
            string[] pairs = content.Split(';');
            for(int i = 0; i < pairs.Length; ++i)
            {
                string[] items = pairs[i].Split(':');
                Debug.Assert(items.Length == 2);
                ret.Add(items[0], float.Parse(items[1]));
            }
        }

        private Dictionary<int, Dictionary<string, Player>> m_PlayerDict1 = new Dictionary<int, Dictionary<string, Player>>();
        public Player GetPlayerByID(int key1, string key2)
        {
            const string tableName = "Player";

            Player desc = null;
            if(FindPlayerByID_2Key(key1, key2, ref desc))
                return desc;

            SqliteDataReader reader = m_Sql.ReadTable(tableName, "id", "=", key2.ToString());
            while(reader.Read())
            {                
                desc.Building_ID = reader.GetString(reader.GetOrdinal("ID"));
                desc.Name = reader.GetString(reader.GetOrdinal("Name"));
                desc.HP = reader.GetFloat(reader.GetOrdinal("HP"));
                desc.Male = reader.GetBoolean(reader.GetOrdinal("Male"));
                desc.MonsterDesc = GetMonsterByID(reader.GetInt32(reader.GetOrdinal("MonsterDesc")));
                Parse(ref desc.variant1, reader.GetString(reader.GetOrdinal("variant1")));
                Parse(ref desc.variant2, reader.GetString(reader.GetOrdinal("variant2")));
                Parse(ref desc.variant3, reader.GetString(reader.GetOrdinal("variant3")));
                Parse(ref desc.variant4, reader.GetString(reader.GetOrdinal("variant4")));
            }

            return desc;
        }

        private Dictionary<int, Player> m_XX = new Dictionary<int, Player>();

        private bool FindPlayerByID_1Key(int key1, ref Player desc)
        {
            if(!m_XX.TryGetValue(key1, out desc))
            {
                desc = new Player();
                return false;
            }

            return true;
        }

        private bool FindPlayerByID_2Key(int key1, string key2, ref Player desc)
        {
            Dictionary<string, Player> dict;
            if(!m_PlayerDict1.TryGetValue(key1, out dict))
            {
                dict = new Dictionary<string, Player>();
                m_PlayerDict1.Add(key1, dict);
            }

            if(!dict.TryGetValue(key2, out desc))
            {
                desc = new Player();
                dict.Add(key2, desc);
                return false;
            }

            return true;
        }

        // private bool Find#TABLENAME#ByID_1Key(#KEY_VALUETYPE# key1, ref #TABLENAME# desc)
        // {
        //     if(!m_#TABLENAME#Dict.TryGetValue(key1, out desc))
        //     {
        //         desc = new #TABLENAME#();
        //         return false;
        //     }
        //     return true;
        // }
        // private bool Find#TABLENAME#ByID_2Key(#KEY_VALUETYPE# key1, #KEY_VALUETYPE# key2, ref #TABLENAME# desc)
        // {
        //     Dictionary<#KEY_VALUETYPE#, #TABLENAME#> dict;
        //     if(!m_#TABLENAME#Dict.TryGetValue(key1, out dict))
        //     {
        //         dict = new Dictionary<string, #TABLENAME#>();
        //         m_#TABLENAME#Dict.Add(key1, dict);
        //     }

        //     if(!dict.TryGetValue(key2, out desc))
        //     {
        //         desc = new #TABLENAME#();
        //         dict.Add(key2, desc);
        //         return false;
        //     }
        //     return true;
        // }
    }
}



