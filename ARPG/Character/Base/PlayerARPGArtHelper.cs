using System;
using System.Collections.Generic;
using ARPG.Character.Base.CustomSpineData;
using Global.ActionBus;
using RPGCore;
using RPGCore.Skill;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
using WorldMapScene.Character;
namespace ARPG.Character.Base
{


	public class PlayerARPGArtHelper : BaseARPGArtHelper
	{




		[InfoBox("用于修饰整体Timeline特效的尺寸")]
		[SerializeField, LabelText("预设于Timeline的特效Holder")]
		protected Transform _holder_TimelineVFXHolder;

		public void SetTimelineVFXHolderScale(float v)
		{
			_holder_TimelineVFXHolder.transform.localScale = v * Vector3.one;
		}
		
		private PlayerARPGConcreteCharacterBehaviour _playerCharacterBehaviourRef;

		public override void InitializeOnInstantiate(LocalActionBus lab)
		{
			base.InitializeOnInstantiate(lab);
			_localActionBusRef = lab;
		}
		public override void InjectBaseRPBehaviourRef(RolePlay_BaseBehaviour behaviourRef)
		{
			base.InjectBaseRPBehaviourRef(behaviourRef);
			_playerCharacterBehaviourRef = behaviourRef as PlayerARPGConcreteCharacterBehaviour;
		}



		public override void FixedUpdateTick(float ct, int cf, float delta)
		{
			base.FixedUpdateTick(ct, cf, delta);
			if (_playerCharacterBehaviourRef.CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction
				._Cache_ANInfo_BattleIdle is SheetAnimationInfoMultipleIdle_帧动画多形态Idle配置_Idle multipleIdle)
			{

				if (CurrentMainAnimationInfoRuntime.ConfigName.Equals(
					_playerCharacterBehaviourRef.CurrentActiveWeaponConfigRuntimeInstance.WeaponFunction
						._Cache_ANInfo_BattleIdle.ConfigName,
					StringComparison.OrdinalIgnoreCase))
				{

					if (multipleIdle.AccumulateDuration > multipleIdle.ConvertIdleAnimationDelay)
					{
						multipleIdle.ResetAccumulateDuration();
						var ds_ani = new DS_ActionBusArguGroup(multipleIdle,
							AnimationPlayOptionsFlagTypeEnum.PlayIdleReplace_播放长Idle动画,
							SelfAnimationPlayResult,
							true,
							FixedOccupiedCancelTypeEnum.ContinueBreak_接续断);
						_localActionBusRef.TriggerActionByType(ds_ani);
					}
				}
			}
		}



#region 动画事件  —— AnimationEvent

#endregion
		
		
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