using System;
using System.Collections.Generic;
namespace RPGCore.Buff.ConcreteBuff.Common
{
	/// <summary>
	/// <para>每【组】具体的可叠层内容。叠在这里面而不是很多个Buff实例。</para>
	/// <para>通过UID匹配。</para>
	/// </summary>
	public class BuffData_PerTypeStackInfoGroup
	{
		public string StackUID;
		/// <summary>
		/// <para>当前最大层数。达到这个层数之后，再试图添加会直接返回</para>
		/// </summary>
		public int CurrentMaxStack;
		public List<PerBuffTimedStack> StackContentList;


		public void ClearStack()
		{
			for (int i = StackContentList.Count - 1; i >= 0; i--)
			{
				var tmp = StackContentList[i];
				tmp.OnStackRemove();
			}
			StackContentList.Clear();
		}

		
		public void BlockThisType()
		{
			foreach (PerBuffTimedStack perBT in StackContentList)
			{
				perBT.OnStackBlock();
			}
		}

		public void RestoreThisType()
		{
			foreach (PerBuffTimedStack perBT in StackContentList)
			{
				perBT.OnStackRestore();
			}
			
		}
		
		
		
		

		public override string ToString()
		{
			string n = $"组UID:{StackUID}，【当前层数】:{StackContentList.Count}，【最大层数】:{CurrentMaxStack}\n";
			foreach (PerBuffTimedStack perStack in StackContentList)
			{
				switch (perStack)
				{
					case SingleBuffTimedStack_WithDataModifier buffTimedStackWithDataModifier:
						n += $" 层:对数据项修饰:{buffTimedStackWithDataModifier.ToString()}\n";
						break;
					case BuffTimedStack_WithPresentValueModifyAndInterval buffTimedStackWithPresentValueModifyAndInterval:
						n += $" 层:对表现值按间隔修饰:{buffTimedStackWithPresentValueModifyAndInterval.ToString()}\n";
						break;
				}
			}
			
			
			return n;
		}
	}

}