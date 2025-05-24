using System;
using ARPG.Character.Enemy.AI.Listen;
using Global.ActionBus;
using Sirenix.OdinInspector;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	[TypeInfoBox("作用——要求立刻【自主推演】")]
	public class DCC_要求立刻自主推演_RequireAutoDeduce : BaseDecisionCommonComponent
	{
		[LabelText("√ 即时当前不能推演也立刻推演  |  口  仅尝试")]
		public bool ForceAutoDeduce = true;

		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			var ds_autoDeduce =
				new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.L_AISideEffect_OnSideEffectRequireAutoDeduce_AI副作用要求自主推演);
			ds_autoDeduce.IntArgu1 = ForceAutoDeduce ? 1 : 0;

			relatedBrain.BrainHandlerFunction.SelfLocalActionBusRef.TriggerActionByType(ds_autoDeduce);
		}

		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} {(ForceAutoDeduce ? "立刻" : "尝试")} 自主推演";
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
// 						DCC_要求立刻自主推演_RequireAutoDeduce newCM =
// 							new DCC_要求立刻自主推演_RequireAutoDeduce();
// 						var targetP = perSO.ConfigContent.CommonComponents[i];
// 						if (targetP is DCC_RequireAutoDeduce ex)
// 						{
// 							newCM.ForceAutoDeduce = ex.ForceAutoDeduce;
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
// 								DCC_要求立刻自主推演_RequireAutoDeduce newCM =
// 									new DCC_要求立刻自主推演_RequireAutoDeduce();
// 								var targetP = aiListenPlayerDistanceListen.Enter1_CommonComponents[i1];
// 								if (targetP is DCC_RequireAutoDeduce ex)
// 								{
// 									newCM.ForceAutoDeduce = ex.ForceAutoDeduce;
// 									aiListenPlayerDistanceListen.Enter1_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
// 						if (aiListenPlayerDistanceListen.Exit1_CommonComponents != null &&
// 						    aiListenPlayerDistanceListen.Exit1_CommonComponents.Count > 0)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Exit1_CommonComponents.Count -1; i1 >=0; i1--)
// 							{
// 								DCC_要求立刻自主推演_RequireAutoDeduce newCM =
// 									new DCC_要求立刻自主推演_RequireAutoDeduce();
// 								var targetP = aiListenPlayerDistanceListen.Exit1_CommonComponents[i1];
// 							if (targetP is DCC_RequireAutoDeduce ex)	
// 								{
// 									newCM.ForceAutoDeduce = ex.ForceAutoDeduce;
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
// 								DCC_要求立刻自主推演_RequireAutoDeduce newCM =
// 									new DCC_要求立刻自主推演_RequireAutoDeduce();
// 								var targetP = aiListenPlayerDistanceListen.Enter2_CommonComponents[i1];
// 							if (targetP is DCC_RequireAutoDeduce ex)
// 								{
// 									newCM.ForceAutoDeduce = ex.ForceAutoDeduce;
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
// 								DCC_要求立刻自主推演_RequireAutoDeduce newCM =
// 									new DCC_要求立刻自主推演_RequireAutoDeduce();
// 								var targetP = aiListenPlayerDistanceListen.Exit2_CommonComponents[i1];
// 							if (targetP is DCC_RequireAutoDeduce ex)
// 								{
// 									newCM.ForceAutoDeduce = ex.ForceAutoDeduce;
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
// 								DCC_要求立刻自主推演_RequireAutoDeduce newCM =
// 									new DCC_要求立刻自主推演_RequireAutoDeduce();
// 								var targetP = aiListenPlayerDistanceListen.Enter3_CommonComponents[i1];
// 							if (targetP is DCC_RequireAutoDeduce ex)
// 								{
// 									newCM.ForceAutoDeduce = ex.ForceAutoDeduce;
// 									aiListenPlayerDistanceListen.Enter3_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
// 						if (aiListenPlayerDistanceListen.Exit3_CommonComponents != null &&
// 						    aiListenPlayerDistanceListen.Exit3_CommonComponents.Count > 0)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Exit3_CommonComponents.Count - 1; i1 >=0 ; i1--)
// 							{
// 								DCC_要求立刻自主推演_RequireAutoDeduce newCM =
// 									new DCC_要求立刻自主推演_RequireAutoDeduce();
// 								var targetP = aiListenPlayerDistanceListen.Exit3_CommonComponents[i1];
// 							if (targetP is DCC_RequireAutoDeduce ex)
// 								{
// 									newCM.ForceAutoDeduce = ex.ForceAutoDeduce;
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
// 								DCC_要求立刻自主推演_RequireAutoDeduce newCM =
// 									new DCC_要求立刻自主推演_RequireAutoDeduce();
// 								var targetP = aiListenPlayerDistanceListen.Enter4_CommonComponents[i1];
// 							if (targetP is DCC_RequireAutoDeduce ex)
// 								{
// 									newCM.ForceAutoDeduce = ex.ForceAutoDeduce;
// 									aiListenPlayerDistanceListen.Enter4_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
// 						if (aiListenPlayerDistanceListen.Exit4_CommonComponents != null &&
// 						    aiListenPlayerDistanceListen.Exit4_CommonComponents.Count > 0)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Exit4_CommonComponents.Count - 1; i1 >=0 ; i1--)
// 							{
// 								DCC_要求立刻自主推演_RequireAutoDeduce newCM =
// 									new DCC_要求立刻自主推演_RequireAutoDeduce();
// 								var targetP = aiListenPlayerDistanceListen.Exit4_CommonComponents[i1];
// 							if (targetP is DCC_RequireAutoDeduce ex)
// 								{
// 									newCM.ForceAutoDeduce = ex.ForceAutoDeduce;
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
// 								DCC_要求立刻自主推演_RequireAutoDeduce newCM =
// 									new DCC_要求立刻自主推演_RequireAutoDeduce();
// 								var targetP = aiListenPlayerDistanceListen.Enter5_CommonComponents[i1];
// 							if (targetP is DCC_RequireAutoDeduce ex)
// 								{
// 									newCM.ForceAutoDeduce = ex.ForceAutoDeduce;
// 									aiListenPlayerDistanceListen.Enter5_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
// 						if (aiListenPlayerDistanceListen.Exit5_CommonComponents != null &&
// 						    aiListenPlayerDistanceListen.Exit5_CommonComponents.Count > 0)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Exit5_CommonComponents.Count - 1; i1 >=0 ; i1--)
// 							{
// 								DCC_要求立刻自主推演_RequireAutoDeduce newCM =
// 									new DCC_要求立刻自主推演_RequireAutoDeduce();
// 								var targetP = aiListenPlayerDistanceListen.Exit5_CommonComponents[i1];
// 							if (targetP is DCC_RequireAutoDeduce ex)
// 								{
// 									newCM.ForceAutoDeduce = ex.ForceAutoDeduce;
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
// 								DCC_要求立刻自主推演_RequireAutoDeduce newCM =
// 									new DCC_要求立刻自主推演_RequireAutoDeduce();
// 								var targetP = aiListenSelfLayoutHitPlayer.Hit_CommonComponents[i];
// 							if (targetP is DCC_RequireAutoDeduce ex)
// 								{
// 									newCM.ForceAutoDeduce = ex.ForceAutoDeduce;
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