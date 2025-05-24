using System;
using System.Collections.Generic;
using ARPG.Character;
using ARPG.Manager;
using Global;
using MeshCombineStudio;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Common
{
	[AddComponentMenu("!#编辑期辅助#/【地形块】这是一个地形块_TerrainChunk")]
	[TypeInfoBox("一块地形。记录了自己处于四叉时，每个叉需要打开的其他地形块。\n" + "但实际上优化后他们通常不是地形而是转换后的网格。\n" + "需要确定：基准点[Pivot]在左下角，然后标识好具体的尺寸！")]
	public class EditorProxy_TerrainChunk : MonoBehaviour
	{
		private static SceneTerrainQuadManager _quadManagerRef;

		[SerializeField, LabelText("地块坐标")] [TitleGroup("===配置===")]
		public Vector2Int ChunkPosCoordinate;


		[LabelText("地块尺寸——于X轴"), TitleGroup("===配置===")]
		public float SelfChunkSize = 60;
		public Vector2 SelfAABB_LeftBottom => new Vector2(transform.position.x, transform.position.z);


		public Vector2 SelfAABB_RightTop =>
			new Vector2(transform.position.x + SelfChunkSize, transform.position.z + SelfChunkSize);

		public Vector2 SelfAABB_Center => (SelfAABB_LeftBottom + SelfAABB_RightTop) / 2;





		[LabelText("左侧地形块的名字"), SerializeField] [TitleGroup("===配置===")]
		public string ChunkName_OnLeft;
		[SerializeField, LabelText("右侧地形块的名字")] [TitleGroup("===配置===")]
		public string ChunkName_OnRight;
		[SerializeField, LabelText("上侧地形块的名字")] [TitleGroup("===配置===")]
		public string ChunkName_OnTop;
		[SerializeField, LabelText("下侧地形块的名字")] [TitleGroup("===配置===")]
		public string ChunkName_OnBottom;








// #if UNITY_EDITOR
// 		[Button("自动查找四方向的其他地形块")]
// 		public void Button_TryFindOtherChunk()
// 		{
// 			ChunkName_OnLeft = null;
// 			ChunkName_OnRight = null;
// 			ChunkName_OnTop = null;
// 			ChunkName_OnBottom = null;
// 			// 1. find left
// 			var left = transform.position + Vector3.left + Vector3.forward + Vector3.up * 100f;
// 			var leftHit = Physics.RaycastAll(left, Vector3.down, 1000, LayerMask.GetMask("Terrain"));
// 			if (leftHit != null && leftHit.Length > 0)
// 			{
// 				var leftChunk = leftHit[0].collider.GetComponent<EditorProxy_TerrainChunk>();
// 				if (leftChunk != null)
// 				{
// 					ChunkName_OnLeft = leftChunk.name;
// 				}
// 			}
//
// 			// 2. find right
// 			var right = transform.position + Vector3.right * SelfChunkWidth + Vector3.right +Vector3.forward + Vector3.up * 100f;
// 			var rightHit = Physics.RaycastAll(right, Vector3.down, 1000, LayerMask.GetMask("Terrain"));
// 			if (rightHit != null && rightHit.Length > 0)
// 			{
// 				var rightChunk = rightHit[0].collider.GetComponent<EditorProxy_TerrainChunk>();
// 				if (rightChunk != null)
// 				{
// 					ChunkName_OnRight = rightChunk.name;
// 				}
// 			}
//
// 			// 3. find top
// 			var top = transform.position + Vector3.forward * SelfChunkDepth + Vector3.forward + Vector3.right + Vector3.up * 100f;
// 			var topHit = Physics.RaycastAll(top, Vector3.down, 1000, LayerMask.GetMask("Terrain"));
// 			if (topHit != null && topHit.Length > 0)
// 			{
// 				var topChunk = topHit[0].collider.GetComponent<EditorProxy_TerrainChunk>();
// 				if (topChunk != null)
// 				{
// 					ChunkName_OnTop = topChunk.name;
// 				}
// 			}
//
// 			// 4. find bottom
// 			var bottom = transform.position +Vector3.back +Vector3.right  + Vector3.up * 100f;
// 			var bottomHit = Physics.RaycastAll(bottom, Vector3.down, 1000, LayerMask.GetMask("Terrain"));
// 			if (bottomHit != null && bottomHit.Length > 0)
// 			{
// 				var bottomChunk = bottomHit[0].collider.GetComponent<EditorProxy_TerrainChunk>();
// 				if (bottomChunk != null)
// 				{
// 					ChunkName_OnBottom = bottomChunk.name;
// 				}
// 			}
// 		}
// #endif






		[SerializeField, LabelText("常驻活跃地块")]
		public List<string> AlwaysNeedChunkNameList = new List<string>();




		public void InitializeBySceneTerrainQuadManager(SceneTerrainQuadManager quadManager)
		{
			_quadManagerRef = quadManager;
			// RebuildQuadTree();
		}


		private void Awake()
		{
#if UNITY_EDITOR
			if (ChunkName_OnLeft == null && ChunkName_OnBottom == null && ChunkName_OnRight == null &&
			    ChunkName_OnTop == null)
			{
				DBug.LogWarning($"地形块{name}上没有指定任何相关联的地形块UID，这可能并不合理");
			}
#endif
			Vector3 leftBottomV3 = transform.position;
		
			if (_SelfHolder == null)
			{
				_SelfHolder = new GameObject("ChunkHolder");
				_SelfHolder.transform.SetParent(transform);
				_SelfHolder.transform.localPosition = Vector3.zero;
				_SelfHolder.transform.localRotation = Quaternion.identity;
				_SelfHolder.transform.localScale = Vector3.one;
			}
			_selfMR = GetComponent<MeshRenderer>();
		}


		/// <summary>
		/// <para>检查目标位置是否在这个地形上</para>
		/// </summary>
		public bool IfPositionInsideAABB(Vector3 v3)
		{
			return IfPositionInsideAABB(new Vector2(v3.x, v3.z));
		}

		/// <summary>
		/// <para>检查目标位置是否在这个地形上</para>
		/// </summary>
		public bool IfPositionInsideAABB(Vector2 pos)
		{
			// check if pos in aabb
			if (pos.x >= SelfAABB_LeftBottom.x && pos.x <= SelfAABB_RightTop.x && pos.y >= SelfAABB_LeftBottom.y &&
			    pos.y <= SelfAABB_RightTop.y)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public int GetCurrentQuadrantIndex(Vector3 pos)
		{
			if (!IfPositionInsideAABB(pos))
			{
				return 0;
			}
			else
			{
				var center = SelfAABB_Center;
				if (pos.x <= center.x)
				{
					if (pos.z <= center.y)
					{
						return 3;
					}
					else
					{
						return 2;
					}
				}
				else
				{
					if (pos.z <= center.y)
					{
						return 4;
					}
					else
					{
						return 1;
					}
				}
			}
		}


#region 规则
		
		[SerializeField, LabelText("当前第一象限的地形块列表")]
		[GUIColor(1f,0f,0f)]
		public List<string> FirstTreeChunkList = new List<string>();
		[SerializeField, LabelText("当前第二象限的地形块列表")]
		[GUIColor(1f, 0.92f, 0.016f)]
		public List<string> SecondTreeChunkList = new List<string>();
		[SerializeField, LabelText("当前第三象限的地形块列表")]
		[GUIColor(0f,1f,0f)]
		public List<string> ThirdTreeChunkList = new List<string>();
		[SerializeField, LabelText("当前第四象限的地形块列表")]
		[GUIColor(0f,1f,1f)]
		public List<string> FourthTreeChunkList = new List<string>();


		/// <summary>
		/// 重新构建四叉树信息，刷新四个象限中将要开启的地块名字
		/// </summary>
		[Button("使规则恢复为默认状态！")]
		public void RebuildQuadTree()
		{
			FirstTreeChunkList.Clear();
			SecondTreeChunkList.Clear();
			ThirdTreeChunkList.Clear();
			FourthTreeChunkList.Clear();
			 
			FirstTreeChunkList.Add(name);
			SecondTreeChunkList.Add(name);
			ThirdTreeChunkList.Add(name);
			FourthTreeChunkList.Add(name);
			_quadManagerRef = FindObjectOfType<SceneTerrainQuadManager>();
			if (AlwaysNeedChunkNameList != null)
			{
				foreach (var per in AlwaysNeedChunkNameList)
				{
					var get = _quadManagerRef.GetTerrainChunkByUID(per);
					if (get != null)
					{
						FirstTreeChunkList.Add(get.name);
						SecondTreeChunkList.Add(get.name);
						ThirdTreeChunkList.Add(get.name);
						FourthTreeChunkList.Add(get.name);
					}
				}
			}
			foreach (SceneTerrainQuadManager.TargetChunkBySelfDirection perDire in _quadManagerRef.FirstQuadRule)
			{
				var get = _quadManagerRef.GetTargetChunkByDirectionType(name, perDire);
				if (get != null)
				{
					FirstTreeChunkList.Add(get.name);
				}
			}

			foreach (SceneTerrainQuadManager.TargetChunkBySelfDirection perDire in _quadManagerRef.SecondQuadRule)
			{
				var get = _quadManagerRef.GetTargetChunkByDirectionType(name, perDire);
				if (get != null)
				{
					SecondTreeChunkList.Add(get.name);
				}
			}

			foreach (SceneTerrainQuadManager.TargetChunkBySelfDirection perDire in _quadManagerRef.ThirdQuadRule)
			{
				var get = _quadManagerRef.GetTargetChunkByDirectionType(name, perDire);
				if (get != null)
				{
					ThirdTreeChunkList.Add(get.name);
				}
			}

			foreach (SceneTerrainQuadManager.TargetChunkBySelfDirection perDire in _quadManagerRef.FourthQuadRule)
			{
				var get = _quadManagerRef.GetTargetChunkByDirectionType(name, perDire);
				if (get != null)
				{
					FourthTreeChunkList.Add(get.name);
				}
			}
		}

#endregion

		private MeshRenderer _selfMR;
		
		private GameObject _SelfHolder;
		public GameObject SelfHolder => _SelfHolder;


		private bool _currentChunkVisualActive;
		/// <summary>
		/// 当前这个区块视觉可见吗？
		/// </summary>
		public bool CurrentChunkVisualActive {
			get
			{
				return _currentChunkVisualActive;
			}
			set
			{
				_currentChunkVisualActive = value;
				ToggleVisualActive();
			}
		}



		private void ToggleVisualActive()
		{
			if (_selfMR)
			{
				_selfMR.enabled = _currentChunkVisualActive;
				_SelfHolder.SetActive(_currentChunkVisualActive);
			}
			else
			{
				
			}
			
			 
		}





#if UNITY_EDITOR
		private void OnDrawGizmosSelected()
		{
			var manager = FindObjectOfType<SceneTerrainQuadManager>();
			
			if (!manager.DrawRuleWireFrame)
			{
				return;
			}
			var allTerrainChunks = FindObjectsOfType<EditorProxy_TerrainChunk>();

			foreach (EditorProxy_TerrainChunk perChunk in allTerrainChunks)
			{
				var leftBottomV3 = perChunk.transform.position;

				if(FirstTreeChunkList.Contains(perChunk.name))
				{
					Gizmos.color = Color.red;
					var centerPos_1 = leftBottomV3 + new Vector3(perChunk.SelfChunkSize / 2f, 0f, perChunk.SelfChunkSize / 2f);
					centerPos_1 += Vector3.right;
					centerPos_1 += Vector3.forward;
					centerPos_1 += Vector3.up;
					Gizmos.DrawWireCube(centerPos_1, new Vector3(perChunk.SelfChunkSize, 100f, perChunk.SelfChunkSize));
					 
				}
				if(SecondTreeChunkList.Contains(perChunk.name))
				{
					Gizmos.color = Color.yellow;
					var centerPos_2 = leftBottomV3 +
					                  new Vector3(perChunk.SelfChunkSize / 2f, 0f, perChunk.SelfChunkSize / 2f);
					centerPos_2 += Vector3.left;
					centerPos_2 += Vector3.forward * 1.25f;
					centerPos_2 += Vector3.up * 1.25f;
					Gizmos.DrawWireCube(centerPos_2, new Vector3(perChunk.SelfChunkSize, 100f, perChunk.SelfChunkSize));
					
				}
				if(ThirdTreeChunkList.Contains(perChunk.name))
				{
					Gizmos.color = Color.green;
					var centerPos_2 = leftBottomV3 +
					                  new Vector3(perChunk.SelfChunkSize / 2f, 0f, perChunk.SelfChunkSize / 2f);
					centerPos_2 += Vector3.left * 1.25f;
					centerPos_2 += Vector3.back ;
					centerPos_2 += Vector3.down;

					Gizmos.DrawWireCube(centerPos_2, new Vector3(perChunk.SelfChunkSize, 100f, perChunk.SelfChunkSize));
				}
				if(FourthTreeChunkList.Contains(perChunk.name))
				{
					Gizmos.color = Color.cyan;
					var centerPos_2 = leftBottomV3 +
					                  new Vector3(perChunk.SelfChunkSize / 2f, 0f, perChunk.SelfChunkSize / 2f);
					centerPos_2 += Vector3.right * 1.25f;
					centerPos_2 += Vector3.back * 1.25f;
					centerPos_2 += Vector3.down * 1.25f;

					Gizmos.DrawWireCube(centerPos_2, new Vector3(perChunk.SelfChunkSize, 100f, perChunk.SelfChunkSize));
				}
				if(AlwaysNeedChunkNameList != null && AlwaysNeedChunkNameList.Contains(perChunk.name))
				{
					Gizmos.color = Color.white;
					var centerPos_2 = leftBottomV3 +
					                  new Vector3(perChunk.SelfChunkSize / 2f, 0f, perChunk.SelfChunkSize / 2f);
					Gizmos.DrawWireCube(centerPos_2, new Vector3(perChunk.SelfChunkSize, 100f, perChunk.SelfChunkSize));
				}
			}
		}
#endif
	}

}