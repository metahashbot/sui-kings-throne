using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using GameplayEvent;
using GameplayEvent.SO;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RPGCore.Buff.ConcreteBuff.Common
{
	[Serializable]
	public class WARResult_效果之触发事件_WeaknessResultOnEvent : BaseWeaknessAffectRule, I_WeaknessComponentAsResult
	{

		[SerializeField, LabelText("需要触发的事件Prefab们"), GUIColor(255f / 255f, 248f / 255f, 10f / 255f),
		 ListDrawerSettings(ShowFoldout = true)]
		protected List<SOConfig_PrefabEventConfig> _prefabEventConfigs = new List<SOConfig_PrefabEventConfig>();

		public static WARResult_效果之触发事件_WeaknessResultOnEvent CreateNew(
			WARResult_效果之触发事件_WeaknessResultOnEvent copyFrom)
		{
			var newRule = new WARResult_效果之触发事件_WeaknessResultOnEvent();
			newRule._prefabEventConfigs = copyFrom._prefabEventConfigs;
			return newRule;
		}
		public override void UnloadAndClearBeforeRemove(Buff_通用敌人弱点_CommonEnemyWeakness relatedBuff)
		{
		}
		public void RegisterToGroup(Buff_通用敌人弱点_CommonEnemyWeakness.WeaknessInfoGroup group)
		{
			var newCopy = CreateNew(this);
			newCopy.RelatedBuffRef = group.RelatedBuff;
			newCopy.RelatedGroupRef = group;
			group.AddResultRule(newCopy);
		}
		public void TriggerWeaknessResult()
		{
			foreach (var perPrefabEventConfig in _prefabEventConfigs)
			{
				var ds_ge = new DS_GameplayEventArguGroup();
				ds_ge.ObjectArgu1 = RelatedBuffRef.Parent_SelfBelongToObject as BaseARPGCharacterBehaviour;
				GameplayEventManager.Instance.StartGameplayEvent(perPrefabEventConfig, ds_ge);
			}
		}
	}
}