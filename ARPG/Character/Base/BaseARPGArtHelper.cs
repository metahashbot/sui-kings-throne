using System;
using System.Collections.Generic;
using ARPG.Character.Base.CustomSpineData;
using ARPG.Common.HitEffect;
using ARPG.Manager;
using Global;
using Global.ActionBus;
using Global.Character;
using RPGCore;
using RPGCore.DataEntry;
using RPGCore.Interface;
using RPGCore.Projectile;
using RPGCore.UtilityDataStructure;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;
namespace ARPG.Character.Base
{
	[TypeInfoBox("一个ARPG的角色可以同时拥有多个Spine或帧动画对象。\n" + "每个Spine或帧动画 对象上都需要挂载 对应的 AnimationHelperBase\n" +
	             "上面记录的【XX索引】就是它在游戏性逻辑中代表的第几套，有些动画会指定某个AnimationHelper播放\n")]
	public class BaseARPGArtHelper : RolePlay_ArtHelperBase, I_RP_ContainVFXContainer
	{





		[FormerlySerializedAs("HUDSpineScale")]
		[SerializeField, LabelText("UI中Spine大小")][TitleGroup("===UI===")]
		protected float _HUDSpineScale = 1.87f;

		[SerializeField, LabelText("UI中Sheet大小")] [TitleGroup("===UI===")]
		protected float _HUDSheetScale = 1f;


		private List<VFX_ParticleSystemPlayProxy> _selfRegisteredPSPPInfoRuntimeList;
		private List<ParticleSystem> _selfRegisteredVFXInfoRuntimeList1;


		public override void InitializeOnInstantiate(LocalActionBus lab)
		{
			base.InitializeOnInstantiate(lab);
			_localActionBusRef = lab;
			_selfRegisteredPSPPInfoRuntimeList = new List<VFX_ParticleSystemPlayProxy>();
			_selfRegisteredVFXInfoRuntimeList1 = new List<ParticleSystem>();

		}

		public override void UpdateTick(float currentTime, int currentFrame, float deltaTime)
		{
			base.UpdateTick(currentTime, currentFrame, deltaTime);
		}

		public override void FixedUpdateTick(float ct, int cf, float delta)
		{
			base.FixedUpdateTick(ct, cf, delta);
			foreach (var perHelper in CharacterAnimationHelperObjectsDict.Values)
			{
				perHelper.FixedUpdateTick(ct, cf, delta);
			}
		}



		// public void ToggleTargetHelpers(bool isOn, params int[] indexArray)
		// {
		// 	foreach (int perIndex in indexArray)
		// 	{
		// 		if (CharacterAnimationHelperObjectsDict.TryGetValue(perIndex,
		// 			out CharacterAnimationHelperBase targetHelper))
		// 		{
		// 			if (isOn && !targetHelper.gameObject.activeSelf)
		// 			{
		// 				targetHelper.gameObject.SetActive(true);
		// 			}
		// 			else if (!isOn && targetHelper.gameObject.activeSelf)
		// 			{
		// 				targetHelper.gameObject.SetActive(false);
		// 			}
		// 		}
		// 	}
		// }




		public PlayableDirector GetPlayableDirector()
		{
			return SelfPlayableDirector;
		}


#if UNITY_EDITOR
		// [Button("为此之下的Spine或者Sheet添加SpineHelper——记得手动设置索引"), GUIColor(0.4f, 0.6f, 1f)]
		// private void _Button_AddSpineHelper()
		// {
		// 	var spines = GetComponentsInChildren<SkeletonAnimation>(true);
		// 	for (int i = 0; i < spines.Length; i++)
		// 	{
		// 		spines[i].zSpacing = (float)-5e-5;
		// 		var spineHelper = spines[i].GetComponent<BaseCharacterSpineHelper>();
		//
		// 		if (spineHelper == null)
		// 		{
		// 			spineHelper = spines[i].gameObject.AddComponent<BaseCharacterSpineHelper>();
		// 			spineHelper.gameObject.transform.localScale = Vector3.one;
		// 			spineHelper.gameObject.transform.localPosition = Vector3.zero;
		//
		// 			spineHelper.SpineObjectIndex = i;
		// 			if (i == 0)
		// 			{
		// 				spineHelper.InitActive = true;
		// 			}
		//
		// 		}
		// 	}
		// }

#endif

		public override void ClearBeforeDestroy()
		{
			for (int i = _selfRegisteredPSPPInfoRuntimeList.Count - 1; i >= 0; i--)
			{
				var perObj = _selfRegisteredPSPPInfoRuntimeList[i];
				if (perObj == null || perObj.gameObject == null)
				{
					_selfRegisteredPSPPInfoRuntimeList.RemoveAt(i);
					continue;
				}
				else
				{
					if (perObj.transform.IsChildOf(SelfRelatedBaseRPBaseBehaviour.transform))
					{
						perObj.transform.SetParent(VFXPoolManager.Instance.transform);
					}
					continue;

				}
			}
		}


		List<VFX_ParticleSystemPlayProxy> I_RP_ContainVFXContainer.SelfRegisteredPSPPInfoRuntimeList
		{
			get => _selfRegisteredPSPPInfoRuntimeList;
			set => _selfRegisteredPSPPInfoRuntimeList = value;
		}
		List<ParticleSystem> I_RP_ContainVFXContainer.SelfRegisteredVFXInfoRuntimeList
		{
			get => _selfRegisteredVFXInfoRuntimeList1;
			set => _selfRegisteredVFXInfoRuntimeList1 = value;
		}
		List<ConSer_VFXHolderInfo> I_RP_ContainVFXContainer.SelfVFXHolderInfoList
		{
			get => _selfVFXHolderInfoList;
			set => _selfVFXHolderInfoList = value;
		}
		public float GetSelfVFXScaleRadius()
		{
			return _VFXScaleRadius;
		}
		public float GetSelfCharacterHierarchyScale()
		{
			return SelfRelatedBaseRPBaseBehaviour.transform.localScale.x;
		}





	}
}