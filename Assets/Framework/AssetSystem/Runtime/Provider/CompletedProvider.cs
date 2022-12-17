using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AssetManagement.Runtime
{
	internal sealed class CompletedProvider : ProviderBase
	{
		public CompletedProvider(AssetInfo assetInfo) : base(null, string.Empty, assetInfo)
		{
		}
		
		public override void Update()
		{
		}

		public void SetCompleted(string error)
		{
			if (status == EProviderStatus.None)
			{
				status = EProviderStatus.Failed;
				lastError = error;
				InvokeCompletion();
			}
		}
	}
}