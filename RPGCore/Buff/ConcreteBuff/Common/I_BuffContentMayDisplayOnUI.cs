using System;
using ARPG.UI.Panel;
using Sirenix.OdinInspector;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	/// <summary>
	/// <para>表明这是一个可能会显示在UI上的Buff内容。不仅仅是Buff本身(BaseRPBuff)，Buff的组件(比如可叠层数据修饰的每层)也会继承这个用于显示。</para>
	/// </summary>
	public interface I_BuffContentMayDisplayOnUI
	{
		[ShowInInspector]
		public ConcreteBuffDisplayOnUIInfo RelatedBuffDisplayOnUIInfo { get; }
		public UIRW_PerPlayerBuffEntry SelfUIRW_BuffEntryRef { get; set; }
	
		/// <summary>
		/// <para>获取Buff的剩余时长</para>
		/// </summary>
		/// <returns></returns>
		public string UI_GetBuffContent_RemainingTimeText();
		/// <summary>
		/// <para>获取Buff的名字文本，剩余时长前面那个文本</para>
		/// </summary>
		/// <returns></returns>
		public string UI_GetBuffContent_NameText();

		public Nullable<int> UI_GetBuffContent_Stack();


		public abstract bool RelatedBehaviourDataValid();
		

		/// <summary>
		/// <para>获取剩余时间比例</para>
		/// </summary>
		/// <returns></returns>
		public float GetRemainingPartial();

		/// <summary>
		/// <para>是否需要闪烁提示了？</para>
		/// </summary>
		/// <returns></returns>
		public bool IfNeedBlink();
	}
}