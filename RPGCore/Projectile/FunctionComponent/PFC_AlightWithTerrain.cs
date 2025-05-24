using System;
using System.Collections.Generic;
using ARPG.Manager;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Projectile
{
	[Serializable]
	public class PFC_AlightWithTerrain : ProjectileBaseFunctionComponent
	{
		protected static readonly int AlightFrequency = 10;

		/// <summary>
		/// <para>校准是否为实时的？在战斗环境下，它并不是即时的，只会校准到刷新后的地形高度。</para>
		/// <para>非战斗环境下， 它是实时的，每帧都会校准</para>
		/// </summary>
		protected static bool IsAlignInstantly = false;
		public static void EnableAlignInstantly()
		{
			IsAlignInstantly = true;
		}

		public static void DisableAlignInstantly()
		{
			IsAlignInstantly = false;
		}
		
		


		
		[LabelText("校准的高度"),FoldoutGroup("配置",true),SerializeField]
		public float AlightHeight = 1f;

		protected static RaycastHit[] _alignCheckResults = new RaycastHit[1];


		protected static SubGameplayLogicManager_ARPG _glmRef;


		public static PFC_AlightWithTerrain GetFromPool(PFC_AlightWithTerrain copyFrom)
		{
			var newInstance = GenericPool<PFC_AlightWithTerrain>.Get();
			newInstance.AlightHeight = copyFrom.AlightHeight;
			return newInstance; 
			
		}
		public override void OnFunctionStart(float currentTime)
		{
			var selfPos = _selfRelatedProjectileBehaviourRef.RelatedGORef.transform.position;
			selfPos.y = CurrentTerrainHeight + AlightHeight;
			// _selfRelatedProjectileBehaviourRef.RelatedGORef.transform.position = selfPos;
			_glmRef = SubGameplayLogicManager_ARPG.Instance;
			_selfRelatedProjectileBehaviourRef.RelatedGORef.transform.position =
				GetAlignPos() + Vector3.up * AlightHeight;
		}

		public override void FixedUpdateTick(float currentTime, int currentFrame, float delta)
		{
			base.FixedUpdateTick(currentTime, currentFrame, delta);
			if (IsAlignInstantly)
			{
				_selfRelatedProjectileBehaviourRef.RelatedGORef.transform.position =
					GetAlignPos() + Vector3.up * AlightHeight;
			}
			else
			{
				if (currentFrame % AlightFrequency == 0)
				{
					_selfRelatedProjectileBehaviourRef.RelatedGORef.transform.position = GetAlignPos() +
					                                                                     Vector3.up * AlightHeight;
				}
			}
		}

		private Vector3 GetAlignPos()
		{
			return _glmRef.GetAlignedTerrainPosition(_selfRelatedProjectileBehaviourRef.RelatedGORef.transform.position) ??
			       _selfRelatedProjectileBehaviourRef.RelatedGORef.transform.position;
		}

		public override void ResetOnReturn()
		{
			GenericPool<PFC_AlightWithTerrain>.Release(this);
		}
		public override void DeepCopyToRuntimeList(ProjectileBaseFunctionComponent copyFrom, List<ProjectileBaseFunctionComponent> targetList)
		{
			targetList.Add(GetFromPool(copyFrom as PFC_AlightWithTerrain));

		}
	}
}