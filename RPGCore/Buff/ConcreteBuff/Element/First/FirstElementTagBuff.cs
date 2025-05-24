using System;
using System.Collections.Generic;
using DG.Tweening;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.Skill;
using RPGCore.Skill.Config;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.Element.First
{
	[Serializable]
	public abstract class FirstElementTagBuff : BaseRPBuff, I_BuffContainLoopVFX
	{

		[LabelText("配置 ：刷新标签时间")]
		[TitleGroup("===基本配置===")]
		[SerializeField]
		public float TagRefreshTime = 10f;

		[LabelText("配置：标签最大层数")]
		[TitleGroup("===基本配置===")]
		[SerializeField]
		public int TagMaxStackCount = 5;


		[NonSerialized, LabelText("当前层数"),ShowInInspector,FoldoutGroup("运行时"),TitleGroup("逻辑数据")]
		public int _currentTagStackCount;





		[SerializeField, LabelText("循环特效UID"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===基本配置===")]
		private string _vfxUid;
		private PerVFXInfo _relatedVFXInfo;
		[SerializeField, LabelText("特效停止时是立刻的吗？")]
		[TitleGroup("===基本配置===")]
		private bool _stopImmediate;




		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			//parent.ReceiveBuff_GetRelatedActionBus().RegisterAction(
			//	ActionBus_ActionTypeEnum.L_Buff_OnBuffInitialized_一个Buff被确定初始化了,
			//	_ABC_ProcessElementAffection);
			//parent.ReceiveBuff_GetRelatedActionBus()
			//	.RegisterAction(ActionBus_ActionTypeEnum.L_Buff_OnBuffRefreshed_已存在Buff被刷新,
			//		_ABC_ProcessElementAffection);


			_buffAvailableTime = TagRefreshTime;
			_buffDuration = TagRefreshTime;
			_currentTagStackCount = 1;
			ResetAvailableTimeAs(TagRefreshTime);
			ResetExistDurationAs(TagRefreshTime);

			(this as I_BuffContainLoopVFX).SpawnVFX();
		}


		protected virtual void _ABC_ProcessElementAffection(DS_ActionBusArguGroup ds)
		{
		}








		/// <summary>
		/// <para>被刷新其实就是加了一层</para>
		/// </summary>
		/// <param name="caster"></param>
		/// <param name="blps"></param>
		/// <returns></returns>
		public override DS_ActionBusArguGroup OnExistBuffRefreshed(
			I_RP_Buff_ObjectCanApplyBuff caster,
			List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnExistBuffRefreshed(caster, blps);
			//刷新等效增加一层
			ModifyStack(true, 1);
			return ds;
		}

		public virtual void ModifyStack(bool modifyOrOverride, int modify)
		{
			if (modifyOrOverride)
			{
				if (_currentTagStackCount >= TagMaxStackCount)
				{
					return;
				}
				_currentTagStackCount += modify;
				if (modify > 0)
				{
					ResetAvailableTimeAs(TagRefreshTime);
					ResetExistDurationAs(TagRefreshTime);
				}
				if (_currentTagStackCount >= TagMaxStackCount)
				{
					_currentTagStackCount = TagMaxStackCount;
				}
				if (_currentTagStackCount < 0)
				{
					_currentTagStackCount = 0;
					MarkAsRemoved = true;
					Parent_SelfBelongToObject.ReceiveBuff_RemoveTargetBuff(SelfBuffType);
				}
			}
			else
			{
				if (modify > _currentTagStackCount)
				{
					ResetAvailableTimeAs(TagRefreshTime);
					ResetExistDurationAs(TagRefreshTime);
					if (modify > TagMaxStackCount)
					{
						_currentTagStackCount = TagMaxStackCount;
					}
					else
					{
						_currentTagStackCount = modify;
					}
				}
				else
				{
					_currentTagStackCount = modify;
					if (_currentTagStackCount < 0)
					{
						_currentTagStackCount = 0;
						MarkAsRemoved = true;
						Parent_SelfBelongToObject.ReceiveBuff_RemoveTargetBuff(SelfBuffType);
						return;
					}
				}
			}
		}



		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			//Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
			//	ActionBus_ActionTypeEnum.L_Buff_OnBuffInitialized_一个Buff被确定初始化了,
			//	_ABC_ProcessElementAffection);
			//Parent_SelfBelongToObject.ReceiveBuff_GetRelatedActionBus().RemoveAction(
			//	ActionBus_ActionTypeEnum.L_Buff_OnBuffRefreshed_已存在Buff被刷新,
			//	_ABC_ProcessElementAffection);

			return base.OnBuffPreRemove();
		}


		string I_BuffContainLoopVFX.vfxUID
		{
			get => _vfxUid;
			set => _vfxUid = value;
		}
		PerVFXInfo I_BuffContainLoopVFX.relatedVFXInfo
		{
			get => _relatedVFXInfo;
			set => _relatedVFXInfo = value;
		}
		bool I_BuffContainLoopVFX.stopImmediate
		{
			get => _stopImmediate;
			set => _stopImmediate = value;
		}

		public override string ToString()
		{
			return $"当前层数：{_currentTagStackCount}，最大层数{TagMaxStackCount}";
		}
	}
}