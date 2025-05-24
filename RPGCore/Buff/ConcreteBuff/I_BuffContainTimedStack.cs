using System;
using System.Collections.Generic;
using RPGCore.Buff.ConcreteBuff.Common;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff
{
	[TypeInfoBox("Buff包含计时的层数")]
	public interface I_BuffContainTimedStack
	{
		protected Dictionary<string, BuffData_PerTypeStackInfoGroup> SelfBuffContentDict { get; set; }
		public BuffData_PerTypeStackInfoGroup GetBuffStackByInfo(string uid);
		public virtual void FixedUpdateTick(float ct, int cf, float delta)
		{
			foreach (BuffData_PerTypeStackInfoGroup perStackGroup in SelfBuffContentDict.Values)	
			{
				for (int i = perStackGroup.StackContentList.Count - 1; i >= 0;i--)
				{
					var perStack = perStackGroup.StackContentList[i];
					perStack.OnStackFixedTick(ct, cf, delta);
					if (Mathf.Approximately(perStack.InitDuration, -1f))
					{
						continue;
					}
					if (ct > perStack.FromTime + perStack.InitDuration)
					{
						perStack.OnStackRemove();
						perStackGroup.StackContentList.RemoveAt(i);
					}
				}
			}

		}
		public void ReceiveAddStackByBCLP(List<BaseBuffLogicPassingComponent> blps);

	}





}