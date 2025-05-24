using System;
using ARPG.Character.Base;
using ARPG.Character.Base.CustomSpineData;
using Global;
using Global.ActionBus;
using RPGCore.DataEntry;
using RPGCore.Projectile.Layout;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Character.Enemy.AI.Decision.ConcreteDecision
{
	[Serializable]
	[TypeInfoBox("通用自爆\n自爆并不总是会让角色死亡，它依然需要一个正常的死亡动画。")]
	public class DH_通用常规自爆_CommonSelfExplosion : BaseDecisionHandler
	{
		
		/*
		 * 有两种情况，分别是立刻自爆和自爆动画
		 * A.直接去世
		 *      不需要动画，也与动画流程无关，不会播任何动画。在进入该决策时会直接要求去世。此时才会用到NonAECLayout。
		 *      也不会走死亡动画流程。直接从Behaviour那里走 [敌人死亡没有尸体] 的流程，下一帧就消失了
		 *      可选NonAECLayout来补偿一个版面
		 *
		 * B.走个过程
		 *      B.1 自爆动画结束后立刻去世，不会进死亡动画和死亡决策
		 *		B.2 自爆动画后转到正常的死亡决策
		 *
		 *
		 *
		 */

		[SerializeField, LabelText("√:A.直接去世  |  口:B.走个过程"), TitleGroup("===配置===")]
		protected bool _selfDieWithoutAnimation;


		[SerializeField, LabelText("A.补偿版面"), TitleGroup("===配置===")]
		[ShowIf(nameof(_selfDieWithoutAnimation))]
		protected SOConfig_ProjectileLayout _layout_SelfExplosion;
		
		protected SOConfig_ProjectileLayout _runtimeLayout;


		[SerializeField, LabelText("√:B.1 自爆后立刻去世 | 口:B.2 自爆后转到常规死亡"), TitleGroup("===配置===")]
		[HideIf(nameof(_selfDieWithoutAnimation))]
		protected bool _dieDirectAfterExplosionAnimation;
		
		
		[SerializeField, LabelText("B._AN_自爆动画配置名"), TitleGroup("===配置==="),
		 GUIColor(149f / 255f, 249 / 255f, 150 / 255f)]
		[HideIf(nameof(_selfDieWithoutAnimation))]
		protected string _an_explosionAnimation;
		
		

		public override void Initialize(SOConfig_AIBrain config)
		{
			base.Initialize(config);
		}

		public override DS_ActionBusArguGroup OnDecisionBeforeStartExecution(bool withSideEffect = true)
		{
			var ds_pick = base.OnDecisionBeforeStartExecution(withSideEffect);
			
			
			//开始执行了，
			//如果不需要自爆动画，那就是直接去世，无需处理动画等，决策后面的业务也是直接返回
			if (_selfDieWithoutAnimation)
			{
				var ds_dieDirect =
					new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_Damage_RequireDirectSelfExplosion_要求直接自爆);
				if (_layout_SelfExplosion != null)
				{
					_runtimeLayout = _layout_SelfExplosion.SpawnLayout_NoAutoStart(_selfRelatedBehaviourRef);
					_runtimeLayout.LayoutHandlerFunction.OverrideSpawnFromPosition =
						_selfRelatedBehaviourRef.transform.position;
					_runtimeLayout.LayoutHandlerFunction.StartLayout();
				}
				_selfRelatedBehaviourRef.GetRelatedActionBus().TriggerActionByType(ds_dieDirect);
				return ds_pick;
			}
			else
			{
				//需要自爆动画，那就播动画
				_Internal_RequireAnimation(GetAnimationInfoFromBrain(_an_explosionAnimation));

			}
			

			
			
		
			return ds_pick;
		}
		
		


		public override void FixedUpdateTick(float ct, int cf, float delta)
		{
			base.FixedUpdateTick(ct, cf, delta);
			
		}



		protected override void _ABC_ProcessGeneralAnimationComplete_OnAnimationComplete(DS_ActionBusArguGroup ds)
		{
			base._ABC_ProcessGeneralAnimationComplete_OnAnimationComplete(ds);
			if (!CheckIfCurrentBrainValidAndCurrentDecisionIsThis(ds.ObjectArguStr as SOConfig_AIDecision))
			{
				return;
			}
			if (!_selfDieWithoutAnimation)
			{
				if ((ds.ObjectArguStr as string).Equals(_an_explosionAnimation, StringComparison.OrdinalIgnoreCase))
				{
					//看看是直接去世还是走死亡流程
					if (_dieDirectAfterExplosionAnimation)
					{
						var ds_dieDirect =
							new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
								.L_Damage_RequireDirectSelfExplosion_要求直接自爆);
						if (_layout_SelfExplosion != null)
						{
							_layout_SelfExplosion.SpawnLayout_NoAutoStart(_selfRelatedBehaviourRef);
							_layout_SelfExplosion.LayoutHandlerFunction.OverrideSpawnFromPosition =
								_selfRelatedBehaviourRef.transform.position;
							_layout_SelfExplosion.LayoutHandlerFunction.StartLayout();
						}
						_selfRelatedBehaviourRef.GetRelatedActionBus().TriggerActionByType(ds_dieDirect);
						return;
					}
					//正常流程，那就是打死自己
					else
					{
						_selfRelatedBehaviourRef.GetFloatPresentValueByType(RP_DataEntry_EnumType.CurrentHP_当前HP)
							.ResetDataToValue(0f);
						RP_DS_DamageApplyInfo dai = RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromRuntime(
							_selfRelatedBehaviourRef,
							_selfRelatedBehaviourRef,
							DamageTypeEnum.TrueDamage_真伤,
							999999f);
						dai.StepOption = DamageProcessStepOption.SelfExplosionDPS();
						dai.DamageWorldPosition = _selfRelatedBehaviourRef.transform.position;
						var r = _glmArpgRef.DamageAssistServiceInstance.ApplyDamage(dai);
						r.ReleaseToPool();
						return;
					}
				
					return;
				}

				
			}
			
		}

	}
}