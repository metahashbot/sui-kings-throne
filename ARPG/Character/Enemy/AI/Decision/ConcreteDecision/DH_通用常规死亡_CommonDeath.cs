using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using ARPG.Character.Base.CustomSpineData;
using ARPG.Character.Player.Ally;
using Global.ActionBus;
using RPGCore.Projectile.Layout;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.ConcreteDecision
{
	[TypeInfoBox("通用的死亡。直接进入到死亡决策并不会死亡，想要自杀要么通过自爆决策要么把血打到负数")]
	[Serializable] 
	public class DH_通用常规死亡_CommonDeath : BaseDecisionHandler
	{
		[Header("=== 动画 ===")]
		[SerializeField, LabelText("使用的动画配置名"), FoldoutGroup("配置", true), GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		public string _an_Death;


		[Header("=== 尸弹 ===")]
		[InfoBox("【尸弹】功能本身无法实现类似于“死亡xx秒后生成“，若要这么做，则直接配置进版面。")]
		[SerializeField, LabelText("包含死亡时尸弹"), FoldoutGroup("配置", true)]
		public bool _containDeathLayout;

		[SerializeField, LabelText("       尸弹版面"), FoldoutGroup("配置", true), ShowIf(nameof(_containDeathLayout)),
		 InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		public List<SOConfig_ProjectileLayout> _relatedLayout ;
		


		public override void Initialize(SOConfig_AIBrain config)
		{
			base.Initialize(config);
			_sai_death = GetAnimationInfoFromBrain(_an_Death);
		}

		
		protected AnimationInfoBase _sai_death;

		public override DS_ActionBusArguGroup OnDecisionBeforeStartExecution(bool withSideEffect = true)
		{
			var ds_start = base.OnDecisionBeforeStartExecution(withSideEffect);
			
			
			

			_Internal_RequireAnimation(_sai_death);
			_selfRelatedBehaviourRef.GetRelatedArtHelper().SetAllAnimationLogicSpeedMul(1f);
			_Internal_BlockBrainAutoDeduce();
		
			
			
			if (_containDeathLayout && _relatedLayout != null)
			{
				foreach (var lay in _relatedLayout)
				{
					lay.SpawnLayout(_selfRelatedBehaviourRef);
				}
			}
			
			
			return ds_start;
		}


        protected override bool CheckIfCurrentBrainValidAndCurrentDecisionIsThis(SOConfig_AIDecision compareDecision)
		{
            if (SelfRelatedBrainInstanceRef == null)
            {
                return false;
            }
            if (_selfRelatedBehaviourRef is AllyARPGCharacterBehaviour ally)
            {
                if (ally.GetAIBrainRuntimeInstance() == null)
                {
                    return false;
                }
            }
            else if (_selfRelatedBehaviourRef is EnemyARPGCharacterBehaviour enemy)
            {
                if (enemy.GetAIBrainRuntimeInstance() == null)
                {
                    return false;
                }
            }
            if (compareDecision == null)
            {
                return false;
            }
            if (compareDecision !=
                this.SelfRelatedDecisionRuntimeInstance)
            {
                return false;
            }
            return true;
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
		// 				DH_通用常规死亡_CommonDeath newCM = new DH_通用常规死亡_CommonDeath();
		// 				switch (perSO.DecisionHandler)
		// 				{
		// 					case CommonDeath_DecisionHandler cd:
		// 						newCM._an_Death = cd._an_Death;
		// 						perSO.DecisionHandler = newCM;
		// 						UnityEditor.EditorUtility.SetDirty(perSO);
		// 						UnityEditor.AssetDatabase.SaveAssetIfDirty(perSO);
		// 						break;
		// 					case CommonDeathWithLayout_DecisionHandler cdh:
		// 						newCM._an_Death = cdh._an_Death;
		// 						perSO.DecisionHandler = newCM;
		// 						UnityEditor.EditorUtility.SetDirty(perSO);
		// 						UnityEditor.AssetDatabase.SaveAssetIfDirty(perSO);
		// 						break;
		// 				}
		// 			}
		// 		}

	}
}