using System;
using RPGCore;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using UnityEngine;
namespace ARPG.Common.HitEffect
{
	[Serializable]
	public class PerDamageVFXInfoPair
	{

		[SerializeReference, LabelText("提供生效前提")]
		public BaseVFXInfoContent VFXInfoContent;
		
		[SerializeReference, LabelText("提供生成优先级|覆盖规则")]
		public BaseVFXSpawnSource VFXSpawnSource;

		[LabelText("无视伤害匹配？")]
		public bool IgnoreDamageType = false;

		public string VFXID;
		
		[AssetsOnly]
		public GameObject VFXPrefab;

	}


	[Serializable]
	public abstract class BaseVFXInfoContent
	{

	}

	[Serializable]
	public sealed class VFXInfo_CommonHit : BaseVFXInfoContent
	{
		[LabelText("关联的伤害类型"), SerializeField]
		public DamageTypeEnum DamageType;

		[LabelText("关联的BuffType"), SerializeField]
		public RolePlay_BuffTypeEnum RelatedBuff;
	}


	[Serializable]
	public sealed class VFXInfo_OnlyBuff : BaseVFXInfoContent
	{
		[LabelText("关联的单一Buff"), SerializeField]
		public RolePlay_BuffTypeEnum RelatedBuff;
	}

	[Serializable]
	public abstract class BaseVFXTransformContent
	{
		
	}
	[TypeInfoBox("仅仅挂在仅缩放的位置上")]
	[Serializable]
	public class VFXTransform_JustScalePosition : BaseVFXTransformContent
	{
		
	}

	[TypeInfoBox("仅仅挂在仅缩放的锚点上")]
	[Serializable]
	public class VFXTransform_JustScaleAnchor : BaseVFXTransformContent
	{
		
	}


	[TypeInfoBox("需要若干骨骼点")]
	[Serializable]
	public class VFXTransform_Bone : BaseVFXTransformContent
	{
		public string[] BoneConfigNames;
	}

	[TypeInfoBox("需要若干骨骼点位置")]
	[Serializable]
	public class VFXTransform_BonePosition : BaseVFXTransformContent
	{
		public string[] BoneConfigNames;
	}
	
	[Serializable]
	public abstract class BaseVFXSpawnSource
	{

	}

	[Serializable]
	public sealed class VFXSpawn_Override : BaseVFXSpawnSource
	{

	}

	[Serializable]
	public sealed class VFXSpawn_Priority : BaseVFXSpawnSource
	{
		public int Priority = 5;
	}


	[Serializable]
	public sealed class VFXSpawn_Addon : BaseVFXSpawnSource
	{
		
	}




}