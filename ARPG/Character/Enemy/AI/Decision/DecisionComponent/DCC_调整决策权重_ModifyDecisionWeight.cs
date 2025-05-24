using System;
using System.Collections.Generic;
using ARPG.Character.Enemy.AI.Decision.ConcreteDecision;
using ARPG.Character.Enemy.AI.Listen;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	[TypeInfoBox("调整决策权重：调整是无视【行为模式】的，它可以调整不是当前活跃行为模式的决策权重")]
	public class DCC_调整决策权重_ModifyDecisionWeight : BaseDecisionCommonComponent
	{

		[Serializable]
		public class ModifyInfo
		{
			[LabelText("目标决策名称"), SerializeField, GUIColor(255f / 255f, 223f / 255f, 239f / 255f)]
			public string DecisionName;
			
			

			[LabelText("√：调整 || 口：重置默认")]
			public bool ModifyOrReset = true;

			[LabelText("√：加算 || 口：乘算(1.5就是*=1.5)"), ShowIf(nameof(ModifyOrReset)), HorizontalGroup("Calculate")]
			public bool CalculateAsAdd = false;

			[LabelText("调整值"), ShowIf(nameof(ModifyOrReset)), HorizontalGroup("Calculate")]
			public float ModifyAddon = 1;

			public override string ToString()
			{
				if (!ModifyOrReset)
				{
					return
						$"{DecisionName} : {(ModifyOrReset ? "" : "重置默认")}";
				}
				else
				{
					return
						$"{DecisionName} : 调整 : { (CalculateAsAdd ? "+" : "*")} : {ModifyAddon}";
				}
			}
		}


		
		[LabelText("跨行为模式调整？")]
		public bool ModifyCrossBehaviourPattern = false;
		
		[SerializeField, LabelText("调整信息们")]
		public List<ModifyInfo> ModifyInfos;
		public override void EnterComponent(SOConfig_AIBrain brain)
		{
			foreach (ModifyInfo perInfo in ModifyInfos)
			{
				if (perInfo.DecisionName == null || perInfo.DecisionName == String.Empty)
				{
					continue;
				}
				if (ModifyCrossBehaviourPattern)
				{
					foreach (var perBehaviourPattern in brain.ConfigContent.BehaviourPatternList)
					{
						foreach (var perDecision in perBehaviourPattern.DecisionList)
						{
							if (perDecision.ConfigContent.DecisionID.Equals(perInfo.DecisionName,
								StringComparison.OrdinalIgnoreCase))
							{
								ConcreteModify(perInfo, perDecision);
							}
						}
					}
				}
				else
				{
					SOConfig_AIDecision targetDecision =
						brain.BrainHandlerFunction.FindSelfDecisionByString(
							perInfo.DecisionName);
					if (targetDecision != null)
					{
						ConcreteModify(perInfo, targetDecision);
					}
				}
			
			}

			void ConcreteModify(ModifyInfo modifyInfo, SOConfig_AIDecision decision)
			{
				//true时为调整，false时为重置
				if (modifyInfo.ModifyOrReset)
				{
					if (modifyInfo.CalculateAsAdd)
					{
						decision.ConfigContent.OriginalPickWeight += modifyInfo.ModifyAddon;
					}
					else
					{
						decision.ConfigContent.OriginalPickWeight *= modifyInfo.ModifyAddon;
					}
				}
				else
				{
					decision.ConfigContent.OriginalPickWeight = decision.OriginalSOAssetTemplate.ConfigContent.OriginalPickWeight;
				}
			}
		}
		
		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} 调整决策权重 :{string.Join(",", ModifyInfos)}";
		}
		
