using System;
using ARPG.Character;
using Global;
using Global.ActionBus;
using RPGCore.Buff;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

namespace RPGCore.Buff.ConcreteBuff
{
	public class Buff_龙炎值_DragonFlame : BaseRPBuff, I_BuffTransferWithinPlayer
	{
		private I_RP_Buff_ObjectCanReceiveBuff _originalParent;

		I_RP_Buff_ObjectCanReceiveBuff I_BuffTransferWithinPlayer.OriginalParent
		{
			get => _originalParent;
			set => _originalParent = value;
		}

		private float _currentFlameValue;

		[ShowInInspector, LabelText("当前龙炎值"), FoldoutGroup("运行时/数据")]
		public float CurrentFlameValue => _currentFlameValue;
		[SerializeField,LabelText("龙炎值最大值") ,TitleGroup("===具体配置===")]
		private float _maxFlameValue = 100f;
		
		[SerializeField, LabelText("每点龙炎值提供的火伤害百分比减少"), TitleGroup("===具体配置===")]
		private float _fireResistMultiplier = 0.01f;
		[FormerlySerializedAs("_attackPowerMutltiplier"), LabelText("每点龙炎值提供的攻击力百分比数值增加"), SerializeField,
		 TitleGroup("===具体配置===")]
		private float _attackPowerMultiplier = 0.02f;
		[SerializeField, LabelText("多少秒减少一点龙炎值"), TitleGroup("===具体配置===")]
		private float _autoReduceTime = 1f;

		private Float_RPDataEntry _entry_AttackPower;
		private Float_ModifyEntry_RPDataEntry _modify_AttackPower;

		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnBuffInitialized(blps);
			_entry_AttackPower =
				Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.AttackPower_攻击力);
			Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeRearMul_对接收方将要进行后乘最终算区计算,
				_ABC_ProcessFireDamageReduce_OnDamageFrontAddCalculate);
			_originalParent = Parent_SelfBelongToObject;
			_modify_AttackPower = Float_ModifyEntry_RPDataEntry.GetNewFromPool(
				_currentFlameValue * _attackPowerMultiplier,
				RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
				ModifyEntry_CalculatePosition.FrontMul);
			_entry_AttackPower.AddDataEntryModifier(_modify_AttackPower);

			_nextAutoReduceTime = BaseGameReferenceService.CurrentFixedTime + _autoReduceTime;
			return ds;
		}

		private void _ABC_ProcessFireDamageReduce_OnDamageFrontAddCalculate(DS_ActionBusArguGroup ds)
		{
			var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
			if (dar.DamageType == DamageTypeEnum.AoNengHuo_奥能火)
			{
				dar.CP_DamageAmount_DPart.MultiplyPart -= (_fireResistMultiplier /100f)* _currentFlameValue;
				//减掉的伤害值
			}
		}

		void I_BuffTransferWithinPlayer.ProcessTransfer(
			I_BuffTransferWithinPlayer transferFrom,
			PlayerARPGConcreteCharacterBehaviour newPlayer)
		{
			_originalParent.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeFrontAdd_对接收方将要进行前加攻防算区计算,
				_ABC_ProcessFireDamageReduce_OnDamageFrontAddCalculate);
			_entry_AttackPower.RemoveEntryModifier(_modify_AttackPower);
			_originalParent = newPlayer;
			newPlayer.GetRelatedActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeFrontAdd_对接收方将要进行前加攻防算区计算,
					_ABC_ProcessFireDamageReduce_OnDamageFrontAddCalculate);
			_entry_AttackPower = newPlayer.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.AttackPower_攻击力);
			_modify_AttackPower = Float_ModifyEntry_RPDataEntry.GetNewFromPool(
				_currentFlameValue * _attackPowerMultiplier,
				RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
				ModifyEntry_CalculatePosition.FrontAdd);
			_entry_AttackPower.AddDataEntryModifier(_modify_AttackPower);
			Parent_SelfBelongToObject = newPlayer;
			_Internal_RequireBuffDisplayContent();
		}

		private float _nextAutoReduceTime;

		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);

			if (currentTime > _nextAutoReduceTime)
			{
				_nextAutoReduceTime += _autoReduceTime;
				_currentFlameValue -= 1;
				Refresh();
			}
		}

		private void Refresh()
		{
			_currentFlameValue = Mathf.Clamp(_currentFlameValue, 0, _maxFlameValue);
			_modify_AttackPower.ModifyValue = _currentFlameValue * _attackPowerMultiplier;
			_entry_AttackPower.Recalculate();
		}

		protected override void ClearAndUnload()
		{
			base.ClearAndUnload();
			_originalParent.ReceiveBuff_GetRelatedActionBus().RemoveAction(
				ActionBus_ActionTypeEnum.L_DamageProcess_Receiver_BeforeFrontAdd_对接收方将要进行前加攻防算区计算,
				_ABC_ProcessFireDamageReduce_OnDamageFrontAddCalculate);
			_entry_AttackPower.RemoveEntryModifier(_modify_AttackPower);
		}



		public override int? UI_GetBuffContent_Stack()
		{
			return (int)_currentFlameValue;
		}


		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_龙炎值数值增加_AddFlameValue addFlameValue:
					_currentFlameValue += addFlameValue._addValue;
					Refresh();
					break;
			}
		}


		[Serializable]
		public class BLP_龙炎值数值增加_AddFlameValue : BaseBuffLogicPassingComponent
		{

			[SerializeField, LabelText("增加的量")]
			public float _addValue = 25f;


			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_龙炎值数值增加_AddFlameValue>.Release(this);
			}
		}
	}
}