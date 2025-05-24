using System;
using System.Collections.Generic;
using ARPG.UI.Panel;
using Global;
using RPGCore.Buff.Config;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Localization;
using UnityEngine.Pool;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace RPGCore.Buff
{


	/// <summary>
	/// 会进行一个事件的广播：UIP_PlayerBuffPanel 和 EnemyHUD 可能会处理这些事件。随后这个类的实例可能就会被弃用
	/// </summary>
	[Serializable]
	public class BLP_显示于UI_DisplayOnUIInfo : BaseBuffLogicPassingComponent
	{

		[LabelText("Buff在UI上的显示类型"), FoldoutGroup("配置", true)]
		public BuffUITypeEnum BuffUIType = BuffUITypeEnum.NoUIBuff_不显示到UI中的Buff;
		public string IconKey;
		public string ContentKey;


		public override void ReleaseOnReturnToPool()
		{
			GenericPool<BLP_显示于UI_DisplayOnUIInfo>.Release(this);
		}
	}


	[Serializable]
	public class ConcreteBuffDisplayOnUIInfo
	{
		[LabelText("Buff在UI上的显示类型")]
		public BuffUITypeEnum BuffUIType = BuffUITypeEnum.NoUIBuff_不显示到UI中的Buff;

		[InfoBox("如果不做额外填充，则会在初始化将IconKey和ContentKey都设置为BuffType的枚举名字，")]
		[LabelText("图标key，于表格[Buff与Perk资源表]第一张")]
		public string IconKey;

		[NonSerialized]
		public Sprite IconSprite;


		public Sprite GetIconSprite()
		{
			if (IconSprite == null)
			{
				IconSprite =
					GlobalConfigurationAssetHolderHelper.Instance.FE_BuffAndPerkResource.GetSpriteByUID(IconKey);
			}
			return IconSprite;
		}

		[LabelText("Buff文本Key，未指定则就是BuffType枚举")]
		public string NameKey;
		
		[NonSerialized]
		public LocalizedString _selfBuffNameLS;
		[NonSerialized]
		public LocalizedString _selfBuffDisplayContentLS;
		[NonSerialized]
		public LocalizedString _selfBuffDescContentLS;

	}

}