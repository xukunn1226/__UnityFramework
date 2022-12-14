using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
    public class AssetSystem
    {
        private EPlayMode           m_PlayMode;
        public IDecryptionServices  decryptionServices  { get; private set; }
        public IBundleServices      bundleServices      { get; private set; }

        public void Init(EPlayMode playMode, IDecryptionServices decryptionServices, IBundleServices bundleServices)
        {
            m_PlayMode = playMode;
            this.decryptionServices = decryptionServices;
            this.bundleServices = bundleServices;
        }

        public void Update()
        {

        }
    }
}