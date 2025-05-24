using System;
using System.Collections.Generic;
using ARPG.Character.Enemy;
using ARPG.Manager;
using DG.Tweening;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.UtilityDataStructure;
using RPGCore.UtilityDataStructure.BuffLogicPassingComponent;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.Skill.RedFang
{
	[Serializable]
	public class Buff_猩红蝙蝠_BatOfRedFang : BaseRPBuff
	{

		[SerializeField, LabelText("VFX_蝙蝠本体") , GUIColor(187f / 255f, 1f, 0f)]
		private string _vfxID_BatSelf;
		
		[SerializeField,LabelText("VFX_蝙蝠攻击") , GUIColor(187f / 255f, 1f, 0f)]
		private string _vfxID_BatAttack;


		[SerializeField,LabelText("蝙蝠攻击间隔")]
		private float _attackInterval = 2f;
		
		[SerializeField,LabelText("蝙蝠攻击范围")]
		private float _attackRange = 5f;
		
		[SerializeField,LabelText("蝙蝠飞过去时长")]
		private float _batFlyDuration = 0.24f;
		
		[SerializeField,LabelText("蝙蝠飞回来时长")]
		private float _batFlyBackDuration = 0.24f;
		
		[SerializeField,LabelText("蝙蝠持续时长")]
		private float _batDuration = 10f;
		
		[SerializeField,LabelText("蝙蝠飞到目的地的伤害信息")]
		 private ConSer_DamageApplyInfo _damageInfo_BatAttack;

		public int GetCurrentBatAmount() { return _batList.Count;}
		
		private class SingleBatInfo
		{
			public ParticleSystem VFXRef_Self;
			public Vector3 OriginalScale;
			public ParticleSystem VFXRef_BatFlying;
			public float RemainingTime;
			public Tweener _tween_SelfRotate;
			public float NextAttackAvailableTime;
		}
		
		private List<SingleBatInfo> _batList = new List<SingleBatInfo>();

		private CharacterOnMapManager _comRef;


		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			_comRef = SubGameplayLogicManager_ARPG.Instance.CharacterOnMapManagerReference;
		}

		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnBuffInitialized(blps);
			return ds;
		}




		public override DS_ActionBusArguGroup OnExistBuffRefreshed(I_RP_Buff_ObjectCanApplyBuff caster, List<BaseBuffLogicPassingComponent> blps)
		{
			return base.OnExistBuffRefreshed(caster, blps);
		}


		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);

			for (int i = _batList.Count - 1; i >= 0; i--)
			{
				var perBat = _batList[i];
				perBat.RemainingTime -= delta;
				RemoveBat(i);

			}


			if (_batList.Count > 0)
			{
				List<EnemyARPGCharacterBehaviour> enemyList = _comRef
					.GetEnemyListInRange(Parent_SelfBelongToObject.GetBuffReceiverPosition(), _attackRange, true)
					.ClipEnemyListOnDefaultType();
				if (enemyList.Count > 0)
				{
					for (int i = _batList.Count - 1; i >= 0; i--)
					{
						EnemyARPGCharacterBehaviour getRandomEnemy =
							enemyList[UnityEngine.Random.Range(0, enemyList.Count)];
						SingleBatInfo perBat = _batList[i];
						if (currentTime > perBat.NextAttackAvailableTime)
						{
							perBat.NextAttackAvailableTime = currentTime + _attackInterval;

							perBat.OriginalScale = perBat.VFXRef_Self.transform.localScale;
							perBat.VFXRef_Self.transform.localScale = Vector3.zero;
							
							perBat.VFXRef_BatFlying =
								VFXPoolManager.Instance.GetParticleSystemRuntimeByPrefab(_VFX_JustGet(_vfxID_BatAttack)
									.Prefab);
							perBat.VFXRef_BatFlying.transform.position = Parent_SelfBelongToObject.GetBuffReceiverPosition();
							
							perBat.VFXRef_BatFlying.transform.DOMove(getRandomEnemy.GetBuffReceiverPosition(), _batFlyDuration)
								.SetEase(Ease.OutCubic).OnComplete((() =>
								{
									if (getRandomEnemy!= null && getRandomEnemy.CharacterDataValid)
									{
										_damageAssistServiceRef.ApplyDamage(
											RP_DS_DamageApplyInfo.BuildDamageApplyInfoFromFromConSer(
												_damageInfo_BatAttack,
												getRandomEnemy,
												Parent_SelfBelongToObject as I_RP_Damage_ObjectCanApplyDamage,
												null));
									}
									perBat.VFXRef_BatFlying.transform.DOMove(Parent_SelfBelongToObject.GetBuffReceiverPosition(),
										_batFlyBackDuration).OnComplete((() =>
									{
										perBat.VFXRef_Self.transform.localScale = perBat.OriginalScale;
										perBat.VFXRef_BatFlying.gameObject.SetActive(false);
									}));
								}));
						}
					}
				}
			}
			
			
		}


		private void GenerateNewBat(int count)
		{
			if (MarkAsRemoved)
			{
				return;
			}
			for (int i = 0; i < count; i++)
			{
				var newInfo = new SingleBatInfo();
				newInfo.VFXRef_Self = _VFX_GetAndSetBeforePlay(_vfxID_BatSelf, true)._VFX__10_PlayThis().GetCurrentMainPSRef;
				newInfo.RemainingTime = _batDuration;
				newInfo._tween_SelfRotate = newInfo.VFXRef_Self.transform
					.DORotate(new Vector3(0, 0, 360), 1f, RotateMode.FastBeyond360).SetLoops(-1, LoopType.Incremental);
				_batList.Add(newInfo);
				
			}
			OnExistBuffRefreshed(Parent_SelfBelongToObject as I_RP_Buff_ObjectCanApplyBuff);
		}

		private void RemoveBat(int index)
		{
			SingleBatInfo perBat = _batList[index];

			if (perBat.RemainingTime < 0f)
			{
				perBat._tween_SelfRotate?.Kill();
				VFXPoolManager.Instance.ReturnParticleSystemToPool(_VFX_JustGet(_vfxID_BatSelf).Prefab, perBat.VFXRef_Self);
				_batList.RemoveAt(index);
			}

			// if (_batList.Count == 0)
			// {
			// 	MarkAsRemoved = true;
			// }
		}

		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);

			switch (blp)
			{
				case BLP_简单层数_JustStackCount stackCount :
					GenerateNewBat(stackCount.StackCount);
					break;
			}
			
		}

		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			VFX_GeneralClear(true);
			return base.OnBuffPreRemove();
			
		}
	}
}