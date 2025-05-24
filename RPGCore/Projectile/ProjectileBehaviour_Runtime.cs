using System;
using System.Collections.Generic;
using ARPG.Manager;
using GameplayEvent;
using Global.ActionBus;
using RPGCore.AssistBusiness;
using RPGCore.Buff;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Projectile.Layout;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;
namespace RPGCore.Projectile
{
	/// <summary>
	/// <para>运行时实际的Projectile。对象池里的也是这个东西。</para>
	/// <para>并不是MonoBehaviour，仅作为数据的容器、引用的记录</para>
	/// <para>ProjectileBehaviour本身的GO只是一个空GO而已，只是内部表现用的ArtHelper是绑定它的GO的</para>
	/// <para>交互主要是和使用它的Layout里面的Handler进行，caster并不关心</para>
	/// </summary>
	public class ProjectileBehaviour_Runtime
	{
#if UNITY_EDITOR
		[OnInspectorGUI]
		private void RedrawConstantly() { Sirenix.Utilities.Editor.GUIHelper.RequestRepaint(); }
#endif
#region 静态引用

		private static ProjectileBehaviourManager _projectileBehaviourManagerReference;
		private static DamageAssistService _damageAssistServiceRef;
		private static Vector4 _currentVanishBoundAABB;

		/// <summary>
		/// 经由PBM调用的静态初始化，在PB的Start初始化时序
		/// .
		/// </summary>
		/// <param name="projectileManager"></param>
		public static void StaticInitialize(ProjectileBehaviourManager projectileManager)

		{
			_projectileBehaviourManagerReference = projectileManager;
			PFC_AlightWithTerrain.EnableAlignInstantly();
			// _damageAssistServiceRef = SubGameplayLogicManager_BattleGame.Instance.DamageAssistServiceInstance;
		}
		public static void SetCurrentSceneVanishBoundInfo(Vector4 aabb)
		{
			_currentVanishBoundAABB = aabb;
		}

#endregion

#region 运行时数据

		[ShowInInspector, LabelText("是由哪个LC组件生成的"), FoldoutGroup("运行时", true)]
		public ILayoutComponent_CanAddProjectileToSpawnCollection FromLayoutComponentRef;

		[ShowInInspector, LabelText("投射物显示名称"), FoldoutGroup("运行时/架构部分", true)]
		public string ProjectileDisplayName;
		
		[ShowInInspector, LabelText("投射物UID"), FoldoutGroup("运行时/架构部分", true)]
		public string SelfUID;


		[ShowInInspector, LabelText("关联的游戏对象"), FoldoutGroup("运行时/架构部分", true)]
		public GameObject RelatedGORef;
		[ShowInInspector, LabelText("关联的艺术Helper对象"), FoldoutGroup("运行时/架构部分", true)]
		public GameObject RelatedArtHelperGORef;

		[ShowInInspector, LabelText("包含[拖尾特效]"), FoldoutGroup("运行时/架构部分", true)]
		public bool ContainTrailVFX;

		[ShowInInspector, LabelText("包含[附带特效]"), FoldoutGroup("运行时/架构部分", true)]
		public bool ContainAppendVFX;

		[ShowInInspector, LabelText("关联【拖尾】Prefab"), FoldoutGroup("运行时/架构部分", true)]
		public GameObject RelationTrailVFXPrefab;
		
		 [ShowInInspector,LabelText("关联【附带】Prefab") ,FoldoutGroup("运行时/架构部分",true)]
		public GameObject RelationAppendVFXPrefab;
		
		[ShowInInspector,LabelText("关联销毁时追加特效Prefab") ,FoldoutGroup("运行时/架构部分",true)]
		public GameObject RelationDestroyAppendVFXPrefab;
		
		/// <summary>
		/// 区别于【附带】。拖尾特效会在投射物停止时一并被停止。
		/// </summary>
		[ShowInInspector, LabelText("[拖尾特效]的运行时引用")]
		public ParticleSystem TrailVFXRuntimeReference_PS;

		[ShowInInspector, LabelText("[拖尾特效]的运行时引用")]
		 public VFX_ParticleSystemPlayProxy TrailVFXRuntimeReference_PSPP;
		 
		
		[ShowInInspector,LabelText("[附带特效]的运行时引用")]
		public ParticleSystem AppendVFXRuntimeReference_PS;

