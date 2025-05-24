using RPGCore.Interface;
namespace RPGCore.Buff
{
	public class RP_DS_BuffApplyResult
	{
		public RolePlay_BuffTypeEnum CurrentBuff;
		public I_RP_Buff_ObjectCanReceiveBuff Receiver;
		public I_RP_Buff_ObjectCanApplyBuff Caster;

		public bool BlockByOtherBuff = false;
		
		
		//reset all content
		public void Reset()
		{
			CurrentBuff = RolePlay_BuffTypeEnum.None;
			Receiver = null;
			Caster = null;
			BlockByOtherBuff = false;
		}
		
	}
}