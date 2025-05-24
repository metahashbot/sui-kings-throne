using System;
using System.Collections.Generic;
using ARPG.Character.Enemy.AI.Listen;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	[TypeInfoBox("作用——要求【排队】决策")]
	public class DCC_要求排队决策_RequireQueueDecision : BaseDecisionCommonComponent
	{
		[SerializeField,LabelText("√:固定决策？ | 口:复合权重决策")]
		public bool IsFixedDecision = true;
		
		[LabelText("决策名列表，会从上到下排"), SerializeField]
		[ShowIf(nameof(IsFixedDecision))]
		public List<string> TargetDecisionNameList = new List<string>(1);

		[LabelText("复合决策列表，会从上到下排"), SerializeField]
		[HideIf(nameof(IsFixedDecision))]
		public List<MultipleDecisionPickConfig> MultipleDecisionPickConfigs = new List<MultipleDecisionPickConfig>();
		
		[LabelText("操作后开启[队列清除锁]"), SerializeField]
		public bool LockQueueAfterOperation = false;

		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			if (IsFixedDecision)
			{
				if (TargetDecisionNameList == null || TargetDecisionNameList.Count < 1)
				{
					return;
				}
				foreach (string perDecisionName in TargetDecisionNameList)
				{
					SOConfig_AIDecision targetDecision =
						relatedBrain.BrainHandlerFunction.FindSelfDecisionByString(
							perDecisionName);
					if (targetDecision == null)
					{
						continue;
					}
					relatedBrain.BrainHandlerFunction.AddDecisionToQueue(targetDecision,
						BaseAIBrainHandler.DecisionEnqueueType.JustEnqueue_加入排队);
				}
			}
			else
			{
				if (MultipleDecisionPickConfigs == null || MultipleDecisionPickConfigs.Count < 1)
				{
					return;
				}
				foreach (MultipleDecisionPickConfig perDecisionPickConfig in MultipleDecisionPickConfigs)
				{
					if (perDecisionPickConfig == null || perDecisionPickConfig.PerDecisionPickInfos == null ||
					    perDecisionPickConfig.PerDecisionPickInfos.Count < 1)
					{
						continue;
					}
					float totalWeight = 0;
					foreach (var perDecisionPickInfo in perDecisionPickConfig.PerDecisionPickInfos)
					{
						totalWeight += perDecisionPickInfo.Weight;
					}
					float randomValue = UnityEngine.Random.Range(0, totalWeight);
					float currentWeight = 0;
					foreach (var perDecisionPickInfo in perDecisionPickConfig.PerDecisionPickInfos)
					{
						currentWeight += perDecisionPickInfo.Weight;
						if (randomValue <= currentWeight)
						{
							SOConfig_AIDecision targetDecision =
								relatedBrain.BrainHandlerFunction.FindSelfDecisionByString(
									perDecisionPickInfo.DecisionName);
							if (targetDecision == null)
							{
								continue;
							}
							relatedBrain.BrainHandlerFunction.AddDecisionToQueue(targetDecision,
								BaseAIBrainHandler.DecisionEnqueueType.JustEnqueue_加入排队);
							break;
						}
					}
				}
			}
			if (LockQueueAfterOperation)
			{
				relatedBrain.BrainHandlerFunction.EnableQueueClearLock();
			}
		
		}

		public override string GetElementNameInList()
		{ 
			if (IsFixedDecision)
			{
				return $"{GetBaseCustomName()} 【排队】 {(TargetDecisionNameList.Count > 1 ? "固定决策列表" : "固定决策")}：{string.Join(",", TargetDecisionNameList)}";
			}
			else
			{
				return $"{GetBaseCustomName()} 【排队】 {(MultipleDecisionPickConfigs.Count > 1 ? "复合决策列表" : "复合决策")}：{string.Join(",", MultipleDecisionPickConfigs)}";
			}
		}

		// [Button("转换其他类似的行为")]
