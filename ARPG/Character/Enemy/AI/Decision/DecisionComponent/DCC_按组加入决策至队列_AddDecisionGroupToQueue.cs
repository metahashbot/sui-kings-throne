using System;
using System.Collections.Generic;
using System.Linq;
using ARPG.Manager;
using Character_角色.Config_配置.AI相关配置;
using Global;
using Global.Utility;
using NodeCanvas.Framework;
using RPGCore.Buff.Requirement;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{


	[Serializable]
	public class DCC_按组加入决策至队列_AddDecisionGroupToQueue : BaseDecisionCommonComponent
	{

		public override string GetElementNameInList()
		{
			if (AIDGraph)
			{
				return $"{GetBaseCustomName()} 使用【图形】{AIDGraph.name}转换";
			}
			else
			{
				return $"{GetBaseCustomName()} 按组加入决策至队列";

			}
		}

		public enum AddToQueueType
		{
			JustEnqueue_加入排队到后方 = 1,
			FullClearAndEnqueue_排到首位且清空队列 = 2,
			BreakAndEnqueue_打断并置首 = 3,
			InsertFirst_插入到首位 = 4
		}

		[Serializable]
		public class NodeInfo
		{
			[LabelText("决策名字"), GUIColor(255f / 255f, 223f / 255f, 239f / 255f)]
			public string NodeToDecision;
			[LabelText("选取权重"), ListDrawerSettings(DefaultExpandedState = false), SerializeReference]
			public List<BaseChoicePickWeight_基本选取权重> ChoicePickWeightList = new List<BaseChoicePickWeight_基本选取权重>()
				{ new BaseChoicePickWeight_基本选取权重(100f) };

			public float GetCurrentWeight(SOConfig_AIBrain aiBrain)
			{
				float initWeight = 0f;
				for (int i = 0; i < ChoicePickWeightList.Count; i++)
				{
					initWeight += ChoicePickWeightList[i].GetFinalWeight(aiBrain);
				}
				return initWeight;
			}
#if UNITY_EDITOR
			[Button("生成|重置子节点容器", Icon = SdfIconType.NodePlus, DirtyOnClick = true), HorizontalGroup("1")]
			private void GenerateLeafs()
			{
				Leafs = new List<NodeInfo>();
			}

			[Button("删除子节点容器 ", Icon = SdfIconType.NodeMinus, DirtyOnClick = true), HorizontalGroup("1")]
			private void DeleteLeafs()
			{
				Leafs = null;
			}
#endif

			[LabelText("子节点容器"), SerializeField, ListDrawerSettings(ListElementLabelName = "NodeToDecision")]
			public List<NodeInfo> Leafs = null;
		}



		[SerializeField, LabelText("加入队列的方式"),]
		private AddToQueueType _enqueueType = AddToQueueType.JustEnqueue_加入排队到后方;

		[SerializeField, LabelText("使用图形转换"), PropertyOrder(-2)]
		private Tree_AIDecisionGroupGraph AIDGraph;

		// [Button("换个颜色"),PropertyOrder ( -2)]
		// private void _btut()
		// {
		// 	foreach (var perNode in AIDGraph.allNodes)
		// 	{
		// 		perNode.OnCreate(AIDGraph);
		//
		// 	}
		// }
		
#if UNITY_EDITOR
		[Button("转换图形！", ButtonSizes.Large,Icon = SdfIconType.Flag), ShowIf("@this.AIDGraph != null"),
		 PropertyOrder(-2)]
		private void Button_Convert()
		{
			DecisionNodes.Clear();
			List<AINode_DecisionNodeInGroup> _firstNodes = new List<AINode_DecisionNodeInGroup>();
			var allNodes = AIDGraph.allNodes;
			for (int i = 0; i < allNodes.Count; i++)
			{
				if (allNodes[i] is AINode_DecisionNodeInGroup aiDecisionNode)
				{
					//如果一个节点的所有输入节点都不是AINode_DecisionNodeInGroup，那它就是一级节点
					var inputs = aiDecisionNode.GetParentNodes();
					if (inputs.Count() == 0 || !inputs.Any((baseNode => baseNode is AINode_DecisionNodeInGroup)))
					{
						_firstNodes.Add(aiDecisionNode);
					}
				}
			}
			
			if (_firstNodes.Count == 0)
			{
				//display dialogue
				UnityEditor.EditorUtility.DisplayDialog("错误", $"在转换【图形】：{AIDGraph.name}时，没有找到任何一个一级节点", "确定");
				return;
			}
			for (int i = 0; i < _firstNodes.Count; i++)
			{
				NodeInfo newNodeAsFirstNode = new NodeInfo();
				AINode_DecisionNodeInGroup currentNodeInGraph = _firstNodes[i];
			
				DecisionNodes.Add(newNodeAsFirstNode);
				newNodeAsFirstNode.NodeToDecision = _firstNodes[i]._决策名字.GetValue();
				newNodeAsFirstNode.ChoicePickWeightList.Clear();
				ProcessWeightOnNode(ref newNodeAsFirstNode, currentNodeInGraph);
			
				//get all outputNode
				var outputNodes = currentNodeInGraph.GetChildNodes();
			
				for (int j = 0; j < outputNodes.Count(); j++)
				{
					if (newNodeAsFirstNode.Leafs == null)
					{
						newNodeAsFirstNode.Leafs = new List<NodeInfo>();
					}
					AINode_DecisionNodeInGroup subNode = outputNodes.ElementAt(j) as AINode_DecisionNodeInGroup;
					if (subNode == null)
					{
						continue;
					}
					ProcessLeftNode(subNode, newNodeAsFirstNode.Leafs);
				}
			}
			
			void ProcessLeftNode(AINode_DecisionNodeInGroup leafNode, List<NodeInfo> nodeList)
			{
				NodeInfo newNode = new NodeInfo();
				newNode.NodeToDecision = leafNode._决策名字.GetValue();
				newNode.ChoicePickWeightList.Clear();
				ProcessWeightOnNode(ref newNode, leafNode);
				nodeList.Add(newNode);
				//get all outputNode
				var outputNodes = leafNode.GetChildNodes();
				for (int j = 0; j < outputNodes.Count(); j++)
				{
					
					AINode_DecisionNodeInGroup subNode = outputNodes.ElementAt(j) as AINode_DecisionNodeInGroup;
					if (subNode == null)
					{
						continue;
					}
					if (newNode.Leafs == null)
					{
						newNode.Leafs = new List<NodeInfo>();
					}
					ProcessLeftNode(subNode, newNode.Leafs);
				}
			}
			
			
			
			void ProcessWeightOnNode(ref NodeInfo node, AINode_DecisionNodeInGroup firstNode)
			{
				//get all input node 
				var outpoutNodes = firstNode.GetChildNodes();
				//如果输入节点中 ，没有任何一个节点是权重节点，那就当它没有外部权重节点，直接内部给一个 基本选取权重
			
				int weightCount = outpoutNodes.Count((baseNode => baseNode is AINode_WeightDecorator || baseNode is AINode_BaseLogicNode));
				if (weightCount == 0)
				{
					node.ChoicePickWeightList.Add(new BaseChoicePickWeight_基本选取权重(firstNode._默认选取权重.GetValue()));
				}
				else
				{
					//如果有权重节点，那么就把权重节点的权重加入到这个节点的权重列表中
					foreach (var outputNode in outpoutNodes)
					{
						if (outputNode is AINode_WeightDecorator weightNode)
						{
							ProcessDecoratorNode(node.ChoicePickWeightList, weightNode, firstNode);
						}
						else if (outputNode is AINode_BaseLogicNode logicNode)
						{
							IEnumerable<Node> allChildNodes = outputNode.GetChildNodes();
							if (allChildNodes.Count() == 0)
							{
								DBug.LogError("出现了一个空的逻辑节点，这不合理，检查一下");
								continue;
							}
							switch (logicNode)
							{
								case AINode_Decorator_SatisfyAll aiNodeDecoratorSatisfyAll:
									var cpw_sa = new CPW_AI_满足以下所有_SatisfyAll(
										aiNodeDecoratorSatisfyAll.Satisfy.GetValue() + firstNode._默认选取权重.GetValue(),
										aiNodeDecoratorSatisfyAll.NotSatisfy.GetValue() + firstNode._默认选取权重.GetValue());
									node.ChoicePickWeightList.Add(cpw_sa);
									ProcessLogicNode(aiNodeDecoratorSatisfyAll, cpw_sa, firstNode);
									break;
								case AINode_Decorator_SatisfyAny aiNodeDecoratorSatisfyAny:
									var cpw_sa2 = new CPW_AI_满足以下任一_SatisfyAny(aiNodeDecoratorSatisfyAny.Satisfy.GetValue() + firstNode._默认选取权重.GetValue(),
										aiNodeDecoratorSatisfyAny.NotSatisfy.GetValue() + firstNode._默认选取权重.GetValue());
									node.ChoicePickWeightList.Add(cpw_sa2);
									ProcessLogicNode(aiNodeDecoratorSatisfyAny, cpw_sa2, firstNode);
									break;
								case AINode_Decorator_SatisfyNone aiNodeDecoratorSatisfyNone:
									var cpw_sa3 = new CPW_AI_以下全不满足_NotSatisfyAll(aiNodeDecoratorSatisfyNone.Satisfy.GetValue() + firstNode._默认选取权重.GetValue(),
										aiNodeDecoratorSatisfyNone.NotSatisfy.GetValue() + firstNode._默认选取权重.GetValue());
									node.ChoicePickWeightList.Add(cpw_sa3);
									ProcessLogicNode(aiNodeDecoratorSatisfyNone, cpw_sa3, firstNode);
									break;
							}
						}
					}
				}
			}
		}



		private void ProcessDecoratorNode(
			List<BaseChoicePickWeight_基本选取权重> choiceList,
			AINode_WeightDecorator decorator,
			AINode_DecisionNodeInGroup firstNode)
		{
			switch (decorator)
			{
				case AINode_Decorator_WeightByDistance ai节点决策组权重当前到玩家距离比对固定值:
					var cpw = new CPW_需要当前到玩家距离的比对_CompareWithDistanceToPlayer(ai节点决策组权重当前到玩家距离比对固定值._比较方式,
						ai节点决策组权重当前到玩家距离比对固定值._到玩家距离.GetValue(),
						ai节点决策组权重当前到玩家距离比对固定值._不满足时权重.GetValue() + (firstNode != null ? firstNode._默认选取权重.GetValue() : 0f),
						ai节点决策组权重当前到玩家距离比对固定值._满足时权重.GetValue() +
						(firstNode != null ? firstNode._默认选取权重.GetValue() : 0f));
					choiceList.Add(cpw);
					break;
				case AINode_Decorator_WeightByCD ai节点决策组权重决策CD比对:
					var cpw2 = new CPW_AI_需要决策CD比对_CompareWithDecisionCD(ai节点决策组权重决策CD比对.Interval.GetValue(),
						ai节点决策组权重决策CD比对.LessThanIntervalWeight.GetValue() +
						(firstNode != null ? firstNode._默认选取权重.GetValue() : 0f),
						ai节点决策组权重决策CD比对.GreaterThanIntervalWeight.GetValue() +
						(firstNode != null ? firstNode._默认选取权重.GetValue() : 0f));
					choiceList.Add(cpw2);
					break;
				case AINode_Decorator_WeighByPlayerMelee ai节点决策组权重玩家近战:
					var cpw3 = new CPW_AI_判断玩家近战远程_CheckIfPlayerRanged(
						ai节点决策组权重玩家近战.WeightOnMelee.GetValue() +
						(firstNode != null ? firstNode._默认选取权重.GetValue() : 0f),
						ai节点决策组权重玩家近战.WeightOnRanged.GetValue() +
						(firstNode != null ? firstNode._默认选取权重.GetValue() : 0f));
					choiceList.Add(cpw3);
					break;
				case AINode_Decorator_WeightCompareDataEntryWithSelf ai_CompareDataWithSelf:

					var cpw4 = new CPW_AI_自身数据项比对_CompareWithSelfDataEntry(
						ai_CompareDataWithSelf.LeftDataEntryType.GetValue(),
						ai_CompareDataWithSelf.CompareSignal,
						ai_CompareDataWithSelf.RightDataEntryType.GetValue(),
						ai_CompareDataWithSelf.RightDataEntryMultiplier.GetValue(),
						ai_CompareDataWithSelf.SatisfyWeight.GetValue() +
						(firstNode != null ? firstNode._默认选取权重.GetValue() : 0f),
						ai_CompareDataWithSelf.NotSatisfyWeight.GetValue() +
						(firstNode != null ? firstNode._默认选取权重.GetValue() : 0f));
					choiceList.Add(cpw4);
					break;
				case AINode_Decorator_当前玩家数据比对_WeightCompareDataOnCurrentPlayer ai_compareOnPlayer:
					var cpw5 = new CPW_AI_玩家数据项比对_CompareWithSelfDataEntry(
						ai_compareOnPlayer.LeftDataEntryType.GetValue(),
						ai_compareOnPlayer.CompareSignal,
						ai_compareOnPlayer.RightDataEntryType.GetValue(),
						ai_compareOnPlayer.RightDataEntryMultiplier.GetValue(),
						ai_compareOnPlayer.SatisfyWeight.GetValue() +
						(firstNode != null ? firstNode._默认选取权重.GetValue() : 0f),
						ai_compareOnPlayer.NotSatisfyWeight.GetValue() +
						(firstNode != null ? firstNode._默认选取权重.GetValue() : 0f));
					choiceList.Add(cpw5);
					break;
				case AINode_Decorator_AIBrain已运行时间大于_RunningTimeLarger aiNodeDecoratorAiRunningTime:
					var cpw6 = new CPW_AI_Brain已运行时间大于_CompareWithRunningTime(aiNodeDecoratorAiRunningTime.RunningTime.GetValue(),
						aiNodeDecoratorAiRunningTime.NotSatisfyWeight.GetValue() +
						(firstNode != null ? firstNode._默认选取权重.GetValue() : 0f),
						aiNodeDecoratorAiRunningTime.SatisfyWeight.GetValue() +
						(firstNode != null ? firstNode._默认选取权重.GetValue() : 0f));
					choiceList.Add(cpw6);
					break;
				case AINode_Decorator_决策们上次结束时间大于_DecisionsLastUseTimeLarger aiNodeDecoratorLastUseTime:
					var cpw7 = new CPW_AI_决策们上次结束时间大于_CompareWithDecisionsLastUseTime(
						aiNodeDecoratorLastUseTime.Decisions.GetValue(),
						aiNodeDecoratorLastUseTime.LastUseTime.GetValue() ,
						aiNodeDecoratorLastUseTime._不满足时权重.GetValue() +
						(firstNode != null ? firstNode._默认选取权重.GetValue() : 0f), 
						 aiNodeDecoratorLastUseTime._满足时权重.GetValue() + 
						 (firstNode != null ? firstNode._默认选取权重.GetValue() : 0f));
					choiceList.Add(cpw7);
					;

					break;
			}
		}



		private void ProcessLogicNode(AINode_BaseLogicNode logic, CPW_AI_基本逻辑节点_BaseLogicChoicePick cpw , AINode_DecisionNodeInGroup parentNode)
		{
			foreach (Node perNode in logic.GetChildNodes())
			{
				switch (perNode)
				{
					case AINode_BaseLogicNode logicNode:
						switch (logicNode)
						{
							case AINode_Decorator_SatisfyAll aiNodeDecoratorSatisfyAll:
								var cpw_sa = new CPW_AI_满足以下所有_SatisfyAll(aiNodeDecoratorSatisfyAll.Satisfy.GetValue(),
									aiNodeDecoratorSatisfyAll.NotSatisfy.GetValue());
								cpw.RelatedChoiceList.Add(cpw_sa);
								ProcessLogicNode(aiNodeDecoratorSatisfyAll, cpw_sa, parentNode);
								break;
							case AINode_Decorator_SatisfyAny aiNodeDecoratorSatisfyAny:
								var cpw_sa2 = new CPW_AI_满足以下任一_SatisfyAny(aiNodeDecoratorSatisfyAny.Satisfy.GetValue(),
									aiNodeDecoratorSatisfyAny.NotSatisfy.GetValue());
								cpw.RelatedChoiceList.Add(cpw_sa2);
								ProcessLogicNode(aiNodeDecoratorSatisfyAny, cpw_sa2, parentNode);
								 
								break;
							case AINode_Decorator_SatisfyNone aiNodeDecoratorSatisfyNone:
								var cpw_sa3 = new CPW_AI_以下全不满足_NotSatisfyAll(aiNodeDecoratorSatisfyNone.Satisfy.GetValue(),
									aiNodeDecoratorSatisfyNone.NotSatisfy.GetValue());
								cpw.RelatedChoiceList.Add(cpw_sa3);
								ProcessLogicNode(aiNodeDecoratorSatisfyNone, cpw_sa3, parentNode);
								break;
						}
						break;
					case AINode_WeightDecorator decorator:
						  ProcessDecoratorNode( cpw.RelatedChoiceList,decorator, parentNode);
						break;
				}
				 
			}
		}
#endif




		[LabelText("操作后开启[队列清除锁]"), SerializeField]
		public bool LockQueueAfterOperation = false;


		[Header("====")]
		[Space]
		[SerializeField, LabelText("决策组"), ListDrawerSettings(ListElementLabelName = "NodeToDecision")]
		public List<NodeInfo> DecisionNodes = new List<NodeInfo>();



		private static List<string> finalList = new List<string>();
		private static List<float> weightList = new List<float>();
		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			finalList.Clear();

			if (DecisionNodes.Count == 0)
			{
				DBug.LogError("决策组为空");
				return;
			}
			//确保每个节点上一定都能拿出一个决策来
			NodeInfo currentPointer = null;
			//首个节点就是 在DecisionNodes 上选

			weightList.Clear();
			for (int i = 0; i < DecisionNodes.Count; i++)
			{
				weightList.Add(DecisionNodes[i].GetCurrentWeight(relatedBrain));
			}
			var indexFirst = ShuffleUtility.ShuffleWeightListAndGetIndex(weightList);
			if (indexFirst == -1)
			{
				DBug.LogError("决策组为空");
				return;
			}
			currentPointer = DecisionNodes[indexFirst];


			while (currentPointer != null)
			{
				finalList.Add(currentPointer.NodeToDecision);
				if (currentPointer.Leafs == null || currentPointer.Leafs.Count == 0)
				{
					currentPointer = null;
					break;
				}
				weightList.Clear();
				for (int i = 0; i < currentPointer.Leafs.Count; i++)
				{
					weightList.Add(currentPointer.Leafs[i].GetCurrentWeight(relatedBrain));
				}
				var index = ShuffleUtility.ShuffleWeightListAndGetIndex(weightList);
				if (index == -1)
				{
					currentPointer = null;
				}
				else
				{
					currentPointer = currentPointer.Leafs[index];
				}
			}
			DBug.Log($"AIDebug:{relatedBrain.name}决策组结果，其中决策数量为{finalList.Count}个,分别为 {string.Join(",", finalList)}");
			switch (_enqueueType)
			{
				case AddToQueueType.JustEnqueue_加入排队到后方:
					for (int i = 0; i < finalList.Count; i++)
					{
						relatedBrain.BrainHandlerFunction.AddDecisionToQueue(finalList[i],
							BaseAIBrainHandler.DecisionEnqueueType.JustEnqueue_加入排队);
					}
					break;
				case AddToQueueType.FullClearAndEnqueue_排到首位且清空队列:
					for (int i = 0; i < finalList.Count; i++)
					{
						if (i == 0)
						{
							relatedBrain.BrainHandlerFunction.AddDecisionToQueue(finalList[i],
								BaseAIBrainHandler.DecisionEnqueueType.EnqueueToSecond_加入排队但是清空后续);
						}
						else
						{
							relatedBrain.BrainHandlerFunction.AddDecisionToQueue(finalList[i],
								BaseAIBrainHandler.DecisionEnqueueType.JustEnqueue_加入排队);
						}
					}
					break;
				case AddToQueueType.BreakAndEnqueue_打断并置首:
					for (int i = 0; i < finalList.Count; i++)
					{
						if (i == 0)
						{
							relatedBrain.BrainHandlerFunction.AddDecisionToQueue(finalList[i],
								BaseAIBrainHandler.DecisionEnqueueType.FullClearAndEnqueueFirst_清空并置首);
						}
						else
						{
							relatedBrain.BrainHandlerFunction.AddDecisionToQueue(finalList[i],
								BaseAIBrainHandler.DecisionEnqueueType.JustEnqueue_加入排队);
						}
					}
					break;
				case AddToQueueType.InsertFirst_插入到首位:
					relatedBrain.BrainHandlerFunction.InsertDecisionToQueue(finalList);

					break;
			}
			if (LockQueueAfterOperation)
			{
				relatedBrain.BrainHandlerFunction.EnableQueueClearLock();
			}
		}

	}
}