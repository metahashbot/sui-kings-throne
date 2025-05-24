using System;
using ARPG.Character;
using Global;
using Global.ActionBus;
using RPGCore.Buff;
using RPGCore.Buff.ConcreteBuff.Skill.Swordman;
using RPGCore.Interface;
using RPGCore.Skill.Config;
using UnityEngine.InputSystem;
namespace RPGCore.Skill.ConcreteSkill.Swordman
{
	[Serializable]
	public class Skill_Swordman_BladeDance : BaseRPSkill
	{

		
		
		
		public override void InitOnObtain(
			RPSkill_SkillHolder skillHolderRef,
			SOConfig_RPSkill configRuntimeInstance,
			I_RP_ObjectCanReleaseSkill parent,
			SkillSlotTypeEnum slot)
		{
			base.InitOnObtain(skillHolderRef, configRuntimeInstance, parent, slot);
		}


		protected override void _IC_ReceiveSkillInput_Start(InputAction.CallbackContext context)
		{
			base._IC_ReceiveSkillInput_Start(context);
		}



		/// <summary>
		/// <para>对于原始准备来说：如果Buff有效，则应当为移除buff并进入CD。</para>
		/// </summary>
		/// <returns></returns>
		protected override bool IfReactToInput(
			bool checkCurrentActivePlayer = true,
			bool checkDataEntryEnough = true,
			bool checkReadyType = true,
			bool checkRunningState = true)
		{
			 if (_characterBehaviourRef.ReleaseSkill_CheckTargetBuff(RolePlay_BuffTypeEnum.FromSkill_BladeDanceA1_剑舞A1) == BuffAvailableType.NotExist)
			{
				return base.IfReactToInput(checkCurrentActivePlayer,
					checkDataEntryEnough,
					checkReadyType,
					checkRunningState);

			}
			else
			{
				if (checkCurrentActivePlayer)
				{
					if (_playerControllerRef.CurrentControllingBehaviour != _characterBehaviourRef)
					{
						return false;
					}
				}
				if (checkRunningState)
				{
					switch (BaseGameReferenceService.GameRunningState)
					{
						case BaseGameReferenceService.GameRunningStateTypeEnum.None_未指定:
						case BaseGameReferenceService.GameRunningStateTypeEnum.Loading_加载中:
						case BaseGameReferenceService.GameRunningStateTypeEnum.Paused_暂停:
							return false;
					}
				}

				return true;
			}
		}
		/// <summary>
		/// <para>当剑舞buff存在时，不会进入CD</para>
		/// </summary>
		/// <returns></returns>
		protected override bool IfSkillCanCDTick()
		{
			if (_characterBehaviourRef.ReleaseSkill_CheckTargetBuff(RolePlay_BuffTypeEnum.FromSkill_BladeDanceA1_剑舞A1)!=BuffAvailableType.NotExist)
			{
				return false;
			}
			return base.IfSkillCanCDTick();
		}

		protected override void _InternalSkillEffect_SkillDefaultTakeEffect()
		{
			base._InternalSkillEffect_SkillDefaultTakeEffect();

			_characterBehaviourRef.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.FromSkill_BladeDanceA1_剑舞A1,
				_characterBehaviourRef,
				_characterBehaviourRef);
			
		}


		public override bool _Internal_TryPrepareSkill()
		{
			if (_characterBehaviourRef.ReleaseSkill_CheckTargetBuff(RolePlay_BuffTypeEnum
				.FromSkill_BladeDanceA1_剑舞A1) == BuffAvailableType.NotExist)
			{
				return base._Internal_TryPrepareSkill();
			}
			else
			{
				_characterBehaviourRef.ReceiveBuff_RemoveTargetBuff(RolePlay_BuffTypeEnum.FromSkill_BladeDanceA1_剑舞A1);
				return false;
			}
		}


		public override DS_ActionBusArguGroup ClearBeforeRemove()
		{

			return base.ClearBeforeRemove();
		}

	}
}