		[ShowInInspector, LabelText("[附带特效]的运行时引用")]
		public VFX_ParticleSystemPlayProxy AppendVFXRuntimeReference_PSPP;
		 
		


		[ShowInInspector, LabelText("关联Series索引"), FoldoutGroup("运行时/架构部分", true)]
		public int RelatedSeriesIndex;


		[ShowInInspector, LabelText("关联Shoots索引？"), FoldoutGroup("运行时/架构部分", true)]
		public int RelatedShootsIndex;

		public void SetSeriesAndShoots(int s, int shoots)
		{
			RelatedSeriesIndex = s;
			RelatedShootsIndex = shoots;

		}

		[ShowInInspector, LabelText("是否激活？"), FoldoutGroup("运行时/架构部分", true)]
		public bool SelfActive { get; private set; }
		
		[ShowInInspector,LabelText("当前暂停?") ,FoldoutGroup("运行时/架构部分",true)]
		public bool SelfPaused { get; private set; } 


		/// <summary>
		/// <para>从上次激活到现在已经过去的时间</para>
		/// </summary>
		[ShowInInspector, LabelText("保持活跃的时间"), FoldoutGroup("运行时/架构部分", true)]
		public float ActiveElapsedTime { get; private set; }


		/// <summary>
		/// <para>直接获取这个Projectile的发射者。</para>
		/// </summary>
		/// <returns></returns>
		[ShowInInspector, InlineEditor(InlineEditorObjectFieldModes.Boxed), LabelText("这个投射物的发射者"),
		 FoldoutGroup("运行时/架构部分", true)]
		public I_RP_Projectile_ObjectCanReleaseProjectile SelfCaster { get; private set; }
#region 由各Component共同影响的数据结构

		[ShowInInspector, LabelText("碰撞掩码"), FoldoutGroup("运行时/原始参数", true)]
		public int CollisionMaskLayer;

		[ShowInInspector, LabelText("起始尺寸"), FoldoutGroup("运行时/原始参数", true)]
		public float StartLocalSize;
		
		
		[ShowInInspector, LabelText("起始位置"), FoldoutGroup("运行时/原始参数", true)]
		public Vector3 StartPosition = Vector3.zero;
		[ShowInInspector, LabelText("起始速度"), FoldoutGroup("运行时/原始参数", true)]
		public float StartSpeed;
		[ShowInInspector, LabelText("起始方向"), FoldoutGroup("运行时/原始参数", true)]
		public Vector3 StartDirection;
		[ShowInInspector, LabelText("起始生命周期"), FoldoutGroup("运行时/原始参数", true)]
		public float StartLifetime;

		[ShowInInspector, LabelText("越界后自动销毁？"), FoldoutGroup("运行时/原始参数", true)]
		public bool AutoRecycleOnBeyondVanishBound;

		[ShowInInspector, LabelText("当前尺寸"), FoldoutGroup("运行时/运行参数", true)]
		public float CurrentLocalSize;
		
		/// <summary>
		/// <para>该Behaviour期望的移动方向。各个子Component会试图修改它</para>
		/// </summary>
		[ShowInInspector, LabelText("当前期望的移动方向"), FoldoutGroup("运行时/运行参数", true)]
		public Vector3 DesiredMoveDirection;
		/// <summary>
		/// <para>该Behaviour期望的移动速度。各个子Component会试图修改它</para>
		/// </summary>
		[ShowInInspector, LabelText("当前期望移动速度"), FoldoutGroup("运行时/运行参数", true)]
		public float DesiredMoveSpeed;

		/// <summary>
		/// <para>当前是否有效的碰撞。有些时候碰撞会处于暂不可用的状态，比如有y轴修正、比如有伤害间隔阈值</para>
		/// </summary>
		[ShowInInspector, LabelText("当前是否有效的碰撞"), FoldoutGroup("运行时/运行参数", true)]
		public bool CurrentCollisionActive
		{
			get
			{
				if (SelfDropFromHeightComponent != null)
				{
					return SelfDropFromHeightComponent.TakeCollisionEffect;
				}
				if (SelfActiveProjectileAfterWarningVFXComponent != null)
				{
					return !SelfActiveProjectileAfterWarningVFXComponent.CurrentPlayingWarnVFX;
				}
					
				return true;
			}
		}

#endregion


