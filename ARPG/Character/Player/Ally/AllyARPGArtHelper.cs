using ARPG.Character.Base;
using ARPG.Character.Base.CustomSpineData;
using Global.ActionBus;
using Global.Character;
using RPGCore;
using UnityEngine;
using WorldMapScene.Character;
using WorldMapScene.RegionMap;
namespace ARPG.Character.Player.Ally
{
	public class AllyARPGArtHelper : BaseARPGArtHelper
	{
		private AllyARPGCharacterBehaviour _allyCharacterBehaviourRef;


		public override void InitializeOnInstantiate(LocalActionBus lab)
		{
			base.InitializeOnInstantiate(lab);
			_localActionBusRef = lab;
		}

		public override void InjectBaseRPBehaviourRef(RolePlay_BaseBehaviour behaviourRef)
		{
			base.InjectBaseRPBehaviourRef(behaviourRef);
			_allyCharacterBehaviourRef = behaviourRef as AllyARPGCharacterBehaviour;

		}

		public void SetEmissionInfo(DS_CharacterEmissionInfo emissionInfo)
		{
			if (emissionInfo == null)
			{
				return;
			}
			foreach (var perSpine in CharacterAnimationHelperObjectsDict.Values)
			{
				SetEmission_All(emissionInfo);
			}
		}






		public override void _SheetCallback_CustomEvent(string eventName)
		{
			var ds_general = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
				.L_AnimationHelper_OnSpineGeneralAnimationEvent_通用动画发出常规动画事件);
			ds_general.ObjectArgu1 = _allyCharacterBehaviourRef.GetAIBrainRuntimeInstance() == null ? null
				: _allyCharacterBehaviourRef.GetAIBrainRuntimeInstance().BrainHandlerFunction.CurrentRunningDecision;
			ds_general.ObjectArgu2 = eventName;
			ds_general.ObjectArguStr = CurrentMainAnimationInfoRuntime;

			_localActionBusRef?.TriggerActionByType(ds_general);
			
		}

		public override void _SheetCallback_OnAnimationStart()
		{
			// DBug.Log("动画开始，当前为"+CurrentMainAnimationInfoRuntime.ConfigName);
			var ds_general =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_AnimationHelper_OnAnimationStart_动画通用开始);

			ds_general.ObjectArgu1 = MainCharacterAnimationHelperRef;
			ds_general.ObjectArgu2 = CurrentMainAnimationInfoRuntime;
			ds_general.ObjectArguStr = _allyCharacterBehaviourRef.GetAIBrainRuntimeInstance() == null ? null
				: _allyCharacterBehaviourRef.GetAIBrainRuntimeInstance().BrainHandlerFunction.CurrentRunningDecision;

			_localActionBusRef?.TriggerActionByType(ds_general);
		}
		public override void _SheetCallback_OnAnimationComplete()
		{
			var ds_general =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_AnimationHelper_OnAnimationComplete_动画通用结束);
			ds_general.ObjectArgu1 = MainCharacterAnimationHelperRef;
			ds_general.ObjectArgu2 = CurrentMainAnimationInfoRuntime;
			ds_general.ObjectArguStr = _allyCharacterBehaviourRef.GetAIBrainRuntimeInstance() == null ? null
				: _allyCharacterBehaviourRef.GetAIBrainRuntimeInstance().BrainHandlerFunction.CurrentRunningDecision;

			_localActionBusRef?.TriggerActionByType(ds_general);

			if (CurrentMainAnimationInfoRuntime is SheetAnimationInfoMultipleIdle_帧动画多形态Idle配置_Idle idle)
			{
				if (_currentAnimationPlayOption.HasFlag(AnimationPlayOptionsFlagTypeEnum.PlayIdleReplace_播放长Idle动画))
				{
					idle.ResetAccumulateDuration();
				}
				SheetAnimation_SetMainAnimation(idle, AnimationPlayOptionsFlagTypeEnum.Default_缺省状态);
			}

		}


#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawWireCube(transform.position,
				new Vector3(_VFXScaleRadius * 2f, _VFXScaleRadius * 2f, _VFXScaleRadius * 2f));
		}
#endif
	}
}