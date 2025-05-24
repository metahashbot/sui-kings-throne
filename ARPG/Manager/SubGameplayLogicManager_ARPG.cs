using System;
using ARPG.Character.Base;
using ARPG.Character.Player;
using ARPG.Common;
using Global;
using Global.ActionBus;
using Global.Task;
using Global.Utility;
using RPGCore.AssistBusiness;
using RPGCore.Buff;
using RPGCore.Projectile;
using RPGCore.Projectile.Layout;
using Sirenix.OdinInspector;
using UnityEngine;
using WorldMapScene.Manager;
using static Global.BaseGameReferenceService;
namespace ARPG.Manager
{
	public class SubGameplayLogicManager_ARPG : BaseGameplayLogicManager
	{
		public static SubGameplayLogicManager_ARPG Instance;

        [Required]
        [ShowInInspector, LabelText("区域管理"), BoxGroup("Runtime")]
        public TriggerAreaManager TriggerAreaManagerInstance;


        #region 子逻辑

        [Required, SerializeField, LabelText("地图上的角色管理器"), FoldoutGroup("配置", true)]
		public CharacterOnMapManager CharacterOnMapManagerReference;

		[Required, SerializeField, LabelText("玩家控制器"), FoldoutGroup("配置/子内容", true)]
		public PlayerCharacterBehaviourController PlayerCharacterBehaviourControllerReference;

		[Required, SerializeField, LabelText("投射物行为管理"), FoldoutGroup("配置/子内容", true)]
		public ProjectileBehaviourManager ProjectileBehaviourManagerReference;

		[Required, SerializeField, LabelText("投射物布局管理"), FoldoutGroup("配置/子内容", true)]
		public ProjectileLayoutManager ProjectileLayoutManagerReference;

		[SerializeField, Required, FoldoutGroup("配置/子内容", true), LabelText("游戏活动管理")]
		public ActivityManager_ARPG ActivityManagerArpgInstance;

		[SerializeField, Required, LabelText("跳字辅助")]
		public DamageHintManager DamageHintManagerReference;

		[NonSerialized, ShowInInspector, LabelText("伤害辅助")]
		public DamageAssistService DamageAssistServiceInstance;
		
		[NonSerialized,Required,LabelText("玩家任务服务")]
		public PlayerGameplayTaskService PlayerGameplayTaskServiceInstance;

#endregion

		
		public override void AwakeInitialize()
		{
			Instance = this;
			Time.timeScale = 1f;            
            CharacterOnMapManagerReference.AwakeInitialize(this);
			PlayerCharacterBehaviourControllerReference.AwakeInitialize(this);

			ProjectileLayoutManagerReference.AwakeInitialize(this, ProjectileBehaviourManagerReference);
			ProjectileBehaviourManagerReference.InitializeOnAwake(this);
			DamageHintManagerReference.AwakeInitialize();

			DamageAssistServiceInstance = new DamageAssistService();
			ActivityManagerArpgInstance.AwakeInitialize(this);
			PlayerGameplayTaskServiceInstance = new PlayerGameplayTaskService();
			PlayerGameplayTaskServiceInstance.AwakeInitialize(this);

			base.AwakeInitialize();
		}



		/// <summary>
		/// <para>Start时序加载</para>
		/// <para>取回EditorProxy的各种编辑期辅助内容</para>
		/// </summary>
		public override void StartInitialize()
		{
			base.StartInitialize();

			ActivityManagerArpgInstance.StartInitialize();            

            CharacterOnMapManagerReference.StartInitialize();

			PlayerCharacterBehaviourControllerReference.StartInitialize();
			ProjectileBehaviourManagerReference.StartInitialize();
			PlayerGameplayTaskServiceInstance.StartInitialize();
			BaseRPBuff.StaticInitialize();

			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_Player_OnPlayerAllCharacterExited_当玩家所有角色退场,
				_ABC_ProcessAllPlayerCharacterExit);
		}




		/// <summary>
		/// <para>LateLoad时序加载。</para>
		/// <para></para>
		/// </summary>
		public override void LateInitialize()
		{
			base.LateInitialize();

            //在合适的位置生成当前的玩家角色们
            TriggerAreaManagerInstance.LateLoadInitialize();
            CharacterOnMapManagerReference.LateInitialize();
			CharacterOnMapManagerReference.SpawnPlayerCharacterOnMapOnLateInit();

			PlayerCharacterBehaviourControllerReference.LateLoadInitialize();
			ProjectileBehaviourManagerReference.LateInitialize();
			ProjectileLayoutManagerReference.LateInitialize();

			ActivityManagerArpgInstance.LateLoadInitialize();
				
			DamageHintManagerReference.LateInitialize();
			PlayerGameplayTaskServiceInstance.LateInitialize();

			// var ep = FindAnyObjectByType<EditorProxy_ARPGEditor>(FindObjectsInactive.Include);
			// var rrl = GlobalConfigurationAssetHolderHelper.GetGCAHH().RuntimeRecordHelper_Level;
			//
			// rrl.CurrentFullLevelConfigRef = ep.RelatedLevelFullConfig;
			// rrl.CurrentSingleLevelConfigRef = ep.RelatedSingleLevelConfig;

		}




		/// <summary>
		/// <para>处理所有玩家角色都退场后，可能要进行一个GameOver</para>
		/// </summary>
		/// <param name="ds"></param>
		private void _ABC_ProcessAllPlayerCharacterExit(DS_ActionBusArguGroup ds)
		{


			GlobalActionBus.GetGlobalActionBus()
				.TriggerActionByType(ActionBus_ActionTypeEnum.G_ConclusionAsAllExit_结算按照全员退场);


		}



#region Tick

		public void UpdateTick(float ct, int cf, float delta)
        {
            if (GameRunningState != GameRunningStateTypeEnum.Paused_暂停)
            {
                ActivityManagerArpgInstance.UpdateTick(ct, cf, delta);
                CharacterOnMapManagerReference.UpdateTick(ct, cf, delta);
                PlayerCharacterBehaviourControllerReference.UpdateTick(ct, cf, delta);
                DamageHintManagerReference.UpdateTick(ct, cf, delta);
                ProjectileLayoutManagerReference.UpdateTick(ct, cf, delta);
            }
		}


		public void FixedUpdateTick(float ct, int cf, float delta)
        {
            if (GameRunningState != GameRunningStateTypeEnum.Paused_暂停)
            {
                ActivityManagerArpgInstance.FixedUpdateTick(ct, cf, delta);
                CharacterOnMapManagerReference.FixedUpdateTick(ct, cf, delta);
                PlayerCharacterBehaviourControllerReference.FixedUpdateTick(ct, cf, delta);
                ProjectileLayoutManagerReference.FixedUpdateTick(ct, cf, delta);
                ProjectileBehaviourManagerReference.FixedUpdateTick(ct, cf, delta);
            }
		}

#endregion

		public void ClearAndUnload()
		{
			ProjectileBehaviourManagerReference.ClearWhenUnload();
		}
	}
}