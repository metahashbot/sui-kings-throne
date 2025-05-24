using System;
using System.Collections.Generic;
using ARPG.Character.Enemy.AI.Listen;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	[TypeInfoBox("作用——仅修改指定决策为 开/关 ,其他的全都设置成相反的\n" +
	             "只会调整【当前活跃行为模式】下的")]
	public class DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce : BaseDecisionCommonComponent
	{

		[LabelText("√：调整为开启  || 口：调整为关闭")] [SerializeField]
		public bool _toggleToOn = true;

		[SerializeField, LabelText("关联的决策ID们"), GUIColor(255f / 255f, 223f / 255f, 239f / 255f)]
		public List<string> _relatedDecisionNames = new List<string>();



		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		 {
			if (_relatedDecisionNames == null || _relatedDecisionNames.Count == 0)
			{
				return;
			}
			foreach (SOConfig_AIDecision perDecision in relatedBrain.BrainHandlerFunction.CurrentActiveBehaviourPattern.DecisionList)
			{
				if (_relatedDecisionNames.Contains(perDecision.ConfigContent.DecisionID))
				{
					perDecision.ConfigContent.CanAutoDeduce = _toggleToOn;
				}
				else
				{
					perDecision.ConfigContent.CanAutoDeduce = !_toggleToOn;
				}
			}
		}

		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()}  {(_toggleToOn ? "仅开启" : "开启除此之外的")}  {string.Join(",", _relatedDecisionNames)}";
		}


		//
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
// 						DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce newCM =
// 							new DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce();
// 						var targetP = perSO.ConfigContent.CommonComponents[i];
// 						if (targetP is DCC_ExceptionToggleDecision ex)
// 						{
// 							newCM._relatedDecisionNames = new List<string>(ex._relatedDecisionNames);
// 							newCM._toggleToOn = ex._toggleToOn;
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
// 								DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce newCM =
// 									new DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce();
// 								var targetP = aiListenPlayerDistanceListen.Enter1_CommonComponents[i1];
// 								if (targetP is DCC_ExceptionToggleDecision ex)
// 								{
// 									newCM._relatedDecisionNames = new List<string>(ex._relatedDecisionNames);
// 									newCM._toggleToOn = ex._toggleToOn;
// 									aiListenPlayerDistanceListen.Enter1_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
// 						if (aiListenPlayerDistanceListen.Exit1_CommonComponents != null &&
// 						    aiListenPlayerDistanceListen.Exit1_CommonComponents.Count > 0)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Exit1_CommonComponents.Count -1; i1 >=0; i1--)
// 							{
// 								DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce newCM =
// 									new DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce();
// 								var targetP = aiListenPlayerDistanceListen.Exit1_CommonComponents[i1];
// 								if (targetP is DCC_ExceptionToggleDecision ex)
// 								{
// 									newCM._relatedDecisionNames = new List<string>(ex._relatedDecisionNames);
// 									newCM._toggleToOn = ex._toggleToOn;
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
// 								DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce newCM =
// 									new DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce();
// 								var targetP = aiListenPlayerDistanceListen.Enter2_CommonComponents[i1];
// 								if (targetP is DCC_ExceptionToggleDecision ex)
// 								{
// 									newCM._relatedDecisionNames = new List<string>(ex._relatedDecisionNames);
// 									newCM._toggleToOn = ex._toggleToOn;
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
// 								DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce newCM =
// 									new DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce();
// 								var targetP = aiListenPlayerDistanceListen.Exit2_CommonComponents[i1];
// 								if (targetP is DCC_ExceptionToggleDecision ex)
// 								{
// 									newCM._relatedDecisionNames = new List<string>(ex._relatedDecisionNames);
// 									newCM._toggleToOn = ex._toggleToOn;
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
// 								DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce newCM =
// 									new DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce();
// 								var targetP = aiListenPlayerDistanceListen.Enter3_CommonComponents[i1];
// 								if (targetP is DCC_ExceptionToggleDecision ex)
// 								{
// 									newCM._relatedDecisionNames = new List<string>(ex._relatedDecisionNames);
// 									newCM._toggleToOn = ex._toggleToOn;
// 									aiListenPlayerDistanceListen.Enter3_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
// 						if (aiListenPlayerDistanceListen.Exit3_CommonComponents != null &&
// 						    aiListenPlayerDistanceListen.Exit3_CommonComponents.Count > 0)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Exit3_CommonComponents.Count - 1; i1 >=0 ; i1--)
// 							{
// 								DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce newCM =
// 									new DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce();
// 								var targetP = aiListenPlayerDistanceListen.Exit3_CommonComponents[i1];
// 								if (targetP is DCC_ExceptionToggleDecision ex)
// 								{
// 									newCM._relatedDecisionNames = new List<string>(ex._relatedDecisionNames);
// 									newCM._toggleToOn = ex._toggleToOn;
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
// 								DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce newCM =
// 									new DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce();
// 								var targetP = aiListenPlayerDistanceListen.Enter4_CommonComponents[i1];
// 								if (targetP is DCC_ExceptionToggleDecision ex)
// 								{
// 									newCM._relatedDecisionNames = new List<string>(ex._relatedDecisionNames);
// 									newCM._toggleToOn = ex._toggleToOn;
// 									aiListenPlayerDistanceListen.Enter4_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
// 						if (aiListenPlayerDistanceListen.Exit4_CommonComponents != null &&
// 						    aiListenPlayerDistanceListen.Exit4_CommonComponents.Count > 0)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Exit4_CommonComponents.Count - 1; i1 >=0 ; i1--)
// 							{
// 								DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce newCM =
// 									new DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce();
// 								var targetP = aiListenPlayerDistanceListen.Exit4_CommonComponents[i1];
// 								if (targetP is DCC_ExceptionToggleDecision ex)
// 								{
// 									newCM._relatedDecisionNames = new List<string>(ex._relatedDecisionNames);
// 									newCM._toggleToOn = ex._toggleToOn;
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
// 								DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce newCM =
// 									new DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce();
// 								var targetP = aiListenPlayerDistanceListen.Enter5_CommonComponents[i1];
// 								if (targetP is DCC_ExceptionToggleDecision ex)
// 								{
// 									newCM._relatedDecisionNames = new List<string>(ex._relatedDecisionNames);
// 									newCM._toggleToOn = ex._toggleToOn;
// 									aiListenPlayerDistanceListen.Enter5_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
// 						if (aiListenPlayerDistanceListen.Exit5_CommonComponents != null &&
// 						    aiListenPlayerDistanceListen.Exit5_CommonComponents.Count > 0)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Exit5_CommonComponents.Count - 1; i1 >=0 ; i1--)
// 							{
// 								DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce newCM =
// 									new DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce();
// 								var targetP = aiListenPlayerDistanceListen.Exit5_CommonComponents[i1];
// 								if (targetP is DCC_ExceptionToggleDecision ex)
// 								{
// 									newCM._relatedDecisionNames = new List<string>(ex._relatedDecisionNames);
// 									newCM._toggleToOn = ex._toggleToOn;
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
// 								DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce newCM =
// 									new DCC_黑白名单调整决策自主推演可用性_ExceptionToggleDecisionAutoDeduce();
// 								var targetP = aiListenSelfLayoutHitPlayer.Hit_CommonComponents[i];
// 								if (targetP is DCC_ExceptionToggleDecision ex)
// 								{
// 									newCM._relatedDecisionNames = new List<string>(ex._relatedDecisionNames);
// 									newCM._toggleToOn = ex._toggleToOn;
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