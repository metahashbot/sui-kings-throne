using System;
using System.Collections.Generic;
using ARPG.Manager;
using Global.Utility;
using RPGCore.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace RPGCore.Projectile.Layout
{




	[Serializable]
	public class LC_Drop : BaseProjectileLayoutComponent, ILayoutComponent_RespondToSpawn,
		ILayoutComponent_CanAddProjectileToSpawnCollection
	{
		[LabelText("生成数量")]
		public int SpawnCount = 1;

		[LabelText("销毁高度")]
		public float DestroyHeight = 0.25f;

		[LabelText("需要在范围内随机选点吗")]
		public bool RandomSpawnPosition = false;

		[LabelText("随机范围于X"), ShowIf(nameof(RandomSpawnPosition))]
		public float RandomRangeX;

		[LabelText("随机范围于Z"), ShowIf(nameof(RandomSpawnPosition))]
		public float RandomRangeZ;

		[LabelText("需要显式地定义生命周期吗")]
		public bool ExplicitlyDefineLifetime = false;

		[LabelText("显式的生命周期"), ShowIf(nameof(ExplicitlyDefineLifetime))]
		public float ExplicitLifetime = 999f;

		[LabelText("参与判定的高度")]
		public float StartCollideHeight = 0.8f;


		[LabelText("起始高度")]
		public float StartHeight = 10f;

		[LabelText("飞行速度")]
		public float FlySpeed = 3.5f;

		[LabelText("偏斜_在X轴上的距离")]
		public float ScaleX = 2f;

		[LabelText("√：自定义ZOY倾角 || 口：使用当前摄像机俯仰角")]
		public bool CustomZOYAngle = true;

		[LabelText("ZOY倾角"), ShowIf(nameof(CustomZOYAngle))]
		public float FlyAngleZOY = 45f;


		[LabelText("√: 使用落点特效 || 口: 不使用落点特效"), TitleGroup("===VFX===")]
		public bool UseVFX = false;


		//生成高度； 
		//尺寸相对于原始
		// 
		[SerializeField]
		public VFXInfoOfDropLC VFXInfo = new VFXInfoOfDropLC();





		private float _offsetLengthX;
		private float _offsetLengthZ;

		[LabelText("加入生成队列时任务等级，通常不用动")]
		public int SpawnIndex = 0;



		public override void InitializeBeforeStart(
			SOConfig_ProjectileLayout config,
			float currentTime,
			int currentFrame)
		{
			base.InitializeBeforeStart(config, currentTime, currentFrame);
			float targetZOY = FlyAngleZOY;
			if (!CustomZOYAngle)
			{
				targetZOY = BaseMainCameraBehaviour.BaseInstance.VirtualCameraRef.transform.eulerAngles.x;
			}


			float flyDistance = MathExtend.Gougu(StartHeight / Mathf.Cos(targetZOY * Mathf.Deg2Rad), ScaleX);

			_offsetLengthX = ScaleX;
			_offsetLengthZ = StartHeight * Mathf.Tan(targetZOY * Mathf.Deg2Rad);
		}

		public BaseLayoutHandler GetHandlerRef()
		{
			return _parentProjectileLayoutConfig.LayoutHandlerFunction;
		}
		public void RespondToSpawnOperation(
			LayoutSpawnInfo_OneSpawn oneSpawnInfo,
			BaseLayoutHandler handler,
			List<BaseLayoutHandler.WaitToSpawnProjectileBehaviourInfo> collection,
			float currentTime,
			int currentFrame,
			int seriesIndex,
			int shootsIndex)
		{
			Vector3 fromPos, fromDirection;
			fromPos = handler.OverrideSpawnFromPosition ??
			          _parentProjectileLayoutConfig.LayoutHandlerFunction.GetCasterPosition();
			Vector3 desiredDropOnTerrainPosition = fromPos;
			//起始的基准位置，为目标原始点算上倾角之后的那个点，随后将要进行随机散布
			fromPos += new Vector3(_offsetLengthX, StartHeight, _offsetLengthZ);
			(this as ILayoutComponent_RespondToSpawn).OneSpawnOperation_SetFromPosition(fromPos, oneSpawnInfo, handler);
			(this as ILayoutComponent_RespondToSpawn).OneSpawnOperation_SetFromSize(
				handler.RelatedProjectileLayoutConfigRef.LayoutContentInSO.RelatedProjectileScale,
				oneSpawnInfo,
				handler);

			fromDirection = new Vector3(-_offsetLengthX, -StartHeight, -_offsetLengthZ).normalized;
			(this as ILayoutComponent_RespondToSpawn).OneSpawnOperation_SetFromDirection(fromDirection,
				oneSpawnInfo,
				handler);

			for (int currentShootIndex = 0; currentShootIndex < SpawnCount; currentShootIndex++)
			{
				ProjectileBehaviour_Runtime projectile = _parentProjectileLayoutConfig.LayoutHandlerFunction
					.GetLayoutRelatedAvailableProjectile();
				projectile.RelatedSeriesIndex = seriesIndex;
				projectile.RelatedShootsIndex = shootsIndex;
				projectile.FromLayoutComponentRef = this;
				Vector3 offsetPos = Vector3.zero;
				Vector3 newDropTargetPos = desiredDropOnTerrainPosition;
				if (RandomSpawnPosition)
				{
					float randomX = Random.Range(-RandomRangeX, RandomRangeX);
					float randomZ = Random.Range(-RandomRangeZ, RandomRangeZ);
					newDropTargetPos += new Vector3(randomX, 0f, randomZ);
					offsetPos = new Vector3(randomX, 0f, randomZ);
				}
				float spawnSize = oneSpawnInfo.SpawnSize;
				Vector3 finalSpawnPos = oneSpawnInfo.SpawnFromPosition + offsetPos;
				Vector3 direction = oneSpawnInfo.SpawnFromDirection;

				projectile.StartPosition = finalSpawnPos;

				//为其额外添加一个DropFromHeight的功能组件
				PFC_DropFromHeight dropFromHeight = PFC_DropFromHeight.GetFromPool();
				dropFromHeight.InitializeComponent(projectile);
				dropFromHeight.TargetDropPoint = newDropTargetPos;
				projectile.RelatedFunctionComponentList.Add(dropFromHeight);
				dropFromHeight.DestroyHeight = DestroyHeight;
				dropFromHeight.TakeCollisionEffect = false;
				dropFromHeight.TakeEffectFromHeight = StartCollideHeight;

				(this as ILayoutComponent_CanAddProjectileToSpawnCollection).SetOneProjectilePreStartPosition(
					projectile,
					finalSpawnPos,
					handler);
				(this as ILayoutComponent_CanAddProjectileToSpawnCollection).SetOneProjectilePreStartDirection(
					projectile,
					direction,
					handler);
				(this as ILayoutComponent_CanAddProjectileToSpawnCollection).SetOneProjectilePreStartSpeed(projectile,
					FlySpeed);
				(this as ILayoutComponent_CanAddProjectileToSpawnCollection).SetOneProjectilePreLifetime(projectile,
					ExplicitLifetime);
				(this as ILayoutComponent_CanAddProjectileToSpawnCollection)
					.SetOneProjectileSize(projectile, spawnSize);


				(this as ILayoutComponent_CanAddProjectileToSpawnCollection)
					.SetOneProjectileBaseInfoDoneAndAddToWaitList(projectile,
						collection,
						currentTime,
						seriesIndex,
						shootsIndex,
						SpawnIndex);

				if (UseVFX)
				{
					VFXInfo.AddNewVFXInfo(projectile);
				}
			}
		}


		public override void FixedUpdateTick(float currentTime, int currentFrame, float delta)
		{
			base.FixedUpdateTick(currentTime, currentFrame, delta);
			if (UseVFX)
			{
				VFXInfo.FixedUpdateTick(currentTime, currentFrame, delta);
			}
		}


		public override void ClearAndUnload(SOConfig_ProjectileLayout parentLayoutRef)
		{
			base.ClearAndUnload(parentLayoutRef);
		}


		[Serializable]
		public class VFXInfoOfDropLC
		{

			[SerializeField, LabelText("特效预制体"), TitleGroup("===VFX===")]
			public GameObject VFXPrefab;

			[LabelText("特效生成距离地面高度"), TitleGroup("===VFX===")]
			public float VFXSpawnHeightOffset = 0.1f;

			[LabelText("√: 覆写生成物尺寸 || 口: 乘算生成物尺寸"), TitleGroup("===VFX===")]
			public bool VFXOffsetScaleOverride = false;


			[LabelText("生成物尺寸修改"), TitleGroup("===VFX===")]
			public float VFXOffsetScale = 1f;

			[LabelText("生成：当低于此高度时"), TitleGroup("===VFX===")]
			public float VFXSpawnHeight = 0.5f;


			[LabelText("销毁：销毁当低于此高度时"), TitleGroup("===VFX===")]
			public float VFXDestroyHeight = 0.5f;


			public class PerTypeInfo
			{
				public ProjectileBehaviour_Runtime RelatedProjectileRef;

				public bool Spawned;

				public ParticleSystem VFXRef;

			}
			[NonSerialized]
			public List<PerTypeInfo> PerTypeInfos = new List<PerTypeInfo>();



			public void AddNewVFXInfo(ProjectileBehaviour_Runtime projectile)
			{
				PerTypeInfo tmpNew = GenericPool<PerTypeInfo>.Get();
				PerTypeInfos.Add(tmpNew);
				tmpNew.RelatedProjectileRef = projectile;
				tmpNew.Spawned = false;
				tmpNew.VFXRef = null;
			}

			public void FixedUpdateTick(float ct, int cf, float delta)
			{
				for (int i = PerTypeInfos.Count - 1; i >= 0; i--)
				{
					var tmpInfo = PerTypeInfos[i];
					if (tmpInfo.RelatedProjectileRef.SelfDropFromHeightComponent == null)
					{
						continue;
					}
					var height = tmpInfo.RelatedProjectileRef.SelfDropFromHeightComponent
						.CurrentProjectileHeightToTerrain;

					//如果已经生成，则检测是否需要销毁特效
					if (tmpInfo.Spawned)
					{
						if (height < VFXDestroyHeight)
						{
							VFXPoolManager.Instance.ReturnParticleSystemToPool(VFXPrefab, tmpInfo.VFXRef);
							tmpInfo.VFXRef = null;
							tmpInfo.Spawned = false;
						}
					}
					else
					{
						if (height < VFXSpawnHeight)
						{
							var spawnPS = VFXPoolManager.Instance.GetParticleSystemRuntimeByPrefab(VFXPrefab);
							ParticleSystem.MainModule spawnPSMain = spawnPS.main;
							spawnPSMain.stopAction = ParticleSystemStopAction.Disable;
							var pos = tmpInfo.RelatedProjectileRef.SelfDropFromHeightComponent.TargetDropPoint;
							pos += Vector3.up * VFXSpawnHeightOffset;
							if (VFXOffsetScaleOverride)
							{
								spawnPS.transform.localScale = Vector3.one * VFXOffsetScale;
							}
							else
							{
								spawnPS.transform.localScale = Vector3.one * tmpInfo.RelatedProjectileRef
									.CurrentLocalSize;
							}
							spawnPS.transform.position = pos;
							spawnPS.Play();

							tmpInfo.VFXRef = spawnPS;
							tmpInfo.Spawned = true;
						}
					}
				}
			}

		}


	}


}