// 		//
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
// 						DCC_调整决策权重_ModifyDecisionWeight newCM =
// 							new DCC_调整决策权重_ModifyDecisionWeight();
// 						var targetP = perSO.ConfigContent.CommonComponents[i];
// 						if (targetP is DCC_ModifyDecisionWeight ex)
// 						{
// 							newCM.ModifyInfos = ex.DeepCopyAsNew(ex.ModifyInfos);
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
// 								DCC_调整决策权重_ModifyDecisionWeight newCM =
// 									new DCC_调整决策权重_ModifyDecisionWeight();
// 								var targetP = aiListenPlayerDistanceListen.Enter1_CommonComponents[i1];
// 								if (targetP is DCC_ModifyDecisionWeight ex)
// 								{
// 									newCM.ModifyInfos = ex.DeepCopyAsNew(ex.ModifyInfos);
// 									aiListenPlayerDistanceListen.Enter1_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
// 						if (aiListenPlayerDistanceListen.Exit1_CommonComponents != null &&
// 						    aiListenPlayerDistanceListen.Exit1_CommonComponents.Count > 0)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Exit1_CommonComponents.Count -1; i1 >=0; i1--)
// 							{
// 								DCC_调整决策权重_ModifyDecisionWeight newCM =
// 									new DCC_调整决策权重_ModifyDecisionWeight();
// 								var targetP = aiListenPlayerDistanceListen.Exit1_CommonComponents[i1];
// 							if (targetP is DCC_ModifyDecisionWeight ex)
// 								{
// 									newCM.ModifyInfos = ex.DeepCopyAsNew(ex.ModifyInfos);
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
// 								DCC_调整决策权重_ModifyDecisionWeight newCM =
// 									new DCC_调整决策权重_ModifyDecisionWeight();
// 								var targetP = aiListenPlayerDistanceListen.Enter2_CommonComponents[i1];
// 							if (targetP is DCC_ModifyDecisionWeight ex)
// 								{
// 									newCM.ModifyInfos = ex.DeepCopyAsNew(ex.ModifyInfos);
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
// 								DCC_调整决策权重_ModifyDecisionWeight newCM =
// 									new DCC_调整决策权重_ModifyDecisionWeight();
// 								var targetP = aiListenPlayerDistanceListen.Exit2_CommonComponents[i1];
// 							if (targetP is DCC_ModifyDecisionWeight ex)
// 								{
// 									newCM.ModifyInfos = ex.DeepCopyAsNew(ex.ModifyInfos);
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
// 								DCC_调整决策权重_ModifyDecisionWeight newCM =
// 									new DCC_调整决策权重_ModifyDecisionWeight();
// 								var targetP = aiListenPlayerDistanceListen.Enter3_CommonComponents[i1];
// 							if (targetP is DCC_ModifyDecisionWeight ex)
// 								{
// 									newCM.ModifyInfos = ex.DeepCopyAsNew(ex.ModifyInfos);
// 									aiListenPlayerDistanceListen.Enter3_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
// 						if (aiListenPlayerDistanceListen.Exit3_CommonComponents != null &&
// 						    aiListenPlayerDistanceListen.Exit3_CommonComponents.Count > 0)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Exit3_CommonComponents.Count - 1; i1 >=0 ; i1--)
// 							{
// 								DCC_调整决策权重_ModifyDecisionWeight newCM =
// 									new DCC_调整决策权重_ModifyDecisionWeight();
// 								var targetP = aiListenPlayerDistanceListen.Exit3_CommonComponents[i1];
// 							if (targetP is DCC_ModifyDecisionWeight ex)
// 								{
// 									newCM.ModifyInfos = ex.DeepCopyAsNew(ex.ModifyInfos);
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
// 								DCC_调整决策权重_ModifyDecisionWeight newCM =
// 									new DCC_调整决策权重_ModifyDecisionWeight();
// 								var targetP = aiListenPlayerDistanceListen.Enter4_CommonComponents[i1];
// 							if (targetP is DCC_ModifyDecisionWeight ex)
// 								{
// 									newCM.ModifyInfos = ex.DeepCopyAsNew(ex.ModifyInfos);
// 									aiListenPlayerDistanceListen.Enter4_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
// 						if (aiListenPlayerDistanceListen.Exit4_CommonComponents != null &&
// 						    aiListenPlayerDistanceListen.Exit4_CommonComponents.Count > 0)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Exit4_CommonComponents.Count - 1; i1 >=0 ; i1--)
// 							{
// 								DCC_调整决策权重_ModifyDecisionWeight newCM =
// 									new DCC_调整决策权重_ModifyDecisionWeight();
// 								var targetP = aiListenPlayerDistanceListen.Exit4_CommonComponents[i1];
// 							if (targetP is DCC_ModifyDecisionWeight ex)
// 								{
// 									newCM.ModifyInfos = ex.DeepCopyAsNew(ex.ModifyInfos);
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
// 								DCC_调整决策权重_ModifyDecisionWeight newCM =
// 									new DCC_调整决策权重_ModifyDecisionWeight();
// 								var targetP = aiListenPlayerDistanceListen.Enter5_CommonComponents[i1];
// 							if (targetP is DCC_ModifyDecisionWeight ex)
// 								{
// 									newCM.ModifyInfos = ex.DeepCopyAsNew(ex.ModifyInfos);
// 									aiListenPlayerDistanceListen.Enter5_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
// 						if (aiListenPlayerDistanceListen.Exit5_CommonComponents != null &&
// 						    aiListenPlayerDistanceListen.Exit5_CommonComponents.Count > 0)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Exit5_CommonComponents.Count - 1; i1 >=0 ; i1--)
// 							{
// 								DCC_调整决策权重_ModifyDecisionWeight newCM =
// 									new DCC_调整决策权重_ModifyDecisionWeight();
// 								var targetP = aiListenPlayerDistanceListen.Exit5_CommonComponents[i1];
// 							if (targetP is DCC_ModifyDecisionWeight ex)
// 								{
// 									newCM.ModifyInfos = ex.DeepCopyAsNew(ex.ModifyInfos);
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
// 								DCC_调整决策权重_ModifyDecisionWeight newCM =
// 									new DCC_调整决策权重_ModifyDecisionWeight();
// 								var targetP = aiListenSelfLayoutHitPlayer.Hit_CommonComponents[i];
// 							if (targetP is DCC_ModifyDecisionWeight ex)
// 								{
// 									newCM.ModifyInfos = ex.DeepCopyAsNew(ex.ModifyInfos);
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