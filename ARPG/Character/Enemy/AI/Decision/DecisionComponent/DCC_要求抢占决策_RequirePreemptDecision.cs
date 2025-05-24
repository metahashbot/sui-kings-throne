using System;
using System.Collections.Generic;
using ARPG.Character.Enemy.AI.Listen;
using Global.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	[TypeInfoBox("作用——要求【抢占】一个决策")]
	public class DCC_要求抢占决策_RequirePreemptDecision : BaseDecisionCommonComponent
	{
		[LabelText("√:单一决策 | 口:复合权重决策")]
		public bool _isSingleDecision = true;

		[LabelText("符合权重决策"), SerializeField, HideIf(nameof(_isSingleDecision))]
		public MultipleDecisionPickConfig _multipleDecisionPickConfig;


		[LabelText("决策名"), SerializeField]
		[ShowIf(nameof(_isSingleDecision))]
		protected string _targetDecisionName;

		[LabelText("同决策时不抢占"), SerializeField]
		protected bool _notPreemptOnSame = false;
		

		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			SOConfig_AIDecision targetDecision = null;
			if (_isSingleDecision)
			{
				if (string.IsNullOrEmpty(_targetDecisionName))
				{
					return;
				}

				if (_notPreemptOnSame)
				{
					var currentDecision = relatedBrain.BrainHandlerFunction.CurrentRunningDecision;
					if (currentDecision.ConfigContent.DecisionID.Equals(_targetDecisionName,
						StringComparison.OrdinalIgnoreCase))
					{
						return;
					}
				}
				targetDecision =
					relatedBrain.BrainHandlerFunction.FindSelfDecisionByString(
						_targetDecisionName);
			}
			else
			{
				if (_multipleDecisionPickConfig == null || _multipleDecisionPickConfig.PerDecisionPickInfos == null ||
				    _multipleDecisionPickConfig.PerDecisionPickInfos.Count == 0)
				{
					return;
				}
				//进行加权挑选
				var pp = CollectionPool<List<KeyValuePair<string, float>>, KeyValuePair<string, float>>.Get();
				foreach (MultipleDecisionPickConfig.PerDecisionPickInfo perDD in _multipleDecisionPickConfig
					.PerDecisionPickInfos)
				{
					pp.Add(new KeyValuePair<string, float>(perDD.DecisionName, perDD.Weight));
				}

				var picked = ShuffleUtility.ShuffleKeyValuePairList(pp)[0];
				CollectionPool<List<KeyValuePair<string, float>>, KeyValuePair<string, float>>.Release(pp);

				if (_notPreemptOnSame)
				{
					var currentDecision = relatedBrain.BrainHandlerFunction.CurrentRunningDecision;
					if (currentDecision.ConfigContent.DecisionID.Equals(picked, StringComparison.OrdinalIgnoreCase));
					{
						return;
					}
				}
				targetDecision =
					relatedBrain.BrainHandlerFunction.FindSelfDecisionByString(picked);
			}

			if (targetDecision == null)
			{
				DBug.LogError($"AIBrain{relatedBrain.name}的一个【作用-抢占】要求排决策{_targetDecisionName}，但是这个Brain并没有记录这个名字的决策");
				return;
			}
			relatedBrain.BrainHandlerFunction.AddDecisionToQueue(targetDecision,
				BaseAIBrainHandler.DecisionEnqueueType.FullClearAndEnqueueFirst_清空并置首);
		}

		
		public override string GetElementNameInList()
		{

			if (_isSingleDecision)
			{
				return $"{GetBaseCustomName()}  要求抢占决策：[{_targetDecisionName}]";
			}
			else
			{
				return $"{GetBaseCustomName()}  要求抢占决策：[复合权重]";
			}
		
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
// 						DCC_要求抢占决策_RequirePreemptDecision newCM =
// 							new DCC_要求抢占决策_RequirePreemptDecision();
// 						var targetP = perSO.ConfigContent.CommonComponents[i];
// 						if (targetP is DCC_RequirePreemptDecision ex)
// 						{
// 							newCM._targetDecisionName = ex._targetDecisionName;
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
// 								DCC_要求抢占决策_RequirePreemptDecision newCM =
// 									new DCC_要求抢占决策_RequirePreemptDecision();
// 								var targetP = aiListenPlayerDistanceListen.Enter1_CommonComponents[i1];
// 								if (targetP is DCC_RequirePreemptDecision ex)
// 								{
// 									newCM._targetDecisionName = ex._targetDecisionName;
// 									aiListenPlayerDistanceListen.Enter1_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
// 						if (aiListenPlayerDistanceListen.Exit1_CommonComponents != null &&
// 						    aiListenPlayerDistanceListen.Exit1_CommonComponents.Count > 0)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Exit1_CommonComponents.Count -1; i1 >=0; i1--)
// 							{
// 								DCC_要求抢占决策_RequirePreemptDecision newCM =
// 									new DCC_要求抢占决策_RequirePreemptDecision();
// 								var targetP = aiListenPlayerDistanceListen.Exit1_CommonComponents[i1];
// 							if (targetP is DCC_RequirePreemptDecision ex)
// 								{
// 									newCM._targetDecisionName = ex._targetDecisionName;
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
// 								DCC_要求抢占决策_RequirePreemptDecision newCM =
// 									new DCC_要求抢占决策_RequirePreemptDecision();
// 								var targetP = aiListenPlayerDistanceListen.Enter2_CommonComponents[i1];
// 							if (targetP is DCC_RequirePreemptDecision ex)
// 								{
// 									newCM._targetDecisionName = ex._targetDecisionName;
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
// 								DCC_要求抢占决策_RequirePreemptDecision newCM =
// 									new DCC_要求抢占决策_RequirePreemptDecision();
// 								var targetP = aiListenPlayerDistanceListen.Exit2_CommonComponents[i1];
// 							if (targetP is DCC_RequirePreemptDecision ex)
// 								{
// 									newCM._targetDecisionName = ex._targetDecisionName;
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
// 								DCC_要求抢占决策_RequirePreemptDecision newCM =
// 									new DCC_要求抢占决策_RequirePreemptDecision();
// 								var targetP = aiListenPlayerDistanceListen.Enter3_CommonComponents[i1];
// 							if (targetP is DCC_RequirePreemptDecision ex)
// 								{
// 									newCM._targetDecisionName = ex._targetDecisionName;
// 									aiListenPlayerDistanceListen.Enter3_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
// 						if (aiListenPlayerDistanceListen.Exit3_CommonComponents != null &&
// 						    aiListenPlayerDistanceListen.Exit3_CommonComponents.Count > 0)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Exit3_CommonComponents.Count - 1; i1 >=0 ; i1--)
// 							{
// 								DCC_要求抢占决策_RequirePreemptDecision newCM =
// 									new DCC_要求抢占决策_RequirePreemptDecision();
// 								var targetP = aiListenPlayerDistanceListen.Exit3_CommonComponents[i1];
// 							if (targetP is DCC_RequirePreemptDecision ex)
// 								{
// 									newCM._targetDecisionName = ex._targetDecisionName;
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
// 								DCC_要求抢占决策_RequirePreemptDecision newCM =
// 									new DCC_要求抢占决策_RequirePreemptDecision();
// 								var targetP = aiListenPlayerDistanceListen.Enter4_CommonComponents[i1];
// 							if (targetP is DCC_RequirePreemptDecision ex)
// 								{
// 									newCM._targetDecisionName = ex._targetDecisionName;
// 									aiListenPlayerDistanceListen.Enter4_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
// 						if (aiListenPlayerDistanceListen.Exit4_CommonComponents != null &&
// 						    aiListenPlayerDistanceListen.Exit4_CommonComponents.Count > 0)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Exit4_CommonComponents.Count - 1; i1 >=0 ; i1--)
// 							{
// 								DCC_要求抢占决策_RequirePreemptDecision newCM =
// 									new DCC_要求抢占决策_RequirePreemptDecision();
// 								var targetP = aiListenPlayerDistanceListen.Exit4_CommonComponents[i1];
// 							if (targetP is DCC_RequirePreemptDecision ex)
// 								{
// 									newCM._targetDecisionName = ex._targetDecisionName;
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
// 								DCC_要求抢占决策_RequirePreemptDecision newCM =
// 									new DCC_要求抢占决策_RequirePreemptDecision();
// 								var targetP = aiListenPlayerDistanceListen.Enter5_CommonComponents[i1];
// 							if (targetP is DCC_RequirePreemptDecision ex)
// 								{
// 									newCM._targetDecisionName = ex._targetDecisionName;
// 									aiListenPlayerDistanceListen.Enter5_CommonComponents[i1] = newCM;
// 								}
// 							}
// 						}
// 						if (aiListenPlayerDistanceListen.Exit5_CommonComponents != null &&
// 						    aiListenPlayerDistanceListen.Exit5_CommonComponents.Count > 0)
// 						{
// 							for (int i1 = aiListenPlayerDistanceListen.Exit5_CommonComponents.Count - 1; i1 >=0 ; i1--)
// 							{
// 								DCC_要求抢占决策_RequirePreemptDecision newCM =
// 									new DCC_要求抢占决策_RequirePreemptDecision();
// 								var targetP = aiListenPlayerDistanceListen.Exit5_CommonComponents[i1];
// 							if (targetP is DCC_RequirePreemptDecision ex)
// 								{
// 									newCM._targetDecisionName = ex._targetDecisionName;
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
// 								DCC_要求抢占决策_RequirePreemptDecision newCM =
// 									new DCC_要求抢占决策_RequirePreemptDecision();
// 								var targetP = aiListenSelfLayoutHitPlayer.Hit_CommonComponents[i];
// 							if (targetP is DCC_RequirePreemptDecision ex)
// 								{
// 									newCM._targetDecisionName = ex._targetDecisionName;
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