using System;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Common
{

	[AddComponentMenu("!#编辑期辅助#/[区域配置信息]/出生点标记_PlayerSpawnRegister", -33)]
	public class PlayerSpawnRegister : BaseSpawnRegister
	{





#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(transform.position, 0.25f);
		}

		[Button("将自身对齐到地形",ButtonSizes.Large)]
		private void _Button_AlignToTerrain()
		{
			var results = Physics.RaycastAll(transform.position + Vector3.up * 100f, Vector3.down, 9999f, 1 << 11);
			float maxHit = -999f;
			foreach (RaycastHit perHit in results)
			{
				if (perHit.point.y > maxHit)
				{
					transform.position = perHit.point;
					maxHit = perHit.point.y;
				}
				
			}
		}
		
		
		
		
		
		
#endif
	}
}