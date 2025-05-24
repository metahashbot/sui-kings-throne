using System;
using System.Collections.Generic;
using ARPG.Character;
using Global.ActionBus;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.Projectile.Layout;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Buff.ConcreteBuff.Boss
{
	[Serializable]
	public class Buff_焚焰与喷射炎珠_BurningAndFireBeadByFlameDragon : BaseRPBuff , I_BuffTransferWithinPlayer
	{

		[SerializeField, LabelText("作为炎珠的版面")]
		[TitleGroup("===具体配置===")]
		private SOConfig_ProjectileLayout _fireBeadLayout;



		[SerializeField, LabelText("作为炎珠的特效"), GUIColor(187f / 255f, 1f, 0f)]
		[TitleGroup("===具体配置===")]
		private string _fireBeadVFXID;

		public class PerFireBeadInfo
		{
			public SOConfig_ProjectileLayout RuntimeLayoutRef;
			public Vector3 SpawnPosition;
			public Vector3 Direction;
			public ParticleSystem RelatedPSRef;
			public float RemainingDuration;
		}


		[SerializeField, LabelText("多少焚焰值才会释放炎珠")]
		[TitleGroup("===具体配置===")]
		private float _fireBeadReleaseThreshold = 100f;


		[ShowInInspector,LabelText("当前焚焰值") ,FoldoutGroup("运行时")]
		public float CurrentFireBeadValue { get; private set; }

		private List<PerFireBeadInfo> _fireBeadRuntimeInfoList = new List<PerFireBeadInfo>();


		[SerializeField, LabelText("焚焰值积累速度 单位/s")]
		[TitleGroup("===具体配置===")]
		private float _fireBeadAccumulateSpeedPerSecond;
		private I_RP_Buff_ObjectCanReceiveBuff _originalParent;

        [SerializeField, LabelText("初始化时的激活的特效")]
        [TitleGroup("===具体配置===")]
        private string _initEffectName;

        [SerializeField, LabelText("间隔释放的特效")]
        [TitleGroup("===具体配置===")]
        private string _intervalEffectName;
        
        [SerializeField, LabelText("间隔释放的特效阈值")]
        [TitleGroup("===具体配置===")]
        private float _IntervalEffectTriggerThreshold = 80f;

        private PerVFXInfo _initEffectHandle;
        private PerVFXInfo _intervalEffectHandle;

        private PlayerARPGConcreteCharacterBehaviour _playerBehaviourRef;


		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);
			_playerBehaviourRef  = Parent_SelfBelongToObject as PlayerARPGConcreteCharacterBehaviour;
			_originalParent = parent;

            _initEffectHandle = _VFX_GetAndSetBeforePlay(_initEffectName)._VFX__10_PlayThis();
			_intervalEffectHandle = _VFX_GetAndSetBeforePlay(_intervalEffectName).VFX_StopThis(true); ;

        }
		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);
			
			CurrentFireBeadValue += _fireBeadAccumulateSpeedPerSecond * delta;

			if (CurrentFireBeadValue > _IntervalEffectTriggerThreshold && !_intervalEffectHandle.GetCurrentMainPSRef.isPlaying)
			{
				_intervalEffectHandle._VFX__10_PlayThis();
			}

			if (CurrentFireBeadValue > _fireBeadReleaseThreshold)
			{
				GenerateNewFireBead();
				CurrentFireBeadValue = 0f;

                _intervalEffectHandle?.VFX_StopThis(true);

            }

			for (int i = _fireBeadRuntimeInfoList.Count - 1; i >= 0; i--)
			{
				_fireBeadRuntimeInfoList[i].RemainingDuration -= delta;
				if (_fireBeadRuntimeInfoList[i].RemainingDuration <= 0f)
				{
					RemoveFireBead(_fireBeadRuntimeInfoList[i]);
				}
				
			}
		}



		
		private void GenerateNewFireBead()
		{
			if (_fireBeadRuntimeInfoList.Count == 4)
			{
				RemoveFireBead(_fireBeadRuntimeInfoList[0]);
				RemoveFireBead(_fireBeadRuntimeInfoList[0]);
				RemoveFireBead(_fireBeadRuntimeInfoList[0]);
				RemoveFireBead(_fireBeadRuntimeInfoList[0]);
			}

			var dir = _playerBehaviourRef.GetCurrentPlayerFaceDirection();

		
			CreateFireBead(Vector3.forward);
			CreateFireBead(Vector3.back);
			CreateFireBead(Vector3.left);
			CreateFireBead(Vector3.right);
		}

		private void CreateFireBead(Vector3 dir)
		{
			PerFireBeadInfo newFireBead = new PerFireBeadInfo();

			var pos = Parent_SelfBelongToObject.GetBuffReceiverPosition();
			newFireBead.SpawnPosition = pos;
		
			newFireBead.Direction = dir;
			//自然积累时长的三倍，场上最多存在三个
			newFireBead.RemainingDuration = _fireBeadReleaseThreshold / _fireBeadAccumulateSpeedPerSecond * 3f + 1;

			newFireBead.RelatedPSRef =
				_VFX_GetAndSetBeforePlay(_fireBeadVFXID,  true).GetCurrentMainPSRef;

			newFireBead.RuntimeLayoutRef =
				_fireBeadLayout.SpawnLayout_NoAutoStart(
					Parent_SelfBelongToObject as I_RP_Projectile_ObjectCanReleaseProjectile,
					false,false);
			// newFireBead.RuntimeLayoutRef.LayoutContentInSO.CollisionInfo.CollisionLayerMask = 10;
			
			newFireBead.RuntimeLayoutRef.LayoutHandlerFunction.OverrideSpawnFromPosition = pos;
			newFireBead.RuntimeLayoutRef.LayoutHandlerFunction.OverrideSpawnFromDirection = dir;
			newFireBead.RelatedPSRef.Play();
			newFireBead.RelatedPSRef.transform.position = pos;
			newFireBead.RuntimeLayoutRef.LayoutHandlerFunction.StartLayout();
			
			_fireBeadRuntimeInfoList.Add(newFireBead);
		}

		private void RemoveFireBead(PerFireBeadInfo toRemove)
		{
			toRemove.RuntimeLayoutRef.StopLayout();
			toRemove.RelatedPSRef.Stop();
			_fireBeadRuntimeInfoList.Remove(toRemove);
		}



		

		I_RP_Buff_ObjectCanReceiveBuff I_BuffTransferWithinPlayer.OriginalParent
		{
			get => _originalParent;
			set => _originalParent = value;
		}
		public void ProcessTransfer(I_BuffTransferWithinPlayer transferFrom, PlayerARPGConcreteCharacterBehaviour newPlayer)
		{


			Parent_SelfBelongToObject = newPlayer;
			
		}

		public override DS_ActionBusArguGroup OnBuffPreRemove()
		{
			var ds =base.OnBuffPreRemove();
			for (int i = _fireBeadRuntimeInfoList.Count - 1; i >= 0; i--)
			{
				 RemoveFireBead( _fireBeadRuntimeInfoList[i]);
			}

			_initEffectHandle.VFX_StopThis(true);
            _intervalEffectHandle.VFX_StopThis(true);

            return ds;
		}



		public override int? UI_GetBuffContent_Stack()
		{
			return (int)CurrentFireBeadValue;
		}

	}
}