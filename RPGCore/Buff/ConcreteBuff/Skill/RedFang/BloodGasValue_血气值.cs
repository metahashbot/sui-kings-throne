using System;
using System.Collections.Generic;
using Global;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Projectile;
using RPGCore.Skill;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RPGCore.Buff.ConcreteBuff.Skill.RedFang
{
	
	[Serializable]
	public class BloodGasValue_血气值 : BaseRPBuff
	{
		[SerializeField, LabelText("血气最大值-满后转换"), TitleGroup("===具体配置===")]
		private float _initMaxStack = 100;
		
		[ ShowInInspector, LabelText("当前最大值"), FoldoutGroup("运行时", true)]
		public float CurrentMaxStack { get; private set; }

		[ ShowInInspector, LabelText("当前值"), FoldoutGroup("运行时", true)]
		public float CurrentStackCount { get; private set; }
		
		protected PerVFXInfo _vfxInfo_Full_BloodGasPrompt;



		[SerializeField, LabelText("每帧移动增加的血气量")]
		[TitleGroup("===具体配置===")]
		private float _add_PerFrameMove = 0.01f;

		[SerializeField, LabelText("普攻命中 -增加的血气量")]
		[TitleGroup("===具体配置===")]
		private float _add_perNormalAttackHit = 5f;

		[SerializeField, LabelText("计为普攻的版面ID们")]
		[TitleGroup("===具体配置===")]
		private List<string> _layoutUIDAsNormalAttack = new List<string>();
		
		[SerializeField,LabelText("技能命中 -增加的血气量")]
		[TitleGroup("===具体配置===")]
		private float _add_perSkillHit = 10f;
		[SerializeField, LabelText("计为技能的版面ID们")]
		[TitleGroup("===具体配置===")]
		private List<string> _layoutUIDAsSkillAttack = new List<string>();


		[SerializeField, LabelText("成功斩杀 -增加的血气量")]
		[TitleGroup("===具体配置===")]
		private float _add_eliminate = 30f;
		 

		//内部用来检查伤害判定的投射物列表，每隔5s会清理一次
		private List<ProjectileBehaviour_Runtime> _internal_checkProjectileList =
			new List<ProjectileBehaviour_Runtime>();
		private float _nextRefreshProjectileListTime;
		
		[SerializeField,LabelText("血球拾取 —— 增加的血气量")]
		[TitleGroup("===具体配置===")]
		private float _add_perBloodBallPickUp = 10f;


		private Vector3 _lastPosition;
		private float _validMovementDeltaSqr = 0.0004f;
		
		
			
		public enum BloodGasBuildingStateTypeEnum
		{
			None_无事发生 = 0, Duration_持续中 = 1, 
		}

		[NonSerialized, LabelText("当前状态"), FoldoutGroup("运行时", true),ShowInInspector, ReadOnly]
		protected  BloodGasBuildingStateTypeEnum CurrentState =  BloodGasBuildingStateTypeEnum.None_无事发生;
        
		[NonSerialized, LabelText("当前满层吗")]
		public bool CurrentAbilityAvailable = false;
        
		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			CurrentStackCount = 0;
			CurrentMaxStack = _initMaxStack;
			parent.ReceiveBuff_GetRelatedActionBus()
				.RegisterAction(ActionBus_ActionTypeEnum.L_DamageProcess_Caster_DamageTakenOnHP_对施加方伤害打到了HP上,
					_ABC_AddStack_OnTakenDamage);
			parent.ReceiveBuff_GetRelatedActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.L_Damage_ToCaster_ResultToEliminateSlash_施加方_造成了斩杀效果,
				_ABC_AddStack_OnEliminate);
		}



		public override DS_ActionBusArguGroup OnExistBuffRefreshed(I_RP_Buff_ObjectCanApplyBuff caster, List<BaseBuffLogicPassingComponent> blps)
		{
			_Internal_RequireBuffDisplayContent();
			return base.OnExistBuffRefreshed(caster, blps);
		}

		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnBuffInitialized(blps);
			return ds;
		}



		private void _ABC_AddStack_OnTakenDamage(DS_ActionBusArguGroup ds)
		{
			if (MarkAsRemoved)
			{
				return;
			}
			var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
			if (!ReferenceEquals(dar.Caster, Parent_SelfBelongToObject))
			{
				return;
			}
			if (dar.RelatedProjectileRuntimeRef == null)
			{
				return;
			}
			if (_internal_checkProjectileList.Contains(dar.RelatedProjectileRuntimeRef))
			{
				return;
			}
			var layoutUID = dar.RelatedProjectileRuntimeRef.SelfLayoutConfigReference.LayoutContentInSO.LayoutUID;
			if(_layoutUIDAsNormalAttack.Contains(layoutUID))
			{
				AddStack(_add_perNormalAttackHit);
			}
			else if(_layoutUIDAsSkillAttack.Contains(layoutUID))
			{
				AddStack(_add_perSkillHit);
			}
		}


		private void _ABC_AddStack_OnEliminate(DS_ActionBusArguGroup ds)
		{
			if (MarkAsRemoved)
			{
				return;
			}
			var dar = ds.GetObj1AsT<RP_DS_DamageApplyResult>();
			if (!ReferenceEquals(dar.Caster, Parent_SelfBelongToObject))
			{
				return;
			}
			if (dar.RelatedProjectileRuntimeRef == null)
			{
				return;
			}
			AddStack(_add_eliminate);



		}


		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);
			var currentPosition = Parent_SelfBelongToObject.GetBuffReceiverPosition();
			if (Vector3.SqrMagnitude(currentPosition - _lastPosition) > _validMovementDeltaSqr)
			{
				AddStack(_add_PerFrameMove);
			}
			_lastPosition = currentPosition;

			if (currentTime > (_nextRefreshProjectileListTime))
			{
				_internal_checkProjectileList.Clear();
				_nextRefreshProjectileListTime = currentTime + 5f;
			}
		}

		public void AddStack(float add)
		{
			if (MarkAsRemoved)
			{
				return;
			}
			CurrentStackCount += add;
			if (CurrentStackCount >= CurrentMaxStack)
			{
				MarkAsRemoved = true;
				Parent_SelfBelongToObject.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.RedFang_BloodOverflow_血气已满,
					Parent_SelfBelongToObject as I_RP_Buff_ObjectCanApplyBuff,
					Parent_SelfBelongToObject);
			}
		}


		public override int? UI_GetBuffContent_Stack()
		{
			return Mathf.CeilToInt(CurrentStackCount);
		}


		public override string UI_GetBuffContent_RemainingTimeText()
		{
			return "";
		}
	}

}