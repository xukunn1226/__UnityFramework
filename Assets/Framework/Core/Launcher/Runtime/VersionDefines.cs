using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    static public class VersionDefines
    {
        static public readonly string       APP_VERSION_PATH                = "Assets/Resources/AppVersion.asset";

        // base
        static public readonly string       BASE_FILELIST_PATH              = "Assets/Resources";
        static public readonly string       BASE_FILELIST_NAME              = "BaseFileList.bytes";         // 首包的FileList
        static public readonly string       BASE_APPVERSION                 = "BaseAppVersion_9695e71e3a224b408c39c7a75c0fa376";    // 标记引擎版本号
    
        // extra
        static public readonly string       EXTRA_FILELIST_PATH             = "Assets/Resources";
        static public readonly string       EXTRA_FILELIST_NAME             = "ExtraFileList.bytes";        // 二次下载包的FileList
        static public readonly string       EXTRA_APPVERSION                = "ExtraAppVersion_9695e71e3a224b408c39c7a75c0fa376";

        // patch
        static public readonly string       CUR_APPVERSION                  = "CurAppVersion_fe2679cf89a145ccb45b715568e6bc07";     // 标记当前最新版本号
        static public readonly string       DIFFCOLLECTION_FILENAME         = "diffcollection.json";
        static public readonly string       DIFF_FILENAME                   = "diff.json";
        static public readonly string       BACKDOOR_FILENAME               = "backdoor.zjson";
        static public readonly string       BASE_FOLDER                     = "base";                       // 首包数据文件夹
        static public readonly string       PATCH_FOLDER                    = "patch";                      // 基于CDN根目录的补丁数据文件夹
        static public readonly string       EXTRA_FOLDER                    = "extra";                      // 二次下载数据文件夹，分包、cdn存储均使用此文件夹
        static public readonly string       PKG_FOLDER_PREFIX               = "pkg_";                       // 基于CDN根目录的分包数据文件夹

        // deployment
        static public readonly string       DEPLOYMENT_ROOT_PATH            = "Deployment";                 // 部署文件夹目录
        static public readonly string       DEPLOYMENT_LATEST_APP_PATH      = "latest/player";              // 放置当前最新编译版本app的路径
        static public readonly string       DEPLOYMENT_LATEST_BUNDLE_PATH   = "latest/assetbundles";        // 放置当前最新编译版本bundles的路径
        static public readonly string       DEPLOYMENT_BACKUP_FOLDER        = "backup";                     // 备份各平台下发布资源的目录（app & bundles）
        static public readonly string       DEPLOYMENT_CDN_FOLDER           = "cdn";                        // cdn path, base on DEPLOYMENT_ROOT_PATH
        static public readonly string       DEPLOYMENT_EXTRA_DATA_FOLDER    = "data";                       // 存储二次下载、边玩边下功能资源数据
        static public readonly string       DEPLOYMENT_FULL_FILELIST_NAME   = "FullFileList.bytes";         // 完整包的FileList，用于diff
        static public readonly string       DEPLOYMENT_BACKUP_APP_FOLDER    = "app";                        // backup目录下存放app的文件夹
        static public readonly string       DEPLOYMENT_BACKUP_BUNDLE_FOLDER = "assetbundles";               // backup目录下存放bundles的文件夹
        static public readonly string       CACHED_BACKDOOR_PATH            = "Assets/Framework/AssetManagement/GameBuilder/Data/" + VersionDefines.BACKDOOR_FILENAME;

        // cdn上存储backdoor的路径
        static public string cdnBackdoorPath
        {
            get
            {
                return string.Format($"{DEPLOYMENT_CDN_FOLDER}/{BACKDOOR_FILENAME}");
            }
        }
        // 放置extra, pkg的资源路径
        static public string cdnExtraDataPath
        {
            get
            {
                return string.Format($"{DEPLOYMENT_CDN_FOLDER}/{DEPLOYMENT_EXTRA_DATA_FOLDER}");
            }
        }

        // 放置patch的资源路径
        static public string cdnPatchDataPath
        {
            get
            {
                return string.Format($"{DEPLOYMENT_CDN_FOLDER}/{PATCH_FOLDER}");
            }
        }
    }
}