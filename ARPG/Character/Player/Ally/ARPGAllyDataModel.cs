using ARPG.Character.Base;
using RPGCore;
namespace ARPG.Character.Player.Ally
{
	public class ARPGAllyDataModel : BaseARPGDataModel
	{

		public ARPGAllyDataModel(RolePlay_BaseBehaviour behaviourReference) : base(behaviourReference)
		{
		}



		public override void ClearBeforeDestroy()
		{

			base.ClearBeforeDestroy();
		}
	}
}