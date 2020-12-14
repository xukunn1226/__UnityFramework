using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Core
{
    [RequireComponent(typeof(BundleExtracter), typeof(Patcher))]
    public class VersionManager : MonoBehaviour, IExtractListener
    {
        private BundleExtracter m_BundleExtracter;
        private Patcher         m_Patcher;

        public int              WorkerCountOfBundleExtracter    = 5;

        private void Awake()
        {
            m_BundleExtracter = GetComponent<BundleExtracter>();
            m_Patcher = GetComponent<Patcher>();

            if (m_BundleExtracter == null || m_Patcher == null)
                throw new System.ArgumentNullException("m_BundleExtracter == null || m_Patcher == null");
        }

        // Start is called before the first frame update
        void Start()
        {
            m_BundleExtracter.StartWork(WorkerCountOfBundleExtracter, this);
        }

        void IExtractListener.OnInit(bool success)
        {
            Debug.Log($"IExtractListener.OnInit:    {success}");
        }

        void IExtractListener.OnShouldExtract(ref bool shouldExtract)
        {
            Debug.Log($"IExtractListener.OnShouldExtract:   {shouldExtract}");
        }

        void IExtractListener.OnBegin(int countOfFiles)
        {
            Debug.Log($"IExtractListener.OnBegin:   {countOfFiles}");
        }

        void IExtractListener.OnEnd(float elapsedTime, string error)
        {
            Debug.Log($"IExtractListener.OnEnd:     elapsedTime({elapsedTime})      error({error})");
        }

        void IExtractListener.OnFileCompleted(string filename, bool success)
        {
            Debug.Log($"IExtractListener.OnFileCompleted:   filename({filename})    success({success})");
        }

        void IExtractListener.OnFileProgress(string filename, ulong downedLength, ulong totalLength, float downloadSpeed)
        {
            Debug.Log($"IExtractListener.OnFileProgress:    filename({filename})    downedLength({downedLength})    totalLength({totalLength})      downloadSpeed({downloadSpeed})");
        }
    }
}