		[ShowInInspector, LabelText("关联事件线"), FoldoutGroup("运行时/架构部分", true)]
		private LocalActionBus _selfActionBus;
		public LocalActionBus GetRelatedActionBusRef() => _selfActionBus;
		/// <summary>
		/// <para>关联的LayoutConfigInstance。</para>
		/// </summary>
		[ShowInInspector, InlineEditor(InlineEditorObjectFieldModes.Boxed), LabelText("关联的Layout运行时实例"),
		 FoldoutGroup("运行时/架构部分", true)]
		public SOConfig_ProjectileLayout SelfLayoutConfigReference { get; private set; }
		
		[ShowInInspector, LabelText("上帧世界位置"), FoldoutGroup("运行时/运行参数", true)]
		public Vector3 LastWorldPosition { get; private set; }
		[ShowInInspector, LabelText("上帧旋转Euler"), FoldoutGroup("运行时/运行参数", true)]
		public Vector3 LastLocalRotationEuler { get; private set; }

		[ShowInInspector, LabelText("碰撞半径"), FoldoutGroup("运行时/运行参数", true)]
		public float ProjectileColliderRadius;

		[ShowInInspector, LabelText("覆写的伤害戳"), FoldoutGroup("运行时/运行参数", true)]
		public Nullable<int> OverrideDamageStamp;

		[ShowInInspector, LabelText("销毁信息"), FoldoutGroup("运行时/运行参数", true)]
		public PBInternal_DestroyInfoPair InternalDestroyInfoPairInstance = new PBInternal_DestroyInfoPair();


		[ShowInInspector, FoldoutGroup("运行时"), NonSerialized, LabelText("运行时额外功能组件")]
		public List<ProjectileBaseFunctionComponent> RelatedFunctionComponentList;


		public class PBInternal_DestroyInfoPair
		{
			public bool DestroyOnLifetimeEnd = true;
			public bool DestroyOnBeyondBorder = true;

			//reset 
			public void Reset()
			{
				DestroyOnLifetimeEnd = true;
				DestroyOnBeyondBorder = true;
			}
		}

#endregion



#region Component的缓存

		/*
		 * 缓存，真的是缓存。编辑期不动这些东西。这些缓存都是运行期构建而成的。
		 */

		private PFC_NeutralizeComponent _selfNeutralizeComponent;
		public PFC_Penetration SelfPenetrationComponent;
		public PFC_DropFromHeight SelfDropFromHeightComponent { get; private set; }
		public PFC_投射物将于预警后实际生成_ActiveProjectileAfterWarningVFX SelfActiveProjectileAfterWarningVFXComponent { get; private set; }
		
		public PFC_命中后触发事件_SpawnGameplayEventOnHit SelfSpawnGameplayEventOnHitComponent { get; private set; }
		
		public PFC_激活时触发事件_SpawnGameplayEventOnActive SelfSpawnGameplayEventOnActiveComponent { get; private set; }

#endregion

#region 初始化

		/// <summary>
		/// <para>在被生成到池中的时候，进行的初始化操作</para>
		/// </summary>
		public void InitializeOnSpawnIntoPool(GameObject parent, GameObject artGO, SOFE_ProjectileLoad.LoadContent rawConfigRef , GameObject trailVFXPrefab , GameObject appendVFXPrefab)
		{
			SelfUID = rawConfigRef.ProjectileNameUID;
			ProjectileDisplayName = rawConfigRef.ProjectileDisplayName;
			RelatedFunctionComponentList =
				CollectionPool<List<ProjectileBaseFunctionComponent>, ProjectileBaseFunctionComponent>.Get();
			RelatedFunctionComponentList.Clear();

			RelatedGORef = parent;
			RelatedArtHelperGORef = artGO;
			if (trailVFXPrefab != null)
			{
				ContainTrailVFX = true;
				RelationTrailVFXPrefab = trailVFXPrefab;
			}
			else
			{
				ContainTrailVFX = false;
				RelationTrailVFXPrefab = null;
			}

			if (appendVFXPrefab != null)
			{
				ContainAppendVFX = true;
				RelationAppendVFXPrefab = appendVFXPrefab;
			}
			else
			{
				ContainAppendVFX = false;
				RelationAppendVFXPrefab = null;
			}
			
			
			if(!string.IsNullOrEmpty(rawConfigRef.DestroyAppendVFXAddress))
			{
				RelationDestroyAppendVFXPrefab = rawConfigRef._op_AppendVFX.Result;
			}
			else
			{
				RelationDestroyAppendVFXPrefab = null;
			}
			
			
			ActiveElapsedTime = 0f;
			RelatedGORef.transform.localScale = CurrentLocalSize * Vector3.one;
		}

		
		
		
		/// <summary>
		/// <para>当一个ProjectileRuntime在运行时，出现了类似拷贝的行为（比如OneToMany的生成）</para>
		/// <para>此时需要把源Projectile的数据都拷给自己，比如lifetime、speed、size之类的，</para>
		/// </summary>
		public void InitializeOnGetFromSame(ProjectileBehaviour_Runtime copy)
		{
			RelatedSeriesIndex = copy.RelatedSeriesIndex;
			RelatedShootsIndex = copy.RelatedShootsIndex;
			CollisionMaskLayer = copy.CollisionMaskLayer;
			StartLocalSize = copy.StartLocalSize;
			CurrentLocalSize = copy.CurrentLocalSize;
			StartPosition = copy.StartPosition;
			StartSpeed = copy.StartSpeed;
			StartDirection = copy.StartDirection;
			StartLifetime = copy.StartLifetime;
			AutoRecycleOnBeyondVanishBound = copy.AutoRecycleOnBeyondVanishBound;
			ProjectileColliderRadius = copy.ProjectileColliderRadius; 
		}

