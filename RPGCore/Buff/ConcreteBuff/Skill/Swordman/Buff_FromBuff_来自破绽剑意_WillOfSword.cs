using System;
using System.Collections.Generic;
using Global;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Skill;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RPGCore.Buff.ConcreteBuff.Skill.Swordman
{
	/// <summary>
	/// 来自Buff的效果。对应buff——破绽
	/// </summary>
	[Serializable]
	public class Buff_FromBuff_来自破绽剑意_WillOfSword : BaseRPBuff
	{
		[SerializeField, LabelText("击破buff最大层数"), TitleGroup("===配置===")]
		private int _initMaxStack = 5;
		
		[NonSerialized, ShowInInspector, LabelText("当前最大层数"), FoldoutGroup("运行时", true)]
		public int CurrentMaxStack = 5;
		
		[ShowInInspector, LabelText("当前层数"), FoldoutGroup("运行时", true)]
		public int CurrentStackCount { get; protected set; }

		[SerializeField, LabelText("击破buff提供每层攻速"), TitleGroup("===配置===")]
		public float AttackSpeedPerStackPercentage = 10f;

		[SerializeField, LabelText("击破buff提供每层移速"), TitleGroup("===配置===")]
		public float MovementSpeedPerStackPercentage = 10f;
		
		[SerializeField, LabelText("常驻buff持续时间"), TitleGroup("===配置===")]
		public float ResidentDurationTime = 10f;
        
		[LabelText("满层持续时间"), SerializeField,TitleGroup("===配置==="), SuffixLabel("秒")]
		public float durationTime;
		
        [SerializeField, LabelText("满层提供攻速"), TitleGroup("===配置===")]
		public float AttackSpeedFullFloor = 10f;

		[SerializeField, LabelText("满层提供移速"), TitleGroup("===配置===")]
		public float MovementSpeedFullFloor = 10f;
		
		[SerializeField, LabelText("击破时回复最大蓝量百分比"), TitleGroup("===配置===")]
		public float BrokenReplySP = 0.1f;
		

		private Float_ModifyEntry_RPDataEntry _modify_AttackSpeed;
		private Float_RPDataEntry _entry_AttackSpeedRef;

		private Float_ModifyEntry_RPDataEntry _modify_MovementSpeed;
		private Float_RPDataEntry _entry_MovementSpeedRef;
        
		
		
		[SerializeField, LabelText("常驻特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		protected string _Vfx_WillOfSwordLoop;

		protected PerVFXInfo _vfxInfo_WillOfSwordLoop;
		
		[SerializeField, LabelText("满层特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		protected string _Vfx_Full_WillOfSwordLoop;

		protected PerVFXInfo _vfxInfo_Full_WillOfSwordLoop;
		
		[SerializeField, LabelText("回蓝特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		public string _vfx_ReplySP;

		private PerVFXInfo _vfxInfo_ReplySP;
		
		
		public enum EagleEyeBuildingStateTypeEnum
		{
			None_无事发生 = 0, Duration_持续中 = 1, Full_满层中 = 2,
		}

		[NonSerialized, LabelText("当前状态"), FoldoutGroup("运行时", true), TitleGroup("运行时/数值"), ShowInInspector, ReadOnly]
		protected  EagleEyeBuildingStateTypeEnum CurrentState =  EagleEyeBuildingStateTypeEnum.None_无事发生;
		        
		protected float _nextChangeStateTime;
        
        
		[NonSerialized, LabelText("当前满层吗")]
		public bool CurrentAbilityFull = true;
		
		[NonSerialized, LabelText("当前持续吗")]
		public bool CurrentAbilityAvailable = true;
		
		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			CurrentStackCount = 0;
		}




		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnBuffInitialized(blps);
			_entry_AttackSpeedRef =
				Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.AttackSpeed_攻击速度);
			_modify_AttackSpeed = _entry_AttackSpeedRef.AddDataEntryModifier(
				Float_ModifyEntry_RPDataEntry.GetNewFromPool(0f,
					RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
					ModifyEntry_CalculatePosition.FrontMul));
			_entry_MovementSpeedRef =
				Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.MoveSpeed_移速);
			_modify_MovementSpeed = _entry_MovementSpeedRef.AddDataEntryModifier(
				Float_ModifyEntry_RPDataEntry.GetNewFromPool(0f,
					RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
					ModifyEntry_CalculatePosition.FrontMul));

			return ds;
		}



		public void ResetWillOfSword()
		{
			//buff归0
			    CurrentStackCount = 0;
			    _vfxInfo_WillOfSwordLoop?.VFX_StopThis(true);
			    _vfxInfo_Full_WillOfSwordLoop?.VFX_StopThis(true);
				CurrentAbilityFull = false;
				CurrentAbilityAvailable = false;
				_modify_AttackSpeed.ModifyValue = AttackSpeedPerStackPercentage * CurrentStackCount;
				_entry_AttackSpeedRef.Recalculate();
				_modify_MovementSpeed.ModifyValue = MovementSpeedPerStackPercentage * CurrentStackCount;
				_entry_MovementSpeedRef.Recalculate();
		}
		public void AddWillOfSword(int i)
		{
			
			//回蓝
			float MaxSp = Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(RP_DataEntry_EnumType.SPMax_最大SP).CurrentValue;
			Parent_SelfBelongToObject.ReceiveBuff_GetFloatPresentValue(RP_DataEntry_EnumType.CurrentSP_当前SP)
				.AddDataEntryModifier(Float_ModifyEntry_RPDataEntry.GetNewFromPool(MaxSp * BrokenReplySP,RPDM_DataEntry_ModifyFrom.FromBuff_Buff,ModifyEntry_CalculatePosition.FrontAdd));
			_vfxInfo_ReplySP = _VFX_GetAndSetBeforePlay(_vfx_ReplySP, true)?._VFX__10_PlayThis(true,true);

			
			CurrentStackCount += i;
            CurrentStackCount = Mathf.Clamp(CurrentStackCount, 0, CurrentMaxStack);
            _vfxInfo_WillOfSwordLoop = _VFX_GetAndSetBeforePlay(_Vfx_WillOfSwordLoop)._VFX__10_PlayThis();
            CurrentAbilityAvailable = true;
            
			if (CurrentStackCount < CurrentMaxStack)
			{
				_modify_AttackSpeed.ModifyValue = AttackSpeedPerStackPercentage * CurrentStackCount;
				_entry_AttackSpeedRef.Recalculate();
				_modify_MovementSpeed.ModifyValue = MovementSpeedPerStackPercentage * CurrentStackCount;
				_entry_MovementSpeedRef.Recalculate();
				
				CurrentAbilityFull = false;
				CurrentState = EagleEyeBuildingStateTypeEnum.None_无事发生;
			}
			else if(CurrentStackCount >= CurrentMaxStack)
			{
				_vfxInfo_WillOfSwordLoop.VFX_StopThis(true);
				_vfxInfo_Full_WillOfSwordLoop = _VFX_GetAndSetBeforePlay(_Vfx_Full_WillOfSwordLoop )._VFX__10_PlayThis();
				
				_modify_AttackSpeed.ModifyValue = AttackSpeedFullFloor;
				_entry_AttackSpeedRef.Recalculate();
				_modify_MovementSpeed.ModifyValue = MovementSpeedFullFloor;
				_entry_MovementSpeedRef.Recalculate();
				CurrentAbilityFull = true;
				CurrentState = EagleEyeBuildingStateTypeEnum.Duration_持续中;
			}
			
			
		}
		
		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);
			switch (CurrentState)
			{
				case EagleEyeBuildingStateTypeEnum.None_无事发生:
					if (!CurrentAbilityAvailable)
					{
						break;
					}
					_nextChangeStateTime = currentTime - 1;
					if (currentTime > _nextChangeStateTime)
					{
						CurrentState = EagleEyeBuildingStateTypeEnum.Duration_持续中;
						_nextChangeStateTime = currentTime + durationTime;
					}
					break;
				case EagleEyeBuildingStateTypeEnum.Duration_持续中:
					if (currentTime > _nextChangeStateTime && !CurrentAbilityFull)
					{
						CurrentStackCount = 0;
						CurrentState = EagleEyeBuildingStateTypeEnum.None_无事发生;
						ResetWillOfSword();
					}
                    
					if (!CurrentAbilityFull)
					{
						break;
					}
					if (currentTime > _nextChangeStateTime)
					{
						CurrentState = EagleEyeBuildingStateTypeEnum.Full_满层中;
						_nextChangeStateTime = currentTime + durationTime;
					}
					break;
				case EagleEyeBuildingStateTypeEnum.Full_满层中:
					if (!CurrentAbilityFull)
					{
						break;
					}
					if (currentTime > _nextChangeStateTime)
					{
						CurrentStackCount = 0;
						CurrentState = EagleEyeBuildingStateTypeEnum.None_无事发生;
						ResetWillOfSword();
					}
					break;
			}
		}
		

	}
}