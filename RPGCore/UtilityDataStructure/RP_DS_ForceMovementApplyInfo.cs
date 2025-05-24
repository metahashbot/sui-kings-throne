using RPGCore.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.UtilityDataStructure
{
	/// <summary>
	/// <para>伤害流程中，用于提供强制位移的信息的数据结构</para>
	/// <para></para>
	/// </summary>
	public class RP_DS_ForceMovementApplyInfo
	{
		[ShowInInspector, ReadOnly]
		public I_RP_Damage_ObjectCanReceiveDamage DamageReceiver;
		[ShowInInspector, ReadOnly]
		public I_RP_Damage_ObjectCanApplyDamage DamageCaster;
		
		public Vector3 ForceMovementDirection;
		
		public float ForceMovementForceAmount;
		
		



	}
}