		/// <summary>
		/// <para>当一个ProjectileLayout获取空闲ProjectileBehaviour的时候，会 从此注入依赖，并且进行内部组件的初始化</para>
		/// </summary>
		public void InitializeOnGet(SOConfig_ProjectileLayout layoutConfigRuntime,
			I_RP_Projectile_ObjectCanReleaseProjectile caster, LocalActionBus lab)
		{
			SelfLayoutConfigReference = layoutConfigRuntime;
			SelfCaster = caster;
			_selfActionBus = lab;

			OverrideDamageStamp = null;
			//加入来自layout配置中的对于projectile的覆写组件。
			if (layoutConfigRuntime.LayoutContentInSO.FunctionComponentsOverride != null)
			{
				foreach (ProjectileBaseFunctionComponent functionComponent in layoutConfigRuntime.LayoutContentInSO
					.FunctionComponentsOverride)
				{
					functionComponent.DeepCopyToRuntimeList(functionComponent, RelatedFunctionComponentList);
				}
			}


			
			
			//基本碰撞信息，是在Layout里就有的
			ProjectileColliderRadius = layoutConfigRuntime.LayoutContentInSO.CollisionInfo.CollisionInfoList[0].Radius; 
			CollisionMaskLayer = layoutConfigRuntime.LayoutContentInSO.CollisionInfo.CollisionLayerMask;
		
			
			//初始化内部组件
			foreach (var perFunctionComponent in RelatedFunctionComponentList)
			{
				perFunctionComponent.InitializeComponent(this);
			}
			
			
			
		}


[NonSerialized]		private List<I_RP_Projectile_ObjectCanReceiveProjectileCollision> _ignoreTargetList;
		public void AddTargetToIgnoreList(List<I_RP_Projectile_ObjectCanReceiveProjectileCollision> list)
		{
			if (_ignoreTargetList == null)
			{
				_ignoreTargetList = CollectionPool<List<I_RP_Projectile_ObjectCanReceiveProjectileCollision>,
					I_RP_Projectile_ObjectCanReceiveProjectileCollision>.Get();
			}
			foreach (I_RP_Projectile_ObjectCanReceiveProjectileCollision baseBehaviour in list)
			{
				if (!_ignoreTargetList.Contains(baseBehaviour))
				{
					_ignoreTargetList.Add(baseBehaviour);
				}
			}
		}


		public void AddTargetToIgnoreList(params I_RP_Projectile_ObjectCanReceiveProjectileCollision[] behaviours)
		{
			if (_ignoreTargetList == null)
			{
				_ignoreTargetList = CollectionPool<List<I_RP_Projectile_ObjectCanReceiveProjectileCollision>,
					I_RP_Projectile_ObjectCanReceiveProjectileCollision>.Get();
			}
			foreach (I_RP_Projectile_ObjectCanReceiveProjectileCollision baseBehaviour in behaviours)
			{
				if (!_ignoreTargetList.Contains(baseBehaviour))
				{
					_ignoreTargetList.Add(baseBehaviour);
				}
			}
		}

