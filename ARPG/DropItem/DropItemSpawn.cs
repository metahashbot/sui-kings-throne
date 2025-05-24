using System;
using ARPG.Character.Base;
using ARPG.Manager;
using DG.Tweening;
using RPGCore.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.DropItem
{
	[Serializable]
	public abstract class DropComponent_DropItemSpawn
	{
		public abstract void OnSpawnDropItem(GameObject relatedGO,BaseARPGCharacterBehaviour behaviour);
	}

	[Serializable]
	[TypeInfoBox("Tween一个Jump")]
	public class DropComponent_TweenJump : DropComponent_DropItemSpawn
	{
		[SerializeField, LabelText("跳跃动画的力度")]
		[FoldoutGroup("配置", true)]
		public float JumpAnimationForce = 1f;
		[SerializeField, LabelText("跳跃动画额时长")]
		[FoldoutGroup("配置", true)]
		public float JumpAnimationDuration = 0.5f;
		[SerializeField, LabelText("跳跃动画使用的插值方式")]
		[FoldoutGroup("配置", true)]
		public Ease EaseType = Ease.InOutSine;
		
		[SerializeField,LabelText("包含生成特效")]
		[FoldoutGroup("配置", true)]
		public bool ContainSpawnEffect = true;
		
		[SerializeField,LabelText("生成特效Prefab")]
		[FoldoutGroup("配置", true),ShowIf(nameof(ContainSpawnEffect))]
		public GameObject SpawnEffectPrefab;
		
		[SerializeField,LabelText("掉落范围随机半径")]
		[FoldoutGroup("配置", true)]
		public float RandomRadius = 0.5f;
		

		public override void OnSpawnDropItem(GameObject relatedGO,BaseARPGCharacterBehaviour behaviour)
		{
			if (ContainSpawnEffect && SpawnEffectPrefab)
			{
				var pos=
				(behaviour.GetRelatedArtHelper() as I_RP_ContainVFXContainer).GetVFXHolderGlobalPosition("胸部");
				if (pos.Item1 != null)
				{
					Vector3 fromPos = pos.Item1.Value;
					var pb = VFXPoolManager.Instance.GetParticleSystemRuntimeByPrefab(SpawnEffectPrefab);
					pb.transform.position = pos.Item1.Value;
					pb.transform.localScale = pos.Item2 * Vector3.one;
					pb.Play();
				}
			}
			Vector3 toPos = relatedGO.transform.position;
			toPos += new Vector3(UnityEngine.Random.Range(-RandomRadius, RandomRadius),
				0f,
				UnityEngine.Random.Range(-RandomRadius, RandomRadius));
			var aligned = SubGameplayLogicManager_ARPG.Instance.GetAlignedTerrainPosition(toPos);
			if (aligned.HasValue)
			{

				relatedGO.transform.DOLocalJump(aligned.Value, JumpAnimationForce, 1, JumpAnimationDuration).SetEase(EaseType);
			}
		}
	}


}