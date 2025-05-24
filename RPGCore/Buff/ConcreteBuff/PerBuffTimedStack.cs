using ARPG.Manager;
using ARPG.UI.Panel;
using Global;
using Global.ActionBus;
using RPGCore.Buff.ConcreteBuff.Common;
using Sirenix.OdinInspector;
namespace RPGCore.Buff.ConcreteBuff
{
	/// <summary>
	/// <para>抽象的，可叠层时【单个的】层</para>
	/// </summary>
	public abstract class PerBuffTimedStack : I_BuffContentMayDisplayOnUI
	{
		
		
		public string StackInfo;
		public BuffData_PerTypeStackInfoGroup RelatedStackInfoGroupRef;
		[ShowInInspector,LabelText("关联Buff"),FoldoutGroup("运行时",true)]
		public I_BuffContainTimedStack RelatedBuffRef;
		[ShowInInspector, LabelText("起始时间"), FoldoutGroup("运行时", true)]
		public float FromTime;

		[ShowInInspector,LabelText("生命周期"),FoldoutGroup("运行时",true)]
		public float InitDuration;
		
		
		

		public abstract void OnStackAdd();
		public virtual void OnStackRemove()
		{
			if (SelfBuffDisplayInfoField != null)
			{
				var ds_DisplayCancel =
					new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.G_UI_BuffDisplayContentClear_清理Buff显示内容);
				ds_DisplayCancel.ObjectArgu1 = this;
				ds_DisplayCancel.ObjectArgu2 = SelfBuffDisplayInfoField;
				GlobalActionBus.GetGlobalActionBus().TriggerActionByType(ds_DisplayCancel);
			}
			
		}

		/// <summary>
		/// <para>当当前层被屏蔽</para>
		/// <para>被屏蔽不等同于被移除(Remove)，也不是暂停。被屏蔽的依然有可能在屏蔽源消除之后重新恢复自身的作用</para>
		/// </summary>
		public abstract void OnStackBlock();

		/// <summary>
		/// <para>当前层从屏蔽中恢复</para>
		/// </summary>
		public abstract void OnStackRestore();

		public abstract void OnStackFixedTick(float ct, int cf, float delta);

		public ConcreteBuffDisplayOnUIInfo SelfBuffDisplayInfoField;
		private UIRW_PerPlayerBuffEntry _selfUIRW_BuffEntryRef;

		public ConcreteBuffDisplayOnUIInfo RelatedBuffDisplayOnUIInfo => SelfBuffDisplayInfoField;
		public UIRW_PerPlayerBuffEntry SelfUIRW_BuffEntryRef
		{
			get => _selfUIRW_BuffEntryRef;
			set =>
				_selfUIRW_BuffEntryRef = value;
		}
		public string UI_GetBuffContent_RemainingTimeText()
		{
			float remainTime = FromTime + InitDuration - BaseGameReferenceService.CurrentFixedTime;
			if (remainTime > 60f)
			{
				int remainTime_Minute = (int) (remainTime / 60f);
				int remainTime_Second = (int) (remainTime % 60f);
				return $"{remainTime_Minute :00}:{remainTime_Second :00}";
			}
			else
			{
				return $"0:{remainTime :00}";
			}
		}
		public string UI_GetBuffContent_NameText()
		{
			string b = GameReferenceService_ARPG.Instance.GetLocalizedStringByTableAndKey(
				LocalizationTableNameC._LCT_BuffContent,
				RelatedBuffDisplayOnUIInfo.NameKey);
			return b;
		}
		public int? UI_GetBuffContent_Stack()
		{
			return null;
		}
		public bool RelatedBehaviourDataValid()
		{
			return (RelatedBuffRef as BaseRPBuff).RelatedBehaviourDataValid();
		}
		public float GetRemainingPartial()
		{
			return (FromTime + InitDuration - BaseGameReferenceService.CurrentFixedTime) / InitDuration;
		}
		public bool IfNeedBlink()
		{
			float remainTime = FromTime + InitDuration - BaseGameReferenceService.CurrentFixedTime;
			float remainPartial = remainTime / InitDuration;
			if (remainPartial < 0.2f)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}