		/// <summary>
		/// <para>检查此次命中是否是需要被忽略的目标。</para>
		/// <para>存在于：直接预设的忽略目标  、  还存在着的穿透记录。</para>
		/// </summary>
		/// <param name="b"></param>
		/// <returns></returns>
		public bool CheckTargetIgnored(I_RP_Projectile_ObjectCanReceiveProjectileCollision b)
		{
			bool contains;
			if (_ignoreTargetList != null && _ignoreTargetList.Contains(b))
			{
				return true;
			}
			if (SelfLayoutConfigReference.LayoutHandlerFunction.IgnoreTargetList.Contains(b))
			{
				return true;
			}

			// //如果有穿透组件，则检查一下是不是已经可以再次穿透了
			if (SelfPenetrationComponent != null)
			{
				if(SelfPenetrationComponent.CheckExistIgnore( b))
				{
					return true;
				}
			}
			return false;
		}

#endregion







		/// <summary>
		/// <para>显示地设置为激活。该操作会直接将这个投射物的GameObject位置和旋转方向设置并激活它</para>
		/// </summary>
		public void ActivateThisProjectile(float currentTime)
		{
			var startPos = StartPosition;
			
			startPos.y += SelfLayoutConfigReference.LayoutContentInSO.ExtraSpawnHeightAddon;
			RelatedGORef.transform.position = startPos;
			DesiredMoveSpeed = StartSpeed;
			RelatedGORef.transform.forward = StartDirection;
			DesiredMoveDirection = StartDirection;
			CurrentLocalSize = StartLocalSize;
			RelatedGORef.transform.localScale = Vector3.one * CurrentLocalSize;

			if (ContainTrailVFX)
			{
				VFXPoolManager.Instance.GetRuntimeRefByPrefab( RelationTrailVFXPrefab, out TrailVFXRuntimeReference_PS, out TrailVFXRuntimeReference_PSPP);
				Transform trans_trail = null;
				if (TrailVFXRuntimeReference_PS)
				{
					trans_trail = TrailVFXRuntimeReference_PS.transform;
					var main = TrailVFXRuntimeReference_PS.main;
					main.stopAction = ParticleSystemStopAction.Disable;
				}
				else
				{
					trans_trail = TrailVFXRuntimeReference_PSPP.transform;
					TrailVFXRuntimeReference_PSPP.SetStopAction(ParticleSystemStopAction.Disable);
				}
				trans_trail.SetParent(RelatedGORef.transform);
				trans_trail.localPosition = Vector3.zero;
				trans_trail.localRotation = Quaternion.identity;
				trans_trail.localScale = Vector3.one;
				if (TrailVFXRuntimeReference_PS)
				{
					TrailVFXRuntimeReference_PS.Play();
				}
				else
				{
					TrailVFXRuntimeReference_PSPP.Play();
				}
			}
			if (ContainAppendVFX)
			{
				VFXPoolManager.Instance.GetRuntimeRefByPrefab(RelationAppendVFXPrefab,
					out AppendVFXRuntimeReference_PS,
					out AppendVFXRuntimeReference_PSPP);
				Transform trans_append = null;
				if (AppendVFXRuntimeReference_PS)
				{
					trans_append = AppendVFXRuntimeReference_PS.transform;
					var main = AppendVFXRuntimeReference_PS.main;
					main.stopAction = ParticleSystemStopAction.Disable;
				}
				else
				{
					trans_append = AppendVFXRuntimeReference_PSPP.transform;
					AppendVFXRuntimeReference_PSPP.SetStopAction(ParticleSystemStopAction.Disable);
				}
				trans_append.SetParent(RelatedGORef.transform);
				trans_append.localPosition = Vector3.zero;
				trans_append.localRotation = Quaternion.identity;
				trans_append.localScale = Vector3.one;
				if (AppendVFXRuntimeReference_PS)
				{
					AppendVFXRuntimeReference_PS.Play(true);
				}
				else
				{
					AppendVFXRuntimeReference_PSPP.Play();
				}
			}
			
			RelatedGORef.SetActive(true);
			SelfActive = true;
			StartAllFunctionComponent(currentTime);



			if (SelfSpawnGameplayEventOnActiveComponent != null &&
			    SelfSpawnGameplayEventOnActiveComponent.GameplayEventNameList != null &&
			    SelfSpawnGameplayEventOnActiveComponent.GameplayEventNameList.Count > 0)
			{
				var ds_ge = new DS_GameplayEventArguGroup();
				ds_ge.ObjectArgu1 = this;
				for (int i = 0; i < SelfSpawnGameplayEventOnActiveComponent.GameplayEventNameList.Count; i++)
				{
					GameplayEventManager.Instance.StartGameplayEvent(
						SelfSpawnGameplayEventOnActiveComponent.GameplayEventNameList[i],
						ds_ge);
				}
			}

		}
		public void PauseProjectile()
		{
			SelfPaused = true;
		}

