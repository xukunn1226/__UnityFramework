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
        static public readonly string       BASE_FILELIST_NAME              = "BaseFileList.bytes";         // �װ���FileList
        static public readonly string       BASE_APPVERSION                 = "BaseAppVersion_9695e71e3a224b408c39c7a75c0fa376";    // �������汾��
    
        // extra
        static public readonly string       EXTRA_FILELIST_PATH             = "Assets/Resources";
        static public readonly string       EXTRA_FILELIST_NAME             = "ExtraFileList.bytes";        // �������ذ���FileList
        static public readonly string       EXTRA_APPVERSION                = "ExtraAppVersion_9695e71e3a224b408c39c7a75c0fa376";

        // patch
        static public readonly string       CUR_APPVERSION                  = "CurAppVersion_fe2679cf89a145ccb45b715568e6bc07";     // ��ǵ�ǰ���°汾��
        static public readonly string       DIFFCOLLECTION_FILENAME         = "diffcollection.json";
        static public readonly string       DIFF_FILENAME                   = "diff.json";
        static public readonly string       BACKDOOR_FILENAME               = "backdoor.zjson";
        static public readonly string       BASE_FOLDER                     = "base";                       // �װ������ļ���
        static public readonly string       PATCH_FOLDER                    = "patch";                      // ����CDN��Ŀ¼�Ĳ��������ļ���
        static public readonly string       EXTRA_FOLDER                    = "extra";                      // �������������ļ��У��ְ���cdn�洢��ʹ�ô��ļ���
        static public readonly string       PKG_FOLDER_PREFIX               = "pkg_";                       // ����CDN��Ŀ¼�ķְ������ļ���

        // deployment
        static public readonly string       DEPLOYMENT_ROOT_PATH            = "Deployment";                 // �����ļ���Ŀ¼
        static public readonly string       DEPLOYMENT_LATEST_APP_PATH      = "latest/player";              // ���õ�ǰ���±���汾app��·��
        static public readonly string       DEPLOYMENT_LATEST_BUNDLE_PATH   = "latest/assetbundles";        // ���õ�ǰ���±���汾bundles��·��
        static public readonly string       DEPLOYMENT_BACKUP_FOLDER        = "backup";                     // ���ݸ�ƽ̨�·�����Դ��Ŀ¼��app & bundles��
        static public readonly string       DEPLOYMENT_CDN_FOLDER           = "cdn";                        // cdn path, base on DEPLOYMENT_ROOT_PATH
        static public readonly string       DEPLOYMENT_EXTRA_DATA_FOLDER    = "data";                       // �洢�������ء�������¹�����Դ����
        static public readonly string       DEPLOYMENT_FULL_FILELIST_NAME   = "FullFileList.bytes";         // ��������FileList������diff
        static public readonly string       DEPLOYMENT_BACKUP_APP_FOLDER    = "app";                        // backupĿ¼�´��app���ļ���
        static public readonly string       DEPLOYMENT_BACKUP_BUNDLE_FOLDER = "assetbundles";               // backupĿ¼�´��bundles���ļ���
        static public readonly string       CACHED_BACKDOOR_PATH            = "Assets/Framework/AssetManagement/GameBuilder/Data/" + VersionDefines.BACKDOOR_FILENAME;

        // cdn�ϴ洢backdoor��·��
        static public string cdnBackdoorPath
        {
            get
            {
                return string.Format($"{DEPLOYMENT_CDN_FOLDER}/{BACKDOOR_FILENAME}");
            }
        }
        // ����extra, pkg����Դ·��
        static public string cdnExtraDataPath
        {
            get
            {
                return string.Format($"{DEPLOYMENT_CDN_FOLDER}/{DEPLOYMENT_EXTRA_DATA_FOLDER}");
            }
        }

        // ����patch����Դ·��
        static public string cdnPatchDataPath
        {
            get
            {
                return string.Format($"{DEPLOYMENT_CDN_FOLDER}/{PATCH_FOLDER}");
            }
        }
    }
}