using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using ARPG.Character.Base.CustomSpineData;
using Global.ActionBus;
using RPGCore.Buff;
using RPGCore.Buff.ConcreteBuff.Common;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.ConcreteDecision
{
	[Serializable]
	public class DH_通用常规失衡_CommonUnbalance : BaseDecisionHandler
	{

		[SerializeField, LabelText("使用的动画配置名"), FoldoutGroup("配置", true), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		protected string _an_Unbalance;

		protected AnimationInfoBase _sai_Unbalance;

		[Header("===========")]
		[ShowInInspector, LabelText("剩余时长"), ReadOnly]
		protected float remainingDuration = 0f;

		protected List<string> _weaknessGroupToRestoreList = new List<string>();
		public void RefreshWeaknessGroupToRestoreList(List<string> list)
		{
			_weaknessGroupToRestoreList.Clear();
			_weaknessGroupToRestoreList.AddRange(list);
		}



		public override void Initialize(SOConfig_AIBrain config)
		{
			base.Initialize(config);
			_sai_Unbalance = GetAnimationInfoFromBrain(_an_Unbalance);
		}


		public override DS_ActionBusArguGroup OnDecisionBeforeStartExecution(bool withSideEffect = true)
		{
			var ds_start = base.OnDecisionBeforeStartExecution(withSideEffect);

			_Internal_RequireAnimation(_sai_Unbalance, true, null, AnimationPlayOptionsFlagTypeEnum.ForceReplay_必定重播 | AnimationPlayOptionsFlagTypeEnum.Default_缺省状态);
			_Internal_BlockBrainAutoDeduce();


			return ds_start;
		}

		public override DS_ActionBusArguGroup OnDecisionNormalComplete(bool autoLaunch = true, bool normalStop = true)
		{
			var ds = base.OnDecisionNormalComplete(autoLaunch, normalStop);
			if (!_selfRelatedBehaviourRef.CharacterDataValid)
			{
				return ds;
			}
			var buff_weakness =
				_selfRelatedBehaviourRef.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum.CommonEnemyWeakness_通用敌人弱点);
			if (buff_weakness != BuffAvailableType.NotExist)
			{
				Buff_通用敌人弱点_CommonEnemyWeakness buff =
					_selfRelatedBehaviourRef.ReceiveBuff_GetTargetBuff(RolePlay_BuffTypeEnum.CommonEnemyWeakness_通用敌人弱点)
						as Buff_通用敌人弱点_CommonEnemyWeakness;
				foreach (string toRestore in _weaknessGroupToRestoreList)
				{
					var get = buff.GetWeaknessGroupByID(toRestore);
					if (get == null)
					{
						continue;
					}
					get.CurrentGroupActive = true;
				}
			}
			return ds;
		}

		public override void FixedUpdateTick(float ct, int cf, float delta)
		{
			base.FixedUpdateTick(ct, cf, delta);
			remainingDuration -= delta;
			if (remainingDuration < 0f)
			{
				RestoreAutoDeduce_OnDecisionDurationEnd();
			}
		}


		public void SetDuration_NotLess(float duration)
		{
			if (remainingDuration < duration)
			{
				remainingDuration = duration;
			}
			
		}



		protected void RestoreAutoDeduce_OnDecisionDurationEnd()
		{
			if (!CheckIfCurrentBrainValidAndCurrentDecisionIsThis(this.SelfRelatedDecisionRuntimeInstance))
			{
				return;
			}
			OnDecisionNormalComplete();
		}



// [Button("转换其他类似的行为")]
// 		public void Convert()
// 		{
// 			//find all soconfig ai decision by asset database
// 			var soConfigs = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_AIDecision");
// 			foreach (var perGUID in soConfigs)
// 			{
// 				var perPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perGUID);
// 				SOConfig_AIDecision perSO = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_AIDecision>(perPath);
// 				DH_通用常规失衡_CommonUnbalance newCM = new DH_通用常规失衡_CommonUnbalance();
// 				switch (perSO.DecisionHandler)
// 				{
// 					case CommonUnbalance_DecisionHandler cd:
// 						newCM._an_Unbalance = cd._an_Unbalance;
// 						perSO.DecisionHandler = newCM;
// 						UnityEditor.EditorUtility.SetDirty(perSO);
// 						UnityEditor.AssetDatabase.SaveAssetIfDirty(perSO);
// 						break;
// 				}
// 			}
// 		}


	}
}