using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using ARPG.Character.Enemy;
using ARPG.Manager;
using GameplayEvent;
using GameplayEvent.SO;
using Global;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Buff.Requirement;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.SpecificCharacter
{
	/// <summary>
	/// <para>被打一下会刷一波怪，场上有其他敌人的时候自身无敌，只剩自己的时候不无敌。</para>
	/// <para>触发什么事件通常是BLP传过来的</para>
	/// <para>用于森林场景的大紫花</para>
	/// </summary>
	[Serializable]
	public class Buff_ForestBigPurpleFlower_森林大紫花 : BaseRPBuff
	{



		private Float_RPDataEntry _entry_currentHP;
		private Float_RPDataEntry _entry_MaxHP;


		private EnemyARPGCharacterBehaviour _selfBehaviourRef;
		
		[SerializeField, LabelText("常驻特效UID"), GUIColor(187f / 255f, 1f, 0f)] [FoldoutGroup("配置/VFX", true)]
		protected string _vfx_charge;
        
		protected PerVFXInfo _vfxInfo_Charge;
		
		
		[SerializeField, LabelText("爆炸特效UID"), GUIColor(187f / 255f, 1f, 0f)] [FoldoutGroup("配置/VFX", true)]
		protected string _vfx_release;
		
		protected PerVFXInfo _vfxInfo_Release;
		
		[SerializeField, LabelText("回收特效UID"), GUIColor(187f / 255f, 1f, 0f)] [FoldoutGroup("配置/VFX", true)]
		protected string _vfx_reclaim;

		protected bool reclaim_flag = false;
		protected PerVFXInfo _vfxInfo_Reclaim;	
		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			_entry_currentHP = parent.ReceiveBuff_GetFloatPresentValue(RP_DataEntry_EnumType.CurrentHP_当前HP);
			_entry_MaxHP = parent.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.HPMax_最大HP);
			_selfBehaviourRef = parent as EnemyARPGCharacterBehaviour;
			var gab = GlobalActionBus.GetGlobalActionBus();
			gab.RegisterAction(ActionBus_ActionTypeEnum.G_CharacterOnMap_SpawnNewEnemy_生成了一个新敌人,
				_ABC_CheckAndModifyInvincibleState_OnEnemySpawnOrDestroyed);
			gab.RegisterAction(ActionBus_ActionTypeEnum.G_CharacterOnMap_OnCharacterBehaviourPreDestroyed_当一个角色行为将要被销毁了,
				_ABC_CheckAndModifyInvincibleState_OnEnemySpawnOrDestroyed);
			_vfxInfo_Charge = _VFX_GetAndSetBeforePlay(_vfx_charge)?._VFX__10_PlayThis();
			_selfBehaviourRef.SelfRelatedHUD.SetUIRWHeight(4.5f);



		}

		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			return base.OnBuffInitialized(blps);
		}



		private void _ABC_CheckAndModifyInvincibleState_OnEnemySpawnOrDestroyed(DS_ActionBusArguGroup ds)
		{
			var com = SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference
				.CurrentAllActiveARPGCharacterBehaviourCollection;
			var checkR = _selfBehaviourRef.ReceiveBuff_CheckTargetBuff(RolePlay_BuffTypeEnum.Invincible_All_WD_完全无敌);
			int notSelfEnemyAndNotVoidEntity = 0;
			bool applied = false;
			foreach (BaseARPGCharacterBehaviour perBehaviour in com)
			{
				if (perBehaviour is EnemyARPGCharacterBehaviour enemy)
				{
					if (enemy != _selfBehaviourRef && enemy.SelfBehaviourNamedType != CharacterNamedTypeEnum.Utility_VoidEntity_空实体)
					{
						notSelfEnemyAndNotVoidEntity += 1;
						if (!applied)
						{
							//有一个敌人不是自己，那就试图添加无敌Buff
							if (checkR == BuffAvailableType.NotExist) 
							{
								_vfxInfo_Charge?.VFX_StopThis(true);
								_vfxInfo_Release = _VFX_GetAndSetBeforePlay(_vfx_release)?._VFX__10_PlayThis();
								_vfxInfo_Reclaim?.VFX_StopThis(true);	
								applied = true;
								reclaim_flag = true;
								_selfBehaviourRef.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.Invincible_All_WD_完全无敌,
									_selfBehaviourRef,
									_selfBehaviourRef);
							}
						}
					}
				}
			}
			//没有不是自己的敌人，那可以移除无敌Buff
			if (notSelfEnemyAndNotVoidEntity == 0)
			{
				_selfBehaviourRef.ReceiveBuff_RemoveTargetBuff(RolePlay_BuffTypeEnum.Invincible_All_WD_完全无敌);
				_vfxInfo_Charge?._VFX__10_PlayThis();
				if(reclaim_flag)
				{
					_vfxInfo_Reclaim = _VFX_GetAndSetBeforePlay(_vfx_reclaim)?._VFX__10_PlayThis();
					reclaim_flag = false;
				}
				_vfxInfo_Release?.VFX_StopThis(true);	
				
				
			}
		}




		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			var gab = GlobalActionBus.GetGlobalActionBus();
			gab.RemoveAction(ActionBus_ActionTypeEnum.G_CharacterOnMap_SpawnNewEnemy_生成了一个新敌人,
				_ABC_CheckAndModifyInvincibleState_OnEnemySpawnOrDestroyed);
			gab.RemoveAction(ActionBus_ActionTypeEnum.G_CharacterOnMap_OnCharacterBehaviourPreDestroyed_当一个角色行为将要被销毁了,
				_ABC_CheckAndModifyInvincibleState_OnEnemySpawnOrDestroyed);
			return base.OnBuffPreRemove();
		}
	}
}