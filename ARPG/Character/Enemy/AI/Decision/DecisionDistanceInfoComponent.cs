using System;
using ARPG.Character.Enemy.AI.Listen;
using ARPG.Manager;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision
{
	/// <summary>
	/// <para>在AI决策中，提供一个基于【距离】的信息。这是最基本的</para>
	/// </summary>
	[Serializable]
	public abstract class DecisionDistanceInfoComponent
	{
		[LabelText("值乘数"), SerializeField, PropertyOrder(1)]
		public float ValueMultiplier = 1f;
		public abstract float GetDistanceValue(SOConfig_AIBrain brainInstance);
		public abstract DecisionDistanceInfoComponent DeepCopy(DecisionDistanceInfoComponent from);

	}
	[TypeInfoBox("两个距离信息的组合，使用[乘算]的方式")]
	[Serializable]
	public class DD_两个距离组合_TwoDistanceInfoComponent : DecisionDistanceInfoComponent
	{

		[LabelText("距离1"), SerializeField]
		public DecisionDistanceInfoComponent Distance1;
		[LabelText("距离1的乘值"), SerializeField]
		public float Distance1Mul = 0.5f;
		[Space]
		[LabelText("距离2"), SerializeField]
		public DecisionDistanceInfoComponent Distance2;
		[LabelText("距离2的乘值"), SerializeField]
		public float Distance2Mul = 0.5f;
		public override float GetDistanceValue(SOConfig_AIBrain brainInstance)
		{
			return Distance1.GetDistanceValue(brainInstance) * Distance1Mul +
			       Distance2.GetDistanceValue(brainInstance) * Distance2Mul;
		}
		public override DecisionDistanceInfoComponent DeepCopy(DecisionDistanceInfoComponent from)
		{
			if(from is not DD_两个距离组合_TwoDistanceInfoComponent from2)
			{
				throw new Exception("类型不匹配");
			}
			else
			{
				return new DD_两个距离组合_TwoDistanceInfoComponent()
				{
					Distance1 = from2.Distance1.DeepCopy(from2.Distance1),
					Distance2 = from2.Distance2.DeepCopy(from2.Distance2),
					Distance1Mul = from2.Distance1Mul,
					Distance2Mul = from2.Distance2Mul,
					ValueMultiplier = from2.ValueMultiplier
				};
			}
		}
	}




	[TypeInfoBox("两个距离信息的组合，使用[乘算]的方式")]
	[Serializable]
	public class TwoDistanceInfoComponent : DecisionDistanceInfoComponent
	{
		[LabelText("距离1"), SerializeField]
		public DecisionDistanceInfoComponent Distance1;
		[LabelText("距离2"), SerializeField]
		public DecisionDistanceInfoComponent Distance2;
		[LabelText("距离1的乘值"), SerializeField]
		public float Distance1Mul = 0.5f;
		[LabelText("距离2的乘值"), SerializeField]
		public float Distance2Mul = 0.5f;
		public override float GetDistanceValue(SOConfig_AIBrain brainInstance)
		{
			return Distance1.GetDistanceValue(brainInstance) * Distance1Mul +
			       Distance2.GetDistanceValue(brainInstance) * Distance2Mul;
		}
		public override DecisionDistanceInfoComponent DeepCopy(DecisionDistanceInfoComponent from)
		{
			
			if(from is not TwoDistanceInfoComponent from2)
			{
				throw new Exception("类型不匹配");
			}
			else
			{
				return new DD_两个距离组合_TwoDistanceInfoComponent()
				{
					Distance1 = from2.Distance1.DeepCopy(from2.Distance1),
					Distance2 = from2.Distance2.DeepCopy(from2.Distance2),
					Distance1Mul = from2.Distance1Mul,
					Distance2Mul = from2.Distance2Mul,
					ValueMultiplier = from2.ValueMultiplier
				};
			}
		}

	}



	[Serializable]
	[TypeInfoBox("【纯值】！与玩家距离没有关系！")]
	public class DD_纯数值_PureValue : DecisionDistanceInfoComponent
	{
		[LabelText("就是一个数值"), SerializeField]
		public float AbsoluteValue;

		public DD_纯数值_PureValue(float absoluteValue =1f)
		{
			AbsoluteValue = absoluteValue;
		}

		public override float GetDistanceValue(SOConfig_AIBrain brainInstance)
		{
			return AbsoluteValue;
		}
		public override DecisionDistanceInfoComponent DeepCopy(DecisionDistanceInfoComponent from)
		{
			
			if(from is not DD_纯数值_PureValue from2)
			{
				throw new Exception("类型不匹配");
			}
			else
			{
				return new DD_纯数值_PureValue()
				{
					AbsoluteValue = from2.AbsoluteValue,
					ValueMultiplier = from2.ValueMultiplier
				};
			}
		}
	}



	[Serializable]
	[TypeInfoBox("基于【距离】的信息，使用【纯值】的方式")]
	public class PureValue_DistanceInfo : DecisionDistanceInfoComponent
	{
		[LabelText("基准值"), SerializeField]
		public float BaseValue;



		public override float GetDistanceValue(SOConfig_AIBrain brainInstance)
		{
			return BaseValue * ValueMultiplier;
		}
		public override DecisionDistanceInfoComponent DeepCopy(DecisionDistanceInfoComponent from)
		{
			
			if(from is not PureValue_DistanceInfo from2)
			{
				throw new Exception("类型不匹配");
			}
			else
			{
				return new DD_纯数值_PureValue()
				{
					AbsoluteValue = from2.BaseValue,
					ValueMultiplier = from2.ValueMultiplier
				};
			}
		}


	}

	
	[Serializable]
	[TypeInfoBox("当前到仇恨目标的距离的比例。如果当前没有仇恨目标，会返回乘数本身")]
	public class DD_自身到仇恨目标距离_CurrentDistanceToHateTarget : DecisionDistanceInfoComponent
	{

		public override float GetDistanceValue(SOConfig_AIBrain brainInstance)
		{
			var ht = brainInstance.BrainHandlerFunction.GetCurrentHatredTarget();
			if (ht)
			{

				var hateTargetPos = ht.transform.position;
				var selfPos = brainInstance.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.transform.position;
				hateTargetPos.y = 0f;
				selfPos.y = 0f;
				return Vector3.Distance(hateTargetPos, selfPos) * ValueMultiplier;
			}
			else
			{
				return ValueMultiplier;
			}
		}
		public override DecisionDistanceInfoComponent DeepCopy(DecisionDistanceInfoComponent from)
		{
			
			if(from is not DD_自身到仇恨目标距离_CurrentDistanceToHateTarget from2)
			{
				throw new Exception("类型不匹配");
			}
			else
			{
				return new DD_自身到仇恨目标距离_CurrentDistanceToHateTarget()
				{
					ValueMultiplier = from2.ValueMultiplier
				};
			}
		}
	}

	[Serializable]
	[TypeInfoBox("当前到玩家的距离的比例")]
	public class DD_自身到玩家距离_CurrentDistanceToPlayer : DecisionDistanceInfoComponent
	{

		public override float GetDistanceValue(SOConfig_AIBrain brainInstance)
		{
			var playerPos = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference
				.CurrentControllingBehaviour.transform.position;
			var selfPos = brainInstance.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.transform.position;
			playerPos.y = 0f;
			selfPos.y = 0f;
			return Vector3.Distance(playerPos, selfPos) * ValueMultiplier;
		}
		public override DecisionDistanceInfoComponent DeepCopy(DecisionDistanceInfoComponent from)
		{
			
			if(from is not DD_自身到玩家距离_CurrentDistanceToPlayer from2)
			{
				throw new Exception("类型不匹配");
			}
			else
			{
				return new DD_自身到玩家距离_CurrentDistanceToPlayer()
				{
					ValueMultiplier = from2.ValueMultiplier
				};
			}
		}
	}

	
	

	[Serializable]
	[TypeInfoBox("当前的【玩家距离监听】的某个监听项")]
	public class DD_匹配距离监听_DistanceByPlayerDistanceListen : DecisionDistanceInfoComponent
	{
		[SerializeField, LabelText("1、2、3、4、5、6对应监听123456，别填其他的")]
		public int ListenComponentIndex = 1;


		public override float GetDistanceValue(SOConfig_AIBrain brainInstance)
		{
			var listenIndex =
				brainInstance.BrainHandlerFunction.CurrentActiveBehaviourPattern.ListenList.FindIndex((listen =>
					listen.ListenComponent is AIListen_PlayerDistanceListen));
			if (listenIndex != -1)
			{
				var listen = brainInstance.BrainHandlerFunction.CurrentActiveBehaviourPattern.ListenList[listenIndex];
				var playerDistanceListen = listen.ListenComponent as AIListen_PlayerDistanceListen;
				switch (ListenComponentIndex)
				{
					case 1:
						return playerDistanceListen.Range1CurrentEnterValue * ValueMultiplier;
					case 2:
						return playerDistanceListen.Range2CurrentEnterValue * ValueMultiplier;
					case 3:
						return playerDistanceListen.Range3CurrentEnterValue * ValueMultiplier;
					case 4:
						return playerDistanceListen.Range4CurrentEnterValue * ValueMultiplier;
					case 5:
						return playerDistanceListen.Range5CurrentEnterValue * ValueMultiplier;
				
				}
			}

			DBug.LogError(
				$"来自角色{brainInstance.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.name}的决策:{brainInstance.BrainHandlerFunction.CurrentRunningDecision.name}没有找到玩家距离监听组件或者监听的索引不对，直接返回了当前玩家距离");
			var playerPos = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference
				.CurrentControllingBehaviour.transform.position;
			var selfPos = brainInstance.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.transform.position;
			playerPos.y = 0f;
			selfPos.y = 0f;
			return Vector3.Distance(playerPos, selfPos) * ValueMultiplier;
		}
		public override DecisionDistanceInfoComponent DeepCopy(DecisionDistanceInfoComponent from)
		{
			
			if(from is not DD_匹配距离监听_DistanceByPlayerDistanceListen from2)
			{
				throw new Exception("类型不匹配");
			}
			else
			{
				return new DD_匹配距离监听_DistanceByPlayerDistanceListen()
				{
					ListenComponentIndex = from2.ListenComponentIndex,
					ValueMultiplier = from2.ValueMultiplier
				};
			}
		}

	}
	[Serializable]
	[TypeInfoBox("当前的【玩家距离监听】的某个监听项")]
	public class DistanceByPlayerDistanceListen : DecisionDistanceInfoComponent
	{
		[SerializeField, LabelText("1、2、3、4、5、6对应监听123456，别填其他的")]
		public int ListenComponentIndex = 1;


		public override float GetDistanceValue(SOConfig_AIBrain brainInstance)
		{
			var listenIndex = brainInstance.BrainHandlerFunction.CurrentActiveBehaviourPattern.ListenList.FindIndex((listen =>
				listen.ListenComponent is AIListen_PlayerDistanceListen));
			if (listenIndex != -1)
			{
				var listen = brainInstance.BrainHandlerFunction.CurrentActiveBehaviourPattern.ListenList[listenIndex];
				var playerDistanceListen = listen.ListenComponent as AIListen_PlayerDistanceListen;
				switch (ListenComponentIndex)
				{
					case 1:
						return playerDistanceListen.Range1CurrentEnterValue * ValueMultiplier;
					case 2:
						return playerDistanceListen.Range2CurrentEnterValue * ValueMultiplier;
					case 3:
						return playerDistanceListen.Range3CurrentEnterValue * ValueMultiplier;
					case 4:
						return playerDistanceListen.Range4CurrentEnterValue * ValueMultiplier;
					case 5:
						return playerDistanceListen.Range5CurrentEnterValue * ValueMultiplier;
				}
			}

			DBug.LogError("没有找到玩家距离监听组件或者监听的索引不对，直接返回了当前玩家距离");
			var playerPos = SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference
				.CurrentControllingBehaviour.transform.position;
			var selfPos = brainInstance.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour.transform.position;
			playerPos.y = 0f;
			selfPos.y = 0f;
			return Vector3.Distance(playerPos, selfPos) * ValueMultiplier;
		}
		public override DecisionDistanceInfoComponent DeepCopy(DecisionDistanceInfoComponent from)
		{
			
			if(from is not DistanceByPlayerDistanceListen from2)
			{
				throw new Exception("类型不匹配");
			}
			else
			{
				return new DD_匹配距离监听_DistanceByPlayerDistanceListen()
				{
					ListenComponentIndex = from2.ListenComponentIndex,
					ValueMultiplier = from2.ValueMultiplier
				};
			}
			
		}

	}

	
	
	
}