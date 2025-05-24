using System;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.DropItem
{
	/// <summary>
	/// <para>正在拾取——往人身上飞的那段</para>
	/// </summary>
	[Serializable]
	public abstract class BaseDropItemPicking
	{
		public abstract void UpdateTick(GameObject self, Transform target, float ct, int cf, float delta);
	}

	[Serializable]
	public class Picking_TweenFlying : BaseDropItemPicking
	{
		[LabelText("飞行速度")]
		public float FlySpeed = 5f;

		[LabelText("飞行额外高度")]
		public float FlyHeightOffset = 0.45f;
		public override void UpdateTick(GameObject self, Transform target, float ct, int cf, float delta)
		{
			Vector3 targetPos = target.position + Vector3.up * FlyHeightOffset;
			self.transform.position = Vector3.MoveTowards(self.transform.position, targetPos, FlySpeed * delta);

		}
	}


}