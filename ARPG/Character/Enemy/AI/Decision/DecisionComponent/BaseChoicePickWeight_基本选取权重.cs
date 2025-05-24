using System;
using System.Collections.Generic;
using ARPG.Character.Job;
using ARPG.Manager;
using Global;
using RPGCore.Buff.Requirement;
using RPGCore.DataEntry;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class BaseChoicePickWeight_基本选取权重
	{
		[SerializeField, LabelText("选取时的权重修饰")]
		public float BaseWeight = 100f;
		public BaseChoicePickWeight_基本选取权重(float baseWeight = 100f)
		{
			BaseWeight = baseWeight;
		}

		public virtual float GetFinalWeight(SOConfig_AIBrain brain)
		{
			return CheckIfSatisfy(brain) ? BaseWeight : 0;
		}

		public virtual bool CheckIfSatisfy(SOConfig_AIBrain brain)
		{
			return true;
		}

	}

	/// <summary>
	/// <para>CD小于时，使用额外传入的数据。CD大于时，使用基类数据</para>
	/// </summary>
	[Serializable]
	public class CPW_AI_需要决策CD比对_CompareWithDecisionCD : BaseChoicePickWeight_基本选取权重
	{

		[SerializeField, LabelText("决策名")]
		public string DecisionName;

		[SerializeField, LabelText("间隔时长")]
		public float Interval;


		[SerializeField, LabelText("小于间隔时权重加数")]
		public float LessThanIntervalWeight;


		public CPW_AI_需要决策CD比对_CompareWithDecisionCD(
			float interval,
			float lessThanIntervalWeight,
			float greaterThanInterval) : base(greaterThanInterval)
		{
			Interval = interval;
			BaseWeight = greaterThanInterval;
			LessThanIntervalWeight = lessThanIntervalWeight;
		}

		public override float GetFinalWeight(SOConfig_AIBrain brain)
		{
			return CheckIfSatisfy(brain) ? BaseWeight : LessThanIntervalWeight;
		}

		public override bool CheckIfSatisfy(SOConfig_AIBrain brain)
		{
			if (string.IsNullOrEmpty(DecisionName))
			{
				return false;
			}

			SOConfig_AIDecision decisionCD = brain.BrainHandlerFunction.FindSelfDecisionByString(DecisionName, true);
			if (decisionCD == null)
			{
				return false;
			}
			var cTime = BaseGameReferenceService.CurrentFixedTime;
			var dLastExitTime = decisionCD.DecisionHandler.LastDecisionEndTime;
			if (cTime - dLastExitTime < Interval)
			{
				return false;
			}
			else
			{
				return true;
			}
		}



	}

	[Serializable]
	public class CPW_需要当前到玩家距离的比对_CompareWithDistanceToPlayer : BaseChoicePickWeight_基本选取权重
	{

		[Button("恢复默认")]
		private void ResetToDefault()
		{
			CompareValue = new DD_纯数值_PureValue(1f);
			CompareMethod = CompareMethodEnum.Less_小于;
			CompareFailedWeight = -100;
			BaseWeight = 100;
		}

		[SerializeField, LabelText("比较符号")]
		public CompareMethodEnum CompareMethod = CompareMethodEnum.Less_小于;
		[SerializeReference, LabelText("右方数值")]
		public DecisionDistanceInfoComponent CompareValue = new DD_纯数值_PureValue(1f);

		[SerializeField, LabelText("比较失败时的权重修饰")]
		public float CompareFailedWeight = -100;

		public CPW_需要当前到玩家距离的比对_CompareWithDistanceToPlayer(
			CompareMethodEnum compareMethod = CompareMethodEnum.Less_小于,
			float compareValue = 1f,
			float compareFailedWeight = 1,
			float baseWeight = 100) : base(baseWeight)
		{
			CompareMethod = compareMethod;
			CompareFailedWeight = compareFailedWeight;
			CompareValue = new DD_纯数值_PureValue(compareValue);
		}

		public override float GetFinalWeight(SOConfig_AIBrain brain)
		{
			 return CheckIfSatisfy(brain) ? BaseWeight : CompareFailedWeight;
		}

		public override bool CheckIfSatisfy(SOConfig_AIBrain brain)
		{
			var selfPos = brain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.transform.position;
			selfPos.y = 0f;
			var playerPos = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference
				.CurrentControllingBehaviour.transform.position;
			playerPos.y = 0f;
			var distance = Vector3.Distance(selfPos, playerPos);
			var valueOfCompareInfo = CompareValue.GetDistanceValue(brain);
			switch (CompareMethod)
			{
				case CompareMethodEnum.Less_小于:
					if (distance < valueOfCompareInfo)
					{
						return true;
					}
					else
					{
						return false;
					}
				case CompareMethodEnum.LessOrEqual_小于等于:
					if (distance <= valueOfCompareInfo)
					{
						return true;
					}
					else
					{
						return false;
					}
				case CompareMethodEnum.Equal_等于:
					if (Mathf.Approximately(distance, valueOfCompareInfo))
					{
						return true;
					}
					else
					{
						return false;
					}
				case CompareMethodEnum.LargerOrEqual_大于等于:
					if (distance >= valueOfCompareInfo)
					{
						return true;
					}
					else
					{
						return false;
					}
				case CompareMethodEnum.Larger_大于:
					if (distance > valueOfCompareInfo)
					{
						return true;
					}
					else
					{
						return false;
					}
				case CompareMethodEnum.NotEqual_不等于:
					if (!Mathf.Approximately(distance, valueOfCompareInfo))
					{
						return true;
					}
					else
					{
						return false;
					}
			}
			return true;
		}
	}

	[Serializable]
	public class CPW_AI_判断玩家近战远程_CheckIfPlayerRanged : BaseChoicePickWeight_基本选取权重
	{
		[SerializeField, LabelText("不是近战时的权重")]
		public float RangedWeight = 100;
		public CPW_AI_判断玩家近战远程_CheckIfPlayerRanged(float meleeWeight = 100, float rangeWeight = 100) : base(meleeWeight)
		{
			RangedWeight = meleeWeight;
			BaseWeight = rangeWeight;
		}

		public override float GetFinalWeight(SOConfig_AIBrain brain)
		{
			return CheckIfSatisfy( brain) ? BaseWeight : RangedWeight;
			
		}

		public override bool CheckIfSatisfy(SOConfig_AIBrain brain)
		{
			if (SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference == null)
			{
				return false;
			}
			var player = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference
				.CurrentControllingBehaviour;
			switch (player.PlayerJobInfo.CurrentJob)
			{
				case RP_BattleJobTypeEnum.TestSwordman_测试剑士:
				case RP_BattleJobTypeEnum.NobleSwordsman_贵族剑士:
				case RP_BattleJobTypeEnum.RedFang_深红獠牙:
					return true;
				case RP_BattleJobTypeEnum.Elementalist_元素使:
					return false;
			}
			return true;
		}
	}

	[Serializable]
	public class CPW_AI_自身数据项比对_CompareWithSelfDataEntry : BaseChoicePickWeight_基本选取权重
	{
		[SerializeField, LabelText("比对左侧数据项")]
		public RP_DataEntry_EnumType LeftDataEntryType = RP_DataEntry_EnumType.CurrentHP_当前HP;

		[SerializeField, LabelText("比较符号")]
		public CompareMethodEnum CompareSignal = CompareMethodEnum.Less_小于;

		[SerializeField, LabelText("比对右侧数据项")]
		public RP_DataEntry_EnumType RightDataEntryType = RP_DataEntry_EnumType.HPMax_最大HP;

		[SerializeField, LabelText("比对右侧乘数|数据项None就是直接比对数值")]
		public float RightDataEntryMultiplier = 1;

		[SerializeField, LabelText("不满足时权重")]
		public float NotSatisfyWeight = -100;

		public CPW_AI_自身数据项比对_CompareWithSelfDataEntry(
			RP_DataEntry_EnumType leftDataEntryType = RP_DataEntry_EnumType.CurrentHP_当前HP,
			CompareMethodEnum compareSignal = CompareMethodEnum.Less_小于,
			RP_DataEntry_EnumType rightDataEntryType = RP_DataEntry_EnumType.HPMax_最大HP,
			float rightDataEntryMultiplier = 1,
			float satisfyWeight = 100,
			float notSatisfyWeight = -100) : base(satisfyWeight)
		{
			LeftDataEntryType = leftDataEntryType;
			CompareSignal = compareSignal;
			RightDataEntryType = rightDataEntryType;
			RightDataEntryMultiplier = rightDataEntryMultiplier;
			BaseWeight = satisfyWeight;
			NotSatisfyWeight = notSatisfyWeight;
		}

		public override float GetFinalWeight(SOConfig_AIBrain brain)
		{ 
			 return CheckIfSatisfy( brain) ? BaseWeight : NotSatisfyWeight;
			 
		}


		public override bool CheckIfSatisfy(SOConfig_AIBrain brain)
		{
			var entry_left =
				brain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.GetFloatDataEntryByType(LeftDataEntryType);

			float rightValue = RightDataEntryMultiplier;
			if (RightDataEntryType != RP_DataEntry_EnumType.None)
			{
				var entry_right =
					brain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.GetFloatDataEntryByType(
						RightDataEntryType);
				rightValue = entry_right.CurrentValue * RightDataEntryMultiplier;
			}

			switch (CompareSignal)
			{
				case CompareMethodEnum.Less_小于:
					if (entry_left.CurrentValue < rightValue)
					{
						return true;
					}
					else
					{
						return false;
					}
					break;
				case CompareMethodEnum.LessOrEqual_小于等于:
					if (entry_left.CurrentValue <= rightValue)
					{
						return true;
					}
					else
					{
						return false;
					}
					break;
				case CompareMethodEnum.Equal_等于:
					if (Mathf.Approximately(entry_left.CurrentValue, rightValue))
					{
						return true;
					}
					else
					{
						return false;
					}
					break;
				case CompareMethodEnum.LargerOrEqual_大于等于:
					if (entry_left.CurrentValue >= rightValue)
					{
						return true;
					}
					else
					{
						return false;
					}
					break;
				case CompareMethodEnum.Larger_大于:
					if (entry_left.CurrentValue > rightValue)
					{
						return true;
					}
					else
					{
						return false;
					}
					break;
				case CompareMethodEnum.NotEqual_不等于:
					if (!Mathf.Approximately(entry_left.CurrentValue, rightValue))
					{
						return true;
					}
					else
					{
						return false;
					}
					break;
			}

			return true;
		}

	}

	[Serializable]
	public class CPW_AI_玩家数据项比对_CompareWithSelfDataEntry : BaseChoicePickWeight_基本选取权重
	{
		[SerializeField, LabelText("比对左侧数据项")]
		public RP_DataEntry_EnumType LeftDataEntryType = RP_DataEntry_EnumType.CurrentHP_当前HP;

		[SerializeField, LabelText("比较符号")]
		public CompareMethodEnum CompareSignal = CompareMethodEnum.Less_小于;

		[SerializeField, LabelText("比对右侧数据项")]
		public RP_DataEntry_EnumType RightDataEntryType = RP_DataEntry_EnumType.HPMax_最大HP;

		[SerializeField, LabelText("比对右侧乘数|数据项None就是直接比对数值")]
		public float RightDataEntryMultiplier = 1;

		[SerializeField, LabelText("不满足时权重")]
		public float NotSatisfyWeight = -100;

		public CPW_AI_玩家数据项比对_CompareWithSelfDataEntry(
			RP_DataEntry_EnumType leftDataEntryType = RP_DataEntry_EnumType.CurrentHP_当前HP,
			CompareMethodEnum compareSignal = CompareMethodEnum.Less_小于,
			RP_DataEntry_EnumType rightDataEntryType = RP_DataEntry_EnumType.HPMax_最大HP,
			float rightDataEntryMultiplier = 1,
			float satisfyWeight = 100,
			float notSatisfyWeight = -100) : base(satisfyWeight)
		{
			LeftDataEntryType = leftDataEntryType;
			CompareSignal = compareSignal;
			RightDataEntryType = rightDataEntryType;
			RightDataEntryMultiplier = rightDataEntryMultiplier;
			BaseWeight = satisfyWeight;
			NotSatisfyWeight = notSatisfyWeight;
		}

		public override float GetFinalWeight(SOConfig_AIBrain brain)
		{
			return CheckIfSatisfy( brain) ? BaseWeight : NotSatisfyWeight;
			
		}

		public override bool CheckIfSatisfy(SOConfig_AIBrain brain)
		{
			var player = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference
				.CurrentControllingBehaviour;
			var entry_left = player.ReleaseSkill_GetRelatedFloatDataEntry(LeftDataEntryType);

			float rightValue = RightDataEntryMultiplier;
			if (RightDataEntryType != RP_DataEntry_EnumType.None)
			{
				var entry_right = player.GetFloatDataEntryByType(RightDataEntryType);
				rightValue = entry_right.CurrentValue * RightDataEntryMultiplier;
			}

			switch (CompareSignal)
			{
				case CompareMethodEnum.Less_小于:
					if (entry_left.CurrentValue < rightValue)
					{
						return true;
					}
					else
					{
						return false;
					}
					break;
				case CompareMethodEnum.LessOrEqual_小于等于:
					if (entry_left.CurrentValue <= rightValue)
					{
						return true;
					}
					else
					{
						return false;
					}
					break;
				case CompareMethodEnum.Equal_等于:
					if (Mathf.Approximately(entry_left.CurrentValue, rightValue))
					{
						return true;
					}
					else
					{
						return false;
					}
					break;
				case CompareMethodEnum.LargerOrEqual_大于等于:
					if (entry_left.CurrentValue >= rightValue)
					{
						return true;
					}
					else
					{
						return false;
					}
					break;
				case CompareMethodEnum.Larger_大于:
					if (entry_left.CurrentValue > rightValue)
					{
						return true;
					}
					else
					{
						return false;
					}
					break;
				case CompareMethodEnum.NotEqual_不等于:
					if (!Mathf.Approximately(entry_left.CurrentValue, rightValue))
					{
						return true;
					}
					else
					{
						return false;
					}
					break;
			}

			return true;
		}
	}


	[Serializable]
	public abstract class CPW_AI_基本逻辑节点_BaseLogicChoicePick : BaseChoicePickWeight_基本选取权重
	{
		[SerializeReference, LabelText("判断容器")]
		public List<BaseChoicePickWeight_基本选取权重> RelatedChoiceList = new List<BaseChoicePickWeight_基本选取权重>();

		protected CPW_AI_基本逻辑节点_BaseLogicChoicePick(float baseWeight = 100) : base(baseWeight)
		{
		}

	}

	[Serializable]
	public class CPW_AI_满足以下所有_SatisfyAll : CPW_AI_基本逻辑节点_BaseLogicChoicePick
	{
		[LabelText("不符合时权重")]
		public float NotSatisfyWeight = -100;

		public CPW_AI_满足以下所有_SatisfyAll(float baseWeight,float notSatisfyWeight) : base(baseWeight)
		{
			BaseWeight = baseWeight;
			NotSatisfyWeight = notSatisfyWeight;
		}



		public override float GetFinalWeight(SOConfig_AIBrain brain)
		{
			return CheckIfSatisfy(brain) ? BaseWeight : NotSatisfyWeight;
		}

		public override bool CheckIfSatisfy(SOConfig_AIBrain brain)
		{
			foreach (BaseChoicePickWeight_基本选取权重 perW in RelatedChoiceList)
			{
				if (!perW.CheckIfSatisfy(brain))
				{
					return false;
				}
			}
			return true;
		}

	}

	
	
	[Serializable]
	public class CPW_AI_满足以下任一_SatisfyAny : CPW_AI_基本逻辑节点_BaseLogicChoicePick
	{
		[LabelText("不符合时权重")]
		public float NotSatisfyWeight = -100;


		public CPW_AI_满足以下任一_SatisfyAny(float baseWeight, float notSatisfyWeight) : base(baseWeight)
		{
			BaseWeight = baseWeight;
			NotSatisfyWeight = notSatisfyWeight;
		}

		public override float GetFinalWeight(SOConfig_AIBrain brain)
		{
			return CheckIfSatisfy(brain) ? BaseWeight : NotSatisfyWeight;
		}

		public override bool CheckIfSatisfy(SOConfig_AIBrain brain)
		{
			foreach (var baseChoicePickWeight in RelatedChoiceList)
			{
				if (baseChoicePickWeight.CheckIfSatisfy(brain))
				{
					return true;
				}
			}
			return false;
		}
	}



	[Serializable]
	public class CPW_AI_以下全不满足_NotSatisfyAll : CPW_AI_基本逻辑节点_BaseLogicChoicePick
	{
		[LabelText("不符合时权重")]
		public float NotSatisfyWeight = -100;

		public CPW_AI_以下全不满足_NotSatisfyAll(float baseWeight, float notSatisfyWeight) : base(baseWeight)
		{
			BaseWeight = baseWeight;
			NotSatisfyWeight = notSatisfyWeight;
		}

		public override float GetFinalWeight(SOConfig_AIBrain brain)
		{
			return CheckIfSatisfy(brain) ? BaseWeight : NotSatisfyWeight;
		}

		public override bool CheckIfSatisfy(SOConfig_AIBrain brain)
		{
			foreach (var baseChoicePickWeight in RelatedChoiceList)
			{
				if (baseChoicePickWeight.CheckIfSatisfy(brain))
				{
					return false;
				}
			}
			return true;
		}
	}

	[Serializable]
	public class CPW_AI_Brain已运行时间大于_CompareWithRunningTime : BaseChoicePickWeight_基本选取权重
	{
		 [LabelText("运行时间大于 ")]
		public float RunningTime = 5;
		 
		[LabelText("不符合时权重")]
		public float NotSatisfyWeight = -100;


		public CPW_AI_Brain已运行时间大于_CompareWithRunningTime(
			float runningTime,
			float notSatisfyWeight,
			float baseWeight = 100) : base(baseWeight)
		{
			RunningTime = runningTime;
			NotSatisfyWeight = notSatisfyWeight;
			BaseWeight = baseWeight;
		}


		public override float GetFinalWeight(SOConfig_AIBrain brain)
		{
			return CheckIfSatisfy(brain) ? BaseWeight : NotSatisfyWeight;
		}
		public override bool CheckIfSatisfy(SOConfig_AIBrain brain)
		{
			var startTime= brain.BrainHandlerFunction.AIBrainStartRunningTime;
			 
			if ((BaseGameReferenceService.CurrentFixedTime - startTime) > RunningTime)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

	}
	
	[Serializable]
	public class  CPW_AI_决策们上次结束时间大于_CompareWithDecisionsLastUseTime : BaseChoicePickWeight_基本选取权重
	{
		[LabelText("决策名")]
		public List<string> Decisions = new List<string>();
		 
		[LabelText("间隔时长")]
		public float LastUseTime = 5;
		 
		[LabelText("不符合时权重")]
		public float NotSatisfyWeight = -100;

		public CPW_AI_决策们上次结束时间大于_CompareWithDecisionsLastUseTime(List<string> decisions,
			float lastUseTime,
			float notSatisfyWeight,
			float baseWeight = 100) : base(baseWeight)
		{
			Decisions.AddRange(decisions);
			LastUseTime = lastUseTime;
			NotSatisfyWeight = notSatisfyWeight;
			BaseWeight = baseWeight;
		}

		public override float GetFinalWeight(SOConfig_AIBrain brain)
		{
			return CheckIfSatisfy(brain) ? BaseWeight : NotSatisfyWeight;
		}

		public override bool CheckIfSatisfy(SOConfig_AIBrain brain)
		{
			foreach (var perS in Decisions)
			{
				SOConfig_AIDecision decisionCD = brain.BrainHandlerFunction.FindSelfDecisionByString(perS, true, true);
				if (decisionCD == null)
				{
					continue;
				}
				var cTime = BaseGameReferenceService.CurrentFixedTime;
				var dLastExitTime = decisionCD.DecisionHandler.LastDecisionEndTime;
				if ((cTime - dLastExitTime) < LastUseTime)
				{
					return false;
				}
			}
			return true;
		}
	}


}