// 		public void Convert()
// 		{
// 			//find all soconfig ai decision by asset database
// 			var soConfigs = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_AIDecision");
// 			foreach (var perGUID in soConfigs)
// 			{
// 				var perPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perGUID);
// 				SOConfig_AIDecision perSO = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_AIDecision>(perPath);
// 				
// 				if (perSO.ConfigContent.CommonComponents != null)
// 				{
// 					for (int i = perSO.ConfigContent.CommonComponents.Count - 1; i >= 0; i--)
// 					{
// 						DCC_要求排队决策_RequireQueueDecision newCM =
// 							new DCC_要求排队决策_RequireQueueDecision();
// 						var targetP = perSO.ConfigContent.CommonComponents[i];
// 						if (targetP is DCC_RequireQueueDecision ex)
// 						{
// 							newCM.TargetDecisionNameList = new List<string> { ex._targetDecisionName };
// 							perSO.ConfigContent.CommonComponents[i] = newCM;
// 						}
// 					}
// 					UnityEditor.EditorUtility.SetDirty(perSO);
// 				}
// 			}
// 			var listen = UnityEditor.AssetDatabase.FindAssets("t:SOConfig_AIListen");
// 			foreach (var perListnGUID in listen)
// 			{
// 				var perPath = UnityEditor.AssetDatabase.GUIDToAssetPath(perListnGUID);
// 				SOConfig_AIListen perSO = UnityEditor.AssetDatabase.LoadAssetAtPath<SOConfig_AIListen>(perPath);
// 			
// 				switch (perSO.ListenComponent)
// 				{
// 					case AIListen_PlayerDistanceListen aiListenPlayerDistanceListen:
// 						if (aiListenPlayerDistanceListen.Enter1_CommonComponents != null &&
// 						    aiListenPlayerDistanceListen.Enter1_CommonComponents.Count > 0)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Enter1_CommonComponents.Count - 1; i1 >=0 ; i1--)
// 							{
// 								DCC_要求排队决策_RequireQueueDecision newCM =
// 									new DCC_要求排队决策_RequireQueueDecision();
// 								var targetP = aiListenPlayerDistanceListen.Enter1_CommonComponents[i1];
// 								if (targetP is DCC_RequireQueueDecision ex)
// 								{
// 									newCM.TargetDecisionNameList = new List<string> { ex._targetDecisionName };
// 									aiListenPlayerDistanceListen.Enter1_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
// 						if (aiListenPlayerDistanceListen.Exit1_CommonComponents != null &&
// 						    aiListenPlayerDistanceListen.Exit1_CommonComponents.Count > 0)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Exit1_CommonComponents.Count -1; i1 >=0; i1--)
// 							{
// 								DCC_要求排队决策_RequireQueueDecision newCM =
// 									new DCC_要求排队决策_RequireQueueDecision();
// 								var targetP = aiListenPlayerDistanceListen.Exit1_CommonComponents[i1];
// 								if (targetP is DCC_RequireQueueDecision ex)
// 								{
// 									newCM.TargetDecisionNameList = new List<string> { ex._targetDecisionName };
// 									aiListenPlayerDistanceListen.Exit1_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
//
// 						if (aiListenPlayerDistanceListen.Enter2_CommonComponents != null&&
// 						    aiListenPlayerDistanceListen.Enter2_CommonComponents.Count >0)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Enter2_CommonComponents.Count -1; i1 >=0; i1--)
// 							{
// 								DCC_要求排队决策_RequireQueueDecision newCM =
// 									new DCC_要求排队决策_RequireQueueDecision();
// 								var targetP = aiListenPlayerDistanceListen.Enter2_CommonComponents[i1];
// 								if (targetP is DCC_RequireQueueDecision ex)
// 								{
// 									newCM.TargetDecisionNameList = new List<string> { ex._targetDecisionName };
// 									aiListenPlayerDistanceListen.Enter2_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
// 						if (aiListenPlayerDistanceListen.Exit2_CommonComponents != null&&
// 						aiListenPlayerDistanceListen.Exit2_CommonComponents.Count >0
// 							)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Exit2_CommonComponents.Count - 1; i1 >=0; i1--)
// 							{
// 								DCC_要求排队决策_RequireQueueDecision newCM =
// 									new DCC_要求排队决策_RequireQueueDecision();
// 								var targetP = aiListenPlayerDistanceListen.Exit2_CommonComponents[i1];
// 								if (targetP is DCC_RequireQueueDecision ex)
// 								{
// 									newCM.TargetDecisionNameList = new List<string> { ex._targetDecisionName };
// 									aiListenPlayerDistanceListen.Exit2_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
//
//
//
// 						if (aiListenPlayerDistanceListen.Enter3_CommonComponents != null &&
// 						    aiListenPlayerDistanceListen.Enter3_CommonComponents.Count > 0)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Enter3_CommonComponents.Count-1 ; i1>=0; i1--)
// 							{
// 								DCC_要求排队决策_RequireQueueDecision newCM =
// 									new DCC_要求排队决策_RequireQueueDecision();
// 								var targetP = aiListenPlayerDistanceListen.Enter3_CommonComponents[i1];
// 								if (targetP is DCC_RequireQueueDecision ex)
// 								{
// 									newCM.TargetDecisionNameList = new List<string> { ex._targetDecisionName };
// 									aiListenPlayerDistanceListen.Enter3_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
// 						if (aiListenPlayerDistanceListen.Exit3_CommonComponents != null &&
// 						    aiListenPlayerDistanceListen.Exit3_CommonComponents.Count > 0)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Exit3_CommonComponents.Count - 1; i1 >=0 ; i1--)
// 							{
// 								DCC_要求排队决策_RequireQueueDecision newCM =
// 									new DCC_要求排队决策_RequireQueueDecision();
// 								var targetP = aiListenPlayerDistanceListen.Exit3_CommonComponents[i1];
// 								if (targetP is DCC_RequireQueueDecision ex)
// 								{
// 									newCM.TargetDecisionNameList = new List<string> { ex._targetDecisionName };
// 									aiListenPlayerDistanceListen.Exit3_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
//
//
// 						if (aiListenPlayerDistanceListen.Enter4_CommonComponents != null &&
// 						    aiListenPlayerDistanceListen.Enter4_CommonComponents.Count > 0)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Enter4_CommonComponents.Count - 1; i1 >=0 ; i1--)
// 							{
// 								DCC_要求排队决策_RequireQueueDecision newCM =
// 									new DCC_要求排队决策_RequireQueueDecision();
// 								var targetP = aiListenPlayerDistanceListen.Enter4_CommonComponents[i1];
// 								if (targetP is DCC_RequireQueueDecision ex)
// 								{
// 									newCM.TargetDecisionNameList = new List<string> { ex._targetDecisionName };
// 									aiListenPlayerDistanceListen.Enter4_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
// 						if (aiListenPlayerDistanceListen.Exit4_CommonComponents != null &&
// 						    aiListenPlayerDistanceListen.Exit4_CommonComponents.Count > 0)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Exit4_CommonComponents.Count - 1; i1 >=0 ; i1--)
// 							{
// 								DCC_要求排队决策_RequireQueueDecision newCM =
// 									new DCC_要求排队决策_RequireQueueDecision();
// 								var targetP = aiListenPlayerDistanceListen.Exit4_CommonComponents[i1];
// 								if (targetP is DCC_RequireQueueDecision ex)
// 								{
// 									newCM.TargetDecisionNameList = new List<string> { ex._targetDecisionName };
// 									aiListenPlayerDistanceListen.Exit4_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
//
// 						if (aiListenPlayerDistanceListen.Enter5_CommonComponents != null&&
// 						    aiListenPlayerDistanceListen.Enter5_CommonComponents.Count >0
// 						    )
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Enter5_CommonComponents.Count - 1; i1 >=0 ; i1--)
// 							{
// 								DCC_要求排队决策_RequireQueueDecision newCM =
// 									new DCC_要求排队决策_RequireQueueDecision();
// 								var targetP = aiListenPlayerDistanceListen.Enter5_CommonComponents[i1];
// 								if (targetP is DCC_RequireQueueDecision ex)
// 								{
// 									newCM.TargetDecisionNameList = new List<string> { ex._targetDecisionName };
// 									aiListenPlayerDistanceListen.Enter5_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
// 						if (aiListenPlayerDistanceListen.Exit5_CommonComponents != null &&
// 						    aiListenPlayerDistanceListen.Exit5_CommonComponents.Count > 0)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Exit5_CommonComponents.Count - 1; i1 >=0 ; i1--)
// 							{
// 								DCC_要求排队决策_RequireQueueDecision newCM =
// 									new DCC_要求排队决策_RequireQueueDecision();
// 								var targetP = aiListenPlayerDistanceListen.Exit5_CommonComponents[i1];
// 								if (targetP is DCC_RequireQueueDecision ex)
// 								{
// 									newCM.TargetDecisionNameList = new List<string> { ex._targetDecisionName };
// 									aiListenPlayerDistanceListen.Exit5_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
//
//
// 						break;
// 					case AIListen_SelfLayoutHitPlayer aiListenSelfLayoutHitPlayer:
// 						if (aiListenSelfLayoutHitPlayer.Hit_CommonComponents != null)
// 						{
// 							for (int i = aiListenSelfLayoutHitPlayer.Hit_CommonComponents.Count - 1; i >= 0;i--)
// 							{
// 								DCC_要求排队决策_RequireQueueDecision newCM =
// 									new DCC_要求排队决策_RequireQueueDecision();
// 								var targetP = aiListenSelfLayoutHitPlayer.Hit_CommonComponents[i];
// 								if (targetP is DCC_RequireQueueDecision ex)
// 								{
// 									newCM.TargetDecisionNameList = new List<string> { ex._targetDecisionName };
// 									aiListenSelfLayoutHitPlayer.Hit_CommonComponents[i] = newCM;
// 								}
// 							}
// 						}
// 						break;
// 				}
// 				UnityEditor.EditorUtility.SetDirty(perSO);
// 			}
// 			UnityEditor.AssetDatabase.SaveAssets();
// 		}
		
		

	}
}