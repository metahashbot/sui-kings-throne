using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ARPG.Common;
using GameplayEvent.Handler.CommonUtility;
using Global;
using Global.ActionBus;
using Global.AreaOnMap;
using Global.AssetLoad;
using Global.TimedTaskManager;
using Global.Utility;
using MeshCombineStudio;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
using WorldMapScene;
using WorldMapScene.RegionMap;
namespace ARPG.Manager
{
	/*
	 * 所有东西都有一个信息，表明自己位于的Terrain索引。
	 * 然后，绝大多数东西都只有一个Terrain信息，有些东西会跨Terrain。
	 * 关联EditorProxy_物件跨地形标识。常见于大的物件、特效。
	 * 常规的ItemHelper自带这个信息，其他东西比如特效、河流等，需要额外的Proxy
	 */
	[TypeInfoBox("场景四叉树。")] [AddComponentMenu("!#编辑期辅助#/【场景四叉树】_SceneTerrainQuadManager")]
	public class SceneTerrainQuadManager : MonoBehaviour
	{
		[SerializeField,LabelText("！！！本场景地形块尺寸！！！")]
		public int AreaChunkSize = 32;


		public enum QuadNeighborDirectionType_BaseOnForward
		{
			Top_上面的 = 1,
			Right_右边的 = 2,
			Bottom_下面的 = 3,
			Left_左边的 = 4
		}



#region 构建区块信息

#if UNITY_EDITOR


		[Button("按坐标重新排序")]
		public void SortChildrenByName()
		{
		
			// Get all children
			var children = GetComponentsInChildren<Transform>().ToList();
			children.Remove(transform);

			// Sort children by name
			var sortedChildren = children.OrderBy(t => t.name).ToList();

			// Rearrange children in hierarchy
			for (int i = 0; i < sortedChildren.Count; i++)
			{
				sortedChildren[i].SetSiblingIndex(i);
			}
			AllTerrainChunkList = new List<EditorProxy_TerrainChunk>();
			foreach (var perChunk in GetComponentsInChildren<EditorProxy_TerrainChunk>(true))
			{
				AllTerrainChunkList.Add(perChunk);
				perChunk.SelfChunkSize = AreaChunkSize;
			}
			
			 // apply override on this prefab
			UnityEditor.PrefabUtility.ApplyPrefabInstance(gameObject, UnityEditor.InteractionMode.UserAction);
			 
		}

		[Button("重设所有地形相邻信息")]
		public void RefreshAllTerrainNeighbor()
		{
			AllTerrainChunkList = new List<EditorProxy_TerrainChunk>();
			foreach (var perChunk in GetComponentsInChildren<EditorProxy_TerrainChunk>(true))
			{
				AllTerrainChunkList.Add(perChunk);
				perChunk.SelfChunkSize = AreaChunkSize;
			}
			if (AllTerrainChunkList == null || AllTerrainChunkList.Count == 0)
			{
				UnityEditor.EditorUtility.DisplayDialog("错误", "找不到任何地形块", "确定");
				return;
			}
			foreach (EditorProxy_TerrainChunk perChunk in AllTerrainChunkList)
			{
				perChunk.ChunkName_OnLeft = TryGetChunkOnPos(perChunk.ChunkPosCoordinate.GetLeft())?.name;
				perChunk.ChunkName_OnRight = TryGetChunkOnPos(perChunk.ChunkPosCoordinate.GetRight())?.name;
				perChunk.ChunkName_OnTop = TryGetChunkOnPos(perChunk.ChunkPosCoordinate.GetUp())?.name;
				perChunk.ChunkName_OnBottom = TryGetChunkOnPos(perChunk.ChunkPosCoordinate.GetDown())?.name;
			}
			UnityEditor.PrefabUtility.ApplyPrefabInstance(gameObject, UnityEditor.InteractionMode.UserAction);
		}
		 
		 
		
		
		
		
		
