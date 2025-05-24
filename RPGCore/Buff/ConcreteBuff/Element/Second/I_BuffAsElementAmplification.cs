namespace RPGCore.Buff.ConcreteBuff.Element.Second
{
	/// <summary>
	/// <para>作为增幅反应的二级反应</para>
	/// </summary>
	public interface I_BuffAsElementAmplification
	{
		public float GetAmplificationResetDuration();


		public int GetCurrentStack();

		public void RefreshAmplificationTime()
		{
			var selfBuf = this as BaseRPBuff;
			selfBuf.ResetAvailableTimeAs(GetAmplificationResetDuration());
			selfBuf.ResetExistDurationAs(GetAmplificationResetDuration());
		}
	}
}