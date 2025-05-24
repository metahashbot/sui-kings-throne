using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.UtilityDataStructure.BuffLogicPassingComponent
{
	[Serializable]
	public class BLP_简单层数_JustStackCount : BaseBuffLogicPassingComponent
	{

		[SerializeField, LabelText("层数,正负")]
		public int StackCount;



		public override void ReleaseOnReturnToPool()
		{
			GenericPool<BLP_简单层数_JustStackCount>.Release(this);
		}
	}
}