		//
		// [SerializeField,LabelText("指定作为0,0的地形块")]
		// private GameObject _zeroZeroChunk;
		//
		// [SerializeField,LabelText("地形块后缀名")]
		// private string _chunkSuffix = "场景名字";
		//
		// //坐标-地形-场景名
		// [Button("重写所有地形块名字")]
		// private void RewriteAllChunkName()
		// {
		// 	
		// 	if(_zeroZeroChunk == null)
		// 	{
		// 		UnityEditor.EditorUtility.DisplayDialog("错误","请指定一个作为0,0的地形块", "确定");
		// 		return;
		// 	}
		//
		// 	var allTerrains = GetComponentsInChildren<Terrain>(true);
		// 	var allMeshes = GetComponentsInChildren<MeshRenderer>(true);
		// 	if (allTerrains.Length > 0 && allMeshes.Length > 0)
		// 	{
		// 		UnityEditor.EditorUtility.DisplayDialog("错误", "Unity地形块和网格地形块不能同时存在", "确定");
		// 		return;
		// 	}
		//
		// 	if (allTerrains.Length > 0)
		// 	{
		// 		  
		// 		 
		// 	}
		// }
		
		
		
#endif

#endregion
		
		
		
		



		[ShowInInspector,LabelText("当前四叉树优化已开启？")]
		public bool Enable { get; private set; } = false;

		
		[SerializeField, LabelText("绘制规则对应的线框提示？")]
		public bool DrawRuleWireFrame = true;

		
		private float _nextRefreshTime;
		

		[SerializeField,LabelText("地形块列表")]
		public List<EditorProxy_TerrainChunk> AllTerrainChunkList = new List<EditorProxy_TerrainChunk>();

		public EditorProxy_TerrainChunk TryGetChunkOnPos(Vector2Int pos)
		{
			foreach (var perChunk in AllTerrainChunkList)
			{
				if (perChunk.ChunkPosCoordinate == pos)
				{
					return perChunk;
				}
			}
			return null;
		}

		public EditorProxy_TerrainChunk GetChunkRefByObjectPosition(Vector3 position)
		{
			for (int i = 0; i < AllTerrainChunkList.Count; i++)
			{
				var perChunk = AllTerrainChunkList[i];
				if (perChunk.IfPositionInsideAABB(position))
				{
					return perChunk;
				}
			}
			return null;
		}
		
		
		
		
		public Dictionary<string, EditorProxy_TerrainChunk> TerrainChunkDict { get; private set; } = null;
		public EditorProxy_TerrainChunk GetTerrainChunkByUID(string uid)
		{

			if (TerrainChunkDict != null)
			{

				if (string.IsNullOrEmpty(uid))
				{
					return null;
				}

				if (TerrainChunkDict.ContainsKey(uid))
				{
					return TerrainChunkDict[uid];
				}
				else
				{
					DBug.LogError($"找不到UID为{uid}的地形块！");
					return null;
				}

			}
			else
			{
			 
				if (string.IsNullOrEmpty(uid))
				{
					return null;
				}

				var fi = AllTerrainChunkList.FindIndex( (chunk) => chunk.name == uid);
				if (fi == -1)
				{
					// DBug.LogError($"找不到UID为{uid}的地形块！");
					return null;
				}
				else
				{
					return AllTerrainChunkList[fi];
				}
			}

		}




		//常规物件，小的Item，会直接被丢到EditorProxy_TerrainChunk之下。它们就简简单单被随着Terrain开启或关闭。
		//大一些的，比如河流和特效，会放到另外一个组里面，并且还会根据它们之上的Proxy信息，在这里面创建额外的数据结构来区分。在调整四叉树的时候，会有额外的逻辑来支持它们

		
		public class CrossChunkInfo
		{
			public readonly List<string> RelatedChunkUIDList;

			public readonly GameObject SelfGO;

			public CrossChunkInfo(List<string> relatedChunk, GameObject selfGo)
			{
				RelatedChunkUIDList = new List<string>(relatedChunk);
				SelfGO = selfGo;
			}
		}

		protected List<CrossChunkInfo> _selfAllCrossChunkInfo;


#region Rule   ——  预设规则

		[Serializable]
		public struct TargetChunkBySelfDirection
		{
			[SerializeField, LabelText("邻居方向")]
			public QuadNeighborDirectionType_BaseOnForward[] NeighborDirectionTypes;
		} 
		
