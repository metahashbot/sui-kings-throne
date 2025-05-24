using System;
using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision
{
	
	/// <summary>
	/// <para>用于在某些AI副作用中挑选决策时，实现[从若干个决策中(按权重地)随机]挑选一个决策</para>
	/// </summary>
	[Serializable]
	public class MultipleDecisionPickConfig
	{
		[Serializable]
		public class PerDecisionPickInfo
		{
			public string DecisionName;
			public float Weight;
		}
		[SerializeField, LabelText("需要挑选的决策"),ListDrawerSettings( ListElementLabelName = "DecisionName")]
		public List<PerDecisionPickInfo> PerDecisionPickInfos;




		public override string ToString()
		{
			if (PerDecisionPickInfos == null || PerDecisionPickInfos.Count == 0)
			{
				return "";
			}
			string s = "组{{";
			foreach (PerDecisionPickInfo info in PerDecisionPickInfos)
			{
				s += ($"决策: {info.DecisionName} 权: {info.Weight}    ");
			}
			s += "}}";
			return s;
		}
	}
}