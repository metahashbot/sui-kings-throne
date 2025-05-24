using System;
using Global;
using Global.ActionBus;
using Global.AreaOnMap;
using Global.AreaOnMap.EditorProxy;
using Global.GlobalConfig;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Manager
{
	
	[TypeInfoBox("这是区域配置信息。运行时会将原先在场景内的区域配置信息删除，使用场景通用配置中生成出的原始prefab。\n" +
	             "所以一切编辑期未保存至Prefab中的修改都将不会生效，为了避免误操作和未上传等问题")]
	[AddComponentMenu("!#编辑期辅助#/[区域配置信息]/[区域配置信息]-AreaLogicHolder", -33)]
	[Serializable]
    public class EditorProxy_AreaLogicHolder : MonoBehaviour
    {
        [SerializeField, LabelText("基本全局预设的EP"), Required]
        public EP_BaseArea BaseAreaEPRef;
		[InfoBox("一个副本入口可以对应多个关卡，如果能从关卡配置读到配置就是肉鸽关卡，否则就是剧情关卡")]
		[LabelText("关卡编号")]
		public int MissionUID;

#if UNITY_EDITOR
        [Button("移除刚体！")]
		private void _Button_RemoveRigidbody()
		{
			foreach (Rigidbody perR in GetComponentsInChildren<Rigidbody>())
			{
				DestroyImmediate(perR);
			}
		}

		[Button("通通对齐！"), GUIColor(1f, 0f, 0f)]
		private void _Button_AlignAll()
		{
			foreach (EP_BaseArea area in GetComponentsInChildren<EP_BaseArea>())
			{
				area._Button_AlignVerticalTerrainCheck();
			}
			foreach (EP_SceneObjectRegister obj in GetComponentsInChildren<EP_SceneObjectRegister>())
			{
				obj._Button_AlignVerticalTerrainCheck();
			}
		}
#endif
		
		private void OnValidate()
		{
			transform.position = Vector3.zero;
		}

		private void Start()
		{
			foreach (Rigidbody perR in GetComponentsInChildren<Rigidbody>())
			{
				Destroy(perR);
			}
        }

		/// <summary>
		/// <para>调整下方的所有空气墙的MeshRenderer</para>
		/// </summary>
		[Button("开启下方所有空气墙的网格渲染",ButtonSizes.Large,Icon = SdfIconType.Box)]
		public void ActivateAirWallMeshRenderer()
		{
			var allMR = GetComponentsInChildren<MeshRenderer>();
			foreach (var perMR in allMR)
			{
				if (perMR.TryGetComponent(out Collider col) && col.gameObject.layer == 16)
				{
					perMR.enabled = true;
				}
			}
		}

		[Button("关闭下方所有空气墙的网格渲染", ButtonSizes.Large, Icon = SdfIconType.DoorClosed)]
		public void DisableAirWallMeshRenderer()
		{
			var allMR = GetComponentsInChildren<MeshRenderer>();
			foreach (var perMR in allMR)
			{
				if (perMR.TryGetComponent(out Collider col) && col.gameObject.layer == 16)
				{
					perMR.enabled = false;
				}
			}
		}
    }
}