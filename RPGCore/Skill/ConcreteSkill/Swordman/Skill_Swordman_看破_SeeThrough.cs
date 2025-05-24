using System;
using System.Collections.Generic;
using System.Linq;
using ARPG.Character;
using ARPG.Character.Base;
using ARPG.Character.Enemy;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using Global.Utility;
using RPGCore.Buff.ConcreteBuff.Skill.Swordman;
using RPGCore.Interface;
using RPGCore.Skill.Config;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace RPGCore.Skill.ConcreteSkill.Swordman
{
	[Serializable]
	public class Skill_Swordman_看破_SeeThrough : BaseRPSkill
	{

		[SerializeField, LabelText("每次产生的标记数量"), TitleGroup("===数值===")]
		public int MarkCountToSpawn;

		[SerializeField, LabelText("标记产生间隔时间"), TitleGroup("===数值===")]
		protected float MarkIntervalTime;
        

		
		
		protected float _nextMarkTime;
		
		[SerializeField, LabelText("不会添加到的角色枚举们 "), TitleGroup("===数值===")]
		public List<CharacterNamedTypeEnum> IgnoreCharacterTypeList = new List<CharacterNamedTypeEnum>
		{
			CharacterNamedTypeEnum.Utility_VoidEntity_空实体,
		};

		protected int CurrentMarkCount;

		private Buff_FromSkill_来自技能剑心_HeartOfSword _bonusBuffRef;



		public override void InitOnObtain(
			RPSkill_SkillHolder skillHolderRef,
			SOConfig_RPSkill configRuntimeInstance,
			I_RP_ObjectCanReleaseSkill parent,
			SkillSlotTypeEnum slot)
		{
            
			base.InitOnObtain(skillHolderRef, configRuntimeInstance, parent, slot);
			_characterBehaviourRef.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.EagleEye_鹰眼,
            				_characterBehaviourRef,
            				_characterBehaviourRef);
			_characterBehaviourRef.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.Buff_FromBuff_来自破绽剑意_WillOfSword,
				_characterBehaviourRef, _characterBehaviourRef);
			CurrentMarkCount = MarkCountToSpawn;

		}


		

		protected override void _IC_ReceiveSkillInput_Start(InputAction.CallbackContext context)
		{
			base._IC_ReceiveSkillInput_Start(context);
		}


		public override bool _Internal_TryPrepareSkill()
		{
			if (!base._Internal_TryPrepareSkill())
			{
				return false;
			}
			else
			{
				ApplyStrongStoic();
				return true;
			}
		}






		protected override void _InternalProgress_Post_CompleteOn_Post()
		{
			base._InternalProgress_Post_CompleteOn_Post();
			RemoveStrongStoic();
		}

		


		protected override void _InternalSkillEffect_SkillDefaultTakeEffect()
		{
			base._InternalSkillEffect_SkillDefaultTakeEffect();
			//撒标签，上buff


			_characterBehaviourRef.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.FromSkill_剑心_HeartOfSword,
				_characterBehaviourRef,
				_characterBehaviourRef);
			_bonusBuffRef =
				_characterBehaviourRef.ReceiveBuff_GetTargetBuff(RolePlay_BuffTypeEnum.FromSkill_剑心_HeartOfSword) as
					Buff_FromSkill_来自技能剑心_HeartOfSword;

			_nextMarkTime = BaseGameReferenceService.CurrentFixedTime;
			CurrentMarkCount = 0;

		}

		public override void FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FixedUpdateTick(currentTime, currentFrameCount, delta);
			if (CurrentMarkCount < MarkCountToSpawn &&  currentTime > _nextMarkTime)
			{
				_nextMarkTime = MarkIntervalTime + currentTime;
				CurrentMarkCount += 1; 
				var enemyList = SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference.GetAllEnemy()
					.ClipEnemyListOnDefaultType().ClipEnemyListOnInvincibleBuff();
				//如果enemy就是空的，就说明场上就没有合适的敌人，那之后直接return就行了
				if (enemyList == null || enemyList.Count == 0)
				{
					return;
				}
				enemyList.Shuffle();
				var blp_newUID = GenericPool<BLP_一个用作UID的标记_OneUIDTag>.Get();
				switch (GetCurrentDamageType())
				{
					case DamageTypeEnum.YuanNengGuang_源能光:
						blp_newUID.UID = "光破绽";
						break;
					case DamageTypeEnum.YuanNengDian_源能电:
						blp_newUID.UID = "电破绽";
						break;
				}
				enemyList[0].ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.FromSkill_来自看破的破绽标记_WeaknessMarkFromSeeThrough,
					_characterBehaviourRef,
					enemyList[0],blp_newUID);
				GenericPool<BLP_一个用作UID的标记_OneUIDTag>.Release(blp_newUID);
				
				
			}
            
		}



	}
}