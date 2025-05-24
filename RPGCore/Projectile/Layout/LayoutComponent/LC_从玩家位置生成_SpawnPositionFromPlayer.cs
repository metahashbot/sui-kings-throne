using System;
using Global.ActionBus;
using Global.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
namespace RPGCore.Projectile.Layout.LayoutComponent
{
	[Serializable]
	public class LC_从玩家位置生成_SpawnPositionFromPlayer : BaseProjectileLayoutComponent
	{

		[LabelText("√:使用当前玩家位置  |  口:使用若干帧前的位置")]
		public bool UseCurrentPlayerPosition = true;

		[LabelText("    延迟帧数")] [HideIf(nameof(UseCurrentPlayerPosition))]
		public int DelayFrame = 0;

		[LabelText("√：每Shoots时设置  |  口：每系列时设置")]
		public bool AlwaysAim_OnShoots = true;

		[LabelText("√:需要偏差  ||  口:准确，不偏差")]
		public bool ContainBeginOffset;

		[LabelText("于X上的偏差位置范围")]
		[ShowIf(nameof(ContainBeginOffset))]
		public Vector2 OffsetRangeOnX;

        [LabelText("于Z上的偏差位置范围")]
        [ShowIf(nameof(ContainBeginOffset))]
        public Vector2 OffsetRangeOnZ;



        [ShowInInspector, ReadOnly, LabelText("玩家位置和方向缓存")]
		private Vector3 _playerPosCache;

		public override void InitializeBeforeStart(
			SOConfig_ProjectileLayout config,
			float currentTime,
			int currentFrame)
		{
			base.InitializeBeforeStart(config, currentTime, currentFrame);
			GetSelfLocalActionBusRef()
				.RegisterAction(ActionBus_ActionTypeEnum.L_PLayout_Spawn_OneSpawnOperationSetFromPosition_一次生成设置了起始位置,
					_ABC_ProcessSpawnFromPosition_OnOneSpawnOperationSetFromPosition);
			if (!UseCurrentPlayerPosition)
			{
				DelayFrame = 0;
			}
		}



		private void _ABC_ProcessSpawnFromPosition_OnOneSpawnOperationSetFromPosition(DS_ActionBusArguGroup ds)
		{
			if (!CheckIfSameLayoutConfigParent(ds.ObjectArgu1 as BaseProjectileLayoutComponent))
			{
				return;
			}


			BaseLayoutHandler handlerRef = ds.ObjectArguStr as BaseLayoutHandler;
			LayoutSpawnInfo_OneSpawn oneSpawnInfo = ds.ObjectArgu2 as LayoutSpawnInfo_OneSpawn;
			Vector3 fromPos = _parentProjectileLayoutConfig.LayoutHandlerFunction._cache_CasterPosition;


			//如果不是每Shoots校准，也return无事发生
			if (AlwaysAim_OnShoots)
			{
				RefreshPlayerInfo();
			}



			LayoutSpawnInfo_OneSpawn thisSpawn = ds.ObjectArgu2 as LayoutSpawnInfo_OneSpawn;


			var finalSetPosition = _playerPosCache;


			if (ContainBeginOffset)
			{
				var randomInX = UnityEngine.Random.Range(OffsetRangeOnX.x, OffsetRangeOnX.y) * (UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1); 
				var randomInZ = UnityEngine.Random.Range(OffsetRangeOnZ.x, OffsetRangeOnZ.y) * (UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1);

                Vector3 rotUnit = new Vector3(randomInX, 0, randomInZ);
				Vector3 rotateUnit = rotUnit;
				var rotateResult = MathExtend.Vector3RotateOnXOZ(rotateUnit, UnityEngine.Random.Range(0, 360));
				finalSetPosition += rotateResult;
			}

			oneSpawnInfo.SpawnFromPosition = finalSetPosition;
		}



		private void RefreshPlayerInfo()
		{
			_playerPosCache = GLMRef.PlayerCharacterBehaviourControllerReference.CurrentControllingBehaviour.transform.position;
		}





	}
}