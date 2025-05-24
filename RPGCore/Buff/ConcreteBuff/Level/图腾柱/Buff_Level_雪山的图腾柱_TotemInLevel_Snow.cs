using System;
using System.Collections.Generic;
using ARPG.Character.Enemy;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Projectile.Layout;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.Level.图腾柱
{
	[Serializable]
	public class Buff_Level_雪山的图腾柱_TotemInLevel_Snow : BaseRPBuff
	{



		[SerializeField, LabelText("额外剔除的类型们")]
		public List<CharacterNamedTypeEnum> _ignoreCharacterList = new List<CharacterNamedTypeEnum>()
		{
			CharacterNamedTypeEnum.Snow_TotemA1_雪山图腾A1
		};
		[SerializeField, LabelText("默认施加间隔")]
		public float _defaultInterval = 10f;
		private float _nextAppendEffectTime;
		
		[SerializeField, LabelText("释放时特效"), FoldoutGroup("配置", true), TitleGroup("配置/VFX"),
		 GUIColor(187f / 255f, 1f, 0f)]
		protected string _vfx_release;

#region 加攻加防的效果

		[SerializeField, LabelText("加攻百分比")]
		protected float _currentATKBonusValue = 10f;
		[SerializeField, LabelText("加防百分比")]
		protected float _currentDEFBonusValue = 10f;

#endregion

#region 加闪避

		protected bool _currentActiveDodgeBonus;

		protected float _currentDodgeBonusValue;

#endregion

#region 回血

		protected bool _currentActiveHPRestore;

		protected float _currentHPPartialRestoreValue;

#endregion
#region 向玩家施加严寒

		protected bool _currentPlayerColdStackActive;
		

		

#endregion



		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnBuffInitialized(blps);
			_nextAppendEffectTime = BaseGameReferenceService.CurrentFixedTime + _defaultInterval;




			return ds;
		}


		private void _SearchCurrentAllEnemyAndApplyBuff(List<EnemyARPGCharacterBehaviour> enemyList)
		{
			foreach (EnemyARPGCharacterBehaviour perEnemy in enemyList)
			{
				if (perEnemy.ApplyDamage_CheckTargetBuff(RolePlay_BuffTypeEnum
					    .Level_角色受雪山图腾柱影响的叠层效果_CharacterAffectByTotemInLevelSnow) !=
				    BuffAvailableType.Available_TimeInAndMeetRequirement)
				{
					perEnemy.ReceiveBuff_TryApplyBuff(
						RolePlay_BuffTypeEnum.Level_角色受雪山图腾柱影响的叠层效果_CharacterAffectByTotemInLevelSnow,
						Parent_SelfBelongToObject as I_RP_Buff_ObjectCanApplyBuff,
						perEnemy);
				}
			}
		}




		private void _Effect_AppendEffect()
		{
			var allEnemyList = _characterOnMapManagerRef.GetAllEnemy().ClipEnemyListOnDefaultType()
				.ClipEnemyListOnCharacterType(_ignoreCharacterList);

			_SearchCurrentAllEnemyAndApplyBuff(allEnemyList);


			foreach (EnemyARPGCharacterBehaviour perEnemy in allEnemyList)
			{
				var targetBuff =
					perEnemy.ApplyDamage_GetTargetBuff(RolePlay_BuffTypeEnum
							.Level_角色受雪山图腾柱影响的叠层效果_CharacterAffectByTotemInLevelSnow) as
						Buff_Level_角色受雪山图腾柱影响的叠层效果_CharacterAffectByTotemInLevelSnow;


				//加攻加防肯定有，直接刷层
				targetBuff.ReceiveNewATKAndDefenseStack(_currentATKBonusValue, _currentDEFBonusValue);

				if (_currentActiveDodgeBonus)
				{
					targetBuff.ReceiveNewDodgeStack(_currentDodgeBonusValue);
				}
				if (_currentActiveHPRestore)
				{
					targetBuff.ReceiveHPRestore(_currentHPPartialRestoreValue);
				}
			}
			if (_currentPlayerColdStackActive)
			{
				var currentPlayer = 
				SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference
					.CurrentControllingBehaviour;
				currentPlayer.ReceiveBuff_TryApplyBuff(RolePlay_BuffTypeEnum.ColdStack_严寒可叠层,
					currentPlayer,
					currentPlayer);
			}
		}





		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);

			if (currentTime > _nextAppendEffectTime)
			{
				_Effect_AppendEffect();
				_VFX_GetAndSetBeforePlay(_vfx_release)?._VFX__10_PlayThis();
				_nextAppendEffectTime += _defaultInterval;
			}
		}




		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			//移除时，会直接移除所有敌人身上的效果buff。即，每消灭一个图腾，会重置场上敌人的所有图腾效果

			var allEnemyList = _characterOnMapManagerRef.GetAllEnemy().ClipEnemyListOnDefaultType()
				.ClipEnemyListOnCharacterType(_ignoreCharacterList);
			for (int i = 0; i < allEnemyList.Count; i++)
			{
				var e = allEnemyList[i];
				e.ReceiveBuff_RemoveTargetBuff(RolePlay_BuffTypeEnum
					.Level_角色受雪山图腾柱影响的叠层效果_CharacterAffectByTotemInLevelSnow);
			}



			return base.OnBuffPreRemove();
		}





		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_Level_雪山图腾柱_附加加攻加防效果_SnowTotemInLevel_AppendATKAndDEF blp_atk:
					_currentATKBonusValue = blp_atk.ATKBonus;
					_currentDEFBonusValue = blp_atk.DEFBonus;
					if (blp_atk.NeedResetInterval)
					{
						_defaultInterval = blp_atk.Interval;
					}

					break;
				case BLP_Level_雪山图腾柱_附加闪避效果 blp_dodge:
					_currentActiveDodgeBonus = true;
					_currentDodgeBonusValue = blp_dodge.DodgeBonus;
					if (blp_dodge.NeedResetInterval)
					{
						_defaultInterval = blp_dodge.Interval;
					}
					break;
				case BLP_Level_雪山图腾柱_附加回血效果 blp_hp:
					_currentActiveHPRestore = true;
					_currentHPPartialRestoreValue = blp_hp.HPRestorePartial / 100f;

					break;
				case BLP_Level_雪山图腾柱_附加对玩家的严寒效果 blp_cold:
					_currentPlayerColdStackActive = true;
					break;
			}
		}

		[Serializable]
		public class BLP_Level_雪山图腾柱_附加加攻加防效果_SnowTotemInLevel_AppendATKAndDEF : BaseBuffLogicPassingComponent
		{


			[SerializeField, LabelText("加攻数值")]
			public float ATKBonus = 10f;

			[SerializeField, LabelText("加防数值")]
			public float DEFBonus = 10f;

			[SerializeField, LabelText("需要重置施加间隔？")]
			public bool NeedResetInterval = false;

			[SerializeField, LabelText("   施加间隔")]
			public float Interval = 10f;


			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_Level_雪山图腾柱_附加加攻加防效果_SnowTotemInLevel_AppendATKAndDEF>.Release(this);
			}
		}



		[Serializable]
		public class BLP_Level_雪山图腾柱_附加闪避效果 : BaseBuffLogicPassingComponent
		{


			[SerializeField, LabelText("加闪避数值")]
			public float DodgeBonus = 10f;


			[SerializeField, LabelText("需要重置施加间隔？")]
			public bool NeedResetInterval = false;

			[SerializeField, LabelText("    施加间隔")]
			public float Interval = 10f;








			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_Level_雪山图腾柱_附加闪避效果>.Release(this);
			}

		}

		[Serializable]
		public class BLP_Level_雪山图腾柱_附加回血效果 : BaseBuffLogicPassingComponent
		{


			[SerializeField, LabelText("回血百分比数值"), SuffixLabel("%")]
			public float HPRestorePartial = 10f;


			[SerializeField, LabelText("需要重置施加间隔？")]
			public bool NeedResetInterval = false;

			[SerializeField, LabelText("    施加间隔")]
			public float Interval = 10f;





			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_Level_雪山图腾柱_附加回血效果>.Release(this);
			}
		}

		[Serializable]
		public class BLP_Level_雪山图腾柱_附加对玩家的严寒效果 : BaseBuffLogicPassingComponent
		{

			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_Level_雪山图腾柱_附加对玩家的严寒效果>.Release(this);
			}
		}

	}


}