		public void UnpauseProjectile()
		{
			SelfPaused = false;
		}

#region 各个子Component常用的设置操作

		/// <summary>
		/// <para>一系列优先于启动Projectile本身的功能组件的，先设置一些内容的各种方法</para>
		/// </summary>
		/// <returns></returns>
		public ProjectileBehaviour_Runtime SetProjectileStartSpeed(float speed)
		{
			StartSpeed = speed;
			DesiredMoveSpeed = StartSpeed;
			return this;
		}
		public ProjectileBehaviour_Runtime SetProjectileStartDirection(Vector3 direction)
		{
			StartDirection = direction;
			DesiredMoveDirection = direction;
			return this;
		}

		public ProjectileBehaviour_Runtime SetProjectileStartDirectionV2(Vector2 direction)
		{
			StartDirection = new Vector3(direction.x, 0f, direction.y);
			DesiredMoveDirection = StartDirection;
			return this;
		}


		


		/// <summary>
		/// <para>设置投射物的完整尺寸。包括艺术尺寸和碰撞尺寸</para>
		/// </summary>
		/// <param name="scale"></param>
		public void SetProjectileFullScale(float scale)
		{
			CurrentLocalSize = scale;
			RelatedGORef.transform.localScale = Vector3.one * CurrentLocalSize;
		}

		public void StartAllFunctionComponent(float _currentTime)
		{
			//也包括构建Component的缓存
			//那么先清除缓存
			_selfNeutralizeComponent = null;
			SelfPenetrationComponent = null;


			foreach (ProjectileBaseFunctionComponent perFunction in RelatedFunctionComponentList)
			{
				perFunction.OnFunctionStart(_currentTime);
			}

			for (int i = 0; i < RelatedFunctionComponentList.Count; i++)
			{
				ProjectileBaseFunctionComponent perFunction = RelatedFunctionComponentList[i];
				if (perFunction is PFC_Penetration)
				{
					SelfPenetrationComponent = perFunction as PFC_Penetration;
				}
				else if (perFunction is PFC_NeutralizeComponent)
				{
					_selfNeutralizeComponent = perFunction as PFC_NeutralizeComponent;
				}
				else if (perFunction is PFC_DropFromHeight)
				{
					SelfDropFromHeightComponent = perFunction as PFC_DropFromHeight;
				}
				else if (perFunction is PFC_投射物将于预警后实际生成_ActiveProjectileAfterWarningVFX)
				{
					SelfActiveProjectileAfterWarningVFXComponent = perFunction as PFC_投射物将于预警后实际生成_ActiveProjectileAfterWarningVFX;
				}
				else if (perFunction is PFC_命中后触发事件_SpawnGameplayEventOnHit)
				{
					SelfSpawnGameplayEventOnHitComponent = perFunction as PFC_命中后触发事件_SpawnGameplayEventOnHit;
				}
				else if (perFunction is PFC_激活时触发事件_SpawnGameplayEventOnActive)
				{
					SelfSpawnGameplayEventOnActiveComponent = perFunction as PFC_激活时触发事件_SpawnGameplayEventOnActive;
				}
			}
		
		}

#endregion

		public void UpdateTick(float ct, int cf, float delta)
		{
if (!SelfActive)
			{
				return;
			}

			if (AppendVFXRuntimeReference_PSPP)
			{
				AppendVFXRuntimeReference_PSPP.UpdateTick(ct, cf, delta);
			}
			if (TrailVFXRuntimeReference_PSPP)
			{
				TrailVFXRuntimeReference_PSPP.UpdateTick(ct, cf, delta);
			}
		}



