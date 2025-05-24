using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Character.Player;
using ARPG.DropItem;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using Global.Utility;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Localization;
namespace RPGCore.Buff.ConcreteBuff.Skill.Swordman
{
	/// <summary>
	/// <para>监听L_Damage_OnDamagePreTakenOnHP_伤害将要计算到HP上之前 ，用来处理增加血球恢复量的效果</para>
	/// </summary>
	[Serializable]
	public class Buff_EagleEye : BaseRPBuff
	{


		[LabelText("警示时间"), FoldoutGroup("配置", true), SuffixLabel("秒")]
		public float WarningTime;


		[LabelText("间隔_上次效果结束到下次标记"), SerializeField, FoldoutGroup("配置", true), SuffixLabel("秒")]
		public float IntervalBetweenEffect;
		
		[SerializeField, LabelText("每次产生的标记数量"), TitleGroup("===数值===")]
		public int MarkCountToSpawn;
		public enum EagleEyeBuildingStateTypeEnum
		{
			None_无事发生 = 0, Interval_间隔中 = 1, Warning_警示中 = 2,
		}
		
		[NonSerialized, LabelText("当前状态"), FoldoutGroup("运行时", true), TitleGroup("运行时/数值"), ShowInInspector, ReadOnly]
        		protected  EagleEyeBuildingStateTypeEnum CurrentState =  EagleEyeBuildingStateTypeEnum.None_无事发生;
		        
		        protected float _nextChangeStateTime;
        
		        
		        [NonSerialized, LabelText("当前能力可用吗")]
		        public bool CurrentAbilityAvailable = true;
        


		        public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
		}
		
		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			EnableAbility();
			return base.OnBuffInitialized(blps);
		}
		
		protected virtual void EnableAbility()
		{
			CurrentAbilityAvailable = true;
			CurrentState = EagleEyeBuildingStateTypeEnum.Interval_间隔中;
		}
        
		protected virtual void DisableAbility()
		{
			if (CurrentAbilityAvailable)
			{
				CurrentAbilityAvailable = false;
			}
		}
		

	
		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{   
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);
            
			if (!ReferenceEquals(Parent_SelfBelongToObject, SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference.CurrentControllingBehaviour))
			{
				return;
            }
			
			
			if (!CurrentAbilityAvailable)
			{
				EnableAbility();
			}
			
			
			if (!CurrentAbilityAvailable)
			{
				return;
			}
			
			switch (CurrentState)
			{
				case EagleEyeBuildingStateTypeEnum.Interval_间隔中:

					if (currentTime > _nextChangeStateTime)
					{
						CurrentState = EagleEyeBuildingStateTypeEnum.Warning_警示中;
						_nextChangeStateTime = currentTime + WarningTime;
					}
					break;
				case EagleEyeBuildingStateTypeEnum.Warning_警示中:
					if (currentTime > _nextChangeStateTime)
					{
						CurrentState = EagleEyeBuildingStateTypeEnum.Interval_间隔中;
						_nextChangeStateTime = currentTime + IntervalBetweenEffect;
						ActivateEffect();
					}
					break;
			}
		}
		
		
		
		protected virtual void ActivateEffect()
		{
			//附加一次破绽
			if (!CurrentAbilityAvailable)
			{
				return;
			}
            
			var enemyList = SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference.GetAllEnemy()
				.ClipEnemyListOnDefaultType().ClipEnemyListOnInvincibleBuff();
			//如果enemy就是空的，就说明场上就没有合适的敌人，那之后直接return就行了
			if (enemyList == null || enemyList.Count == 0)
			{
				return;
			}
			for (int i = 0; i < MarkCountToSpawn; i++)
			{
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
				var currentPlayer = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference
					.CurrentControllingBehaviour;
				enemyList[0].ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.FromSkill_来自看破的破绽标记_WeaknessMarkFromSeeThrough,
					currentPlayer,
					enemyList[0],blp_newUID);
				GenericPool<BLP_一个用作UID的标记_OneUIDTag>.Release(blp_newUID);
			}
           
		}
		
		
		
	}
}