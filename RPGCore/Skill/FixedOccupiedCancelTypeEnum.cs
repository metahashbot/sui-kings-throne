namespace RPGCore.Skill
{
	public enum FixedOccupiedCancelTypeEnum
	{
		None_未指定 = 0,
		/// <summary>
		/// 出现于占用信息为“受击硬直”时取消，即 TryOccupyByOccupationInfo 时，发起占用方配置名包含“受击硬直”
		/// </summary>
		StiffnessBreak_硬直断 = 1,
		/// <summary>
		/// 出现于 被弱霸体抵抗的效果，比如 沉默|缴械
		/// </summary>
		WeakBreak_弱断 = 11,
		/// <summary>
		/// 出现于 被强霸体抵抗的效果，比如 失衡推拉 | 眩晕 | 冰冻
		/// 在内部逻辑上，虽然失衡推拉之类的依然使用了受击硬直的一些逻辑，但仅仅有单纯的“受击硬直”才会被分类为硬直断
		/// </summary>
		StrongBreak_强断 = 21,
		/// <summary>
		/// 返回Idle（对于Player来说，只要没有移动输入，则时刻都在试图返回idle，没有真的返回就是因为所有的动作都有占用级）
		/// 由 L_ARPGCharacter_TryRestoreAnimation_ARPG角色尝试恢复动画 试图返回idle（所有异常状态的buff被移除时都会调用）
		/// 对于武器来说，当接续的动画不是自己所关联的动画时，会将 接续 改写为 机制其他 。
		/// 对于技能来说，技能试图准备 、技能试图流转到多段脱手前摇 。当接续的动画不是自己所关联的动画时，会将 接续 改写为 机制其他 。
		/// </summary>
		Logic_OtherBreak_机制其他动画打断 = 32,
		/// <summary>
		/// 长idle打断idle，跑动停止打断跑动
		/// 技能和武器 前摇→中段、中段→后摇 的各个情况
		/// </summary>
		ContinueBreak_接续断 = 41,
		/// <summary>
		/// 目前所有技能不能取消
		/// </summary>
		CancelBreak_自主取消断 = 51,
		/// <summary>
		/// 换人入场、离场
		/// </summary>
		Logic_SwitchCharacter_机制换人断 = 100,
		/// <summary>
		/// 死亡离场
		/// </summary>
		DeathBreak_死亡断 = 101, 
	}

}