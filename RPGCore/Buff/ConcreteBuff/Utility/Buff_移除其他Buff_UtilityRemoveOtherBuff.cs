using System;
using System.Collections.Generic;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.Utility
{
	[Serializable]
	public class Buff_移除其他Buff_UtilityRemoveOtherBuff : BaseRPBuff
	{



		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_移除其他Buff_UtilityRemoveOtherBuff blpRemoveOtherBuff:
				{
					foreach (var buffType in blpRemoveOtherBuff._listOfBuffTypeToBeRemoved)
					{
						if (Parent_SelfBelongToObject.ReceiveBuff_CheckTargetBuff(buffType) !=
						    BuffAvailableType.NotExist)
						{
							Parent_SelfBelongToObject.ReceiveBuff_RemoveTargetBuff(buffType);
						}
					}
					break;
				}
			}
		}




		[Serializable]
		public class BLP_移除其他Buff_UtilityRemoveOtherBuff : BaseBuffLogicPassingComponent
		{
			[SerializeField,LabelText("将要移除的Buff们")]
			public List<RolePlay_BuffTypeEnum> _listOfBuffTypeToBeRemoved;
			
			
			
			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_移除其他Buff_UtilityRemoveOtherBuff>.Release(this);
			}
		}
		
	}
}