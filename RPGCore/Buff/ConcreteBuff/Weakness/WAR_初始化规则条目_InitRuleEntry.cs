using System;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	[Serializable]
	public class WAR_初始化规则条目_InitRuleEntry : BaseWeaknessAffectRule
	{

		[SerializeField, LabelText("初始开启状态")] [TitleGroup("===配置===")]
		public bool InitActive = false;

		[SerializeField, LabelText("计量计数")] [TitleGroup("===配置===")]
		[HideInInspector]
		public float CountAmount = 100f;
		
		[SerializeField,LabelText("√:常驻计量 | 口:机制计量")] [TitleGroup("===配置===")]
		public bool IsResidentCounter = false;
		
		
		[SerializeField,LabelText("激活时要求UI显示计量条")]
		[TitleGroup("===配置===")]
		 public bool ActiveRequireShowUI = false;
		
		[SerializeField,LabelText("触发效果后自动禁用")]
		[TitleGroup("===配置===")]
		public bool AutoDisableAfterTrigger = false;
		
		
		
		
		public override void UnloadAndClearBeforeRemove(Buff_通用敌人弱点_CommonEnemyWeakness relatedBuff)
		{
			
		}
	}
}