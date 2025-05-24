using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Common
{
	/// <summary>
	/// 玩家自动吸附的运行时信息。用于技能和武器上。
	/// <para>由配置转换而来。PAEC可以调整吸附的参数。</para>
	/// </summary>
	public class PlayerAutoAbsorbRuntimeInfo
	{
		public bool CurrentAbsorbActive;
		public bool IncludeDistanceLimit = true;
		public float AbsorbDistance = 0.5f;


		public bool IncludeAngleLimit = false;
		public float AngleLimit = 45f;
		
		
		
		public PlayerAutoAbsorbConfig.AbsorbSortTypeEnum AbsorbSortType =
			PlayerAutoAbsorbConfig.AbsorbSortTypeEnum.Distance_按距离排序;

		public List<CharacterNamedTypeEnum> ExcludeAbsorbTypes;
		public void ReceiveNewConfig(PlayerAutoAbsorbConfig newConfig)
		{
			CurrentAbsorbActive = newConfig.ToggleTo;
		
			IncludeDistanceLimit = newConfig.IncludeDistanceLimit;
			AbsorbDistance = newConfig.AbsorbDistance;
			// IncludeAngleLimit = newConfig.IncludeAngleLimit;
			// AngleLimit = newConfig.AngleLimit;
			 
			ExcludeAbsorbTypes = newConfig.ExcludeAbsorbTypes;
		}


	}


	[Serializable]
	public class PlayerAutoAbsorbConfig
	{
		[SerializeField, LabelText("调整为：")]
		public bool ToggleTo;

		[Sirenix.OdinInspector.ShowIf("@this.ToggleTo")]
		[LabelText("包含距离限制")]
		public bool IncludeDistanceLimit = true;

		[Sirenix.OdinInspector.ShowIf("@(this.ToggleTo && this.IncludeDistanceLimit)")]
		[LabelText("吸附距离限制")]
		public float AbsorbDistance = 1f;
		
		// [Sirenix.OdinInspector.ShowIf(nameof(ToggleTo))]
		// [LabelText("包含角度限制")]
		// public bool IncludeAngleLimit = false;
		//
		// [Sirenix.OdinInspector.ShowIf("@(this.ToggleTo && this.IncludeAngleLimit)")]
		// [LabelText("角度限制")]
		// public float AngleLimit = 45f;


		[Sirenix.OdinInspector.ShowIf("@this.ToggleTo")]
		[SerializeField,LabelText("除了COM默认Clip类型外，不吸附的类型")]
		 public List<CharacterNamedTypeEnum> ExcludeAbsorbTypes = new List<CharacterNamedTypeEnum>();



		public enum AbsorbSortTypeEnum
		{
			Distance_按距离排序 = 1, Angle_按角度排序 = 2, Type_按类型排序 = 3,
		}

		[Sirenix.OdinInspector.ShowIf(nameof(ToggleTo))]
		[LabelText("吸附排序类型")]
		public AbsorbSortTypeEnum AbsorbSortType = AbsorbSortTypeEnum.Distance_按距离排序;





		public override string ToString()
		{
			if (ToggleTo)
			{
				return $"关闭吸附";
			}
			else
			{
				return $"开启吸附 | {(IncludeDistanceLimit ? ($"距离限制为{AbsorbDistance}") : "")} | {AbsorbSortType}";
			}
		}



	}


}