		//位于第一象限的规则
		[FoldoutGroup("象限默认规则")]
		[SerializeField, LabelText("第一象限规则")]
		public List<TargetChunkBySelfDirection> FirstQuadRule = new List<TargetChunkBySelfDirection>
		{
			//右侧、右侧前方、前方、   左侧、左侧前方 、前方的前方
			//右侧
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
					{ QuadNeighborDirectionType_BaseOnForward.Right_右边的 }
			},
			//右侧前方
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
				{
					QuadNeighborDirectionType_BaseOnForward.Right_右边的, QuadNeighborDirectionType_BaseOnForward.Top_上面的
				}
			},
			//前方
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
					{ QuadNeighborDirectionType_BaseOnForward.Top_上面的 }
			},
			//左侧
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
					{ QuadNeighborDirectionType_BaseOnForward.Left_左边的 }
			},
			//左侧前方
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
				{
					QuadNeighborDirectionType_BaseOnForward.Left_左边的, QuadNeighborDirectionType_BaseOnForward.Top_上面的
				}
			},
			//前方的前方
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
				{
					QuadNeighborDirectionType_BaseOnForward.Top_上面的, QuadNeighborDirectionType_BaseOnForward.Top_上面的
				}
			},
		};

		[FoldoutGroup("象限默认规则")]
		[SerializeField, LabelText("第二象限规则")]
		public List<TargetChunkBySelfDirection> SecondQuadRule = new List<TargetChunkBySelfDirection>
		{
			//左侧、左侧前方、前方 、 右侧、右侧前方 、前方的前方
			//左侧
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
					{ QuadNeighborDirectionType_BaseOnForward.Left_左边的 }
			},
			//左侧前方
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
				{
					QuadNeighborDirectionType_BaseOnForward.Left_左边的, QuadNeighborDirectionType_BaseOnForward.Top_上面的
				}
			},
			//前方
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
					{ QuadNeighborDirectionType_BaseOnForward.Top_上面的 }
			},
			//右侧
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
					{ QuadNeighborDirectionType_BaseOnForward.Right_右边的 }
			},
			//右侧前方
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
				{
					QuadNeighborDirectionType_BaseOnForward.Right_右边的, QuadNeighborDirectionType_BaseOnForward.Top_上面的
				}
			},
			//前方的前方
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
				{
					QuadNeighborDirectionType_BaseOnForward.Top_上面的, QuadNeighborDirectionType_BaseOnForward.Top_上面的
				}
			},
		};


		[FoldoutGroup("象限默认规则")]
		[SerializeField, LabelText("第三象限规则")]
		public List<TargetChunkBySelfDirection> ThirdQuadRule = new List<TargetChunkBySelfDirection>
		{
			//左侧、左侧前方、前方、左侧下方、下方、右侧、右侧前方 。前方的前方
			//左侧
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
					{ QuadNeighborDirectionType_BaseOnForward.Left_左边的 }
			},
			//左侧前方
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
				{
					QuadNeighborDirectionType_BaseOnForward.Top_上面的, QuadNeighborDirectionType_BaseOnForward.Left_左边的
				}
			},
			//前方
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
					{ QuadNeighborDirectionType_BaseOnForward.Top_上面的 }
			},
			//左侧下方
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
				{
					QuadNeighborDirectionType_BaseOnForward.Bottom_下面的, QuadNeighborDirectionType_BaseOnForward.Left_左边的
				}
			},
			//下方
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
					{ QuadNeighborDirectionType_BaseOnForward.Bottom_下面的 }
			},
			//右侧
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
					{ QuadNeighborDirectionType_BaseOnForward.Right_右边的 }
			},
			//右侧前方
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
				{
					QuadNeighborDirectionType_BaseOnForward.Top_上面的, QuadNeighborDirectionType_BaseOnForward.Right_右边的
				}
			},
			//前方的前方
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
				{
					QuadNeighborDirectionType_BaseOnForward.Top_上面的, QuadNeighborDirectionType_BaseOnForward.Top_上面的
				}
			},
		};


		[FoldoutGroup("象限默认规则")]
		[SerializeField, LabelText("第四象限规则")]
		public List<TargetChunkBySelfDirection> FourthQuadRule = new List<TargetChunkBySelfDirection>
		{
			//前方、右侧、右侧前方、右侧下方、下方 、 左侧、左侧前方、前方的前方 

			//前方
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
					{ QuadNeighborDirectionType_BaseOnForward.Top_上面的 }
			},
			//右侧
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
					{ QuadNeighborDirectionType_BaseOnForward.Right_右边的 }
			},
			//右侧前方
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
				{
					QuadNeighborDirectionType_BaseOnForward.Top_上面的, QuadNeighborDirectionType_BaseOnForward.Right_右边的
				}
			},
			//右侧下方
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
				{
					QuadNeighborDirectionType_BaseOnForward.Bottom_下面的,
					QuadNeighborDirectionType_BaseOnForward.Right_右边的
				}
			},
			//下方
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
					{ QuadNeighborDirectionType_BaseOnForward.Bottom_下面的 }
			},
			//左侧
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
					{ QuadNeighborDirectionType_BaseOnForward.Left_左边的 }
			},
			//左侧前方
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
				{
					QuadNeighborDirectionType_BaseOnForward.Top_上面的, QuadNeighborDirectionType_BaseOnForward.Left_左边的
				}
			},
			//前方的前方
			new TargetChunkBySelfDirection
			{
				NeighborDirectionTypes = new QuadNeighborDirectionType_BaseOnForward[]
				{
					QuadNeighborDirectionType_BaseOnForward.Top_上面的, QuadNeighborDirectionType_BaseOnForward.Top_上面的
				}
			},
		};


