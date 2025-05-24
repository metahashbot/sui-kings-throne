using System;
using System.Collections.Generic;
using ARPG.Manager;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Common
{
	[AddComponentMenu("!#编辑期辅助#/【物件】/【跨地形】这是一个跨地形物件，用于加载四叉树优化")]
	[TypeInfoBox("物件跨地形标识。常见于大的物件、特效。\n")]
	public class EditorProxy_CrossChunkObject : MonoBehaviour
	{

		
		[Header("注意看Scene，有关的区域会被红框框起来")]
		[LabelText("关联的地形区块")]
		public List<string> RelatedTerrainList = new List<string>();
	

		[Button("把自己当前位于的地形块加进去")]
		private void _Button_AddCurrentOn()
		{
			_ValidateList();
			var self = GetSelfDirectOnTerrain();
			if (self == null)
			{
				DBug.LogError($"{name}之下不存在任何地形，不能这么添加。");
				return;
			}
			if (!RelatedTerrainList.Exists(s => s.Equals(self.name,StringComparison.OrdinalIgnoreCase)))
			{
				RelatedTerrainList.Add(self.name);
			}
			
		}

		
		
		
		

		private EditorProxy_TerrainChunk GetSelfDirectOnTerrain()
		{
			//raycast to terrain, to get the terrain chunk
			var ray = new Ray(transform.position + Vector3.up * 1000, Vector3.down);
			if (Physics.Raycast(ray, out var hit, 2000, 1 << 11))
			{
				var terrainChunk = hit.collider.GetComponent<EditorProxy_TerrainChunk>();
				return terrainChunk;
			}

			return null;
		}
		private void _ValidateList()
		{
			if (RelatedTerrainList == null)
			{
				return;
			}
			for (int i = RelatedTerrainList.Count - 1; i >= 0; i--)
			{
				if (RelatedTerrainList[i] == null)
				{
					RelatedTerrainList.RemoveAt(i);
				}
			}
		}


#if UNITY_EDITOR
		
		// private void OnDrawGizmosSelected()
		// {
		// 	var QuadRef = FindObjectOfType<SceneTerrainQuadManager>();
		// 	if (QuadRef == null)
		// 	{
		// 		return;
		// 	}
		// 	_ValidateList();
		//
		// 	foreach (string perTerrain in RelatedTerrainList)
		// 	{
		// 		var t = QuadRef.GetTerrainChunkByUID(perTerrain);
		// 		if (t == null)
		// 		{
		// 			continue;
		// 		}
		// 		Vector3 leftBottomV3 = t.transform.position;
		// 	
		// 		var centerPos = leftBottomV3 + new Vector3(t.SelfChunkWidth / 2f, 0f, t.SelfChunkDepth / 2f);
		// 		Gizmos.color = Color.red;
		// 		Gizmos.DrawWireCube(centerPos, new Vector3(t.SelfChunkWidth, 100f, t.SelfChunkDepth));
		// 		;
		// 	}
		// 	
		// 	
		// }
		
#endif



	}
}