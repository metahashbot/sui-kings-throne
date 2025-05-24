using ARPG.Character;
using RPGCore.Skill;
namespace RPGCore.Interface
{
	/// <summary>
	/// <para>需要对自己被OccupyInfo取消掉的事情做出响应</para>
	/// </summary>
	public interface I_RP_NeedReactToOccupy
	{

		public abstract FixedOccupiedCancelTypeEnum OnOccupiedCanceledByOther(
			DS_OccupationInfo occupySourceInfo,
			FixedOccupiedCancelTypeEnum cancelType = FixedOccupiedCancelTypeEnum.None_未指定,
			bool invokeBreakResult = true);


		/// <summary>
		/// 通用的退出技能效果。用于硬直断、弱断、抢断、机制其他打断、机制换人。
		/// 死亡断会调用这个，并由额外操作
		/// </summary>
		public abstract void BR_CommonExitEffect();

		public abstract void BreakResult_StiffnessBreak();

		public abstract void BreakResult_WeakBreak();

		public abstract void BreakResult_StrongBreak();

		public abstract void BreakResult_OtherBreak();

		public abstract void BreakResult_ContinueBreak();

		public abstract void BreakResult_CancelBreak();

		public abstract void BreakResult_SwitchCharacter();


		public abstract void BreakResult_DeathBreak();

	}

}