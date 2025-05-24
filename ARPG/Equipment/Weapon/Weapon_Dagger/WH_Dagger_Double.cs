using System;
using System.Collections.Generic;
using ARPG.Character;
using Global;
using Global.ActionBus;
using RPGCore.Buff;
using RPGCore.Buff.ConcreteBuff.Common;
using RPGCore.Buff.ConcreteBuff.Skill.RedFang;
using RPGCore.DataEntry;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace ARPG.Equipment.Weapon.Weapon_Dagger
{
	[Serializable]
	public class WH_Dagger_Double : WeaponHandler_MultiAttack
	{

		[SerializeField, LabelText("背击时召唤蝙蝠概率"), SuffixLabel("%")]
		private float _batChanceOnBack = 10f;

		[SerializeField, LabelText("来自精神的增加上限")]
		private int _batChanceOnBackFromSpirit = 3;

		[SerializeField, LabelText("每个蝙蝠增加的移速百分比"), SuffixLabel("%")]
		private float _speedAdditionPerBat = 5f;
		
		[SerializeField, LabelText("关联版面ID")]
		private List<string> _relatedLayoutIDList = new List<string>();

		private Float_RPDataEntry _entry_MoveSpeed;
		private Float_RPDataEntry _entry_Spirit;
		private Float_ModifyEntry_RPDataEntry _modify_MoveSpeed;


		public override void InitializeOnInstantiate(
			PlayerARPGConcreteCharacterBehaviour behaviour,
			LocalActionBus lab,
			SOConfig_WeaponTemplate configRuntime,
			DamageTypeEnum damageType)
		{
			base.InitializeOnInstantiate(behaviour, lab, configRuntime, damageType);
			RelatedCharacterBehaviourRef.GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Caster_DamageTakenOnHP_对施加方伤害打到了HP上,
				_ABC_CheckAndAddBatBuff_OnTakeOnHp);

			_entry_Spirit = behaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.M_Spirit_主精神);
			_entry_MoveSpeed = behaviour.GetFloatDataEntryByType(RP_DataEntry_EnumType.MoveSpeed_移速);
			_modify_MoveSpeed = Float_ModifyEntry_RPDataEntry.GetNewFromPool(0f,
				RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
				ModifyEntry_CalculatePosition.FrontMul);
			_entry_MoveSpeed.AddDataEntryModifier(_modify_MoveSpeed);

			behaviour.ReleaseSkill_GetActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_Buff_OnBuffRefreshed_已存在Buff被刷新,
				_ABC_RefreshModifyOnMoveSpeed_OnBuffRefresh);
		}


		private void _ABC_RefreshModifyOnMoveSpeed_OnBuffRefresh(DS_ActionBusArguGroup ds)
		{
			var buff = ds.GetObj1AsT<BaseRPBuff>();
			if (buff.SelfBuffType == RolePlay_BuffTypeEnum.RedFang_蝙蝠_Bat)
			{
				//刷新移速
				_modify_MoveSpeed.ModifyValue =
					_speedAdditionPerBat * (buff as Buff_猩红蝙蝠_BatOfRedFang).GetCurrentBatAmount();
				_entry_MoveSpeed.Recalculate();
			}
		}



		private void _ABC_CheckAndAddBatBuff_OnTakeOnHp(DS_ActionBusArguGroup ds)
		{
			var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
			if (dar.DamageIsFromBack.HasValue && dar.DamageIsFromBack.Value)
			{
				//进行背击了！

				if (dar.RelatedProjectileRuntimeRef == null ||
				    dar.RelatedProjectileRuntimeRef.SelfLayoutConfigReference == null)
				{
					return;
				}
				if (!_relatedLayoutIDList.Contains(dar.RelatedProjectileRuntimeRef.SelfLayoutConfigReference
					.LayoutContentInSO.LayoutUID))
				{
					return;
				}
				//概率比对
				if (UnityEngine.Random.Range(0f, 1f) < _batChanceOnBack)
				{
					//召唤蝙蝠
					var spiritAmount = Mathf.CeilToInt(_entry_Spirit.CurrentValue % 10);
					spiritAmount = Mathf.Clamp(spiritAmount, 0, _batChanceOnBackFromSpirit);

					var blp_stack = GenericPool<BLP_简单层数_JustStackCount>.Get();
					blp_stack.StackCount = spiritAmount;
					var buff_bat = RelatedCharacterBehaviourRef.ReceiveBuff_TryApplyBuff(
						RolePlay_BuffTypeEnum.RedFang_蝙蝠_Bat,
						RelatedCharacterBehaviourRef,
						RelatedCharacterBehaviourRef,
						blp_stack);
					blp_stack.ReleaseOnReturnToPool();
				}
			}
		}
	}
}