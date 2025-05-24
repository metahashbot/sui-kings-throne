using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.DropItem
{
	[Serializable]
	public class PerDropItemConfig
	{
		[LabelText("掉落物配置的ID")]
		public string DropItemConfigID;

		[LabelText("包含可用性调整"), SerializeField]
		public bool ContainAvailabilityToggle = true;

		[LabelText("调整可用性为"), SerializeField, ShowIf(nameof(ContainAvailabilityToggle))]
		public bool ToggleAvailabilityTo = true;

		[LabelText("掉落预设Prefab")]
		public GameObject _prefab_Item;

		[LabelText("尺寸区间")]
		public Vector2 SizeRange = new Vector2(0.75f, 1.25f);


		[LabelText("触发吸附判定半径") ]
		public float BeginAbsorbingRadiusMul = 2f;

		[LabelText("真的拾取到的半径")]
		public float FinalPickupDistance = 0.5f;

		[LabelText("刚生成有多长时间不能被拾取")]
		public float SpawnPickupTime = 0.35f;

		[LabelText("持续时长")]
		public float DropItemExistDuration = 25f;

		[SerializeReference, LabelText("掉落的条件们")]
		public List<DropComponent_DropCondition> DropConditions;

		[SerializeReference, LabelText("掉落的生成方式组件")]
		public DropComponent_DropItemSpawn SpawnComponent;

		[SerializeReference, LabelText("拾取过程中")]
		public BaseDropItemPicking Picking;
		

		[SerializeReference, LabelText("拾取后的功能组件们")]
		public List<BaseDropItemPickedEffect> PickupEffectList;

	}



}