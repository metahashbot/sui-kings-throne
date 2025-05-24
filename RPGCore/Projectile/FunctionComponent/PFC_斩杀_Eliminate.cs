using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Pool;
namespace RPGCore.Projectile
{
	[Serializable]
	public class PFC_斩杀_Eliminate : ProjectileBaseFunctionComponent
	{
		[SerializeField, LabelText("斩杀线——对普通敌人"), FoldoutGroup("配置", true), SuffixLabel("%")]
		public float EliminatePartial_ToNormal = 5f;


		[SerializeField, LabelText("斩杀对精英敌人生效"), FoldoutGroup("配置", true)]
		public bool AvailableToElite = true;

		[SerializeField, LabelText("斩杀线——对精英敌人"), FoldoutGroup("配置", true), SuffixLabel("%"),
		 ShowIf(nameof(AvailableToElite))]
		public float EliminatePartial_ToElite = 5f;

		[SerializeField, LabelText("斩杀对BOSS生效"), FoldoutGroup("配置", true)]
		public bool AvailableToBoss = false;

		[SerializeField, LabelText("斩杀线——对BOSS"), FoldoutGroup("配置", true), SuffixLabel("%"),
		 ShowIf(nameof(AvailableToBoss))]
		public float EliminatePartial_ToBoss = 5f;



		public static PFC_斩杀_Eliminate GetFromPool(PFC_斩杀_Eliminate copy = null)
		{
			PFC_斩杀_Eliminate tmpNew = GenericPool<PFC_斩杀_Eliminate>.Get();
			if (copy != null)
			{
				tmpNew.EliminatePartial_ToNormal = copy.EliminatePartial_ToNormal;
				tmpNew.EliminatePartial_ToElite = copy.EliminatePartial_ToElite;
				tmpNew.EliminatePartial_ToBoss = copy.EliminatePartial_ToBoss;
				tmpNew.AvailableToElite = copy.AvailableToElite;
				tmpNew.AvailableToBoss = copy.AvailableToBoss;
			}
			return tmpNew;
		}

		public override void ResetOnReturn()
		{
			GenericPool<PFC_斩杀_Eliminate>.Release(this);
		}
		public override void DeepCopyToRuntimeList(
			ProjectileBaseFunctionComponent copyFrom,
			List<ProjectileBaseFunctionComponent> targetList)
		{
			targetList.Add(GetFromPool(copyFrom as PFC_斩杀_Eliminate));
		}
	}
}