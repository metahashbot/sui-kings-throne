using System;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.AssistBusiness
{
	[Serializable]
	[TypeInfoBox("用于跳字业务的配置信息")]
	[CreateAssetMenu(fileName = "跳字配置", menuName = "#SO Assets#/#Global Config#/跳字配置——背击变体", order = -200)]
	public class SOConfig_DamageHintVariant_BackHit : SOConfig_DamageHintConfiguration
	{
		public override void InitBuild()
		{
			base.InitBuild();
			NextAvailablePopupTime = 0f;
		}

		[Header("==========")]
		[SerializeField,LabelText("跳字冷却")]
		public float PopupHintCooldown = 0.5f;
		
		[NonSerialized]
		public float NextAvailablePopupTime = 0f;
		
	}
}