#endregion   

#region 查找方法

		/// <summary>
		/// 根据传入的地形块，以及当前GRS的方向，来查找目标地形块
		/// </summary>
		public EditorProxy_TerrainChunk GetTargetChunkByDirectionType(string from,
			SceneTerrainQuadManager.QuadNeighborDirectionType_BaseOnForward directionInfo)
		{
			switch (directionInfo)
			{
				case SceneTerrainQuadManager.QuadNeighborDirectionType_BaseOnForward.Left_左边的:
					return GetTerrainChunkByUID(GetTerrainChunkByUID(from).ChunkName_OnLeft);
				case SceneTerrainQuadManager.QuadNeighborDirectionType_BaseOnForward.Right_右边的:
					return GetTerrainChunkByUID(GetTerrainChunkByUID(from).ChunkName_OnRight);
				case SceneTerrainQuadManager.QuadNeighborDirectionType_BaseOnForward.Top_上面的:
					return GetTerrainChunkByUID(GetTerrainChunkByUID(from).ChunkName_OnTop);
				case SceneTerrainQuadManager.QuadNeighborDirectionType_BaseOnForward.Bottom_下面的:
					return GetTerrainChunkByUID(GetTerrainChunkByUID(from).ChunkName_OnBottom);
			}
			return null;
		}

		public EditorProxy_TerrainChunk GetTargetChunkByDirectionType(
			EditorProxy_TerrainChunk from,
			SceneTerrainQuadManager.QuadNeighborDirectionType_BaseOnForward directionInfo)
		{
			switch (directionInfo)
			{
				case SceneTerrainQuadManager.QuadNeighborDirectionType_BaseOnForward.Left_左边的:
					return GetTerrainChunkByUID(from.ChunkName_OnLeft);
				case SceneTerrainQuadManager.QuadNeighborDirectionType_BaseOnForward.Right_右边的:
					return GetTerrainChunkByUID(from.ChunkName_OnRight);
				case SceneTerrainQuadManager.QuadNeighborDirectionType_BaseOnForward.Top_上面的:
					return GetTerrainChunkByUID(from.ChunkName_OnTop);
				case SceneTerrainQuadManager.QuadNeighborDirectionType_BaseOnForward.Bottom_下面的:
					return GetTerrainChunkByUID(from.ChunkName_OnBottom);
			}
			return null;
		}


		
		

		public EditorProxy_TerrainChunk GetTargetChunkByDirectionType(string from,
			SceneTerrainQuadManager.TargetChunkBySelfDirection directionInfo)
		{
			var currentChunk = GetTerrainChunkByUID(from);
			for (int i = 0; i < directionInfo.NeighborDirectionTypes.Length; i++)
			{
				SceneTerrainQuadManager.QuadNeighborDirectionType_BaseOnForward perType =
					directionInfo.NeighborDirectionTypes[i];
				var get = GetTargetChunkByDirectionType(currentChunk, perType);
				if (get == null)
				{
					// DBug.LogError($"在进行场景四叉树查找的时候，地块{currentChunk.SelfChunkNameUID}的{perType}方向上没有找到任何地块");
					return null;
				}
				else
				{
					currentChunk = get;
				}
			}
			return currentChunk;
		}


		public EditorProxy_TerrainChunk GetTargetChunkByDirectionType(
			EditorProxy_TerrainChunk from,
			SceneTerrainQuadManager.TargetChunkBySelfDirection directionInfo)
		{
			var currentChunk = from;
			for (int i = 0; i < directionInfo.NeighborDirectionTypes.Length; i++)
			{
				SceneTerrainQuadManager.QuadNeighborDirectionType_BaseOnForward perType =
					directionInfo.NeighborDirectionTypes[i];
				var get = GetTargetChunkByDirectionType(currentChunk, perType);
				if (get == null)
				{
					DBug.LogError($"在进行场景四叉树查找的时候，地块{currentChunk.name}的{perType}方向上没有找到任何地块");
					return null;
				}
				else
				{
					currentChunk = get;
				}
			}
			return currentChunk;
		}
 
		

