using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    public class InitializeParameters
    {
		public EPlayMode PlayMode;

		/// <summary>
		/// �ļ����ܷ���ӿ�
		/// </summary>
		public IDecryptionServices DecryptionServices = null;

		public IBundleServices BundleServices = null;
		
		/// <summary>
		/// ��Դ��λ��ַ��Сд������
		/// ע�⣺Ĭ��ֵΪFalse
		/// </summary>
		public bool LocationToLower = false;

		/// <summary>
		/// ��Դ���ص��������
		/// ע�⣺Ĭ��ֵΪMaxValue
		/// </summary>
		public int AssetLoadingMaxNumber = int.MaxValue;

		/// <summary>
		/// Ĭ�ϵ���Դ���������ص�ַ
		/// </summary>
		public string DefaultHostServer = string.Empty;

		/// <summary>
		/// ���õ���Դ���������ص�ַ
		/// </summary>
		public string FallbackHostServer = string.Empty;
	}
}