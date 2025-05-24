using System.Collections.Generic;
using Global;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace RPGCore.Buff.Config
{
	public class RPBuffAssetEntry : BaseAssetEntry
	{

		public Sprite BuffSprite;
		
		


		public void LoadEntryByAssetEntryConfigSO(RPBuffConfigContentInSO_LoadAssetEntry assetEntry)
		{
			_selfOpList = new List<AsyncOperationHandle>();
			if (assetEntry.SpriteAR != null)
			{
				var op = Addressables.LoadAssetAsync<Sprite>(assetEntry.SpriteAR);
				_selfOpList.Add(op);
				BuffSprite = op.WaitForCompletion();
			}


			Loaded = true;
		}

	}
}