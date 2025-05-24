using System;
using System.Collections.Generic;
using ARPG.Character.Base;
using RPGCore.UtilityDataStructure;
using UnityEngine;
using UnityEngine.Playables;
namespace RPGCore.Interface
{
	/// <summary>
	/// <para>包含VFX容器。被挂载在其之下的特效将会被记录，用于</para>
	/// </summary>
	public interface I_RP_ContainVFXContainer
	{
		public List<VFX_ParticleSystemPlayProxy> SelfRegisteredPSPPInfoRuntimeList { get; protected set; }
	
		 public List<ParticleSystem> SelfRegisteredVFXInfoRuntimeList { get; protected set; }
		 
		
		public List<ConSer_VFXHolderInfo> SelfVFXHolderInfoList { get; protected set; }
		
		/// <summary>
		/// <para>获取自身物件在场景中的特效缩放半径。这是一个在ArtHelper上配置的数值</para>
		/// </summary>
		/// <returns></returns>
		public abstract float GetSelfVFXScaleRadius();

		public abstract PlayableDirector GetPlayableDirector();
		/// <summary>
		/// <para>获取自身所属的RPG角色在Hierarchy中的尺寸。某些特效配置可能需要剔除这个尺寸的影响。</para>
		/// </summary>
		public abstract float GetSelfCharacterHierarchyScale();
		/// <summary>
		/// <para>获取关联的骨骼跟随器的Transform。同时需要传入VFXInfo。如果仅仅是位置同步那就不用记住，如果是挂点那就需要记录一下，在自身角色销毁的时候，这个VFXInfo是需要被归还到池中的</para>
		/// </summary>
		public virtual (Transform, float) GetVFXHolderTransformAndRegisterVFX(
			string boneConfigName,
			VFX_ParticleSystemPlayProxy relatedVFXInfo,
			bool mustExist = false)
		{
			int findIndex = SelfVFXHolderInfoList.FindIndex((info =>
				string.Equals(info.FollowConfigName, boneConfigName, StringComparison.OrdinalIgnoreCase)));
			if (findIndex == -1)
			{
				if (mustExist)
				{
					DBug.LogError($"特效配置错误：{(this as MonoBehaviour).name}上的VFX挂点信息们不存在配置：{boneConfigName}，返回了自身transform");
					return ((this as MonoBehaviour).transform, 0f);
				}
				return (null, 0f);
			}
			ConSer_VFXHolderInfo info = SelfVFXHolderInfoList[findIndex];
			if(info.UseCollisionCenter && this is BaseARPGArtHelper arpgArt && arpgArt.SelfRelatedBaseRPBaseBehaviour is BaseARPGCharacterBehaviour arpgBehaviour)
			{
				DBug.LogError( $"特效配置错误：{(this as MonoBehaviour).name}上的VFX挂点信息们配置了碰撞中心。【碰撞中心】只允许使用“位置”而不是“挂载”，返回了自身transform");
			}

			if (!SelfRegisteredPSPPInfoRuntimeList.Contains(relatedVFXInfo))
			{
				SelfRegisteredPSPPInfoRuntimeList.Add(relatedVFXInfo);
			}
			
			return (info.SelfAnchorTransform, GetSelfVFXScaleRadius());
		}
		public virtual (Transform, float) GetVFXHolderTransformAndRegisterVFX(
			string boneConfigName,
			ParticleSystem relatedVFXInfo,
			bool mustExist = false)
		{
			int findIndex = SelfVFXHolderInfoList.FindIndex((info =>
				string.Equals(info.FollowConfigName, boneConfigName, StringComparison.OrdinalIgnoreCase)));
			if (findIndex == -1)
			{
				if (mustExist)
				{
					DBug.LogError(
						$"特效配置错误：{(this as MonoBehaviour).name}上的VFX挂点信息们不存在配置：{boneConfigName}，返回了自身transform");
					return ((this as MonoBehaviour).transform, 0f);
				}
				return (null, 0f);
			}
			ConSer_VFXHolderInfo info = SelfVFXHolderInfoList[findIndex];
			if (info.UseCollisionCenter)
			{
				DBug.LogError($"特效配置错误：{(this as MonoBehaviour).name}上的VFX挂点信息们配置了碰撞中心。【碰撞中心】只允许使用“位置”而不是“挂载”，返回了自身transform");
				return ((this as MonoBehaviour).transform, 0f);
			}

			if (!SelfRegisteredVFXInfoRuntimeList.Contains(relatedVFXInfo))
			{
				SelfRegisteredVFXInfoRuntimeList.Add(relatedVFXInfo);
			}

			return (info.SelfAnchorTransform, GetSelfVFXScaleRadius());
		}
		
		public virtual (Nullable<Vector3>,float) GetVFXHolderGlobalPosition(string configName,bool allowNotExist = false)
		{
			
			int findIndex = SelfVFXHolderInfoList.FindIndex((info =>
				string.Equals(info.FollowConfigName, configName, StringComparison.OrdinalIgnoreCase)));
			if (findIndex == -1 && !allowNotExist)
			{
				DBug.LogError($"{(this as MonoBehaviour).name}上的VFX挂点信息们不存在配置：{configName}，没有返回任何挂点");
				return (null, 0f);
			}
			ConSer_VFXHolderInfo info = SelfVFXHolderInfoList[findIndex];

			if (info.UseCollisionCenter && this is BaseARPGArtHelper arpgArt && arpgArt.SelfRelatedBaseRPBaseBehaviour is BaseARPGCharacterBehaviour arpgBehaviour)
			{
				return (arpgBehaviour.GetCollisionCenter(), GetSelfVFXScaleRadius());
			}
			return (info.SelfAnchorTransform.position, GetSelfVFXScaleRadius());
		}

		public virtual (Nullable<Vector3>, float) GetVFXHolderGlobalPosition(params string[] boneToFind)
		{
			for (int i = 0; i < boneToFind.Length; i++)
			{
				(Vector3?, float) get2 = GetVFXHolderGlobalPosition(boneToFind[i], false);
				if (get2.Item1 != null)
				{
					return get2;
				}
			}
			DBug.LogError( $"{(this as MonoBehaviour).name}上的VFX挂点信息们不存在配置：{string.Join(",", boneToFind)}，没有返回任何挂点");
	
			return (null, 0f);
		}

	}
}
