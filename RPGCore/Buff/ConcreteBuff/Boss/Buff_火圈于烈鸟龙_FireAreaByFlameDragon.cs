using System;
using System.Collections.Generic;
using ARPG.Character.Enemy;
using ARPG.Manager;
using GameplayEvent;
using GameplayEvent.Handler.CommonUtility;
using Global;
using Global.ActionBus;
using Global.AreaOnMap.AreaFunctionHandler;
using Global.Audio;
using RPGCore.Buff.BuffHolder;
using RPGCore.Buff.Config;
using RPGCore.Interface;
using RPGCore.Projectile.Layout;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Buff.ConcreteBuff.Boss
{
	[Serializable]
	public class Buff_火圈于烈鸟龙_FireAreaByFlameDragon : BaseRPBuff
	{


		[SerializeField, LabelText("火圈初始半径")]
		[TitleGroup("===具体配置===")]
		private float _initialRadius;

		[SerializeField, LabelText("火圈初始缩圈速度 单位/s")]
		[TitleGroup("===具体配置===")]
		private float _initialFireAreaRadiusReduceSpeed;


		[SerializeField, LabelText("火圈缩圈最小范围")]
		[TitleGroup("===具体配置===")]
		private float _initialFireAreaRadiusMin = 8f;

        [SerializeField, LabelText("表现为火圈的版面"), InlineEditor(InlineEditorObjectFieldModes.Boxed)]
		[TitleGroup("===具体配置===")]
		private SOConfig_ProjectileLayout _fireAreaLayout;

		public SOConfig_ProjectileLayout CurrentActiveFireAreaLayoutRuntimeInstance { get; private set; }

		[SerializeField, LabelText("vfxID_火圈")]
		[TitleGroup("===具体配置==="), GUIColor(187f / 255f, 1f, 0f)]
		private string _fireAreaVFXID;

        [SerializeField, LabelText("vfxID_火圈外圈")]
        [TitleGroup("===具体配置==="), GUIColor(187f / 255f, 1f, 0f)]
        private string _fireArea2VFXID;

        [SerializeField, LabelText("vfxID_装饰外围的火圈")]
        [TitleGroup("===具体配置==="), GUIColor(187f / 255f, 1f, 0f)]
        private string _decoratefireAreaVFXID;

        [SerializeField, LabelText("装饰外围的火圈出现的百分比")]
        [TitleGroup("===具体配置==="), GUIColor(187f / 255f, 1f, 0f)]
        private float _activeDecorateFireAreaVFXPercent = 0.5f;


        [SerializeField,LabelText("GEH_火圈的音频Command")]
         public GEH_播放音频_PlayAudio
 		   _audioCommand = new GEH_播放音频_PlayAudio();	
        
        [SerializeField, LabelText("火圈的音频任务")]
        [TitleGroup("===具体配置===")]
        private Config_二动态位置音频任务_TwoDynamicPositionTask  _audioTask = new Config_二动态位置音频任务_TwoDynamicPositionTask(); 

        
         

        public PerVFXInfo CurrentActiveFireAreaVFX { get; private set; }
        public PerVFXInfo CurrentActiveFireArea2VFX { get; private set; }

        public PerVFXInfo CurrentActiveDecorateFireAreaVFX { get; private set; }


        /// <summary>
        /// 当前火圈中心半径
        /// </summary>
        public float CurrentFireAreaRadius { get; private set; }

		/// <summary>
		///  当前火圈缩圈速度
		/// </summary>
		public float CurrentFireAreaRadiusReduceSpeed { get; private set; }


		private LC_NWay _nWayComponentInRuntimLayout;

		private Vector3 _spawnPositionRuntimeInfo_CenterPos;

		public override void Init(
			RPBuff_BuffHolder buffHolderRef,
			SOConfig_RPBuff configRuntimeInstance,
			SOConfig_RPBuff configRawTemplate,
			I_RP_Buff_ObjectCanReceiveBuff parent,
			I_RP_Buff_ObjectCanApplyBuff selfReceiveFrom)
		{
			base.Init(buffHolderRef, configRuntimeInstance, configRawTemplate, parent, selfReceiveFrom);



			CurrentFireAreaRadius = _initialRadius;
			CurrentFireAreaRadiusReduceSpeed = _initialFireAreaRadiusReduceSpeed;
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_Audio_AudioPlayWithBroadcast_音频播放并广播,
				_ABC_RegisterFireAreaAudioTask);
		}
		
		

		public override DS_ActionBusArguGroup OnBuffInitialized(List<BaseBuffLogicPassingComponent> blps)
		{
			var ds = base.OnBuffInitialized(blps);


			CurrentActiveFireAreaLayoutRuntimeInstance =
				_fireAreaLayout.SpawnLayout_NoAutoStart(
					Parent_SelfBelongToObject as I_RP_Projectile_ObjectCanReleaseProjectile);

			_nWayComponentInRuntimLayout =
				CurrentActiveFireAreaLayoutRuntimeInstance.LayoutContentInSO.LayoutComponentList.Find(x => x is LC_NWay) as LC_NWay;
			if (_nWayComponentInRuntimLayout == null)
			{
				DBug.LogError("火圈版面中没有找到NWay组件");
			}

			CurrentActiveFireAreaVFX = _VFX_GetAndSetBeforePlay( _fireAreaVFXID);
            CurrentActiveFireArea2VFX = _VFX_GetAndSetBeforePlay(_fireArea2VFXID);

            _spawnPositionRuntimeInfo_CenterPos =
				SubGameplayLogicManager_ARPG.Instance.ActivityManagerArpgInstance
					.EnemySpawnServiceSubActivityServiceComponentRef
					.GetTargetSpawnPoint((Parent_SelfBelongToObject as EnemyARPGCharacterBehaviour)
						.RelatedSpawnConfigInstance_SingleSpawnInfo.SpawnAreaID,
						"中心")[0].GetCurrentSpawnPosition();
			CurrentActiveFireAreaVFX._VFX__10_PlayThis(true, true);
			CurrentActiveFireAreaVFX._VFX_2_SetPositionToGlobalPosition(_spawnPositionRuntimeInfo_CenterPos);

            CurrentActiveFireArea2VFX._VFX__10_PlayThis(true, true);
            CurrentActiveFireArea2VFX._VFX_2_SetPositionToGlobalPosition(_spawnPositionRuntimeInfo_CenterPos);

            CurrentActiveFireAreaLayoutRuntimeInstance.LayoutHandlerFunction.OverrideSpawnFromPosition = _spawnPositionRuntimeInfo_CenterPos;

            CurrentActiveFireAreaLayoutRuntimeInstance.LayoutHandlerFunction.PresetNeedUniformTimeStamp = true;

            _nWayComponentInRuntimLayout.SpawnDistanceOffset = CurrentFireAreaRadius;
			CurrentActiveFireAreaLayoutRuntimeInstance.LayoutHandlerFunction.StartLayout();
			GameplayEventManager.Instance.ExecuteGEHCommandDirectly(_audioCommand, new DS_GameplayEventArguGroup());
			return ds;
		}




		public override void FromHolder_FixedUpdateTick(float currentTime, int currentFrameCount, float delta)
		{
			base.FromHolder_FixedUpdateTick(currentTime, currentFrameCount, delta);
			if (currentFrameCount % 10 == 0)
			{
				CurrentFireAreaRadius -= CurrentFireAreaRadiusReduceSpeed * delta * 10;
				if (CurrentFireAreaRadius <= _initialFireAreaRadiusMin)
				{
					CurrentFireAreaRadius = _initialFireAreaRadiusMin;
                }

				_nWayComponentInRuntimLayout.SpawnDistanceOffset = CurrentFireAreaRadius;
				CurrentActiveFireAreaLayoutRuntimeInstance.LayoutHandlerFunction.UniformTimeStamp =
					currentFrameCount + GetHashCode();
				CurrentActiveFireAreaVFX?.VFX_4_SetLocalScale(CurrentFireAreaRadius);
                CurrentActiveFireArea2VFX?.VFX_4_SetLocalScale(CurrentFireAreaRadius);
            }

			if (CurrentActiveDecorateFireAreaVFX == null && CurrentFireAreaRadius / _initialRadius < _activeDecorateFireAreaVFXPercent)
			{
                CurrentActiveDecorateFireAreaVFX = _VFX_GetAndSetBeforePlay(_decoratefireAreaVFXID)._VFX__10_PlayThis(true,true);
                CurrentActiveDecorateFireAreaVFX._VFX_2_SetPositionToGlobalPosition(_spawnPositionRuntimeInfo_CenterPos);
            }
		}

		
		

		protected override void ProcessPerBLP(BaseBuffLogicPassingComponent blp)
		{
			base.ProcessPerBLP(blp);
			switch (blp)
			{
				case BLP_火圈速度调整_FireAreaSpeedAdjust blp火圈速度调整FireAreaSpeedAdjust:
					CurrentFireAreaRadiusReduceSpeed *= blp火圈速度调整FireAreaSpeedAdjust.Multiplier;
					break;
			}
		}

		private void _ABC_RegisterFireAreaAudioTask(DS_ActionBusArguGroup ds)
		{
			var ds_str = ds.ObjectArguStr as string;
			if (!string.Equals(ds_str, _audioTask.AudioBroadcastID))
			{
				return;
			}
			 _audioTask.AudioBroadcastID = ds_str;
			 _audioTask.SelfAudioSource = ds.GetObj1AsT<AudioSource>();
			 _audioTask.TargetPositionGetter1 = CurrentActiveFireAreaLayoutRuntimeInstance.GetLayoutSpawnPositionFunc();
			 _audioTask.TargetPositionGetter2 = SubGameplayLogicManager_ARPG.Instance
				 .PlayerCharacterBehaviourControllerReference.GetCurrentActivePlayerPositionFunc();
			 GeneralAudioManager.Instance.AddNewAudioTask(_audioTask);
			 
		}



		[Serializable]
		public class BLP_火圈速度调整_FireAreaSpeedAdjust : BaseBuffLogicPassingComponent
		{
			[SerializeField,LabelText("速度乘数")]
			public float Multiplier = 1f;
			
			
			
			public override void ReleaseOnReturnToPool()
			{
				GenericPool<BLP_火圈速度调整_FireAreaSpeedAdjust>.Release(this);
			}
		}

	}
}