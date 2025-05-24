using System;
using ARPG.Character.Base;
using ARPG.Character.Enemy.AI.Decision.ConcreteDecision;
using RPGCore.Projectile.Layout;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
namespace ARPG.Character.Enemy.AI.Decision.DecisionComponent
{
	[Serializable]
	public class DCC_生成版面_SpawnLayout : BaseDecisionCommonComponent
	{
		

		[SerializeField, LabelText("将要生成的版面"),]
		public SOConfig_ProjectileLayout RelatedConfig;

		[SerializeField, LabelText("需要统一伤害时间戳？")]
		public bool _needSyncDamageTimeStamp = false;

		[SerializeField, LabelText("是否覆盖版面的方向信息")]
		public bool _overrideDirectionFromLayout = true;

		[FormerlySerializedAs("_attackDirectionRelatedToPlayer"), LabelText("攻击方向"),
		 ToggleButtons("与仇恨目标有关","仅与自身朝向有关"),
		 SerializeField, ShowIf(nameof(_overrideDirectionFromLayout))]
		public bool _attackDirectionRelatedToHatredTarget = true;

		[ShowIf(nameof(_overrideDirectionFromLayout))]
		[SerializeField, LabelText("反转发射方向")]
		public bool _flipDirection = false;
		
		
		
		

		[FormerlySerializedAs("_playerDirectionOnEnter"), LabelText("        攻击方向使用 √：进入决策时获得方向  || 口：攻击时的获得方向"),
		 SerializeField, ShowIf("@this._overrideDirectionFromLayout && this._attackDirectionRelatedToHatredTarget")]
		public bool UseDirectionOnEnter = false;

		[SerializeField, LabelText(""),ToggleButtons("生成时改变自身朝向","不改变")]
		public bool _modifyFaceDirectionOnSpawnLayout = false;

		[SerializeField, LabelText("生成时改变的朝向"), ToggleButtons("面朝玩家", "背向玩家")]
		public bool _faceDirectionToPlayer = false;
		

		[SerializeField, LabelText("生成时自动匹配Layout的碰撞层")]
		public bool _autoMatchLayerOnSpawn = true;


		private BaseARPGCharacterBehaviour _relatedBehaviourRef;
		public override void EnterComponent(SOConfig_AIBrain relatedBrain)
		{
			var dh = relatedBrain.BrainHandlerFunction.CurrentRunningDecision.DecisionHandler;
			_relatedBehaviourRef = relatedBrain.BrainHandlerFunction.SelfRelatedARPGCharacterBehaviour;
			if (_modifyFaceDirectionOnSpawnLayout)
			{
				if (_faceDirectionToPlayer)
				{
					dh.QuickFaceHateTargetDirection();
				}
				else
				{
					dh.QuickBackHateTargetDirection();
				}
			}
			var thisLayout = RelatedConfig.SpawnLayout_NoAutoStart(_relatedBehaviourRef, _autoMatchLayerOnSpawn);
			if (_overrideDirectionFromLayout)
			{
				Vector3 finalDirection = Vector3.zero;
				if (_attackDirectionRelatedToHatredTarget)
				{
					if (UseDirectionOnEnter)
					{
						finalDirection = dh.HatredPositionOnEnterDecision;
					}
					else
					{
						finalDirection = dh.GetDirectionToHatredTarget();
					}
				}
				else
				{
					finalDirection = dh.GetSelfCharacterFaceDirection();
				}
				if (_flipDirection)
				{
					finalDirection *= -1f;
				}
				thisLayout.LayoutHandlerFunction.OverrideSpawnFromDirection = finalDirection;
			}
			thisLayout.LayoutHandlerFunction.StartLayout();
		
		}
		public override string GetElementNameInList()
		{
			return $"{GetBaseCustomName()} 生成版面：_{(RelatedConfig ? RelatedConfig.name : "")}_";
		}
	}
}