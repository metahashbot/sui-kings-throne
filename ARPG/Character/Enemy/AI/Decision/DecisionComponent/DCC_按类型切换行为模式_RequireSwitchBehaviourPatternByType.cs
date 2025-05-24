using System;
using ARPG.Character.Enemy.AI.BehaviourPattern;
using Sirenix.OdinInspector;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class DCC_按类型切换行为模式_RequireSwitchBehaviourPatternByType : BaseDecisionCommonComponent
	{

		[InfoBox("只能选一个！不能多选，多选了会出现意外错误。")]
		[LabelText("试图切换到的目标行为模式")]
		public AIBehaviourPatternCommonFlag TargetFlag;


		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			foreach (SOConfig_AIBehaviourPattern perCurrentBP in relatedBrain.ConfigContent.BehaviourPatternList)
			{
				if (perCurrentBP.IsAvailable && perCurrentBP.CommonFlag.HasFlag(TargetFlag))
				{
					relatedBrain.BrainHandlerFunction.SwitchBehaviourPattern(perCurrentBP);
					return;
				}
			}
			DBug.LogError($" {relatedBrain.name} 试图切换到的目标行为模式 {TargetFlag} 不存在或者不可用！没有执行任何切换");
		}

	}
}