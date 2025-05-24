using System;
using ARPG.Character.Base;
using ARPG.Character.Base.CustomSpineData;
using Global;
using Global.ActionBus;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.ConcreteDecision
{
	[TypeInfoBox("通用的战斗中发呆")]
	[Serializable]
	public class DH_通用常规发呆_CommonIdle : BaseDecisionHandler
	{
		[SerializeField, LabelText("使用的发呆动画 动画信息名"), FoldoutGroup("配置", true),
		 GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		protected string _AN_IdleSpineAnimation;


		[SerializeField, LabelText("最小发呆时长"), FoldoutGroup("配置", true)]
		protected float _minIdleDuration = 0.5f;
		[SerializeField, LabelText("最大发呆时长"), FoldoutGroup("配置", true)]
		protected float _maxIdleDuration = 1.5f;



		[Header("=== 结束 ===")]
		[LabelText("结束：达到最大时间后结束"), FoldoutGroup("配置", true), SerializeField]
		protected bool _stopOnTimeEnd = true;

		[SerializeField, LabelText("结束：与玩家距离过近时结束"), FoldoutGroup("配置", true)]
		protected bool _stopOnPlayerNear = true;

		[SerializeReference, LabelText("       与玩家距离小于该值时结束当前决策"), FoldoutGroup("配置", true),ShowIf(nameof(_stopOnPlayerNear))]
		protected DecisionDistanceInfoComponent _stopDistance = new DD_纯数值_PureValue
		{
			ValueMultiplier = 1,
			AbsoluteValue = 1
		};


		[SerializeField,LabelText("结束：与仇恨目标距离过近时结束"),FoldoutGroup("配置",true)]
		protected bool _stopOnHateTargetNear = true;
		
		[SerializeReference, LabelText("       与仇恨目标距离小于该值时结束当前决策"), FoldoutGroup("配置", true),ShowIf(nameof(_stopOnHateTargetNear))]
		protected DecisionDistanceInfoComponent _stopHateTargetDistance = new DD_纯数值_PureValue
		{
			ValueMultiplier = 1,
			AbsoluteValue = 1
		};


		[ShowInInspector, LabelText("结束时间"), FoldoutGroup("运行时", true)]
		protected float _stopTime;

		protected AnimationInfoBase _sai_idleAnimation;





		public override void Initialize(SOConfig_AIBrain config)
		{
			base.Initialize(config);
			_sai_idleAnimation = GetAnimationInfoFromBrain(_AN_IdleSpineAnimation);
		}

		public override DS_ActionBusArguGroup OnDecisionBeforeStartExecution(bool withSideEffect = true)
		{
			var ds = base.OnDecisionBeforeStartExecution(withSideEffect);
			if (_stopOnTimeEnd)
			{
				_stopTime = UnityEngine.Random.Range(_minIdleDuration, _maxIdleDuration) +
				            BaseGameReferenceService.CurrentTimeInSecond;
			}


			_Internal_RequireAnimation(_sai_idleAnimation);

			return ds;
		}


		public override void FixedUpdateTick(float ct, int cf, float delta)
		{
			base.FixedUpdateTick(ct, cf, delta);

			if (_stopOnTimeEnd)
			{
				if (ct > _stopTime)
				{
					OnDecisionNormalComplete();
					return;
				}
			}

			if (_stopOnPlayerNear)
			{
				var stopDistanceV = _stopDistance.GetDistanceValue(SelfRelatedBrainInstanceRef);
				if (GetSelfToPlayerDistanceSQ() < stopDistanceV * stopDistanceV)
				{
					OnDecisionNormalComplete();
					return;
				}
			}
			
			if (_stopOnHateTargetNear)
			{
				var stopDistanceV = _stopHateTargetDistance.GetDistanceValue(SelfRelatedBrainInstanceRef);
				if (GetSelfToHatredDistanceSQ() < stopDistanceV * stopDistanceV)
				{
					OnDecisionNormalComplete();
					return;
				}
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
// 				DH_通用常规发呆_CommonIdle newCM = new DH_通用常规发呆_CommonIdle();
// 				switch (perSO.DecisionHandler)
// 				{
// 					case DecisionHandler_CommonBattleIdle cbi:
// 						newCM._AN_IdleSpineAnimation = cbi._AN_IdleSpineAnimation;
// 						newCM._minIdleDuration = cbi.MinIdleDuration;
// 						newCM._maxIdleDuration = cbi.MaxIdleDuration;
// 						
// 						perSO.DecisionHandler = newCM;
// 						UnityEditor.EditorUtility.SetDirty(perSO);
// 						UnityEditor.AssetDatabase.SaveAssetIfDirty(perSO);
// 						break;
// 				}
// 			}
// 		}



	}
}