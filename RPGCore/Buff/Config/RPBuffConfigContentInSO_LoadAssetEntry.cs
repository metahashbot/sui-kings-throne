using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
namespace RPGCore.Buff.Config
{
	/// <summary>
	/// <para>BuffConfig中的需要额外加载的资产项目，比如图标、特效、音效、额外GameObject等等</para>
	/// </summary>
	[Serializable]
	public class RPBuffConfigContentInSO_LoadAssetEntry
	{
		public AssetReferenceT<Sprite> SpriteAR;
	}
}