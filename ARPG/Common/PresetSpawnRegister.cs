using System;
using System.Collections.Generic;
using ARPG.Config.BattleLevelConfig;
using Global.AreaOnMap.AreaFunctionHandler;
using Global.AreaOnMap.EditorProxy;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Common
{

	[TypeInfoBox("场景内用来标记生成点的Helper")]
	[AddComponentMenu("!#编辑期辅助#/刷怪生成点_PresetSpawnRegister")]
	public class PresetSpawnRegister : BaseSpawnRegister
	{
		
		
		[LabelText("这个生成点关联的区域名"), GUIColor(97f / 255f, 191f / 255f, 199f / 255f)]
		[InlineButton(nameof(_Button_FindParentAreaName), SdfIconType.Search, "向上查找父级区域")]
		public string RelatedAreaName = "_关联区域_待指定_";


		[LabelText("这个生成点的类型")]
		public EnemySpawnPoint_PresetTypeEnum SpawnPointPresetType = EnemySpawnPoint_PresetTypeEnum.Default_默认通用;


		[LabelText("[那里]一个目的地")]
		[ShowIf("@this.SpawnPointPresetType == EnemySpawnPoint_PresetTypeEnum.FromAndTo_从这里到那里")]
		public GameObject TargetPointPositionGO;

		[LabelText("一些地区，自己也要放进来！")]
		[ShowIf("@this.SpawnPointPresetType == EnemySpawnPoint_PresetTypeEnum.OrderName_按顺序的")]
		public List<GameObject> SomePositionsGOList;





		[LabelText("这个生成点的名字"), GUIColor(133f / 255f, 209f / 255f, 173f / 255f)]
#if UNITY_EDITOR
		[OnValueChanged(nameof(_OVC_RegisterNameChanged))]
		[InlineButton("_IB_SyncName" , "同步为物件名")]
#endif
		public string SelfRegisterName = "_待指定_";
		
		
		
		

		

#if UNITY_EDITOR
		private void _OVC_RegisterNameChanged()
		{
			if (!RelatedAreaName.Equals("_关联区域_待指定_", StringComparison.OrdinalIgnoreCase))
			{
				_Button_FindParentAreaName(true);
			}
		}

		private void _IB_SyncName()
		{
			SelfRegisterName = gameObject.name;
		}
#endif

		
		public void _Button_FindParentAreaName(bool noDialogue = false)
		{
			EP_AreaByTrigger area;
			area = GetComponentInParentRecursively(gameObject, typeof(EP_AreaByTrigger));

			if (area == null && !noDialogue)
			{
#if UNITY_EDITOR
				UnityEditor.EditorUtility.DisplayDialog("错误", "找不到父级区域，它真的处于某个【区域】之下吗？", "确定");
#endif
				return;
			}

			RelatedAreaName = area.AreaUID;
			
		}


		EP_AreaByTrigger GetComponentInParentRecursively(GameObject obj, Type type)
		{
			if (obj == null)
			{
				return null;
			}
			Component comp = obj.GetComponent(type);
			if (comp != null)
			{
				return comp as EP_AreaByTrigger;
			}
			return GetComponentInParentRecursively(obj.transform.parent.gameObject, type);
		}
		
		
		
		
#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(transform.position, 0.25f);


			if (SpawnPointPresetType == EnemySpawnPoint_PresetTypeEnum.FromAndTo_从这里到那里)
			{
				if (TargetPointPositionGO != null)
				{
					Gizmos.color = Color.cyan;
					Gizmos.DrawSphere(TargetPointPositionGO.transform.position, 0.25f);

				}
			}
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