		public void FixedUpdateTick(float currentTime, int currentFrame, float deltaTime)
		{
			if (!SelfActive)
			{
				return;
			}
			ActiveElapsedTime += deltaTime;



			foreach (ProjectileBaseFunctionComponent projectileBaseFunctionComponent in RelatedFunctionComponentList)
			{
				projectileBaseFunctionComponent.FixedUpdateTick(currentTime, currentFrame, deltaTime);
			}

			if (SelfPaused)
			{
				deltaTime = 0f;
			}


			//各个子Component都计算完成了，接下来要应用各种结果了
			//此处将要应用 运动步进
			Vector3 movementStep = DesiredMoveDirection * (DesiredMoveSpeed * deltaTime);
			Vector3 currentPosition = RelatedGORef.transform.position + movementStep;
			RelatedGORef.transform.position = currentPosition;


			//此处计算lifetime
			if (StartLifetime < 0f)
			{
			}
			else
			{
				if (ActiveElapsedTime > StartLifetime)
				{
					foreach (ProjectileBaseFunctionComponent perComponent in RelatedFunctionComponentList)
					{
						perComponent.OnProjectileLifetimeEnd();
					}


					_selfActionBus.TriggerActionByType(ActionBus_ActionTypeEnum.L_PBR_ProjectileLifetimeEnd_一个投射物生命周期结束,
						new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_PBR_ProjectileLifetimeEnd_一个投射物生命周期结束,
							this));

					return;
				}
			}


			//计算边界
			if (AutoRecycleOnBeyondVanishBound)
			{
				if (CurrentCollisionActive)
				{
					if (currentPosition.x > _currentVanishBoundAABB.z ||
					    currentPosition.x < _currentVanishBoundAABB.x ||
					    currentPosition.z > _currentVanishBoundAABB.w || currentPosition.z < _currentVanishBoundAABB.y)
					{
						_selfActionBus.TriggerActionByType(
							ActionBus_ActionTypeEnum.L_PBR_OnProjectileBeyondBorder_一个投射物超出边界,
							new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum.L_PBR_OnProjectileBeyondBorder_一个投射物超出边界,
								this));
						return;
					}
				}
			}
			LastWorldPosition = RelatedGORef.transform.position;
		}





		public void UnregisterCallback()
		{
		}



		/// <summary>
		/// <para>在退回前重置。包括重置即时，</para>
		/// </summary>
		public void ResetOnReturn()
		{
			if (ContainTrailVFX)
			{
				if (TrailVFXRuntimeReference_PSPP)
				{
					TrailVFXRuntimeReference_PSPP.StopEmitNewParticle();
					TrailVFXRuntimeReference_PSPP.transform.SetParent(VFXPoolManager.Instance.transform);
				}
				else
				{
					TrailVFXRuntimeReference_PS.Stop();
					TrailVFXRuntimeReference_PS.transform.SetParent(VFXPoolManager.Instance.transform);
				}
			}
			if (ContainAppendVFX)
			{
				if (AppendVFXRuntimeReference_PSPP)
				{
					AppendVFXRuntimeReference_PSPP.transform.SetParent(VFXPoolManager.Instance.transform);
				}
				else
				{
					AppendVFXRuntimeReference_PS.transform.SetParent(VFXPoolManager.Instance.transform);
				}
			}

			AppendVFXRuntimeReference_PSPP = null;
			TrailVFXRuntimeReference_PSPP = null;
			AppendVFXRuntimeReference_PS = null;
			TrailVFXRuntimeReference_PS = null;
			DesiredMoveDirection = Vector3.zero;
			DesiredMoveSpeed = 0f;
			SelfActive = false;
			SelfPaused = false;
			ActiveElapsedTime = 0f;


			RelatedGORef.gameObject.SetActive(false);
			if (_ignoreTargetList != null)
			{
				_ignoreTargetList.Clear();
			}

			_selfNeutralizeComponent = null;
			SelfPenetrationComponent = null;
			SelfDropFromHeightComponent = null;
			SelfSpawnGameplayEventOnHitComponent = null;
			SelfSpawnGameplayEventOnActiveComponent = null;
			

			RelatedFunctionComponentList.Clear();
		}


		public void ClearAndUnload()
		{
			if (_ignoreTargetList != null)
			{
				CollectionPool<List<I_RP_Projectile_ObjectCanReceiveProjectileCollision>, I_RP_Projectile_ObjectCanReceiveProjectileCollision>.Release(_ignoreTargetList);
			}
			foreach (ProjectileBaseFunctionComponent perComponent in RelatedFunctionComponentList)
			{
				perComponent.ResetOnReturn();
			}
			CollectionPool<List<ProjectileBaseFunctionComponent>, ProjectileBaseFunctionComponent>.Release(
				RelatedFunctionComponentList);
		}



	}
}