#endregion

		private enum UpdateModeTypeEnum
		{
			ARPG = 0,
			WorldMap = 1,
			
		}
		

		private UpdateModeTypeEnum _updateType;


		public void AwakeInitialize()
		{
			
		}

		private List<EditorProxy_CrossChunkObject> _allCrossChunkHelperList = new List<EditorProxy_CrossChunkObject>();

		
		public void StartInitialize()
		{
			GlobalActionBus.GetGlobalActionBus().RegisterAction(
				ActionBus_ActionTypeEnum.G_Global_EPCSetItemShaderProcessed_编辑器辅助功能组_物件组_物件Shader处理完毕,
				_ABC_ProcessMCSCombineAfterSetItemShader);
			TerrainChunkDict = new Dictionary<string, EditorProxy_TerrainChunk>();
			switch (GlobalConfigurationAssetHolderHelper.GetCurrentLoadPhase())
			{
				case GeneralGameAssetLoadPhaseEnum.WorldMap:
				case GeneralGameAssetLoadPhaseEnum.RegionMap:
					_updateType = UpdateModeTypeEnum.WorldMap;
					break;
				case GeneralGameAssetLoadPhaseEnum.ARPG:
					_updateType = UpdateModeTypeEnum.ARPG;

					break;
			}
			
			
			/*
			 * 顺序：
			 * 1.找所有的TerrainChunk，构建容器
			 *
			 *2.运行MCS，进行网格合并。然后将合并完成后的物件放到对应的TerrainChunk下面
			 * 
			 *
			 * 3.找所有的CrossChunkObject，放到自己这
			 */

			

			//1.
			var allChunk =
				FindObjectsByType<EditorProxy_TerrainChunk>(FindObjectsInactive.Include, FindObjectsSortMode.None);
			for (int i = 0; i < allChunk.Length; i++)
			{
				var tChunk = allChunk[i];
				if (TerrainChunkDict.ContainsKey(tChunk.name))
				{

					DBug.LogError( $"场景四叉树管理器在初始化的时候，发现了重复的地形块UID：{tChunk.name}，将会忽略后面的这个地形块");
				}
				else
				{
					TerrainChunkDict.Add(tChunk.name, tChunk);
				}
			}

			foreach (EditorProxy_TerrainChunk perChunk in allChunk)
			{
				perChunk.InitializeBySceneTerrainQuadManager(this);
			}
			
			
			
			
			//3.找所有的CrossChunk
			_allCrossChunkHelperList = new List<EditorProxy_CrossChunkObject>(
				FindObjectsOfType<EditorProxy_CrossChunkObject>(true));
			_selfAllCrossChunkInfo = new List<CrossChunkInfo>();
			foreach (var perObject in _allCrossChunkHelperList)
			{
				CrossChunkInfo newInfo = new CrossChunkInfo(perObject.RelatedTerrainList,perObject.gameObject);
				_selfAllCrossChunkInfo.Add(newInfo);
			}

			
		}



		/// <summary>
		/// <para>在EPC_SetItemShader运行完成后，进行合并。合并所有的Item</para>
		/// <para></para>
		/// </summary>
		/// <param name="ds"></param>
		private void _ABC_ProcessMCSCombineAfterSetItemShader(DS_ActionBusArguGroup ds)
		{
			Enable = ds.IntArgu1.Value == 1;


			
		}
		
		

		public void UpdateTick(float ct, int cf, float delta)
		{
			if (Enable)
			{
				
				if (ct > _nextRefreshTime)
				{
					_nextRefreshTime = ct + 1f;
					RefreshQuadState();
				}
			}
		}


		/// <summary>
		/// <para>上一个所在的Chunk。</para>
		/// </summary>
		private EditorProxy_TerrainChunk _lastChunk;
		/// <summary>
		/// 上一个所在的象限
		/// </summary>
		private int _lastQuadrant;


		private void RefreshQuadState(bool forceUpdate = false)
		{
			if (!Enable)
			{
				return;
			}
			Vector3 playerPosition = Vector3.zero;
			switch (_updateType)
			{
				case UpdateModeTypeEnum.ARPG:
					playerPosition = GameReferenceService_ARPG.Instance.SubGameplayLogicManagerRef
						.PlayerCharacterBehaviourControllerReference.CurrentControllingBehaviour.transform.position;
					break;
				case UpdateModeTypeEnum.WorldMap:
					playerPosition = GameReferenceService_WorldMap.Instance.PlayerBehaviourControllerReference
						.CurrentControllingPlayerTeam.MainCharacterOfThisGroup.transform.position;
					break;
			}
			//获得当前所在的chunk和象限
			EditorProxy_TerrainChunk currentChunk = null;
			int currentQuadrant = 0;

			//遍历所有的Chunk，找到玩家当前在的那个
			foreach (EditorProxy_TerrainChunk perTerrain in TerrainChunkDict.Values)
			{
				if (perTerrain.IfPositionInsideAABB(playerPosition))
				{
					currentChunk = perTerrain;
					currentQuadrant = perTerrain.GetCurrentQuadrantIndex(playerPosition);
					break;
				}
			}


			//如果之前的chunk是空的，或者现在的chunk发生了变动，或者象限发生了变动，都会进行刷新
			if (!ReferenceEquals(_lastChunk, currentChunk) || _lastChunk == null || _lastQuadrant != currentQuadrant)
			{
				ProcessQuadTreeRefresh(currentChunk, currentQuadrant);
			}
			_lastChunk = currentChunk;
			_lastQuadrant = currentQuadrant;
		}


		private List<EditorProxy_TerrainChunk> activeList = new List<EditorProxy_TerrainChunk>();
		private void ProcessQuadTreeRefresh(EditorProxy_TerrainChunk chunk, int quadrant)
		{
			// activeList.Clear();
			//
			// //玩家在的chunk刷新了
			//
			// //象限默认规则
			// switch (quadrant)
			// {
			// 	case 1:
			// 		foreach (string perChunk in chunk.FirstTreeChunkList)
			// 		{
			// 			activeList.Add(GetTerrainChunkByUID(perChunk));
			//
			// 		}
			// 		break;
			// 	case 2:
			// 		foreach (string perChunk in chunk.SecondTreeChunkList)
			// 		{
			// 			activeList.Add(GetTerrainChunkByUID(perChunk));
			//
			// 		}
			// 		break;
			// 	case 3:
			// 		foreach (string perChunk in chunk.ThirdTreeChunkList)
			// 		{
			// 			activeList.Add(GetTerrainChunkByUID(perChunk));
			//
			// 		}
			// 		break;
			// 	case 4:
			// 		foreach (string perChunk in chunk.FourthTreeChunkList)
			// 		{
			// 			activeList.Add(GetTerrainChunkByUID(perChunk));
			//
			// 		}
			// 		break;
			// }
			// //额外常驻规则
			// foreach (string perA in chunk.AlwaysNeedChunkNameList)
			// {
			// 	activeList.Add(GetTerrainChunkByUID(perA));
			// }
			//
			//
			// foreach (var perTerrain in TerrainChunkDict.Values)
			// {
			// 	if (activeList.Contains(perTerrain))
			// 	{
			// 		perTerrain.CurrentChunkVisualActive = true;
			// 	}
			// 	else
			// 	{
			// 		perTerrain.CurrentChunkVisualActive = false;
			// 	}
			// }
			//
			// //处理CrossItem
			// foreach (EditorProxy_CrossChunkObject perCross in _allCrossChunkHelperList)
			// {
			// 	//如果它的关联列表里面有任何一个是active的，那就激活它
			// 	bool ifActive = false;
			// 	foreach (string perChunkName in perCross.RelatedTerrainList)
			// 	{
			// 		if (activeList.Exists((terrainChunk => terrainChunk.name.Contains(perChunkName))))
			// 		{
			// 			ifActive = true;
			// 			break;	
			// 		}
			// 	}
			//
			// 	perCross.gameObject.SetActive(ifActive);
			// 	
			// }
			
		}


	}

	
}