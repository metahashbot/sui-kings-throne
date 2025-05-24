using System;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public abstract class BaseDecisionCommonComponent
	{
		[SerializeField, LabelText("需要动画匹配"), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public bool RequireAnimationMatching = false;
		[LabelText("关联动画配置名"), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		[ShowIf("@this.RequireAnimationMatching")]
		public string _AN_RelatedAnimationConfigName;

		[LabelText("事件点预设,[结束]仅会在第一次结束时触发")]
		[ShowIf("@this.RequireAnimationMatching")]
		public AnimationEventPresetEnumType AnimationEventPreset = AnimationEventPresetEnumType.Start_开始;

		[LabelText("自定义事件名"),
		 ShowIf(
			 "@(this.AnimationEventPreset == AnimationEventPresetEnumType.Custom_自定义 && this.RequireAnimationMatching)")]
		public string CustomEventString;



		public abstract void EnterComponent(SOConfig_AIBrain relatedBrain);

		public virtual void FixedUpdateTick(float ct, int cf, float delta) { }

		public virtual void OnDecisionExit(SOConfig_AIBrain relatedBrain) { }

		public virtual string GetElementNameInList()
		{
			return GetType().Name;
		}

		public string GetBaseCustomName()
		{
			if (RequireAnimationMatching)
			{
				if (AnimationEventPreset == AnimationEventPresetEnumType.Custom_自定义)
				{
					return $"于动画[{_AN_RelatedAnimationConfigName}] 的 [自定义事件] [{CustomEventString}]时， ";
				}
				else
				{
					return $"于动画 [{_AN_RelatedAnimationConfigName}] 的 [{AnimationEventPreset}] 时， ";
				}
			}
			else
			{
				return "直接 ";
			}
		}
	}

}