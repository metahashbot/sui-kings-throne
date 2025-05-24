using RPGCore.DataEntry;
using RPGCore.UtilityDataStructure;
namespace RPGCore.Buff.ConcreteBuff
{
		
	public class RP_DS_StackApplyResult
	{
		public RP_DataEntry_EnumType DataType;
		public BaseBuffLogicPassingComponent RelatedBLP;
		public string UID;
		public RP_BuffInternalFunctionFlagTypeEnum InternalFunctionFlagType;
		public bool BlockByStoic ;


		public void Reset()
		{
			DataType = RP_DataEntry_EnumType.None;
			RelatedBLP = null;
			UID = null;
			InternalFunctionFlagType = RP_BuffInternalFunctionFlagTypeEnum.None;
			BlockByStoic = false;
		}
	}
}