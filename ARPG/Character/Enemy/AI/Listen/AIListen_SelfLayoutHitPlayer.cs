using System;
using System.Collections.Generic;
using ARPG.Character.Enemy.AI.Decision.DecisionComponent;
using Global;
using Global.ActionBus;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Listen
{
	[Serializable]
	public class AIListen_SelfLayoutHitPlayer : BaseAIListenComponent
	{
		[SerializeField, LabelText("关联的版面 UID"), FoldoutGroup("配置", true)]
		private string _relatedLayoutUID;


		[LabelText("已命中的版面ID(不是UID)，已存在的不会再次触发"),FoldoutGroup("运行时",true)]
		private List<int> _hitRecordLayoutIDList = new List<int>();

		[SerializeField,LabelText("已命中的版面实例不会再次触发"),FoldoutGroup("配置",true)]
		private bool _hitRecordLayoutInstance = true;


		// [SerializeReference, LabelText("直属=命中判定后：触发的副作用组件们【运行时只读】"), FoldoutGroup("配置", true)]
		// public List<BaseDecisionCommonComponent> Hit_CommonComponents;
		//
		// [SerializeField,LabelText("文件=命中判定后：触发的副作用组件们【运行时只读】"),FoldoutGroup("配置",true),ListDrawerSettings(ShowFoldout = true),
		//  InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		// public List<SOConfig_PresetSideEffects> Hit_CommonComponents_File;
		
		[SerializeField,]
		public DCCConfigInfo SelfDCCConfigInfo;

		public override void InitializeAndProcessRegister(SOConfig_AIBrain brainRef)
		{
			base.InitializeAndProcessRegister(brainRef);
			
			
			SelfDCCConfigInfo.CommonComponents_RuntimeAll = new List<BaseDecisionCommonComponent>();
			SelfDCCConfigInfo.BuildRuntimeDCC(ref SelfDCCConfigInfo.CommonComponents_RuntimeAll);
			
			
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_DamageAssistService_OnNewDamageEntryGenerated,
				_ABC_CheckGlobalDamageEntry_OnNewDamageEntryGenerate);
		}


		private void _ABC_CheckGlobalDamageEntry_OnNewDamageEntryGenerate(DS_ActionBusArguGroup ds)
		{
			var damageApplyResult = ds.ObjectArgu1 as RP_DS_DamageApplyResult;
			var casterBehaviour = damageApplyResult.Caster as EnemyARPGCharacterBehaviour;
			//前提：caster就是自己，
			if (casterBehaviour == null)
			{
				return;
			}
			//前提，receiver是玩家
			if (damageApplyResult.Receiver is not PlayerARPGConcreteCharacterBehaviour)
			{
				return;
			}
			
			if(casterBehaviour == RelatedAIBrainRuntimeInstance.BrainHandlerFunction
				.SelfRelatedARPGCharacterBehaviour)
			{
				if (_hitRecordLayoutInstance)
				{
					if (_hitRecordLayoutIDList.Contains(damageApplyResult.RelatedProjectileRuntimeRef
						.SelfLayoutConfigReference.LayoutHandlerFunction.SelfHandlerID))
					{
						return;
					}
					else
					{
						_hitRecordLayoutIDList.Add(damageApplyResult.RelatedProjectileRuntimeRef
							.SelfLayoutConfigReference.LayoutHandlerFunction.SelfHandlerID);
					}
				}
				
				//关联UID匹配
				if (string.Equals(
					damageApplyResult.RelatedProjectileRuntimeRef.SelfLayoutConfigReference.LayoutContentInSO.LayoutUID,
					_relatedLayoutUID,
					StringComparison.OrdinalIgnoreCase))
				{
					var ds_selfHit = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
						.L_AIListen_OnSelfLayoutHitPlayer_AI监听_自身版面命中玩家);
					RelatedAIBrainRuntimeInstance.BrainHandlerFunction.SelfLocalActionBusRef.TriggerActionByType(
						ds_selfHit);
					if (SelfDCCConfigInfo.CommonComponents_RuntimeAll != null)
					{
						foreach (var component in SelfDCCConfigInfo.CommonComponents_RuntimeAll)
						{
							component.EnterComponent(RelatedAIBrainRuntimeInstance);
						}
					}
				}
			}
			




		}

		public override void UnRegisterListenInActionBus()
		{
			base.UnRegisterListenInActionBus();
			GlobalActionBus.GetGlobalActionBus()
				.RemoveAction(ActionBus_ActionTypeEnum.G_DamageAssistService_OnNewDamageEntryGenerated,
					_ABC_CheckGlobalDamageEntry_OnNewDamageEntryGenerate);
			SelfDCCConfigInfo.ClearOnUnload();
		}



	}
}