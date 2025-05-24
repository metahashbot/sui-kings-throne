using System;
using RPGCore.Buff.ConcreteBuff.Utility;
namespace RPGCore.Skill.ConcreteSkill.Elementalist
{
	[Serializable]
	public class Skill_Elementalist_ChainTurbulence_元素乱流 : BaseRPSkill
	{
		
		

		protected Buff_PlayerSwitchCharacterBlock _block;
		protected Buff_PlayerSwitchCharacterBlock GetBlockBuffRef()
		{
			if (_block == null)
			{
				_block = _characterBehaviourRef.ReleaseSkill_GetRelatedBuff(RolePlay_BuffTypeEnum
					.ARPG_PlayerSwitchCharacterBlock_玩家更换角色阻塞) as Buff_PlayerSwitchCharacterBlock;
			}
			return _block;
		}

		
		


		protected override void BindingInput()
		{
			
		}

		public override void ModifyRemainingCD(bool modifyOrReset, float modify)
		{
			base.ModifyRemainingCD(modifyOrReset, modify);
			GetBlockBuffRef().ResetAvailableTimeAs(RemainingCoolDownDuration);

		}

		public override bool _Internal_TryPrepareSkill()
		{
			if (base._Internal_TryPrepareSkill())
			{
				ApplyDirectorInvincible(-1f);
				return true;
			}
			else { return false; }
		}


		protected override void _InternalSkillEffect_SkillDefaultTakeEffect()
		{
			base._InternalSkillEffect_SkillDefaultTakeEffect();
			GetBlockBuffRef().ResetAvailableTimeAs(RemainingCoolDownDuration);
			GetBlockBuffRef().CurrentSingleBlockDuration = RemainingCoolDownDuration;
		}

		protected override void _InternalProgress_Post_CompleteOn_Post()
		{
			base._InternalProgress_Post_CompleteOn_Post();
			RemoveDirectorInvincible();
		}


	}
}