using System;
using System.Collections.Generic;
using ARPG.Manager;
using Global.ActionBus;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;
namespace RPGCore.Projectile
{
	[Serializable] [TypeInfoBox("沿运行距离是真的运行的距离，会一边运动一边生成那种")]
	public class PFC_沿运行距离生成特效_SpawnVFXOverDistance : ProjectileBaseFunctionComponent
	{
			
		[SerializeField, LabelText("每运行多少距离生成一次特效")]
		public float SpawnDistanceInterval = 1f;


		[SerializeField, LabelText("特效预制体")]
		public GameObject VFXPrefab;


		
		[SerializeField,LabelText("生成时校准到地形的高度")]
		public float SpawnHeightOffset = 0.1f;
		
		[SerializeField, LabelText("√: 覆写生成物尺寸 || 口: 乘算生成物尺寸")]
		public bool OffsetScaleOverride = false;
		
		
		[SerializeField,LabelText("生成物尺寸修改")]
		public float OffsetScale = 1f;
		
		[SerializeField,LabelText("附带一次生成")]
		public bool AppendProjectile = false;
		
		[SerializeField,LabelText("    生成的响应ID")][ShowIf(nameof(AppendProjectile))]
		public string AppendProjectileMatchingID = "追加";
		

		protected Vector3 lastFramePos;
		protected float _remainingDistance;

		public override void OnFunctionStart(float currentTime)
		{
			base.OnFunctionStart(currentTime);
			_remainingDistance = SpawnDistanceInterval;
			if (VFXPrefab.GetComponent<ParticleSystem>())
			{
				ParticleSystem.MainModule mainModule = VFXPrefab.GetComponent<ParticleSystem>().main;
				mainModule.stopAction = ParticleSystemStopAction.Disable;
			}
			lastFramePos = _selfRelatedProjectileBehaviourRef.RelatedGORef.transform.position;
		}



		public override void FixedUpdateTick(float currentTime, int currentFrame, float delta)
		{
			base.FixedUpdateTick(currentTime, currentFrame, delta);
			var thisFramePos = _selfRelatedProjectileBehaviourRef.RelatedGORef.transform.position;

			var deltaMovement = Vector3.Distance(thisFramePos, lastFramePos);
			_remainingDistance -= deltaMovement;
			
			
			if (_remainingDistance < 0f)
			{
				_remainingDistance += SpawnDistanceInterval;
				SpawnRelatedVFX();
				
				//
				// _selfRelatedProjectileBehaviourRef.SelfLayoutConfigReference.LayoutHandlerFunction.
				
			}
			
			
			
			
			
			
			
			lastFramePos = _selfRelatedProjectileBehaviourRef.RelatedGORef.transform.position;
			
		}


		private void SpawnRelatedVFX()
		{
			if (AppendProjectile)
			{
				var ds = new DS_ActionBusArguGroup(ActionBus_ActionTypeEnum
					.L_PLC_Spawn_InternalAppendProjectile_内部要求追加投射物);
				ds.ObjectArguStr = AppendProjectileMatchingID;
				ds.ObjectArgu1 = _selfRelatedProjectileBehaviourRef;
				_selfRelatedProjectileBehaviourRef.GetRelatedActionBusRef()?.TriggerActionByType(ds);
			}
			if (!VFXPrefab)
			{
				DBug.LogError($"PFC_沿运行距离生成特效的时候，没有指定关联的Prefab");
				return;
			}


			var spawnPS = VFXPoolManager.Instance.GetParticleSystemRuntimeByPrefab(VFXPrefab);
			var pos = _selfRelatedProjectileBehaviourRef.RelatedGORef.transform.position;
			if (GameReferenceService_ARPG.Instance)
			{
				pos = GameReferenceService_ARPG.Instance.SubGameplayLogicManagerRef.GetAlignedTerrainPosition(pos) ??
				      pos;
				pos += Vector3.up * SpawnHeightOffset;
			}

			if (OffsetScaleOverride)
			{
				spawnPS.transform.localScale = Vector3.one * OffsetScale;
			}
			else
			{
				spawnPS.transform.localScale = Vector3.one * VFXPrefab.transform.localScale.x;
			}
			
			spawnPS.transform.position = pos;
			spawnPS.Play();


			
			

		}

		public static PFC_沿运行距离生成特效_SpawnVFXOverDistance GetFromPool(PFC_沿运行距离生成特效_SpawnVFXOverDistance copy = null)
		{
			var tmpNew = GenericPool<PFC_沿运行距离生成特效_SpawnVFXOverDistance>.Get();
			if (copy != null)
			{

				tmpNew.SpawnDistanceInterval = copy.SpawnDistanceInterval;
				tmpNew.VFXPrefab = copy.VFXPrefab;
				tmpNew.SpawnHeightOffset = copy.SpawnHeightOffset;
				tmpNew.OffsetScale = copy.OffsetScale;
				tmpNew.OffsetScaleOverride = copy.OffsetScaleOverride;
				tmpNew.AppendProjectile = copy.AppendProjectile;
				tmpNew.AppendProjectileMatchingID = copy.AppendProjectileMatchingID;
				

			}
			return tmpNew;
		}
        

		public override void ResetOnReturn()
		{
			GenericPool<PFC_沿运行距离生成特效_SpawnVFXOverDistance>.Release(this);
		}
		public override void DeepCopyToRuntimeList(
			ProjectileBaseFunctionComponent copyFrom,
			List<ProjectileBaseFunctionComponent> targetList)
		{
			targetList.Add(GetFromPool(copyFrom as PFC_沿运行距离生成特效_SpawnVFXOverDistance));
		}
	}
}