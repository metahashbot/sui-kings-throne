using Global;
using RPGCore.DataEntry;
using Sirenix.OdinInspector;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff
{
	/// <summary>
	/// <para>每隔一段时间对PresentValue进行修饰</para>
	/// </summary>
	public class BuffTimedStack_WithPresentValueModifyAndInterval : PerBuffTimedStack
	{
		[ShowInInspector, LabelText("关联PresentValue"), FoldoutGroup("运行时")]
		public FloatPresentValue_RPDataEntry ModifyPresentValue;


		[ShowInInspector, LabelText("作用间隔"), FoldoutGroup("运行时")]
		public float ApplyInterval;

		[ShowInInspector, LabelText("下次作用的时间点"), FoldoutGroup("运行时")]
		public float NextApplyTime;

		[ShowInInspector, LabelText("修饰数值"), FoldoutGroup("运行时")]
		public float ModifyValue;

		[ShowInInspector, LabelText("计算位置"), FoldoutGroup("运行时")]
		public ModifyEntry_CalculatePosition ModifyPosition;


		[ShowInInspector, LabelText("当前是否生效"), FoldoutGroup("运行时")]
		public bool CurrentActive;

		public static BuffTimedStack_WithPresentValueModifyAndInterval GetFromPool()
		{
			var get = GenericPool<BuffTimedStack_WithPresentValueModifyAndInterval>.Get();

			get.CurrentActive = true;
			return get;
		}




		/// <summary>
		/// <para>会在刚刚添加的时候应用一次，之后就在Tick里面应用</para>
		/// </summary>
		public override void OnStackAdd()
		{
			ProcessEffectApply();
		}


		private void ProcessEffectApply()
		{
			if (!CurrentActive)
			{
				return;
			}
			NextApplyTime = BaseGameReferenceService.CurrentFixedTime + ApplyInterval;

			ModifyPresentValue.AddDataEntryModifier(Float_ModifyEntry_RPDataEntry.GetNewFromPool(ModifyValue,
				RPDM_DataEntry_ModifyFrom.FromBuff_Buff,
				ModifyPosition,
				this));
		}


		public override void OnStackRemove()
		{
			base.OnStackRemove();
		}
		public override void OnStackBlock()
		{
			CurrentActive = false;
		}
		public override void OnStackRestore()
		{
			CurrentActive = true;
		}
		public override void OnStackFixedTick(float ct, int cf, float delta)
		{
			if (ct > NextApplyTime)
			{
				ProcessEffectApply();
			}
		}

		public override string ToString()
		{
			string s =
				$"目标修饰项:{ModifyPresentValue.RP_DataEntryType},修饰值{ModifyPresentValue},计算位置:{ModifyPosition}；修饰间隔:{ApplyInterval}," +
				$"起始时间{FromTime},生命周期{InitDuration}";
			return s;
		}
	}
}