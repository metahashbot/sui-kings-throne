using System;
using System.Collections.Generic;
using ARPG.Manager;
using Global.Utility;
using RPGCore.Buff.ConcreteBuff.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Skill.ConcreteSkill.Swordman
{
	[Serializable]
	public class Skill_Swordman_Chain_Slash_剑士连协入场 : BaseRPSkill
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

		
		
		[SerializeField, LabelText("搜索时 额外的不搜索类型们"),]
		[TitleGroup("===配置===")]
		public List<CharacterNamedTypeEnum> _notSearchEnemyTypes = new List<CharacterNamedTypeEnum>();


		[SerializeField,LabelText("吸附搜索范围")]
		[TitleGroup("===配置===")]
		public float _searchRange = 15f;





		protected override void BindingInput()
		{
			// base.BindingInput();
		}
		public override void ModifyRemainingCD(bool modifyOrReset, float modify)
		{
			base.ModifyRemainingCD(modifyOrReset, modify);
			GetBlockBuffRef().ResetAvailableTimeAs(RemainingCoolDownDuration);
		}

		public override bool _Internal_TryPrepareSkill()
		{
			if (!base._Internal_TryPrepareSkill())
			{
				return false;
			}
			//移动到最近位置

			var get = SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference
				.GetEnemyListInRange(_characterBehaviourRef.transform.position, _searchRange)
				.ClipEnemyListOnDefaultType().ClipEnemyListOnCharacterType(_notSearchEnemyTypes)
				.ClipEnemyListOnInvincibleBuff();
			ApplyDirectorInvincible();
			//施加无敌
			if(get.Count == 0)
			{
				return true;
			}
			//有角色可以移动，那就直接闪现！
			get.Shuffle();
			var pos = get[0].transform.position;
			var delta = pos -_characterBehaviourRef.transform.position;
			_characterBehaviourRef.TryMovePosition_XYZ(delta);
			
			
			

			return true;
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