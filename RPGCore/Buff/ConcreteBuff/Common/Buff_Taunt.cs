using System;
using Sirenix.OdinInspector;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	[Serializable]
	[TypeInfoBox("嘲讽")]
	public class Buff_Taunt : BaseRPBuff
	{

		[NonSerialized,LabelText("当前嘲讽提供的威胁度"),FoldoutGroup("运行时",true)]
		public float CurrentTauntToThreatValue = 1;

		[NonSerialized, LabelText("当前嘲讽作用范围"), FoldoutGroup("运行时", true)]
		public float CurrentTauntRange = 5f;



	}
}