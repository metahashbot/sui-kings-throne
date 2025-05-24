using System;
using System.Collections.Generic;
using RPGCore.Buff.BuffComponent;
using RPGCore.Buff.Requirement;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace RPGCore.Buff.Config
{
	/// <summary>
	/// <para>Buff信息的可序列化版本。</para>
	/// <para>用于BuffSO的储存</para>
	/// </summary>
	[Serializable]
	public class RPBuffConfigContentInSO
	{
		[LabelText("Buff类型")]
		public RolePlay_BuffTypeEnum BuffType;

		
		[Header("持续时间，为-1表示buff常驻Holder而不会被移除")]
		public float BuffDuration = -1f;

		[InfoBox("注意区分持续时间和生效时间\n" + "生效时间即使为-1也只是表明buff不会因为超时而报告无效，但它依然可能会因为\n" +
		         "被其他Buff阻挡/不符合DataEntry条件而无效，当Duration消耗完毕时依然会被移除")]
		[Header("生效时间，为-1表示buff始终报告TimeIn而不会超时")]
		public float BuffAvailableTime = -1;

		[InfoBox("每一条注册的，BuffHolder都会处理，并向调用对应的具体BuffFunction。")]
		[SerializeReference, LabelText("Buff注册监听的事件们")]
		public List<BaseBuffTriggerSource> BuffTriggerSources;




		// [LabelText("Buff影响列表。列表每项作为一类结果，每类之间独立计算。")]
		// public List<PerBuffFunctionComponent> SelfAllResults;

		// [LabelText("可序列化的参数组")]
		// public List<RP_DS_BuffSelfCalculationGroup> ArguGroup = new List<RP_DS_BuffSelfCalculationGroup>(1);

	}
	
	public enum BuffUITypeEnum
	{
		NoUIBuff_不显示到UI中的Buff = 0,
		NeutralBuff_个人中性Buff = 1,
		SelfPositiveBuff_个人正面Buff = 1 << 2,
		SelfNegativeBuff_个人负面Buff = 1 << 3,
		EnemyShowBuff_敌人需要显示的Buff = 1 << 4, 
		EnemyOnlyBroadcast_敌人需要广播的Buff = 1 << 5,
	}
}