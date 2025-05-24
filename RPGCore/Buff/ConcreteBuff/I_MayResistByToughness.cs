using RPGCore.DataEntry;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff
{
	[TypeInfoBox("接口——该buff的持续时间和重设事件将受韧性影响而等比缩放\n" +
	             "作用于 ： Init时的时间设置  &  BLP 时间设置")]
	
	public interface I_MayResistByToughness  
	{
		
		
		public float GetFinalValueResistByToughness(float originValue)
		{
			var buff_original = this as BaseRPBuff;
			if (buff_original == null)
			{
				return originValue;
			}
			var toughnessEntry =
				buff_original.Parent_SelfBelongToObject.ReceiveBuff_GetFloatDataEntry(
					RP_DataEntry_EnumType.Toughness_韧性);
			var toughnessValue = toughnessEntry.CurrentValue;
			toughnessValue = Mathf.Clamp(toughnessValue, 0f, 100f);
			originValue = originValue * (1f - toughnessValue / 100f);
			return originValue;
		}
		
	}
}