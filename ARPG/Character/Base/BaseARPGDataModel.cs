using RPGCore;
namespace ARPG.Character.Base
{
	public class BaseARPGDataModel : RolePlay_DataModelBase
	{

		public BaseARPGDataModel(RolePlay_BaseBehaviour behaviourReference) : base(behaviourReference)
		{
			
		}
		
		
		public override void ClearBeforeDestroy()
		{
			base.ClearBeforeDestroy();
		}
	}
}