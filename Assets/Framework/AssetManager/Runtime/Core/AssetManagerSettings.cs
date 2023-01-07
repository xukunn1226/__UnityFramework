using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
	[CreateAssetMenu(fileName = "AssetManagerSettings", menuName = "AssetManager Settings/Create Settings")]
	public class AssetManagerSettings : ScriptableObject
	{
		/// <summary>
		/// AssetBundle�ļ��ĺ�׺��
		/// </summary>
		public string AssetBundleFileVariant = "bundle";

		/// <summary>
		/// ԭ���ļ��ĺ�׺��
		/// </summary>
		public string RawFileVariant = "rawfile";

		/// <summary>
		/// �嵥�ļ�����
		/// </summary>
		public string PatchManifestFileName = "AssetManifest";


		/// <summary>
		/// �嵥�ļ�ͷ���
		/// </summary>
		public const uint PatchManifestFileSign = 0x594F4F;

		/// <summary>
		/// �嵥�ļ����޴�С��100MB��
		/// </summary>
		public const int PatchManifestFileMaxSize = 104857600;

		/// <summary>
		/// �嵥�ļ���ʽ�汾
		/// </summary>
		public const int ManifestSerializationVersion = 1;

		/// <summary>
		/// ��������ļ�������
		/// </summary>
		public const string OutputFolderName = "OutputCache";

		/// <summary>
		/// ��������ı����ļ�
		/// </summary>
		public const string ReportFileName = "BuildReport";

		/// <summary>
		/// Unity��ɫ����Դ������
		/// </summary>
		public const string UnityShadersBundleName = "unityshaders";

		/// <summary>
		/// ������ԴĿ¼����
		/// </summary>
		public const string StreamingAssetsBuildinFolder = "BuildinFiles";

		/// <summary>
		/// �༭��ģ�⻷�����첽����ʱ���ӳ�֡��
		/// </summary>
		public const int DelayedFrameNumInEditorSimulateMode = 2;

		/// <summary>
		/// ���Ե��ļ�����
		/// </summary>
		public static readonly string[] IgnoreFileExtensions = { "", ".so", ".dll", ".cs", ".js", ".boo", ".meta", ".cginc" };
	}
}