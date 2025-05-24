namespace RPGCore.Skill
{
	public enum SkillReadyTypeEnum
	{
		None,
		/// <summary>
		/// <para>可用</para>
		/// </summary>
		Ready,
		/// <summary>
		/// <para>被buff阻挡。比如沉默、缴械。</para>
		/// </summary>
		BlockByBuff,
		/// <summary>
		/// <para>被数据项要求阻挡，即 不满足目标数据项要求。体力不足也是这个</para>
		/// </summary>
		BlockByDataEntry,

		/// <summary>
		/// <para>CD没转好</para>
		/// </summary>
		BlockByCD,
		
		Using_还在使用 ,

	}
}