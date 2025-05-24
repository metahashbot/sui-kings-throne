using System;
using Sirenix.OdinInspector;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	[Serializable]
	public class Buff_ThreatReduce : BaseRPBuff
	{

		[NonSerialized, LabelText("当前降低的威胁度"), FoldoutGroup("运行时", true)]
		public float CurrentTauntToThreatValue = 1;

	}
}