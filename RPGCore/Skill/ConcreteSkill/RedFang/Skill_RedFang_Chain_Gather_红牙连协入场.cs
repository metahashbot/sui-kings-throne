using System;
using ARPG.Character.Base;
using RPGCore.Buff.ConcreteBuff.Utility;
namespace RPGCore.Skill.ConcreteSkill.RedFang
{
	[Serializable]
	public class Skill_RedFang_Chain_Gather_红牙连协入场 : BaseRPSkill
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


		public override bool _Internal_TryPrepareSkill()
		{
			if (base._Internal_TryPrepareSkill())
			{
				ApplyDirectorInvincible();
				return true;
			}
			else
			{
				return false;
			}
		}


		protected override void BindingInput()
		{
		}



		public override void ModifyRemainingCD(bool modifyOrReset, float modify)
		{
			base.ModifyRemainingCD(modifyOrReset, modify);
			 GetBlockBuffRef().ResetAvailableTimeAs(RemainingCoolDownDuration);
		}

		protected override void _InternalSkillEffect_SkillDefaultTakeEffect()
		{
			base._InternalSkillEffect_SkillDefaultTakeEffect();
		
			 GetBlockBuffRef().ResetAvailableTimeAs(RemainingCoolDownDuration);
			 GetBlockBuffRef().CurrentSingleBlockDuration = RemainingCoolDownDuration;
		}

		protected override void _InternalProgress_CompleteOn_Prepare(
			string configName,
			SheetAnimationInfo_帧动画配置 sheetAnimationInfo,
			BaseCharacterSheetAnimationHelper animationHelper)
		{
			base._InternalProgress_CompleteOn_Prepare(configName, sheetAnimationInfo, animationHelper);
			RemoveDirectorInvincible();
			;
		}
	}
}