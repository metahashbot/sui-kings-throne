using ARPG.Character;
using RPGCore.Interface;
namespace RPGCore.Buff.ConcreteBuff
{
	
	/// <summary>
	/// <para>会在角色之间传递的buff。</para>
	/// <para>在切换角色的时候，由被切换方将buff连带Config塞给新的角色</para>
	/// </summary>
	public interface I_BuffTransferWithinPlayer 
	{
		
		
		/// <summary>
		/// <para>逻辑上的原始parent，也就是最开始这个buff是由谁上给自己的。</para>
		/// </summary>
		public I_RP_Buff_ObjectCanReceiveBuff OriginalParent { get; protected set; }
		

		/// <summary>
		/// <para>处理具体的转移。相当于重新初始化一下。在这里面通常要处理 监听的上下，重新播放特效，重新维持数据引用等</para>
		/// </summary>
		/// <param name="transferFrom"></param>
		/// <param name="newPlayer"></param>
		public void ProcessTransfer(I_BuffTransferWithinPlayer transferFrom, PlayerARPGConcreteCharacterBehaviour newPlayer);

	}
}