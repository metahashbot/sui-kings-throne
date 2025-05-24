/*
│	File:ProjectileBehaviourManager.cs
│	DroyLouo(ccd775@gmail.com)
│	CreateTime:2021-1-27 15:14:46
╰━━━━━━━━━━━━━━━━
*/
//#pragma warning disable CS0162
//#pragma warning disable CS0414


using System;
using System.Collections.Generic;
using ARPG.Character.Player;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using RPGCore.Interface;
using RPGCore.Projectile.Layout;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Profiling;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace RPGCore.Projectile
{
	/// <summary>
	/// <para> 所有Projectile 的管理 子Manager，从属于GLM;</para>
	/// <para> 包含 池化、维持、Tick。加载归属于RAH处理，</para>
	/// </summary>
	public partial class ProjectileBehaviourManager : MonoBehaviour
	{


#region 外部引用

		// private SubGameplayLogicManager_BattleGame _glmRef;
		// private RPBehaviourRecordService_BattleGame _rpRecordRef;
		// private PlayerCharacterInBattleGame_ARPG _playerCharacterRef;
		private GlobalActionBus _globalActionBusRef;


		/// <summary>
		/// <para>投射物消失边界，xy是左下角的XZ，zw是右上角的XZ</para>
		/// </summary>
		[SerializeField]
		public Vector4 ProjectileVanishBoundAABB;


		private PlayerCharacterBehaviourController _playerCharacterBehaviourControllerRef;

#endregion

#region 池化部分

		/// <summary>
		/// <para>Key是UID，不是显示名字</para>
		/// </summary>
		[ShowInInspector, LabelText("投射物对象池信息"),FoldoutGroup("运行时",true)]
		private Dictionary<string, ProjectilePerTypePool> _selfProjectileTypePoolDict;


		

		/// <summary>
		/// <para>PBM内部使用。某一个类型的Projectile的池的类。</para>
		/// </summary>
		private class ProjectilePerTypePool
		{
			/// <summary>
			/// <para>ProjectileManager的Transform</para>
			/// </summary>
			private static Transform ProjectileTransformParent;
			public static void SetProjectileManagerParentReference(Transform parent)
			{
				ProjectileTransformParent = parent;
			}


			[ShowInInspector, LabelText("投射物显示名称")]
			public string SelfDisplayName;
			/// <summary>
			/// 将要于下一帧退回池中的活跃对象的索引
			/// </summary>
			private List<int> _needToReturn;


			[ShowInInspector, LabelText("来自原始SO的配置内容")]
			public SOFE_ProjectileLoad.LoadContent OriginalConfig;

			[ShowInInspector, LabelText("已加载艺术GO")]
			public GameObject LoadedOriginalArtGO;

			[ShowInInspector, LabelText("包含[拖尾特效]?")]
			public bool ContainTrailVFX;

			[ShowInInspector, LabelText("[拖尾特效]的已加载艺术GO")]
			public GameObject LoadedTrailArtGO;


			[ShowInInspector, LabelText("包含[附带特效]?")]
			public bool ContainAppendVFX;
			
			[ShowInInspector, LabelText("[附带特效]的已加载艺术GO")]
			public GameObject LoadedAppendArtGO;
			
			
			private Nullable<AsyncOperationHandle<GameObject>> op_addon;

			private AsyncOperationHandle<GameObject> op;
			
			
			
			

			[ShowInInspector,LabelText("当前空闲投射物")]
			private List<ProjectileBehaviour_Runtime> _currentFreeProjectileList;
			[ShowInInspector,LabelText("当前忙碌投射物")]
			private List<ProjectileBehaviour_Runtime> _currentBusyProjectileList;


			public void ClearWhenUnload()
			{
				if (op.IsValid())
				{
					Addressables.Release(op);
				}
				if(op_addon!=null && op_addon.Value.IsValid())
				{
					Addressables.Release(op_addon.Value);
				}
				foreach (ProjectileBehaviour_Runtime behaviour in _currentBusyProjectileList)
				{
					GameObject.Destroy(behaviour.RelatedGORef);
				}

				foreach (ProjectileBehaviour_Runtime behaviour in _currentFreeProjectileList)
				{
					GameObject.Destroy(behaviour.RelatedGORef);
				}
			}

			/// <summary>
			/// <para>创建一个新的运行时Projectile</para>
			/// </summary>
			/// <returns></returns>
			private ProjectileBehaviour_Runtime CreateNew()
			{
				int nameIndex = _currentBusyProjectileList.Count + _currentFreeProjectileList.Count;

				var tmpGO = new GameObject(
					$"{OriginalConfig.ProjectileDisplayName}_{nameIndex}");
				tmpGO.transform.SetParent(ProjectileTransformParent);
				tmpGO.SetActive(false);
				var newArtHelper = GameObject.Instantiate(LoadedOriginalArtGO, tmpGO.transform);
				


				ProjectileBehaviour_Runtime currentNewBehaviour = new ProjectileBehaviour_Runtime();

				currentNewBehaviour.InitializeOnSpawnIntoPool(tmpGO,
					newArtHelper,
					OriginalConfig,
					LoadedTrailArtGO,
					LoadedAppendArtGO);

				var targetEditorHelper = tmpGO.GetComponentInChildren<ProjectileBehaviour_EditorHelper>();
				//编辑期状态下，始终添加EditorHelper
				if (Application.isEditor)
				{
					if (targetEditorHelper == null)
					{
						tmpGO.AddComponent<ProjectileBehaviour_EditorHelper>().SelfRelatedProjectileBehaviour =
							currentNewBehaviour;
					}
				}
				//非编辑期下，则删除它
				else
				{

					if (targetEditorHelper != null)
					{
						Destroy(targetEditorHelper);
					}
				}
				
				return currentNewBehaviour;
			}
			
			
			/// <summary>
			/// <para>构造。准备一系列Projectile的GameObject。（带艺术表现）</para>
			/// </summary>
			public ProjectilePerTypePool(string uid)
			{
				SelfDisplayName = uid;
				OriginalConfig = GlobalConfigurationAssetHolderHelper.Instance.FE_ProjectileLoad
					.GetLoadContentByUID(uid);

				op = Addressables.LoadAssetAsync<GameObject>(OriginalConfig.PrefabAddress);
				LoadedOriginalArtGO = op.WaitForCompletion();
				if (OriginalConfig.RelationAddress != null && OriginalConfig.RelationAddress.Length > 1)
				{
					ContainTrailVFX = true;
					op_addon = Addressables.LoadAssetAsync<GameObject>(OriginalConfig.RelationAddress);
					LoadedTrailArtGO = op_addon.Value.WaitForCompletion();
				}
				else
				{
					ContainTrailVFX = false;
				}
				
				if(OriginalConfig.AppendVFXAddress !=null && OriginalConfig.AppendVFXAddress.Length > 1)
				{
					ContainAppendVFX = true;
					op_addon = Addressables.LoadAssetAsync<GameObject>(OriginalConfig.AppendVFXAddress);
					LoadedAppendArtGO = op_addon.Value.WaitForCompletion();
				}
				else
				{
					ContainAppendVFX = false;
				}


				_needToReturn = new List<int>();
				_currentFreeProjectileList = new List<ProjectileBehaviour_Runtime>();
				_currentBusyProjectileList = new List<ProjectileBehaviour_Runtime>();

				int count = OriginalConfig.PreloadCount;

				if (count > 0)
				{
					for (int i = 0; i < count; i++)
					{
						ProjectileBehaviour_Runtime newP = CreateNew();
						_currentFreeProjectileList.Add(newP);
					}
				}
			}


			public void ReturnToPool(ProjectileBehaviour_Runtime projectile)
			{
				projectile.ResetOnReturn();
				if (_currentBusyProjectileList.Contains(projectile))
				{
					_currentBusyProjectileList.Remove(projectile);
				}
				if (!_currentFreeProjectileList.Contains(projectile))
				{
					_currentFreeProjectileList.Add(projectile);
				}
			}

			public ProjectileBehaviour_Runtime GerFreeProjectile()
			{
				
				//先试着拿，拿不出来再新建
				for (int i = _currentFreeProjectileList.Count - 1; i >= 0; i--)
				{
					var tmpProjectile = _currentFreeProjectileList[i];
					if (!tmpProjectile.SelfActive)
					{
						_currentFreeProjectileList.Remove(tmpProjectile);
						_currentBusyProjectileList.Add(tmpProjectile);
						return tmpProjectile;
					}
				}
				
				var newp = CreateNew();
				_currentBusyProjectileList.Add(newp);
				return newp;
				
			}

#if UNITY_EDITOR
			public void OnDrawGizmos()
			{
				
				
				
			}
#endif
			
		}

#endregion
		
		public void InitializeOnAwake(SubGameplayLogicManager_ARPG glm)
		{
			_selfProjectileTypePoolDict = new Dictionary<string, ProjectilePerTypePool>();
			_globalActionBusRef = GlobalActionBus.GetGlobalActionBus();
			ProjectilePerTypePool.SetProjectileManagerParentReference(transform);
			ProjectileBehaviour_Runtime.StaticInitialize(this);
			BaseProjectileLayoutComponent.StaticInitializeByPBM(this, glm);
		}


		public void StartInitialize()
		{
			_playerCharacterBehaviourControllerRef =
				SubGameplayLogicManager_ARPG.Instance.PlayerCharacterBehaviourControllerReference;
			// GlobalActionBus.GetGlobalActionBus().RegisterAction(ActionBus_ActionTypeEnum.G_Projectile_OnProjectileReadyToDestroy,
			// 	_ABC_RegisterReturnProjectile_OnProjectileReadyToDestroy);
		}

		public void LateInitialize()
		{
			//需要拿回当前场景的边界。
			//如果没拿到，那就给一个调试用的边界

			// var mapEditorProxy = FindObjectOfType<EditorProxy_BattleMapEditor>();
			// if (mapEditorProxy != null)
			// {
			// 	ProjectileBehaviour_Runtime.SetCurrentSceneVanishBoundInfo(mapEditorProxy.RelatedMapConfig.ProjectileVanishBoundAABB);
			//
			//
			// }
			// else
			// {
			// 	
			// 	
			// 	
			// 	ProjectileBehaviour_Runtime.SetCurrentSceneVanishBoundInfo(ProjectileVanishBoundAABB);
			// }
		}



		public void FixedUpdateTick(float currentTime, int currentFrame, float delta)
		{
			



		}


		/// <summary>
		/// <para>PBM使用的获取玩家方向和玩家位置的方式，可选传入offset，表示获取多少帧前的位置</para>
		/// </summary>
		/// <returns>第一个是世界位置，第二个是相对方向，由起始点到玩家的.nor</returns>
		public (Vector3, Vector3) GetAimPlayerPositionAndDirectionOfPBM(Vector3 casterFromPosition, int offset = 0)
		{
			return _playerCharacterBehaviourControllerRef.GetAimPlayerOfPlayerPosition(casterFromPosition , offset);
		}





		public void ReturnProjectileBehaviourRuntime(ProjectileBehaviour_Runtime p)
		{
			if (p.RelationDestroyAppendVFXPrefab)
			{
				var get = 
				VFXPoolManager.Instance.GetParticleSystemRuntimeByPrefab(p.RelationDestroyAppendVFXPrefab);
				get.transform.position = p.RelatedGORef.transform.position;
				get.Play();
			}
			_selfProjectileTypePoolDict[p.SelfUID].ReturnToPool(p);
		}
		

		public void ClearWhenUnload()
		{
			foreach (var pool in _selfProjectileTypePoolDict.Values)
			{
				pool.ClearWhenUnload();
			}

			_selfProjectileTypePoolDict.Clear();
		}



		public ProjectileBehaviour_Runtime GetAvailableProjectileByUID(string projectileUID)
		{
			if (!_selfProjectileTypePoolDict.ContainsKey(projectileUID))
			{
				ProjectilePerTypePool perTypePool = new ProjectilePerTypePool(projectileUID);
				_selfProjectileTypePoolDict.Add(projectileUID, perTypePool);
			}
			return _selfProjectileTypePoolDict[projectileUID].GerFreeProjectile();
		}



#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			float xSize = ProjectileVanishBoundAABB.z - ProjectileVanishBoundAABB.x;
			float zSize = ProjectileVanishBoundAABB.w - ProjectileVanishBoundAABB.y;
			Vector2 center = new Vector2((ProjectileVanishBoundAABB.z + ProjectileVanishBoundAABB.x) / 2f,
				(ProjectileVanishBoundAABB.w + ProjectileVanishBoundAABB.y) / 2f);
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(new Vector3(center.x, 0f, center.y), new Vector3(xSize, 10f, zSize));


			if (_selfProjectileTypePoolDict != null)
			{

				foreach (ProjectilePerTypePool projectilePerTypePool in _selfProjectileTypePoolDict.Values)
				{
					projectilePerTypePool.OnDrawGizmos();
				}
			}
		}
#endif
	}
}