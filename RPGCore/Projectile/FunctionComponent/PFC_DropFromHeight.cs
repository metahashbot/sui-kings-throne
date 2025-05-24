using System;
using System.Collections.Generic;
using RPGCore.Interface;
using RPGCore.Projectile.Layout;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Projectile
{

	/// <summary>
	/// <para>这是一个从高处降落的Projectile。在一定高度之上才会计入判定</para>
	/// </summary>
	[Serializable]
	public class PFC_DropFromHeight : ProjectileBaseFunctionComponent
	{


		[LabelText("距离地面这么多高度时开始有碰撞判定"), ShowInInspector, NonSerialized]
		public float TakeEffectFromHeight;

		public bool TakeCollisionEffect = false;
		
		[LabelText("目标落点") ,ShowInInspector, NonSerialized]
		public Vector3 TargetDropPoint;

		[LabelText("销毁高度"), ShowInInspector, NonSerialized]
		public float DestroyHeight;
		
		[NonSerialized, LabelText("当前投射物相对于地形的高度")]
		public float  CurrentProjectileHeightToTerrain;

		public static PFC_DropFromHeight GetFromPool(PFC_DropFromHeight copyFrom = null)
		{
			var newPFC = GenericPool<PFC_DropFromHeight>.Get();

			if (copyFrom != null)
			{
				newPFC.TakeEffectFromHeight = copyFrom.TakeEffectFromHeight;
				newPFC.TargetDropPoint = copyFrom.TargetDropPoint;
				newPFC.DestroyHeight = copyFrom.DestroyHeight;
			}

			return newPFC;
		}



		public override void ResetOnReturn()
		{
			GenericPool<PFC_DropFromHeight>.Release(this);
		}
		public override void DeepCopyToRuntimeList(ProjectileBaseFunctionComponent copyFrom, List<ProjectileBaseFunctionComponent> targetList)
		{
			targetList.Add(GetFromPool(copyFrom as PFC_DropFromHeight));
		}


		public override void FixedUpdateTick(float currentTime, int currentFrame, float delta)
		{
			float currentHeightY = _selfRelatedProjectileBehaviourRef.RelatedGORef.transform.position.y;
			CurrentProjectileHeightToTerrain = currentHeightY - CurrentTerrainHeight;
			if (currentHeightY < CurrentTerrainHeight + TakeEffectFromHeight)
			{
				TakeCollisionEffect = true;
			}
			else
			{
				TakeCollisionEffect = false;
			}
			
			if (currentHeightY < CurrentTerrainHeight + DestroyHeight)
			{
				_selfRelatedProjectileBehaviourRef.SelfLayoutConfigReference.LayoutHandlerFunction.AddWaitToRemove(
					_selfRelatedProjectileBehaviourRef);
				return;
			}

			
			
		}


	}
}